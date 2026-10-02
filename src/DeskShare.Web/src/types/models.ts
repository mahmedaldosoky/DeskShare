export const DESK_FEATURES = ['Monitor', 'Standing', 'Window'] as const;
export type DeskFeature = (typeof DESK_FEATURES)[number];

export interface Desk {
  id: string;
  code: string;
  floor: number;
  features: DeskFeature[];
}

export interface SaveDeskRequest {
  code: string;
  floor: number;
  features: DeskFeature[];
}

export interface Booking {
  id: string;
  date: string;
  deskId: string;
  deskCode: string;
  deskFloor: number;
  employeeName: string;
}

export interface SaveBookingRequest {
  deskId: string;
  date: string;
}

export type Role = 'Employee' | 'OfficeManager';
export type SignInMode = 'Development' | 'Oidc';

export interface SessionUser {
  displayName: string;
  email: string;
  roles: Role[];
}

export interface Session {
  signInMode: SignInMode;
  user: SessionUser | null;
}

export interface DevelopmentSignInRequest {
  displayName: string;
  email: string;
  isOfficeManager: boolean;
}
