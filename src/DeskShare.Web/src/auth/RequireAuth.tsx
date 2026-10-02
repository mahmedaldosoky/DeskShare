import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router';
import type { Role } from '../types/models';
import { useAuth } from './AuthContext';

interface RequireAuthProps {
  role?: Role;
  children: ReactNode;
}

export function RequireAuth({ role, children }: RequireAuthProps) {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (role && !user.roles.includes(role)) return <Navigate to="/" replace />;

  return children;
}
