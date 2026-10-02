import { Link } from 'react-router';
import { bookingsApi } from '../../api/bookingsApi';
import { Alert } from '../../components/Alert';
import { PageHeader } from '../../components/PageHeader';
import { useAction } from '../../hooks/useAction';
import { useAsyncData } from '../../hooks/useAsyncData';
import type { Booking } from '../../types/models';
import { formatDate } from '../../utils/dates';

export function MyBookingsPage() {
  const myBookings = useAsyncData(() => bookingsApi.getMine(), []);
  const cancellation = useAction();

  async function cancel(booking: Booking) {
    if (!window.confirm(`Cancel your booking of ${booking.deskCode} on ${formatDate(booking.date)}?`)) return;

    const succeeded = await cancellation.run(() => bookingsApi.cancel(booking.id));
    if (succeeded) myBookings.reload();
  }

  return (
    <>
      <PageHeader title="My bookings" description="Your upcoming desk bookings." />
      <Alert kind="error" message={cancellation.error ?? myBookings.error} />

      {myBookings.data?.length === 0 && (
        <p className="empty-state">
          You have no upcoming bookings. <Link to="/">Book a desk</Link>
        </p>
      )}

      {!!myBookings.data?.length && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Desk</th>
              <th>Floor</th>
              <th className="data-table__actions">Actions</th>
            </tr>
          </thead>
          <tbody>
            {myBookings.data.map((booking) => (
              <tr key={booking.id}>
                <td>{formatDate(booking.date)}</td>
                <td>
                  <strong>{booking.deskCode}</strong>
                </td>
                <td>{booking.deskFloor}</td>
                <td className="data-table__actions">
                  <button
                    type="button"
                    className="button button--small button--danger"
                    disabled={cancellation.isPending}
                    onClick={() => cancel(booking)}
                  >
                    Cancel
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  );
}
