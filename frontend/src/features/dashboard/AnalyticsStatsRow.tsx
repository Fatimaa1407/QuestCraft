import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Flame, Trophy, CheckCircle2, Clock, Calendar } from 'lucide-react';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';

function formatTotalTime(ms: number, t: (key: string, opts?: Record<string, unknown>) => string) {
  if (ms <= 0) return '—';
  const totalMinutes = Math.round(ms / 60000);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  if (hours > 0) return t('dashboard.heatmapHoursMinutes', { hours, minutes });
  return t('dashboard.heatmapMinutes', { minutes: Math.max(1, minutes) });
}

function Chip({
  icon: Icon,
  label,
  value,
  tint,
}: {
  icon: typeof Flame;
  label: string;
  value: React.ReactNode;
  tint: string;
}) {
  return (
    <motion.div
      whileHover={{ y: -2 }}
      className="flex min-w-[140px] flex-1 items-center gap-3 rounded-2xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/[0.03]"
    >
      <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${tint}`}>
        <Icon size={18} />
      </div>
      <div className="min-w-0">
        <p className="truncate text-[11px] text-slate-500 dark:text-slate-400">{label}</p>
        <p className="text-lg font-bold text-slate-900 dark:text-white">{value}</p>
      </div>
    </motion.div>
  );
}

export function AnalyticsStatsRow({
  currentStreak,
  longestStreak,
  totalSolved,
  totalCodingTimeMs,
  dailyPuzzleSolved,
}: {
  currentStreak: number;
  longestStreak: number;
  totalSolved: number;
  totalCodingTimeMs: number;
  dailyPuzzleSolved: boolean | undefined;
}) {
  const { t } = useTranslation();
  const animatedCurrent = useAnimatedNumber(currentStreak);
  const animatedLongest = useAnimatedNumber(longestStreak);
  const animatedSolved = useAnimatedNumber(totalSolved);

  return (
    <div className="flex flex-wrap gap-3">
      <Chip
        icon={Flame}
        label={t('dashboard.heatmapCurrentStreak')}
        value={t('dashboard.streakDays', { count: animatedCurrent })}
        tint="bg-amber-500/10 text-amber-600 dark:text-amber-400"
      />
      <Chip
        icon={Trophy}
        label={t('dashboard.heatmapLongestStreak')}
        value={t('dashboard.streakDays', { count: animatedLongest })}
        tint="bg-orange-500/10 text-orange-600 dark:text-orange-400"
      />
      <Chip
        icon={CheckCircle2}
        label={t('dashboard.heatmapTotalSolved')}
        value={animatedSolved}
        tint="bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
      />
      <Chip
        icon={Clock}
        label={t('dashboard.heatmapTotalTime')}
        value={formatTotalTime(totalCodingTimeMs, t)}
        tint="bg-violet-500/10 text-violet-600 dark:text-violet-400"
      />
      <Chip
        icon={Calendar}
        label={t('dashboard.heatmapDailyPuzzle')}
        value={
          dailyPuzzleSolved === undefined ? (
            '—'
          ) : dailyPuzzleSolved ? (
            <span className="text-emerald-500">✓</span>
          ) : (
            <span className="text-slate-400 dark:text-slate-500">—</span>
          )
        }
        tint="bg-fuchsia-500/10 text-fuchsia-600 dark:text-fuchsia-400"
      />
    </div>
  );
}
