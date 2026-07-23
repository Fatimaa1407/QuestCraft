import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { CalendarDays } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import type { HeatmapDay } from '../../types/gamification';

const DAYS = 30;

function toIsoDate(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

// 0 = inactive, 1-4 = increasing intensity buckets by action count.
function intensityLevel(count: number): 0 | 1 | 2 | 3 | 4 {
  if (count <= 0) return 0;
  if (count === 1) return 1;
  if (count <= 3) return 2;
  if (count <= 5) return 3;
  return 4;
}

const LEVEL_CLASSES: Record<0 | 1 | 2 | 3 | 4, string> = {
  0: 'bg-slate-200/70 dark:bg-white/[0.06]',
  1: 'bg-blue-300/60 dark:bg-cyan-900/60',
  2: 'bg-blue-400/80 dark:bg-cyan-700/80',
  3: 'bg-blue-500 dark:bg-cyan-500',
  4: 'bg-gradient-to-br from-blue-600 to-cyan-500 shadow-sm shadow-blue-500/40',
};

interface DayCell {
  iso: string;
  count: number;
  isToday: boolean;
  weekday: number; // 0 = Monday .. 6 = Sunday
}

// Builds a GitHub-contribution-graph-style grid: columns are weeks, rows are weekdays
// (Monday-first), covering the last 30 days. Leading cells before day 1 of the range
// are left as empty spacers so the first real day lands on its correct weekday row.
function buildWeeks(countByDate: Map<string, number>): DayCell[][] {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const days: DayCell[] = [];
  for (let i = DAYS - 1; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const iso = toIsoDate(d);
    const jsWeekday = d.getDay(); // 0 = Sunday
    const weekday = (jsWeekday + 6) % 7; // 0 = Monday
    days.push({ iso, count: countByDate.get(iso) ?? 0, isToday: i === 0, weekday });
  }

  const weeks: DayCell[][] = [];
  let currentWeek: (DayCell | null)[] = new Array(days[0].weekday).fill(null);
  for (const day of days) {
    currentWeek.push(day);
    if (currentWeek.length === 7) {
      weeks.push(currentWeek as DayCell[]);
      currentWeek = [];
    }
  }
  if (currentWeek.length > 0) {
    while (currentWeek.length < 7) currentWeek.push(null);
    weeks.push(currentWeek as DayCell[]);
  }
  return weeks;
}

export function ActivityHeatmap({ activeDates, isLoading }: { activeDates: HeatmapDay[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const weeks = useMemo(
    () => buildWeeks(new Map((activeDates ?? []).map((d) => [d.date, d.count]))),
    [activeDates],
  );

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <CalendarDays size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.activityHeatmapTitle')}</h2>
      </div>

      {isLoading ? (
        <Skeleton className="h-32 w-full" />
      ) : (
        <div className="flex items-start gap-3 overflow-x-auto pb-1">
          <div className="flex gap-[3px]">
            {weeks.map((week, weekIdx) => (
              <div key={weekIdx} className="flex flex-col gap-[3px]">
                {week.map((day, dayIdx) =>
                  day ? (
                    <div
                      key={day.iso}
                      title={`${day.iso}${day.count > 0 ? ` · ${t('dashboard.activityHeatmapCount', { count: day.count })}` : ''}`}
                      className={`h-3.5 w-3.5 rounded-[3px] ${LEVEL_CLASSES[intensityLevel(day.count)]} ${
                        day.isToday ? 'ring-1 ring-offset-1 ring-blue-500 ring-offset-white dark:ring-cyan-400 dark:ring-offset-slate-900' : ''
                      }`}
                    />
                  ) : (
                    <div key={dayIdx} className="h-3.5 w-3.5" />
                  ),
                )}
              </div>
            ))}
          </div>

          <div className="ml-1 flex shrink-0 items-center gap-1.5 self-end text-[11px] text-slate-400 dark:text-slate-500">
            <span>{t('dashboard.activityHeatmapLess')}</span>
            {([0, 1, 2, 3, 4] as const).map((level) => (
              <span key={level} className={`h-3 w-3 rounded-[3px] ${LEVEL_CLASSES[level]}`} />
            ))}
            <span>{t('dashboard.activityHeatmapMore')}</span>
          </div>
        </div>
      )}
    </GlassCard>
  );
}
