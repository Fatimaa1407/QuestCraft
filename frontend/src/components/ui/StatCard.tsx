import type { LucideIcon } from 'lucide-react';
import { GlassCard } from './GlassCard';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';

export const statCardTints = {
  // 'blue'/'cyan' are the two primary/brand tints — tied to the accent CSS vars so an equipped
  // Theme re-colors these stat cards along with buttons, sidebar and glow (not just semantic
  // tints like amber/violet/emerald, which stay fixed since they carry their own meaning).
  blue: 'bg-app-accent/10 text-app-accent shadow-app-accent/20',
  cyan: 'bg-app-accent-2/10 text-app-accent-2 shadow-app-accent-2/20',
  amber: 'bg-amber-500/10 text-amber-600 shadow-amber-500/20 dark:text-amber-400',
  violet: 'bg-violet-500/10 text-violet-600 shadow-violet-500/20 dark:text-violet-400',
  emerald: 'bg-emerald-500/10 text-emerald-600 shadow-emerald-500/20 dark:text-emerald-400',
} as const;

export type StatCardTint = keyof typeof statCardTints;

export function StatCard({
  icon: Icon,
  label,
  value,
  tint,
}: {
  icon: LucideIcon;
  label: string;
  value: string | number;
  tint: StatCardTint;
}) {
  const animated = useAnimatedNumber(typeof value === 'number' ? value : 0);
  const displayValue = typeof value === 'number' ? animated : value;

  return (
    <GlassCard className="p-6">
      <div className={`mb-4 flex h-11 w-11 items-center justify-center rounded-xl shadow-lg ${statCardTints[tint]}`}>
        <Icon size={20} />
      </div>
      <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
      <p className="mt-1 text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{displayValue}</p>
    </GlassCard>
  );
}
