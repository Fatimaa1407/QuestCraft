import type { SubmissionVerdict } from '../../types/submission';

const verdictStyles: Record<SubmissionVerdict, string> = {
  Accepted: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
  WrongAnswer: 'bg-red-500/10 text-red-600 dark:text-red-400',
  TimeLimitExceeded: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  RuntimeError: 'bg-orange-500/10 text-orange-600 dark:text-orange-400',
  CompileError: 'bg-orange-500/10 text-orange-600 dark:text-orange-400',
  Pending: 'bg-slate-500/10 text-slate-500 dark:text-slate-400',
};

export function VerdictBadge({ verdict }: { verdict: SubmissionVerdict }) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${verdictStyles[verdict]}`}>
      {verdict}
    </span>
  );
}
