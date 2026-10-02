import type { DevelopmentSignInRequest, Session } from '../types/models';
import { http } from './httpClient';

export const authApi = {
  getSession: () => http.get<Session>('/api/auth/session'),
  developmentSignIn: (request: DevelopmentSignInRequest) => http.post<void>('/api/auth/dev-login', request),
  singleSignOnUrl: (returnUrl: string) => `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`,
  signOutUrl: '/api/auth/logout',
};
