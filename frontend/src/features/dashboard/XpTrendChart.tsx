import { useTranslation } from 'react-i18next';
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { TrendingUp } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { useThemeStore } from '../../app/themeStore';
import type { XpDayPoint } from '../../types/gamification';

function formatTick(dateStr: string) {
  const d = new Date(dateStr);
  return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}`;
}

function TooltipContent({ active, payload }: { active?: boolean; payload?: Array<{ value: number; payload: XpDayPoint }> }) {
  const { t } = useTranslation();
  if (!active || !payload || payload.length === 0) return null;
  const point = payload[0].payload;
  return (
    <div className="rounded-xl border border-slate-200/70 bg-white/95 px-3 py-2 text-xs shadow-lg backdrop-blur-xl dark:border-white/[0.08] dark:bg-slate-900/95">
      <p className="font-medium text-slate-500 dark:text-slate-400">{formatTick(point.date)}</p>
      <p className="mt-0.5 font-semibold text-blue-600 dark:text-cyan-400">{t('dashboard.xpTooltip', { xp: point.xp })}</p>
    </div>
  );
}

export function XpTrendChart({ data, isLoading }: { data: XpDayPoint[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const theme = useThemeStore((s) => s.theme);
  const isDark = theme === 'dark';
  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : 'rgba(15,23,42,0.08)';
  const axisColor = isDark ? '#94a3b8' : '#64748b';

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <TrendingUp size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.xpTrendTitle')}</h2>
      </div>

      {isLoading ? (
        <Skeleton className="h-56 w-full" />
      ) : !data || data.length === 0 || data.every((p) => p.xp === 0) ? (
        <div className="flex h-56 items-center justify-center">
          <p className="text-sm text-slate-500 dark:text-slate-500">{t('dashboard.noXpData')}</p>
        </div>
      ) : (
        <div className="h-56 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id="xpFill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#06b6d4" stopOpacity={0.02} />
                </linearGradient>
                <linearGradient id="xpStroke" x1="0" y1="0" x2="1" y2="0">
                  <stop offset="0%" stopColor="#3b82f6" />
                  <stop offset="100%" stopColor="#06b6d4" />
                </linearGradient>
              </defs>
              <CartesianGrid stroke={gridColor} vertical={false} />
              <XAxis
                dataKey="date"
                tickFormatter={formatTick}
                tick={{ fill: axisColor, fontSize: 11 }}
                axisLine={{ stroke: gridColor }}
                tickLine={false}
                interval="preserveStartEnd"
                minTickGap={24}
              />
              <YAxis
                allowDecimals={false}
                tick={{ fill: axisColor, fontSize: 11 }}
                axisLine={false}
                tickLine={false}
                width={40}
                domain={[0, (dataMax: number) => Math.max(dataMax, 10)]}
              />
              <Tooltip content={<TooltipContent />} cursor={{ stroke: gridColor, strokeWidth: 1 }} />
              <Area
                type="monotone"
                dataKey="xp"
                stroke="url(#xpStroke)"
                strokeWidth={2}
                fill="url(#xpFill)"
                activeDot={{ r: 4, strokeWidth: 0, fill: '#06b6d4' }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}
    </GlassCard>
  );
}
