import { useState } from 'react';
import { desksApi } from '../../api/desksApi';
import { Alert } from '../../components/Alert';
import { FeatureTags } from '../../components/FeatureTags';
import { PageHeader } from '../../components/PageHeader';
import { useAction } from '../../hooks/useAction';
import { useAsyncData } from '../../hooks/useAsyncData';
import type { Desk } from '../../types/models';
import { DeskFormDialog } from './DeskFormDialog';

type EditorState = { mode: 'closed' } | { mode: 'create' } | { mode: 'edit'; desk: Desk };

export function ManageDesksPage() {
  const desks = useAsyncData(() => desksApi.getAll(), []);
  const [editor, setEditor] = useState<EditorState>({ mode: 'closed' });
  const deletion = useAction();

  async function remove(desk: Desk) {
    if (!window.confirm(`Delete desk ${desk.code}?`)) return;

    const succeeded = await deletion.run(() => desksApi.remove(desk.id));
    if (succeeded) desks.reload();
  }

  function onDeskSaved() {
    setEditor({ mode: 'closed' });
    desks.reload();
  }

  return (
    <>
      <PageHeader
        title="Manage desks"
        description="Add, edit and remove the desks employees can book."
        actions={
          <button type="button" className="button button--primary" onClick={() => setEditor({ mode: 'create' })}>
            + Add desk
          </button>
        }
      />
      <Alert kind="error" message={deletion.error ?? desks.error} />

      {desks.data?.length === 0 && <p className="empty-state">No desks yet. Add the first one.</p>}

      {!!desks.data?.length && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Floor</th>
              <th>Features</th>
              <th className="data-table__actions">Actions</th>
            </tr>
          </thead>
          <tbody>
            {desks.data.map((desk) => (
              <tr key={desk.id}>
                <td>
                  <strong>{desk.code}</strong>
                </td>
                <td>{desk.floor}</td>
                <td>
                  <FeatureTags features={desk.features} />
                </td>
                <td className="data-table__actions">
                  <button type="button" className="button button--small button--link" onClick={() => setEditor({ mode: 'edit', desk })}>
                    Edit
                  </button>
                  <button
                    type="button"
                    className="button button--small button--danger"
                    disabled={deletion.isPending}
                    onClick={() => remove(desk)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {editor.mode !== 'closed' && (
        <DeskFormDialog
          desk={editor.mode === 'edit' ? editor.desk : undefined}
          onClose={() => setEditor({ mode: 'closed' })}
          onSaved={onDeskSaved}
        />
      )}
    </>
  );
}
