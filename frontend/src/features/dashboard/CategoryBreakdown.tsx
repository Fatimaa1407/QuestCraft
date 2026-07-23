import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { PieChart } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import type { CategoryProgress } from '../../types/gamification';

function CategoryRow({ categoryName, completed, total }: CategoryProgress) {
  const percent = total > 0 ? Math.round((completed / total) * 100) : 0;
  const animatedPercent = useAnimatedNumber(percent, 900);

  return (
    <div>
      <div className="mb-1.5 flex items-center justify-between gap-2 text-sm">
        <span className="font-medium text-slate-700 dark:text-slate-200">{categoryName}</span>
        <span className="flex items-center gap-2 text-xs">
          <span className="text-slate-500 dark:text-slate-400">
            {completed}/{total}
          </span>
          <span className="font-semibold text-blue-600 dark:text-cyan-400">{animatedPercent}%</span>
        </span>
      </div>
      <div className="h-2.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
        <motion.div
          className="relative h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-400 shadow-[0_0_10px_rgba(6,182,212,0.5)]"
          initial={{ width: 0 }}
          animate={{ width: `${percent}%` }}
          transition={{ duration: 1, ease: 'easeOut' }}
        >
          <span className="animate-shimmer-sweep-loop absolute inset-y-0 w-8 bg-gradient-to-r from-transparent via-white/50 to-transparent" />
        </motion.div>
      </div>
    </div>
  );
}

export function CategoryBreakdown({ data, isLoading }: { data: CategoryProgress[] | undefined; isLoading: boolean }) {
  const { t } = useTranslation();
  const rows = data ?? [];

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <PieChart size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.categoryBreakdownTitle')}</h2>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="space-y-1.5">
              <Skeleton className="h-4 w-1/3" />
              <Skeleton className="h-2.5 w-full" />
            </div>
          ))}
        </div>
      ) : rows.length === 0 ? (
        <div className="flex h-40 items-center justify-center">
          <p className="text-sm text-slate-500 dark:text-slate-500">{t('dashboard.noCategoryData')}</p>
        </div>
      ) : (
        <div className="space-y-4">
          {rows.map((row) => (
            <CategoryRow key={row.categoryName} {...row} />
          ))}
        </div>
      )}
    </GlassCard>
  );
}
