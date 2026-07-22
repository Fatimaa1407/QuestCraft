import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { Gift, Sparkles } from 'lucide-react';
import { Z_INDEX } from '../../styles/zIndex';
import { Confetti } from './Confetti';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';

interface DailyRewardModalProps {
  isOpen: boolean;
  coinsAwarded: number;
  xpAwarded: number;
  isMysteryBonus: boolean;
  onClose: () => void;
}

// Fires at most once per day (gated by the backend's LastLoginRewardClaimedAt check) — a small,
// self-contained celebratory moment, deliberately lighter-weight than LevelUpModal since this
// happens far more often.
export function DailyRewardModal({ isOpen, coinsAwarded, xpAwarded, isMysteryBonus, onClose }: DailyRewardModalProps) {
  const { t } = useTranslation();
  const animatedCoins = useAnimatedNumber(isOpen ? coinsAwarded : 0);
  const animatedXp = useAnimatedNumber(isOpen ? xpAwarded : 0);

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0" style={{ zIndex: Z_INDEX.modal }}>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.25 }}
            className="absolute inset-0 bg-slate-950/70 backdrop-blur-md"
            onClick={onClose}
          />

          <div className="pointer-events-none absolute inset-0 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, y: 24, scale: 0.9 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 16, scale: 0.92 }}
              transition={{ type: 'spring', stiffness: 280, damping: 22 }}
              className="pointer-events-auto relative w-full max-w-sm overflow-hidden rounded-[28px] border border-white/20 bg-white/85 p-8 text-center shadow-[0_40px_80px_-24px_rgba(0,0,0,0.5)] backdrop-blur-2xl dark:border-white/10 dark:bg-slate-900/85"
            >
              <motion.span
                initial={{ scale: 0, rotate: -15 }}
                animate={{ scale: 1, rotate: 0 }}
                transition={{ delay: 0.1, type: 'spring', stiffness: 300, damping: 16 }}
                className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-app-accent to-app-accent-2 text-white shadow-lg"
              >
                {isMysteryBonus ? <Sparkles size={28} /> : <Gift size={28} />}
              </motion.span>

              <motion.h2
                initial={{ opacity: 0, y: 6 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2, duration: 0.35 }}
                className="mt-4 text-xl font-bold text-slate-900 dark:text-white"
              >
                {isMysteryBonus ? t('dailyReward.mysteryTitle') : t('dailyReward.title')}
              </motion.h2>
              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.3, duration: 0.35 }}
                className="mt-1 text-sm text-slate-500 dark:text-slate-400"
              >
                {t('dailyReward.subtitle')}
              </motion.p>

              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.4, type: 'spring', stiffness: 260, damping: 20 }}
                className="mt-6 flex items-center justify-center gap-6"
              >
                <div>
                  <p className="text-3xl font-extrabold text-amber-500">🪙 {animatedCoins}</p>
                  <p className="mt-1 text-xs font-medium text-slate-500 dark:text-slate-400">{t('dailyReward.coins')}</p>
                </div>
                <div className="h-10 w-px bg-slate-200 dark:bg-white/10" />
                <div>
                  <p className="text-3xl font-extrabold text-app-accent dark:text-app-accent-2">+{animatedXp}</p>
                  <p className="mt-1 text-xs font-medium text-slate-500 dark:text-slate-400">XP</p>
                </div>
              </motion.div>

              <motion.button
                type="button"
                onClick={onClose}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.6, duration: 0.4 }}
                whileHover={{ scale: 1.03 }}
                whileTap={{ scale: 0.97 }}
                className="mt-7 w-full rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-3 text-sm font-semibold text-white shadow-lg transition-shadow hover:shadow-xl"
              >
                {t('dailyReward.continue')}
              </motion.button>
            </motion.div>
          </div>

          {isMysteryBonus && <Confetti count={50} style={{ zIndex: Z_INDEX.modal + 1 }} />}
        </div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
