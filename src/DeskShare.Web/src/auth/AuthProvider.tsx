import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { authApi } from '../api/authApi';
import { setUnauthorizedHandler, toErrorMessage } from '../api/httpClient';
import type { Session } from '../types/models';
import { AuthContext, type AuthContextValue } from './AuthContext';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session>();
  const [loadError, setLoadError] = useState<string>();

  const refreshSession = useCallback(async () => {
    try {
      setSession(await authApi.getSession());
      setLoadError(undefined);
    } catch (error) {
      setLoadError(toErrorMessage(error));
    }
  }, []);

  useEffect(() => {
    void refreshSession();
    setUnauthorizedHandler(() => setSession((current) => current && { ...current, user: null }));
  }, [refreshSession]);

  const value = useMemo<AuthContextValue | null>(
    () =>
      session
        ? {
            user: session.user,
            signInMode: session.signInMode,
            isOfficeManager: session.user?.roles.includes('OfficeManager') ?? false,
            refreshSession,
          }
        : null,
    [session, refreshSession],
  );

  if (loadError) return <p className="page-message page-message--error">Could not reach DeskShare: {loadError}</p>;
  if (!value) return <p className="page-message">Loading…</p>;

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
