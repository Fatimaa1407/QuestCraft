import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { GlassCard } from './GlassCard';
import { Z_INDEX } from '../../styles/zIndex';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';

export function ChallengeCompleteModal({
  xp,
  coins,
  passedTestCases,
  totalTestCases,
  onContinue,
}: {
  xp: number;
  coins: number;
  passedTestCases: number;
  totalTestCases: number;
  onContinue: () => void;
}) {
  const { t } = useTranslation();
  const ratio = totalTestCases > 0 ? passedTestCases / totalTestCases : 0;
  const stars = ratio === 1 ? 3 : ratio >= 0.66 ? 2 : ratio > 0 ? 1 : 0;
  const isPerfect = stars === 3;

  // useAnimatedNumber snaps to its initial target with no animation on mount — so the count-up
  // starts at 0 here and is bumped to the real value one tick later, giving the hook a genuine
  // delta to animate across.
  const [xpTarget, setXpTarget] = useState(0);
  const [coinsTarget, setCoinsTarget] = useState(0);
  useEffect(() => {
    setXpTarget(xp);
    setCoinsTarget(coins);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  const animatedXp = useAnimatedNumber(xpTarget, 900);
  const animatedCoins = useAnimatedNumber(coinsTarget, 900);

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-slate-950/50 backdrop-blur-sm" style={{ zIndex: Z_INDEX.modal }}>
      <GlassCard className="animate-modal-in mx-4 w-full max-w-sm p-8 text-center">
        <p className="text-4xl">🎉</p>
        <h2 className="mt-2 text-xl font-bold text-slate-900 dark:text-white">{t('challenges.completed')}</h2>

        <div className="mt-4 flex items-center justify-center gap-4">
          <p className="flex items-center gap-1.5 text-2xl font-bold text-blue-600 dark:text-cyan-400">⭐ +{animatedXp} XP</p>
          {coins > 0 && (
            <p className="flex items-center gap-1.5 text-2xl font-bold text-amber-500">🪙 +{animatedCoins}</p>
          )}
        </div>

        <p className="mt-4 text-2xl tracking-widest">
          {Array.from({ length: 3 }, (_, i) => (
            <span key={i} className={i < stars ? 'text-amber-400' : 'text-slate-300 dark:text-slate-700'}>
              ★
            </span>
          ))}
        </p>

        {isPerfect && <p className="mt-1 text-sm font-medium text-amber-500">{t('challenges.perfectScore')}</p>}

        <div className="mt-4">
          <div className="h-2 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
            <motion.div
              className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-teal-400"
              initial={{ width: 0 }}
              animate={{ width: `${ratio * 100}%` }}
              transition={{ duration: 0.8, ease: 'easeOut', delay: 0.2 }}
            />
          </div>
          <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">
            {passedTestCases}/{totalTestCases}
          </p>
        </div>

        <button
          onClick={onContinue}
          className="mt-6 w-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-5 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-500/25 transition hover:brightness-110"
        >
          {t('challenges.continue')}
        </button>
      </GlassCard>
    </div>
  );
}
