import { useState } from 'react';
import { bookingsApi } from '../../api/bookingsApi';
import { desksApi } from '../../api/desksApi';
import { Alert } from '../../components/Alert';
import { DateField } from '../../components/DateField';
import { PageHeader } from '../../components/PageHeader';
import { useAction } from '../../hooks/useAction';
import { useAsyncData } from '../../hooks/useAsyncData';
import type { Desk } from '../../types/models';
import { formatDate, todayIso } from '../../utils/dates';
import { AvailableDeskCard } from './AvailableDeskCard';
import styles from './BookDeskPage.module.scss';

export function BookDeskPage() {
  const [date, setDate] = useState(todayIso());
  const [confirmation, setConfirmation] = useState<string>();
  const availableDesks = useAsyncData(() => desksApi.getAvailable(date), [date]);
  const booking = useAction();

  function changeDate(newDate: string) {
    setDate(newDate);
    setConfirmation(undefined);
    booking.clearError();
  }

  async function book(desk: Desk) {
    setConfirmation(undefined);
    const succeeded = await booking.run(async () => {
      await bookingsApi.create({ deskId: desk.id, date });
    });

    if (succeeded) {
      setConfirmation(`Desk ${desk.code} is booked for you on ${formatDate(date)}.`);
      availableDesks.reload();
    }
  }

  return (
    <>
      <PageHeader
        title="Book a desk"
        description="Pick a day to see which desks are still free."
        actions={<DateField label="Date" value={date} min={todayIso()} onChange={changeDate} />}
      />

      <div className={styles.messages}>
        <Alert kind="success" message={confirmation} />
        <Alert kind="error" message={booking.error ?? availableDesks.error} />
      </div>

      {availableDesks.isLoading && <p className="page-message">Loading desks…</p>}

      {availableDesks.data?.length === 0 && <p className="empty-state">Every desk is taken on {formatDate(date)}.</p>}

      <div className={styles.grid}>
        {availableDesks.data?.map((desk) => (
          <AvailableDeskCard key={desk.id} desk={desk} isBooking={booking.isPending} onBook={book} />
        ))}
      </div>
    </>
  );
}
