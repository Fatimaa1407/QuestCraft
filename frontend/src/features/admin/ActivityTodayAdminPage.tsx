import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Swords, ClipboardList, Activity } from 'lucide-react';
import { getAdminActivityToday } from '../../api/admin';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { VerdictBadge } from '../../components/ui/VerdictBadge';
import type { SubmissionVerdict } from '../../types/submission';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function ActivityTodayAdminPage() {
  const { t } = useTranslation();

  const activityQuery = useQuery({ queryKey: ['admin-activity-today'], queryFn: getAdminActivityToday });
  const items = activityQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.activityToday.title')}</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('admin.activityToday.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="max-h-[30rem] overflow-auto rounded-lg">
            <table className="w-full text-left text-sm">
              <thead className="sticky top-0 z-10 bg-white/90 backdrop-blur-sm dark:bg-slate-900/90">
                <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
                  <th className="px-3 py-2 font-medium">{t('admin.activityToday.columnUser')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.activityToday.columnItem')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.activityToday.columnResult')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.activityToday.columnTime')}</th>
                </tr>
              </thead>
              <tbody>
                {activityQuery.isLoading ? (
                  Array.from({ length: 6 }).map((_, rowIndex) => (
                    <tr key={`skeleton-${rowIndex}`} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                      {Array.from({ length: 4 }).map((__, colIndex) => (
                        <td key={colIndex} className="px-3 py-2.5">
                          <Skeleton className="h-4 w-full max-w-[140px]" />
                        </td>
                      ))}
                    </tr>
                  ))
                ) : items.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-3 py-6">
                      <EmptyState bare icon={Activity} title={t('admin.activityToday.empty')} />
                    </td>
                  </tr>
                ) : (
                  items.map((item, idx) => (
                    <tr
                      key={`${item.kind}-${item.userId}-${item.timestamp}`}
                      className={`border-b border-slate-100 last:border-0 transition-colors dark:border-white/[0.04] ${
                        idx % 2 === 1 ? 'bg-slate-50/60 dark:bg-white/[0.02]' : ''
                      } hover:bg-blue-50/60 dark:hover:bg-white/[0.06]`}
                    >
                      <td className="px-3 py-2.5">
                        <span className="flex items-center gap-2 font-medium text-slate-800 dark:text-slate-100">
                          <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-white">
                            {item.kind === 'Submission' ? <Swords size={12} /> : <ClipboardList size={12} />}
                          </span>
                          {item.username}
                        </span>
                      </td>
                      <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.title}</td>
                      <td className="px-3 py-2.5">
                        {item.kind === 'Submission' && item.verdict ? (
                          <VerdictBadge verdict={item.verdict as SubmissionVerdict} />
                        ) : (
                          <span className="text-xs font-semibold text-blue-600 dark:text-cyan-400">
                            {item.score}/{item.totalQuestions}
                          </span>
                        )}
                      </td>
                      <td className="px-3 py-2.5 whitespace-nowrap text-slate-500 dark:text-slate-400">
                        {new Date(item.timestamp).toLocaleTimeString()}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}
