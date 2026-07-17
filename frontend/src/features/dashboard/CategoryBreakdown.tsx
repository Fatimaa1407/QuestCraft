import { useTranslation } from 'react-i18next';
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { PieChart } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { useThemeStore } from '../../app/themeStore';
import type { CategoryProgress } from '../../types/gamification';

interface Row extends CategoryProgress {
  remaining: number;
}

function TooltipContent({ active, payload }: { active?: boolean; payload?: Array<{ payload: Row }> }) {
  const { t } = useTranslation();
  if (!active || !payload || payload.length === 0) return null;
  const row = payload[0].payload;
  return (
    <div className="rounded-xl border border-slate-200/70 bg-white/95 px-3 py-2 text-xs shadow-lg backdrop-blur-xl dark:border-white/[0.08] dark:bg-slate-900/95">
      <p className="font-medium text-slate-700 dark:text-slate-200">{row.categoryName}</p>
      <p className="mt-0.5 text-blue-600 dark:text-cyan-400">{t('dashboard.categoryTooltip', { completed: row.completed, total: row.total })}</p>
    </div>
  );
}

export function CategoryBreakdown({ data, isLoading }: { data: CategoryProgress[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const theme = useThemeStore((s) => s.theme);
  const isDark = theme === 'dark';
  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : 'rgba(15,23,42,0.08)';
  const axisColor = isDark ? '#94a3b8' : '#64748b';
  const trackColor = isDark ? 'rgba(255,255,255,0.06)' : 'rgba(15,23,42,0.08)';

  const rows: Row[] = (data ?? []).map((c) => ({ ...c, remaining: Math.max(0, c.total - c.completed) }));
  const chartHeight = Math.max(140, rows.length * 44);

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <PieChart size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.categoryBreakdownTitle')}</h2>
      </div>

      {isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : rows.length === 0 ? (
        <div className="flex h-40 items-center justify-center">
          <p className="text-sm text-slate-500 dark:text-slate-500">{t('dashboard.noCategoryData')}</p>
        </div>
      ) : (
        <div style={{ height: chartHeight }} className="w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={rows} layout="vertical" margin={{ top: 0, right: 16, left: 0, bottom: 0 }} barCategoryGap={14}>
              <CartesianGrid stroke={gridColor} horizontal={false} />
              <XAxis type="number" allowDecimals={false} tick={{ fill: axisColor, fontSize: 11 }} axisLine={false} tickLine={false} />
              <YAxis
                type="category"
                dataKey="categoryName"
                tick={{ fill: axisColor, fontSize: 12 }}
                axisLine={false}
                tickLine={false}
                width={110}
              />
              <Tooltip content={<TooltipContent />} cursor={{ fill: isDark ? 'rgba(255,255,255,0.03)' : 'rgba(15,23,42,0.03)' }} />
              <Bar dataKey="completed" stackId="progress" fill="#06b6d4" radius={[4, 0, 0, 4]} barSize={16} />
              <Bar dataKey="remaining" stackId="progress" fill={trackColor} radius={[0, 4, 4, 0]} barSize={16} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </GlassCard>
  );
}
