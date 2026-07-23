import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Users, Percent, Timer } from 'lucide-react';
import { getChallengeStats } from '../../api/challenges';

function formatMinutes(ms: number | null) {
  if (ms === null) return '—';
  const minutes = ms / 60000;
  if (minutes < 1) return `${Math.round(ms / 1000)} san`;
  return `${minutes.toFixed(1)} dəq`;
}

export function ChallengeStatsBar({ challengeId }: { challengeId: number }) {
  const { t } = useTranslation();
  const statsQuery = useQuery({
    queryKey: ['challenge', 'stats', challengeId],
    queryFn: () => getChallengeStats(challengeId),
  });

  const data = statsQuery.data;
  if (!data || data.totalSubmissions === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap items-center gap-4 rounded-xl bg-slate-100/70 px-4 py-2.5 text-xs text-slate-600 dark:bg-white/5 dark:text-slate-300">
      <span className="flex items-center gap-1.5">
        <Users size={13} />
        {t('challenges.statsSolvers', { count: data.solverCount })}
      </span>
      <span className="flex items-center gap-1.5">
        <Percent size={13} />
        {t('challenges.statsAcceptance', { rate: Math.round(data.acceptanceRate * 100) })}
      </span>
      <span className="flex items-center gap-1.5">
        <Timer size={13} />
        {t('challenges.statsAverageTime', { time: formatMinutes(data.averageSolveTimeMs) })}
      </span>
    </div>
  );
}
