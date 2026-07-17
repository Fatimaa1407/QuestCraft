import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { Award, ListChecks, Sparkles, Swords, Zap } from 'lucide-react';
import { Z_INDEX } from '../../styles/zIndex';
import { Confetti } from './Confetti';
import { playLevelUpSound } from '../../utils/sounds';

interface LevelUpModalProps {
  isOpen: boolean;
  previousLevel: number;
  newLevel: number;
  xpEarned: number;
  coinsEarned: number;
  newChallengesUnlocked: number;
  newQuizzesUnlocked: number;
  onContinue: () => void;
}

const rowVariants = {
  hidden: { opacity: 0, y: 10 },
  show: { opacity: 1, y: 0 },
};

export function LevelUpModal({
  isOpen,
  previousLevel,
  newLevel,
  xpEarned,
  coinsEarned,
  newChallengesUnlocked,
  newQuizzesUnlocked,
  onContinue,
}: LevelUpModalProps) {
  const { t } = useTranslation();
  const [displayLevel, setDisplayLevel] = useState(previousLevel);
  const [showConfetti, setShowConfetti] = useState(false);

  useEffect(() => {
    if (!isOpen) return;
    setDisplayLevel(previousLevel);
    setShowConfetti(false);

    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const flipDelay = reduced ? 0 : 700;

    const flipTimer = window.setTimeout(() => {
      setDisplayLevel(newLevel);
      setShowConfetti(true);
      playLevelUpSound();
    }, flipDelay);

    const confettiOffTimer = window.setTimeout(() => setShowConfetti(false), flipDelay + 2200);

    return () => {
      window.clearTimeout(flipTimer);
      window.clearTimeout(confettiOffTimer);
    };
  }, [isOpen, previousLevel, newLevel]);

  const rewardRows = [
    xpEarned > 0 && { icon: Zap, tint: 'text-blue-500 dark:text-cyan-400', bg: 'bg-blue-500/10', label: t('levelUp.xpEarned', { xp: xpEarned }) },
    coinsEarned > 0 && { icon: Award, tint: 'text-amber-500 dark:text-amber-400', bg: 'bg-amber-500/10', label: t('levelUp.coinsEarned', { coins: coinsEarned }) },
    newChallengesUnlocked > 0 && {
      icon: Swords,
      tint: 'text-blue-500 dark:text-cyan-400',
      bg: 'bg-blue-500/10',
      label: t('levelUp.challengesUnlocked', { count: newChallengesUnlocked }),
    },
    newQuizzesUnlocked > 0 && {
      icon: ListChecks,
      tint: 'text-emerald-500 dark:text-emerald-400',
      bg: 'bg-emerald-500/10',
      label: t('levelUp.quizzesUnlocked', { count: newQuizzesUnlocked }),
    },
  ].filter(Boolean) as { icon: typeof Zap; tint: string; bg: string; label: string }[];

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0" style={{ zIndex: Z_INDEX.modal }}>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="absolute inset-0 bg-slate-950/70 backdrop-blur-md"
          />

          <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
            <motion.div
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 0.55, scale: [1, 1.18, 1] }}
              transition={{ opacity: { duration: 0.6 }, scale: { duration: 3.5, repeat: Infinity, ease: 'easeInOut' } }}
              className="h-[26rem] w-[26rem] rounded-full bg-gradient-to-br from-blue-500/40 via-cyan-400/30 to-amber-400/20 blur-[80px]"
            />
          </div>

          <div className="absolute inset-0 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, y: 24, scale: 0.92 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 16, scale: 0.95 }}
              transition={{ type: 'spring', stiffness: 260, damping: 22 }}
              className="relative w-full max-w-sm overflow-hidden rounded-[28px] border border-white/20 bg-white/80 p-8 text-center shadow-[0_40px_80px_-24px_rgba(0,0,0,0.5)] backdrop-blur-2xl dark:border-white/10 dark:bg-slate-900/80"
            >
              <motion.span
                initial={{ scale: 0, rotate: -20 }}
                animate={{ scale: 1, rotate: 0 }}
                transition={{ delay: 0.15, type: 'spring', stiffness: 300, damping: 18 }}
                className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-400 text-white shadow-lg shadow-blue-500/30"
              >
                <Sparkles size={30} />
              </motion.span>

              <motion.h2
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3, duration: 0.4 }}
                className="mt-4 text-2xl font-bold text-slate-900 dark:text-white"
              >
                {t('levelUp.congratulations')}
              </motion.h2>

              <div className="relative mt-3 flex h-16 items-center justify-center">
                <AnimatePresence mode="wait">
                  <motion.span
                    key={displayLevel}
                    initial={{ opacity: 0, scale: 0.6, y: 12 }}
                    animate={{ opacity: 1, scale: 1, y: 0 }}
                    exit={{ opacity: 0, scale: 1.2 }}
                    transition={{ type: 'spring', stiffness: 320, damping: 20 }}
                    className="bg-gradient-to-r from-blue-600 to-cyan-500 bg-clip-text font-mono text-5xl font-extrabold text-transparent"
                  >
                    {displayLevel}
                  </motion.span>
                </AnimatePresence>
              </div>

              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.5, duration: 0.4 }}
                className="text-sm font-medium text-slate-500 dark:text-slate-400"
              >
                {t('levelUp.unlocked', { level: newLevel })}
              </motion.p>

              {rewardRows.length > 0 && (
                <motion.div
                  initial="hidden"
                  animate="show"
                  transition={{ staggerChildren: 0.1, delayChildren: 0.9 }}
                  className="mt-6 flex flex-col gap-2 border-t border-slate-200/70 pt-5 dark:border-white/10"
                >
                  {rewardRows.map((row) => (
                    <motion.div
                      key={row.label}
                      variants={rowVariants}
                      transition={{ duration: 0.35, ease: 'easeOut' }}
                      className="flex items-center gap-3 rounded-xl bg-slate-50/70 px-3.5 py-2.5 text-left dark:bg-white/5"
                    >
                      <span className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ${row.bg} ${row.tint}`}>
                        <row.icon size={16} />
                      </span>
                      <span className="text-sm font-medium text-slate-700 dark:text-slate-200">{row.label}</span>
                    </motion.div>
                  ))}
                </motion.div>
              )}

              <motion.button
                type="button"
                onClick={onContinue}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.9 + rewardRows.length * 0.1 + 0.2, duration: 0.4 }}
                whileHover={{ scale: 1.03 }}
                whileTap={{ scale: 0.97 }}
                className="mt-7 w-full rounded-full bg-gradient-to-r from-blue-600 to-cyan-500 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-600/25 transition-shadow hover:shadow-xl hover:shadow-cyan-500/30"
              >
                {t('levelUp.continue')}
              </motion.button>
            </motion.div>
          </div>

          {showConfetti && <Confetti count={46} style={{ zIndex: Z_INDEX.modal + 1 }} />}
        </div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
