import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../../app/authStore';
import type { UserRole } from '../../types/auth';

export function ProtectedRoute({ allowedRoles }: { allowedRoles?: UserRole[] }) {
  const user = useAuthStore((s) => s.user);

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
