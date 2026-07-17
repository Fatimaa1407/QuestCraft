import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { CalendarDays } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';

const DAYS = 30;

function toIsoDate(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

interface DayCell {
  iso: string;
  isActive: boolean;
  isToday: boolean;
  weekday: number; // 0 = Monday .. 6 = Sunday
}

// Builds a GitHub-contribution-graph-style grid: columns are weeks, rows are weekdays
// (Monday-first), covering the last 30 days. Leading cells before day 1 of the range
// are left as empty spacers so the first real day lands on its correct weekday row.
function buildWeeks(activeDates: Set<string>): DayCell[][] {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const days: DayCell[] = [];
  for (let i = DAYS - 1; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const iso = toIsoDate(d);
    const jsWeekday = d.getDay(); // 0 = Sunday
    const weekday = (jsWeekday + 6) % 7; // 0 = Monday
    days.push({ iso, isActive: activeDates.has(iso), isToday: i === 0, weekday });
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

export function ActivityHeatmap({ activeDates, isLoading }: { activeDates: string[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const weeks = useMemo(() => buildWeeks(new Set(activeDates ?? [])), [activeDates]);

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
                      title={`${day.iso}${day.isActive ? ` · ${t('dashboard.activityHeatmapActive')}` : ''}`}
                      className={`h-3.5 w-3.5 rounded-[3px] ${
                        day.isActive
                          ? 'bg-gradient-to-br from-blue-500 to-cyan-500 shadow-sm shadow-blue-500/30'
                          : 'bg-slate-200/70 dark:bg-white/[0.06]'
                      } ${day.isToday ? 'ring-1 ring-offset-1 ring-blue-500 ring-offset-white dark:ring-cyan-400 dark:ring-offset-slate-900' : ''}`}
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
            <span className="h-3 w-3 rounded-[3px] bg-slate-200/70 dark:bg-white/[0.06]" />
            <span className="h-3 w-3 rounded-[3px] bg-gradient-to-br from-blue-500 to-cyan-500" />
            <span>{t('dashboard.activityHeatmapMore')}</span>
          </div>
        </div>
      )}
    </GlassCard>
  );
}
