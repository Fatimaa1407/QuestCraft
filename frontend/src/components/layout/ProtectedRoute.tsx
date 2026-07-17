import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../app/authStore';
import type { UserRole } from '../../types/auth';

export function ProtectedRoute({ allowedRoles }: { allowedRoles?: UserRole[] }) {
  const user = useAuthStore((s) => s.user);
  const location = useLocation();

  if (!user) {
    // The root URL is the public landing page's job when signed out — deep links to
    // a specific feature still funnel through /login instead.
    return <Navigate to={location.pathname === '/' ? '/welcome' : '/login'} replace />;
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
