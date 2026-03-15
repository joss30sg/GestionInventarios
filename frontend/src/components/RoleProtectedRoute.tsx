import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { UserRole } from '../types';

interface RoleProtectedRouteProps {
  children: ReactNode;
  requiredRole?: UserRole | UserRole[];
  fallback?: ReactNode;
}

export function RoleProtectedRoute({
  children,
  requiredRole,
  fallback,
}: RoleProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!requiredRole) {
    return <>{children}</>;
  }

  const roles = Array.isArray(requiredRole) ? requiredRole : [requiredRole];
  const hasRequiredRole = user?.role && roles.includes(user.role);

  if (!hasRequiredRole) {
    return fallback ?? <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
}
