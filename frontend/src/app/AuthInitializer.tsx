import { useEffect, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from './authStore';
import { refreshAccessToken } from '../api/client';

export function AuthInitializer({ children }: { children: ReactNode }) {
  const { t } = useTranslation();
  const isInitializing = useAuthStore((s) => s.isInitializing);
  const setInitializing = useAuthStore((s) => s.setInitializing);

  // Runs once on mount to silently restore a session from the refresh-token cookie.
  useEffect(() => {
    refreshAccessToken().finally(() => setInitializing(false));
  }, [setInitializing]);

  if (isInitializing) {
    return (
      <div className="flex min-h-svh items-center justify-center bg-slate-50 text-slate-500 dark:bg-slate-950 dark:text-slate-300">
        {t('common.loading')}
      </div>
    );
  }

  return <>{children}</>;
}
