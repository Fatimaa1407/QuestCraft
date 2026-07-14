import { Outlet, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { useAuthStore } from '../../app/authStore';
import { ThemeSwitcher } from '../ui/ThemeSwitcher';
import { LanguageSwitcher } from '../ui/LanguageSwitcher';
import { AmbientGlow } from '../ui/AmbientGlow';
import { Sidebar } from './Sidebar';
import { pageTransition } from '../../utils/motion';

export function AppLayout() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const location = useLocation();

  return (
    <div className="relative flex min-h-svh bg-app-bg text-slate-900 dark:bg-gradient-to-br dark:from-[#0b1220] dark:via-[#0d1526] dark:to-[#0a0f1c] dark:text-slate-50">
      <AmbientGlow />
      <Sidebar />

      <div className="relative z-10 flex min-w-0 flex-1 flex-col">
        <header className="border-b border-slate-200/70 bg-white/60 backdrop-blur-xl dark:border-white/[0.06] dark:bg-app-bg-dark/60">
          <div className="flex flex-wrap items-center justify-end gap-3 px-5 py-4 sm:px-8">
            <div className="mr-auto flex items-center gap-2 text-sm font-medium text-slate-600 sm:hidden dark:text-slate-300">
              {t('app.name')}
            </div>

            <div className="flex items-center gap-3 rounded-full border border-slate-200/70 bg-white/80 px-3.5 py-1.5 text-sm dark:border-white/[0.08] dark:bg-white/5">
              {user?.avatarUrl ? (
                <img src={user.avatarUrl} alt="" className="h-6 w-6 rounded-full" />
              ) : (
                <span className="flex h-6 w-6 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-xs font-semibold text-white">
                  {(user?.firstName ?? user?.username ?? '?').charAt(0).toUpperCase()}
                </span>
              )}
              <span className="text-slate-700 dark:text-slate-100">{user?.username}</span>
              <span className="hidden text-slate-400 sm:inline dark:text-slate-600">·</span>
              <span className="hidden text-slate-500 sm:inline dark:text-slate-400">Lvl {user?.level}</span>
              <span className="hidden text-blue-600 sm:inline dark:text-cyan-400">{user?.xp} XP</span>
              <span className="hidden text-amber-500 sm:inline">{user?.coins} 🪙</span>
            </div>

            <LanguageSwitcher />
            <ThemeSwitcher />
          </div>
        </header>
        <main className="relative flex-1 px-5 py-8 sm:px-8">
          <div className="mx-auto max-w-6xl">
            <AnimatePresence mode="wait">
              <motion.div key={location.pathname} variants={pageTransition} initial="hidden" animate="show" exit="exit">
                <Outlet />
              </motion.div>
            </AnimatePresence>
          </div>
        </main>
      </div>
    </div>
  );
}
