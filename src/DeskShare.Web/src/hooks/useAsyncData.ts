import { useCallback, useEffect, useState, type DependencyList } from 'react';
import { toErrorMessage } from '../api/httpClient';

interface AsyncDataState<T> {
  data: T | undefined;
  error: string | undefined;
  isLoading: boolean;
}

export function useAsyncData<T>(load: () => Promise<T>, dependencies: DependencyList) {
  const [state, setState] = useState<AsyncDataState<T>>({ data: undefined, error: undefined, isLoading: true });
  const [reloadCount, setReloadCount] = useState(0);

  useEffect(() => {
    let isCurrent = true;
    setState((previous) => ({ ...previous, error: undefined, isLoading: true }));

    load().then(
      (data) => isCurrent && setState({ data, error: undefined, isLoading: false }),
      (error: unknown) => isCurrent && setState({ data: undefined, error: toErrorMessage(error), isLoading: false }),
    );

    return () => {
      isCurrent = false;
    };
    // `load` is recreated on every render, so callers list what it really depends on.
  }, [...dependencies, reloadCount]);

  const reload = useCallback(() => setReloadCount((count) => count + 1), []);

  return { ...state, reload };
}
