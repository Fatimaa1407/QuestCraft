import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Swords, ListChecks, Target, Clock, Flame, Trophy, Zap, Coins as CoinsIcon, Crown, Medal } from 'lucide-react';
import { getMyStatistics } from '../../api/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { StatCard } from '../../components/ui/StatCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { QueryErrorState } from '../../components/ui/QueryErrorState';
import { fadeInUp, staggerContainer } from '../../utils/motion';

function formatSolveTime(ms: number | null): string {
  if (ms === null) return '—';
  const totalSeconds = Math.round(ms / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return minutes > 0 ? `${minutes}m ${seconds}s` : `${seconds}s`;
}

export function StatisticsPage() {
  const { t } = useTranslation();
  const statsQuery = useQuery({ queryKey: ['gamification', 'statistics'], queryFn: getMyStatistics });
  const stats = statsQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('statistics.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('statistics.subtitle')}</p>
      </motion.div>

      {statsQuery.isLoading ? (
        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-5">
          {Array.from({ length: 10 }).map((_, i) => (
            <GlassCard key={i} className="p-6">
              <Skeleton className="mb-4 h-11 w-11 rounded-xl" />
              <Skeleton className="h-3.5 w-16" />
              <Skeleton className="mt-2 h-7 w-20" />
            </GlassCard>
          ))}
        </div>
      ) : statsQuery.isError ? (
        <QueryErrorState onRetry={() => statsQuery.refetch()} />
      ) : stats ? (
        <motion.div variants={staggerContainer} className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-5">
          <StatCard icon={Swords} label={t('statistics.challengesSolved')} value={stats.challengesSolved} tint="blue" />
          <StatCard icon={ListChecks} label={t('statistics.quizzesCompleted')} value={stats.quizzesCompleted} tint="cyan" />
          <StatCard icon={Target} label={t('statistics.successRate')} value={`${stats.successRatePercent}%`} tint="emerald" />
          <StatCard icon={Clock} label={t('statistics.avgSolveTime')} value={formatSolveTime(stats.averageSolveTimeMs)} tint="violet" />
          <StatCard icon={Flame} label={t('statistics.currentStreak')} value={stats.currentStreak} tint="amber" />
          <StatCard icon={Medal} label={t('statistics.longestStreak')} value={stats.longestStreak} tint="amber" />
          <StatCard icon={Zap} label={t('statistics.totalXp')} value={stats.totalXp} tint="blue" />
          <StatCard icon={CoinsIcon} label={t('statistics.coinsEarned')} value={stats.totalCoinsEarned} tint="amber" />
          <StatCard icon={Crown} label={t('statistics.battlesWon')} value={stats.battlesWon} tint="violet" />
          <StatCard icon={Trophy} label={t('statistics.rank')} value={`#${stats.rank} / ${stats.totalUsers}`} tint="cyan" />
        </motion.div>
      ) : null}
    </motion.div>
  );
}
