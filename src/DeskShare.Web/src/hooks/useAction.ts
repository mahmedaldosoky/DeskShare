import { useState } from 'react';
import { toErrorMessage } from '../api/httpClient';

/** Runs a user-triggered request (save, delete, ...) and tracks its pending and error state. */
export function useAction() {
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<string>();

  async function run(action: () => Promise<void>): Promise<boolean> {
    setIsPending(true);
    setError(undefined);
    try {
      await action();
      return true;
    } catch (caught) {
      setError(toErrorMessage(caught));
      return false;
    } finally {
      setIsPending(false);
    }
  }

  return { run, isPending, error, clearError: () => setError(undefined) };
}
