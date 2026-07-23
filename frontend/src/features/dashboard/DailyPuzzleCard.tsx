import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Puzzle, CheckCircle2, ArrowRight } from 'lucide-react';
import { getDailyPuzzle } from '../../api/challenges';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';

export function DailyPuzzleCard() {
  const { t } = useTranslation();
  const puzzleQuery = useQuery({ queryKey: ['daily-puzzle'], queryFn: getDailyPuzzle });

  if (puzzleQuery.isLoading) {
    return (
      <GlassCard hoverLift={false} className="p-6">
        <Skeleton className="h-5 w-1/2" />
        <Skeleton className="mt-3 h-9 w-full" />
      </GlassCard>
    );
  }

  const data = puzzleQuery.data;
  if (!data || data.challengeId === null) {
    return null;
  }

  return (
    <GlassCard hoverLift={false} glow className="p-6">
      <div className="mb-3 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-fuchsia-500/10 text-fuchsia-600 dark:text-fuchsia-400">
          <Puzzle size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.dailyPuzzleTitle')}</h2>
      </div>

      {data.solvedToday ? (
        <p className="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
          <CheckCircle2 size={16} />
          {t('dashboard.dailyPuzzleSolved')}
        </p>
      ) : (
        <Link
          to={`/challenges/${data.challengeId}`}
          className="flex items-center justify-between rounded-xl border border-fuchsia-400/30 bg-fuchsia-500/[0.06] px-4 py-3 text-sm transition-colors hover:bg-fuchsia-500/10"
        >
          <span>
            <span className="font-medium text-slate-800 dark:text-slate-100">{data.title}</span>
            <span className="ml-2 text-xs text-slate-500 dark:text-slate-400">{data.difficulty}</span>
          </span>
          <ArrowRight size={15} className="text-fuchsia-600 dark:text-fuchsia-400" />
        </Link>
      )}
    </GlassCard>
  );
}
