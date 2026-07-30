import { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { PartyPopper, Crown, Coins, Award, LayoutDashboard, UserRound } from 'lucide-react';
import { Z_INDEX } from '../../styles/zIndex';
import { Confetti } from './Confetti';
import { playLevelUpSound } from '../../utils/sounds';

interface GameCompleteModalProps {
  isOpen: boolean;
  maxLevel: number;
  bonusCoins: number;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
  onGoDashboard: () => void;
  onViewProfile: () => void;
}

// The one-time "finished every level" celebration — deliberately bigger and more permanent-feeling
// than LevelUpModal (no auto-dismiss, no level-flip animation): this only ever fires once per
// account, so it gets its own full-screen moment instead of reusing the per-level modal.
export function GameCompleteModal({
  isOpen,
  maxLevel,
  bonusCoins,
  titleText,
  badgeImageUrl,
  badgeName,
  onGoDashboard,
  onViewProfile,
}: GameCompleteModalProps) {
  const { t } = useTranslation();

  useEffect(() => {
    if (!isOpen) return;
    playLevelUpSound();
  }, [isOpen]);

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0" style={{ zIndex: Z_INDEX.modal }}>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="absolute inset-0 bg-slate-950/75 backdrop-blur-md"
          />

          <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 0.6, scale: [1, 1.2, 1] }}
              transition={{ opacity: { duration: 0.6 }, scale: { duration: 3.5, repeat: Infinity, ease: 'easeInOut' } }}
              className="h-[30rem] w-[30rem] rounded-full bg-gradient-to-br from-amber-400/40 via-app-accent/30 to-app-accent-2/20 blur-[90px]"
            />
          </div>

          <div className="absolute inset-0 flex items-center justify-center overflow-y-auto p-4">
            <motion.div
              initial={{ opacity: 0, y: 24, scale: 0.92 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 16, scale: 0.95 }}
              transition={{ type: 'spring', stiffness: 260, damping: 22 }}
              className="relative my-8 w-full max-w-md overflow-hidden rounded-[28px] border border-white/20 bg-white/85 p-8 text-center shadow-[0_40px_80px_-24px_rgba(0,0,0,0.5)] backdrop-blur-2xl dark:border-white/10 dark:bg-slate-900/85"
            >
              <motion.span
                initial={{ scale: 0, rotate: -20 }}
                animate={{ scale: 1, rotate: 0 }}
                transition={{ delay: 0.15, type: 'spring', stiffness: 300, damping: 18 }}
                className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-amber-400 to-amber-600 text-white shadow-lg shadow-amber-500/30"
              >
                <PartyPopper size={30} />
              </motion.span>

              <motion.h2
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3, duration: 0.4 }}
                className="mt-4 text-2xl font-bold text-slate-900 dark:text-white"
              >
                {t('gameComplete.title')}
              </motion.h2>

              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.4, duration: 0.4 }}
                className="mt-2 text-sm font-medium text-slate-500 dark:text-slate-400"
              >
                {t('gameComplete.subtitle')}
              </motion.p>

              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.5, type: 'spring', stiffness: 260, damping: 20 }}
                className="mt-5"
              >
                <div className="h-2.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/10">
                  <motion.div
                    initial={{ width: 0 }}
                    animate={{ width: '100%' }}
                    transition={{ delay: 0.6, duration: 0.8, ease: 'easeOut' }}
                    className="h-full rounded-full bg-gradient-to-r from-amber-400 to-amber-600"
                  />
                </div>
                <div className="mt-2 flex items-center justify-between text-xs font-medium text-slate-500 dark:text-slate-400">
                  <span>{t('gameComplete.progressLabel')}</span>
                  <span className="font-mono font-bold text-amber-600 dark:text-amber-400">100%</span>
                </div>
              </motion.div>

              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.65, duration: 0.4 }}
                className="mt-4 text-xs font-semibold uppercase tracking-wide text-slate-400 dark:text-slate-500"
              >
                {t('gameComplete.maxLevelReached', { level: maxLevel })}
              </motion.p>

              <motion.div
                initial="hidden"
                animate="show"
                transition={{ staggerChildren: 0.1, delayChildren: 0.8 }}
                className="mt-4 flex flex-col gap-2 border-t border-slate-200/70 pt-5 dark:border-white/10"
              >
                {titleText && (
                  <motion.div
                    variants={{ hidden: { opacity: 0, y: 10 }, show: { opacity: 1, y: 0 } }}
                    transition={{ duration: 0.35, ease: 'easeOut' }}
                    className="flex items-center gap-3 rounded-xl bg-slate-50/70 px-3.5 py-2.5 text-left dark:bg-white/5"
                  >
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-violet-500/10 text-violet-500 dark:text-violet-400">
                      <Crown size={16} />
                    </span>
                    <span className="text-sm font-medium text-slate-700 dark:text-slate-200">
                      {t('gameComplete.titleUnlocked', { title: titleText })}
                    </span>
                  </motion.div>
                )}

                {bonusCoins > 0 && (
                  <motion.div
                    variants={{ hidden: { opacity: 0, y: 10 }, show: { opacity: 1, y: 0 } }}
                    transition={{ duration: 0.35, ease: 'easeOut' }}
                    className="flex items-center gap-3 rounded-xl bg-slate-50/70 px-3.5 py-2.5 text-left dark:bg-white/5"
                  >
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-amber-500/10 text-amber-500 dark:text-amber-400">
                      <Coins size={16} />
                    </span>
                    <span className="text-sm font-medium text-slate-700 dark:text-slate-200">
                      {t('gameComplete.bonusCoins', { coins: bonusCoins })}
                    </span>
                  </motion.div>
                )}

                {badgeName && (
                  <motion.div
                    variants={{ hidden: { opacity: 0, y: 10 }, show: { opacity: 1, y: 0 } }}
                    transition={{ duration: 0.35, ease: 'easeOut' }}
                    className="flex items-center gap-3 rounded-xl bg-slate-50/70 px-3.5 py-2.5 text-left dark:bg-white/5"
                  >
                    <span className="flex h-8 w-8 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
                      {badgeImageUrl ? <img src={badgeImageUrl} alt="" className="h-full w-full object-cover" /> : <Award size={16} />}
                    </span>
                    <span className="text-sm font-medium text-slate-700 dark:text-slate-200">
                      {t('gameComplete.badgeUnlocked', { badge: badgeName })}
                    </span>
                  </motion.div>
                )}
              </motion.div>

              <div className="mt-7 flex flex-col gap-2.5 sm:flex-row">
                <motion.button
                  type="button"
                  onClick={onViewProfile}
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 1.1, duration: 0.4 }}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  className="flex flex-1 items-center justify-center gap-1.5 rounded-full border border-slate-200/70 px-5 py-3 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 dark:border-white/10 dark:text-slate-200 dark:hover:bg-white/5"
                >
                  <UserRound size={15} />
                  {t('gameComplete.viewProfile')}
                </motion.button>
                <motion.button
                  type="button"
                  onClick={onGoDashboard}
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 1.2, duration: 0.4 }}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  className="flex flex-1 items-center justify-center gap-1.5 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-app-accent/25 transition-shadow hover:shadow-xl hover:shadow-app-accent-2/30"
                >
                  <LayoutDashboard size={15} />
                  {t('gameComplete.goDashboard')}
                </motion.button>
              </div>
            </motion.div>
          </div>

          <Confetti count={80} style={{ zIndex: Z_INDEX.modal + 1 }} />
        </div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
