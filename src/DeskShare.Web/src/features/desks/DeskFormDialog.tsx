import { useState, type FormEvent } from 'react';
import { desksApi } from '../../api/desksApi';
import { Alert } from '../../components/Alert';
import { Modal } from '../../components/Modal';
import { useAction } from '../../hooks/useAction';
import { DESK_FEATURES, type Desk, type DeskFeature, type SaveDeskRequest } from '../../types/models';

interface DeskFormDialogProps {
  desk?: Desk;
  onClose: () => void;
  onSaved: () => void;
}

const emptyDesk: SaveDeskRequest = { code: '', floor: 1, features: [] };

export function DeskFormDialog({ desk, onClose, onSaved }: DeskFormDialogProps) {
  const [form, setForm] = useState<SaveDeskRequest>(
    desk ? { code: desk.code, floor: desk.floor, features: desk.features } : emptyDesk,
  );
  const save = useAction();
  const isEditing = desk !== undefined;

  function toggleFeature(feature: DeskFeature, isChecked: boolean) {
    const features = isChecked ? [...form.features, feature] : form.features.filter((existing) => existing !== feature);
    setForm({ ...form, features });
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const succeeded = await save.run(async () => {
      if (isEditing) await desksApi.update(desk.id, form);
      else await desksApi.create(form);
    });
    if (succeeded) onSaved();
  }

  return (
    <Modal title={isEditing ? `Edit desk ${desk.code}` : 'Add desk'} onClose={onClose}>
      <form onSubmit={submit}>
        <label className="field">
          <span className="field__label">Code</span>
          <input
            className="field__input"
            value={form.code}
            maxLength={20}
            required
            placeholder="e.g. A-101"
            onChange={(event) => setForm({ ...form, code: event.target.value })}
          />
        </label>

        <label className="field">
          <span className="field__label">Floor</span>
          <input
            className="field__input"
            type="number"
            min={0}
            max={200}
            value={form.floor}
            required
            onChange={(event) => setForm({ ...form, floor: event.target.valueAsNumber })}
          />
        </label>

        <fieldset className="field">
          <legend className="field__label">Features</legend>
          {DESK_FEATURES.map((feature) => (
            <label key={feature} className="checkbox">
              <input
                type="checkbox"
                checked={form.features.includes(feature)}
                onChange={(event) => toggleFeature(feature, event.target.checked)}
              />
              {feature}
            </label>
          ))}
        </fieldset>

        <Alert kind="error" message={save.error} />

        <div className="form-actions">
          <button type="button" className="button" onClick={onClose}>
            Close
          </button>
          <button type="submit" className="button button--primary" disabled={save.isPending}>
            {isEditing ? 'Save changes' : 'Add desk'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
