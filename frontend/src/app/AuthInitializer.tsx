import { useEffect, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Code2 } from 'lucide-react';
import { useAuthStore } from './authStore';
import { refreshAccessToken } from '../api/client';

// First screen every user sees, however briefly — mirrors AuthLayout's flat dark-navy canvas with
// a cyan/blue glow so it doesn't feel like a different, unbranded app flashing before the real UI.
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
      <div className="relative flex min-h-svh items-center justify-center overflow-hidden bg-slate-950">
        <div
          aria-hidden
          className="pointer-events-none absolute left-1/2 top-1/2 h-[28rem] w-[28rem] -translate-x-1/2 -translate-y-1/2 rounded-full bg-gradient-to-br from-blue-500/20 to-cyan-500/10 blur-3xl"
        />

        <motion.div
          initial={{ opacity: 0, scale: 0.92 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.4, ease: 'easeOut' }}
          className="relative flex flex-col items-center gap-4"
        >
          <motion.div
            animate={{ scale: [1, 1.08, 1], opacity: [0.85, 1, 0.85] }}
            transition={{ duration: 1.8, repeat: Infinity, ease: 'easeInOut' }}
            className="flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 shadow-lg shadow-cyan-500/30"
          >
            <Code2 size={30} className="text-white" />
          </motion.div>

          <span className="text-lg font-semibold text-white">{t('app.name')}</span>

          <div role="status" className="flex items-center gap-1.5">
            <span className="sr-only">{t('common.loading')}</span>
            {[0, 1, 2].map((i) => (
              <motion.span
                key={i}
                aria-hidden
                className="h-1.5 w-1.5 rounded-full bg-cyan-400"
                animate={{ opacity: [0.25, 1, 0.25] }}
                transition={{ duration: 1.1, repeat: Infinity, delay: i * 0.15, ease: 'easeInOut' }}
              />
            ))}
          </div>
        </motion.div>
      </div>
    );
  }

  return <>{children}</>;
}
