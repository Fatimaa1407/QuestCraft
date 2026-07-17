import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Swords, ClipboardList, ListChecks } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { VerdictBadge } from '../../components/ui/VerdictBadge';
import { useRelativeTime } from '../../utils/useRelativeTime';
import type { SubmissionListItem } from '../../types/submission';
import type { QuizAttemptListItem } from '../../types/quiz';

type FeedItem =
  | { kind: 'submission'; timestamp: string; data: SubmissionListItem }
  | { kind: 'quiz'; timestamp: string; data: QuizAttemptListItem };

export function ActivityFeed({
  submissions,
  quizzes,
  isLoading,
}: {
  submissions: SubmissionListItem[] | undefined;
  quizzes: QuizAttemptListItem[] | undefined;
  isLoading: boolean;
}) {
  const { t } = useTranslation();
  const formatRelative = useRelativeTime();

  const items = useMemo<FeedItem[]>(() => {
    const submissionItems: FeedItem[] = (submissions ?? []).map((s) => ({ kind: 'submission', timestamp: s.submittedAt, data: s }));
    const quizItems: FeedItem[] = (quizzes ?? []).map((q) => ({ kind: 'quiz', timestamp: q.completedAt, data: q }));
    return [...submissionItems, ...quizItems].sort(
      (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
    );
  }, [submissions, quizzes]);

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <ListChecks size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.activityFeedTitle')}</h2>
      </div>

      {isLoading ? (
        <div className="space-y-2.5">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
        </div>
      ) : items.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('dashboard.noActivity')}</p>
      ) : (
        <ul className="space-y-2.5">
          {items.map((item) => (
            <li
              key={`${item.kind}-${item.data.id}`}
              className="flex items-center gap-3 rounded-2xl border border-slate-200/70 p-4 dark:border-white/[0.06]"
            >
              <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-white">
                {item.kind === 'submission' ? <Swords size={14} /> : <ClipboardList size={14} />}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm text-slate-900 dark:text-slate-100">
                  {item.kind === 'submission' ? item.data.challengeTitle : item.data.quizTitle}
                </span>
                <span className="mt-0.5 block text-[11px] text-slate-400 dark:text-slate-500">{formatRelative(item.timestamp)}</span>
              </span>
              {item.kind === 'submission' ? (
                <VerdictBadge verdict={item.data.verdict} />
              ) : (
                <span className="shrink-0 text-xs font-semibold text-blue-600 dark:text-cyan-400">
                  {item.data.score}/{item.data.totalQuestions}
                </span>
              )}
            </li>
          ))}
        </ul>
      )}
    </GlassCard>
  );
}
