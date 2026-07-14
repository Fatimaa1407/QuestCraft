import type { LucideIcon } from 'lucide-react';
import { GlassCard } from './GlassCard';

export const statCardTints = {
  blue: 'bg-blue-500/10 text-blue-600 shadow-blue-500/20 dark:text-blue-400',
  cyan: 'bg-cyan-500/10 text-cyan-600 shadow-cyan-500/20 dark:text-cyan-400',
  amber: 'bg-amber-500/10 text-amber-600 shadow-amber-500/20 dark:text-amber-400',
  violet: 'bg-violet-500/10 text-violet-600 shadow-violet-500/20 dark:text-violet-400',
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
  return (
    <GlassCard className="p-6">
      <div className={`mb-4 flex h-11 w-11 items-center justify-center rounded-xl shadow-lg ${statCardTints[tint]}`}>
        <Icon size={20} />
      </div>
      <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
      <p className="mt-1 text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{value}</p>
    </GlassCard>
  );
}
