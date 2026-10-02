import { createContext, useContext } from 'react';
import type { SessionUser, SignInMode } from '../types/models';

export interface AuthContextValue {
  user: SessionUser | null;
  signInMode: SignInMode;
  isOfficeManager: boolean;
  refreshSession: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used inside <AuthProvider>.');
  return context;
}
