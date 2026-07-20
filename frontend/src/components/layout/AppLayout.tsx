import { useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { AnimatePresence, motion } from 'framer-motion';
import { useAuthStore } from '../../app/authStore';
import { ThemeSwitcher } from '../ui/ThemeSwitcher';
import { LanguageSwitcher } from '../ui/LanguageSwitcher';
import { NotificationBell } from '../ui/NotificationBell';
import { AmbientGlow } from '../ui/AmbientGlow';
import { SeasonalEventBanner } from '../ui/SeasonalEventBanner';
import { FramedAvatar } from '../ui/FramedAvatar';
import { Sidebar } from './Sidebar';
import { MobileNav } from './MobileNav';
import { pageTransition } from '../../utils/motion';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { useNotificationsHub } from '../../utils/useNotificationsHub';
import { getMyEquippedCosmetics } from '../../api/marketplace';
import { applyAccentPalette } from '../../utils/themePalettes';

export function AppLayout() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const location = useLocation();
  useNotificationsHub();
  const animatedLevel = useAnimatedNumber(user?.level ?? 0);
  const animatedXp = useAnimatedNumber(user?.xp ?? 0);
  const animatedCoins = useAnimatedNumber(user?.coins ?? 0);

  const { data: equipped } = useQuery({
    queryKey: ['profile', 'equipped'],
    queryFn: getMyEquippedCosmetics,
    enabled: !!user,
  });

  // Re-applies the equipped Theme's accent colors whenever it changes (including on
  // equip/unequip elsewhere, since that invalidates this same query key) — no page reload needed.
  useEffect(() => {
    applyAccentPalette(equipped?.themeName ?? null);
  }, [equipped?.themeName]);

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
              <FramedAvatar
                username={user?.firstName ?? user?.username ?? '?'}
                avatarUrl={equipped?.avatarUrl ?? user?.avatarUrl}
                frameImageUrl={equipped?.frameImageUrl}
                size={24}
              />
              <span className="flex flex-col leading-tight">
                <span className="text-slate-700 dark:text-slate-100">{user?.username}</span>
                {equipped?.titleText && (
                  <span className="text-[10px] font-medium text-blue-600 dark:text-cyan-400">{equipped.titleText}</span>
                )}
              </span>
              <span className="text-slate-400 dark:text-slate-600">·</span>
              <span className="text-slate-500 dark:text-slate-400">Lvl {animatedLevel}</span>
              <span className="hidden text-blue-600 sm:inline dark:text-cyan-400">{animatedXp} XP</span>
              <span className="hidden text-amber-500 sm:inline">{animatedCoins} 🪙</span>
            </div>

            <NotificationBell />
            <LanguageSwitcher />
            <ThemeSwitcher />
          </div>
        </header>
        <SeasonalEventBanner />
        <main className="relative flex-1 px-5 py-8 pb-24 sm:px-8 sm:pb-8">
          <div className="mx-auto max-w-6xl">
            <AnimatePresence mode="wait">
              <motion.div key={location.pathname} variants={pageTransition} initial="hidden" animate="show" exit="exit">
                <Outlet />
              </motion.div>
            </AnimatePresence>
          </div>
        </main>
      </div>

      <MobileNav />
    </div>
  );
}
