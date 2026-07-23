import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { History, CheckCircle2, XCircle } from 'lucide-react';
import { getChallengeReplay } from '../../api/submissions';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';

function formatDuration(ms: number | null) {
  if (ms === null) return '—';
  if (ms < 1000) return `${ms} ms`;
  const seconds = ms / 1000;
  if (seconds < 60) return `${seconds.toFixed(1)} san`;
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = Math.round(seconds % 60);
  return `${minutes} dəq ${remainingSeconds} san`;
}

export function ChallengeReplayPanel({ challengeId }: { challengeId: number }) {
  const { t } = useTranslation();
  const replayQuery = useQuery({
    queryKey: ['submissions', 'replay', challengeId],
    queryFn: () => getChallengeReplay(challengeId),
  });

  if (replayQuery.isLoading) {
    return (
      <GlassCard hoverLift={false} className="p-6">
        <Skeleton className="h-5 w-1/3" />
        <div className="mt-4 space-y-2">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </GlassCard>
    );
  }

  const data = replayQuery.data;
  if (!data || data.totalAttempts === 0) {
    return null;
  }

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-4 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
          <History size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('challenges.replayTitle')}</h2>
      </div>

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <ReplayStat label={t('challenges.replayTotalAttempts')} value={String(data.totalAttempts)} />
        <ReplayStat label={t('challenges.replayWrongAttempts')} value={String(data.wrongAttempts)} />
        <ReplayStat
          label={t('challenges.replayFirstAccepted')}
          value={data.firstAcceptedAt ? new Date(data.firstAcceptedAt).toLocaleDateString() : '—'}
        />
        <ReplayStat label={t('challenges.replayTimeToSolve')} value={formatDuration(data.timeToSolveMs)} />
      </div>

      <div className="mt-4 space-y-1.5">
        {data.attempts.slice(0, 10).map((attempt) => (
          <div
            key={attempt.id}
            className="flex items-center justify-between rounded-lg border border-slate-200/70 px-3 py-2 text-xs dark:border-white/[0.06]"
          >
            <span className="flex items-center gap-1.5 font-medium text-slate-700 dark:text-slate-200">
              {attempt.verdict === 'Accepted' ? (
                <CheckCircle2 size={13} className="text-emerald-500" />
              ) : (
                <XCircle size={13} className="text-red-500" />
              )}
              {t(`challenges.verdict.${attempt.verdict}`, attempt.verdict)}
            </span>
            <span className="text-slate-400 dark:text-slate-500">{new Date(attempt.submittedAt).toLocaleString()}</span>
          </div>
        ))}
      </div>
    </GlassCard>
  );
}

function ReplayStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl bg-slate-100/70 px-3 py-2.5 dark:bg-white/5">
      <p className="text-[11px] text-slate-500 dark:text-slate-400">{label}</p>
      <p className="mt-0.5 text-sm font-semibold text-slate-900 dark:text-slate-100">{value}</p>
    </div>
  );
}
