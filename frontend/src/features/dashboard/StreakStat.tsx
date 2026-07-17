import { Flame } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { GlassCard } from '../../components/ui/GlassCard';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';

export function StreakStat({ currentStreak, longestStreak }: { currentStreak: number; longestStreak: number }) {
  const { t } = useTranslation();
  const animatedCurrent = useAnimatedNumber(currentStreak);

  return (
    <GlassCard className="p-6">
      <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-xl bg-amber-500/10 text-amber-600 shadow-lg shadow-amber-500/20 dark:text-amber-400">
        <Flame size={20} />
      </div>
      <p className="text-sm text-slate-500 dark:text-slate-400">{t('dashboard.streak')}</p>
      <p className="mt-1 text-3xl font-bold tracking-tight text-slate-900 dark:text-white">
        {t('dashboard.streakDays', { count: animatedCurrent })}
      </p>
      <p className="mt-1 text-xs text-slate-400 dark:text-slate-500">{t('dashboard.longestStreak', { count: longestStreak })}</p>
    </GlassCard>
  );
}
