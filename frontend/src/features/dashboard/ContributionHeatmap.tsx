import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import type { HeatmapDayDetail } from '../../types/gamification';

const WEEKS = 26;
const DAYS = WEEKS * 7;
const WEEKDAY_LABELS_AZ = ['B.e', '', 'Ç.a', '', 'C', '', ''];

function toIsoDate(d: Date) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function intensityLevel(count: number): 0 | 1 | 2 | 3 | 4 {
  if (count <= 0) return 0;
  if (count === 1) return 1;
  if (count <= 3) return 2;
  if (count <= 5) return 3;
  return 4;
}

const LEVEL_CLASSES: Record<0 | 1 | 2 | 3 | 4, string> = {
  0: 'bg-slate-200/70 dark:bg-white/[0.06]',
  1: 'bg-blue-300/60 dark:bg-cyan-900/70',
  2: 'bg-blue-400/80 dark:bg-cyan-700/90',
  3: 'bg-blue-500 dark:bg-cyan-500',
  4: 'bg-gradient-to-br from-blue-600 to-cyan-400 shadow-[0_0_8px_rgba(6,182,212,0.6)]',
};

interface DayCell {
  iso: string;
  date: Date;
  detail: HeatmapDayDetail | null;
  isToday: boolean;
  isFuture: boolean;
  weekday: number; // 0 = Monday .. 6 = Sunday
}

function buildWeeks(byDate: Map<string, HeatmapDayDetail>): DayCell[][] {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const days: DayCell[] = [];
  for (let i = DAYS - 1; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const iso = toIsoDate(d);
    const jsWeekday = d.getDay();
    const weekday = (jsWeekday + 6) % 7;
    days.push({ iso, date: d, detail: byDate.get(iso) ?? null, isToday: i === 0, isFuture: false, weekday });
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

function formatCodingTime(ms: number, t: (key: string, opts?: Record<string, unknown>) => string) {
  if (ms <= 0) return t('dashboard.heatmapNoTime');
  const totalMinutes = Math.round(ms / 60000);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  if (hours > 0) return t('dashboard.heatmapHoursMinutes', { hours, minutes });
  return t('dashboard.heatmapMinutes', { minutes: Math.max(1, minutes) });
}

const MONTH_KEYS_AZ = ['Yan', 'Fev', 'Mar', 'Apr', 'May', 'İyn', 'İyl', 'Avq', 'Sen', 'Okt', 'Noy', 'Dek'];

export function ContributionHeatmap({ days, isLoading }: { days: HeatmapDayDetail[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const [hovered, setHovered] = useState<DayCell | null>(null);

  const weeks = useMemo(() => buildWeeks(new Map((days ?? []).map((d) => [d.date, d]))), [days]);

  const monthLabels = useMemo(() => {
    let lastMonth = -1;
    return weeks.map((week) => {
      const firstReal = week.find((d) => d !== null);
      if (!firstReal) return null;
      const month = firstReal.date.getMonth();
      if (month !== lastMonth) {
        lastMonth = month;
        return MONTH_KEYS_AZ[month];
      }
      return null;
    });
  }, [weeks]);

  if (isLoading) {
    return <div className="h-44 w-full animate-pulse rounded-2xl bg-slate-200/50 dark:bg-white/5" />;
  }

  return (
    <div className="relative">
      <div className="overflow-x-auto pb-1">
        <div className="inline-flex flex-col gap-1">
          <div className="ml-7 flex gap-[3px]">
            {weeks.map((_, weekIdx) => (
              <div key={weekIdx} className="w-[13px] shrink-0 text-[10px] text-slate-400 dark:text-slate-500">
                {monthLabels[weekIdx] ?? ''}
              </div>
            ))}
          </div>
          <div className="flex gap-[3px]">
            <div className="mr-1 flex w-6 shrink-0 flex-col gap-[3px]">
              {WEEKDAY_LABELS_AZ.map((label, i) => (
                <div key={i} className="h-[13px] text-[9px] leading-[13px] text-slate-400 dark:text-slate-500">
                  {label}
                </div>
              ))}
            </div>
            {weeks.map((week, weekIdx) => (
              <div key={weekIdx} className="flex flex-col gap-[3px]">
                {week.map((day, dayIdx) =>
                  day ? (
                    <motion.div
                      key={day.iso}
                      initial={{ opacity: 0, scale: 0.3 }}
                      animate={{ opacity: 1, scale: 1 }}
                      transition={{ delay: Math.min(1.1, (weekIdx * 7 + dayIdx) * 0.0035), duration: 0.25 }}
                      onMouseEnter={() => setHovered(day)}
                      onMouseLeave={() => setHovered((current) => (current?.iso === day.iso ? null : current))}
                      className={`h-[13px] w-[13px] cursor-pointer rounded-[3px] transition-transform hover:scale-125 ${
                        LEVEL_CLASSES[intensityLevel(day.detail?.count ?? 0)]
                      } ${day.isToday ? 'ring-1 ring-offset-1 ring-blue-500 ring-offset-white dark:ring-cyan-400 dark:ring-offset-slate-900' : ''}`}
                    />
                  ) : (
                    <div key={dayIdx} className="h-[13px] w-[13px]" />
                  ),
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="mt-3 ml-7 flex items-center gap-1.5 text-[11px] text-slate-400 dark:text-slate-500">
        <span>{t('dashboard.activityHeatmapLess')}</span>
        {([0, 1, 2, 3, 4] as const).map((level) => (
          <span key={level} className={`h-3 w-3 rounded-[3px] ${LEVEL_CLASSES[level]}`} />
        ))}
        <span>{t('dashboard.activityHeatmapMore')}</span>
      </div>

      {hovered && (
        <div className="pointer-events-none absolute -top-2 left-1/2 z-10 w-56 -translate-x-1/2 -translate-y-full rounded-xl border border-slate-200/70 bg-white/95 p-3 text-xs shadow-xl backdrop-blur-xl dark:border-white/[0.08] dark:bg-slate-900/95">
          <p className="font-semibold text-slate-800 dark:text-slate-100">{hovered.date.toLocaleDateString()}</p>
          <div className="mt-1.5 space-y-1 text-slate-500 dark:text-slate-400">
            <p>{t('dashboard.heatmapTooltipSolved', { count: hovered.detail?.challengesSolved ?? 0 })}</p>
            <p>{t('dashboard.heatmapTooltipXp', { xp: hovered.detail?.xpEarned ?? 0 })}</p>
            <p>{t('dashboard.heatmapTooltipTime', { time: formatCodingTime(hovered.detail?.codingTimeMs ?? 0, t) })}</p>
          </div>
        </div>
      )}
    </div>
  );
}
