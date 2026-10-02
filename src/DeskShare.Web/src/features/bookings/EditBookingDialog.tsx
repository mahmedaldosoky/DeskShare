import { useState, type FormEvent } from 'react';
import { bookingsApi } from '../../api/bookingsApi';
import { desksApi } from '../../api/desksApi';
import { Alert } from '../../components/Alert';
import { DateField } from '../../components/DateField';
import { Modal } from '../../components/Modal';
import { useAction } from '../../hooks/useAction';
import { useAsyncData } from '../../hooks/useAsyncData';
import type { Booking } from '../../types/models';
import { todayIso } from '../../utils/dates';

interface EditBookingDialogProps {
  booking: Booking;
  onClose: () => void;
  onSaved: () => void;
}

interface DeskOption {
  id: string;
  label: string;
}

export function EditBookingDialog({ booking, onClose, onSaved }: EditBookingDialogProps) {
  const [date, setDate] = useState(booking.date);
  const [deskId, setDeskId] = useState(booking.deskId);
  const availableDesks = useAsyncData(() => desksApi.getAvailable(date), [date]);
  const save = useAction();

  // The employee's own desk is not "available" on the original date, but keeping it must stay possible.
  const deskOptions: DeskOption[] = [
    ...(date === booking.date ? [{ id: booking.deskId, label: `${booking.deskCode} · Floor ${booking.deskFloor} (current)` }] : []),
    ...(availableDesks.data ?? []).map((desk) => ({ id: desk.id, label: `${desk.code} · Floor ${desk.floor}` })),
  ];
  const selectedDeskIsOffered = deskOptions.some((option) => option.id === deskId);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const succeeded = await save.run(async () => {
      await bookingsApi.update(booking.id, { deskId, date });
    });
    if (succeeded) onSaved();
  }

  return (
    <Modal title="Change booking" onClose={onClose}>
      <form onSubmit={submit}>
        <DateField label="Date" value={date} min={todayIso()} onChange={setDate} />

        <label className="field">
          <span className="field__label">Desk</span>
          <select
            className="field__input"
            value={selectedDeskIsOffered ? deskId : ''}
            required
            disabled={availableDesks.isLoading}
            onChange={(event) => setDeskId(event.target.value)}
          >
            <option value="" disabled>
              {deskOptions.length === 0 ? 'No free desks on this day' : 'Choose a desk'}
            </option>
            {deskOptions.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
        </label>

        <Alert kind="error" message={save.error ?? availableDesks.error} />

        <div className="form-actions">
          <button type="button" className="button" onClick={onClose}>
            Close
          </button>
          <button type="submit" className="button button--primary" disabled={save.isPending || !selectedDeskIsOffered}>
            Save changes
          </button>
        </div>
      </form>
    </Modal>
  );
}
