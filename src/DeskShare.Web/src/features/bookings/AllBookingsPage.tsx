import { useState } from 'react';
import { bookingsApi } from '../../api/bookingsApi';
import { Alert } from '../../components/Alert';
import { DateField } from '../../components/DateField';
import { PageHeader } from '../../components/PageHeader';
import { useAsyncData } from '../../hooks/useAsyncData';
import { formatDate, todayIso } from '../../utils/dates';

export function AllBookingsPage() {
  const [date, setDate] = useState(todayIso());
  const bookings = useAsyncData(() => bookingsApi.getAllOnDate(date), [date]);

  return (
    <>
      <PageHeader
        title="All bookings"
        description="Who is sitting where on a given day."
        actions={<DateField label="Date" value={date} onChange={setDate} />}
      />
      <Alert kind="error" message={bookings.error} />

      {bookings.data?.length === 0 && <p className="empty-state">No desks are booked on {formatDate(date)}.</p>}

      {!!bookings.data?.length && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Desk</th>
              <th>Floor</th>
              <th>Employee</th>
            </tr>
          </thead>
          <tbody>
            {bookings.data.map((booking) => (
              <tr key={booking.id}>
                <td>
                  <strong>{booking.deskCode}</strong>
                </td>
                <td>{booking.deskFloor}</td>
                <td>{booking.employeeName}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  );
}
