import type { LucideIcon } from 'lucide-react';
import { Link } from 'react-router-dom';
import { GlassCard } from './GlassCard';
import { fadeInUp } from '../../utils/motion';

export type EmptyStateTint = 'blue' | 'cyan' | 'amber' | 'violet' | 'slate';

// Mirrors StatCard's `statCardTints` pattern (soft tinted circle, shadow-matched to the tint) so
// empty states read as part of the same design system. Adds a neutral `slate` option for generic
// "nothing here" cases that don't map to one of the app's semantic colors.
const emptyStateTints: Record<EmptyStateTint, string> = {
  // blue/cyan intentionally mirror the equipped Theme's accent pair (same convention as
  // StatCard's tint map) rather than a fixed brand blue, so empty states pick up theme changes too.
  blue: 'bg-app-accent/10 text-app-accent shadow-app-accent/20',
  cyan: 'bg-app-accent-2/10 text-app-accent-2 shadow-app-accent-2/20',
  amber: 'bg-amber-500/10 text-amber-600 shadow-amber-500/20 dark:text-amber-400',
  violet: 'bg-violet-500/10 text-violet-600 shadow-violet-500/20 dark:text-violet-400',
  slate: 'bg-slate-500/10 text-slate-500 shadow-slate-500/10 dark:text-slate-400',
};

interface EmptyStateAction {
  label: string;
  onClick?: () => void;
  to?: string;
}

interface EmptyStateProps {
  icon: LucideIcon;
  title: string;
  description?: string;
  action?: EmptyStateAction;
  tint?: EmptyStateTint;
  /** Renders without the GlassCard shell — for use inside a card that's already glass-styled. */
  bare?: boolean;
  className?: string;
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  tint = 'slate',
  bare = false,
  className = '',
}: EmptyStateProps) {
  const actionClasses =
    'mt-6 inline-flex items-center gap-1.5 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2.5 text-sm font-medium text-white shadow-sm shadow-app-accent/30 transition hover:brightness-110';

  const content = (
    <>
      <div className={`mb-5 flex h-16 w-16 items-center justify-center rounded-full shadow-lg ${emptyStateTints[tint]}`}>
        <Icon size={28} />
      </div>
      <p className="text-base font-medium text-slate-700 dark:text-slate-200">{title}</p>
      {description && <p className="mt-1.5 max-w-sm text-sm text-slate-500 dark:text-slate-400">{description}</p>}
      {action &&
        (action.to ? (
          <Link to={action.to} className={actionClasses}>
            {action.label}
          </Link>
        ) : (
          <button type="button" onClick={action.onClick} className={actionClasses}>
            {action.label}
          </button>
        ))}
    </>
  );

  if (bare) {
    return <div className={`flex flex-col items-center justify-center py-10 text-center ${className}`}>{content}</div>;
  }

  return (
    <GlassCard
      hoverLift={false}
      variants={fadeInUp}
      className={`flex flex-col items-center justify-center px-6 py-16 text-center ${className}`}
    >
      {content}
    </GlassCard>
  );
}
