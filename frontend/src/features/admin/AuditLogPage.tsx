import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { ChevronLeft, ChevronRight, ScrollText } from 'lucide-react';
import { getAuditLogs } from '../../api/admin';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function AuditLogPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);

  const logsQuery = useQuery({
    queryKey: ['admin-audit-logs', page],
    queryFn: () => getAuditLogs(page, 20),
  });

  const data = logsQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.sections.auditLog')}</h1>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <div className="max-h-[26rem] overflow-auto rounded-lg">
          <table className="w-full text-left text-sm">
            <thead className="sticky top-0 z-10 bg-white/90 backdrop-blur-sm dark:bg-slate-900/90">
              <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
                <th className="px-3 py-2 font-medium">Timestamp</th>
                <th className="px-3 py-2 font-medium">User</th>
                <th className="px-3 py-2 font-medium">Action</th>
                <th className="px-3 py-2 font-medium">Entity</th>
                <th className="px-3 py-2 font-medium">IP</th>
              </tr>
            </thead>
            <tbody>
              {logsQuery.isLoading ? (
                Array.from({ length: 8 }).map((_, rowIndex) => (
                  <tr key={`skeleton-${rowIndex}`} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                    {Array.from({ length: 5 }).map((__, colIndex) => (
                      <td key={colIndex} className="px-3 py-2.5">
                        <Skeleton className="h-4 w-full max-w-[140px]" />
                      </td>
                    ))}
                  </tr>
                ))
              ) : !data || data.items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-3 py-6">
                    <EmptyState bare icon={ScrollText} title={t('admin.empty')} />
                  </td>
                </tr>
              ) : (
                data.items.map((log, idx) => (
                  <tr
                    key={log.id}
                    className={`border-b border-slate-100 last:border-0 transition-colors dark:border-white/[0.04] ${
                      idx % 2 === 1 ? 'bg-slate-50/60 dark:bg-white/[0.02]' : ''
                    } hover:bg-blue-50/60 dark:hover:bg-white/[0.06]`}
                  >
                    <td className="px-3 py-2.5 whitespace-nowrap text-slate-700 dark:text-slate-200">{new Date(log.timestamp).toLocaleString()}</td>
                    <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{log.username ?? '—'}</td>
                    <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{log.action}</td>
                    <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">
                      {log.entityName}
                      {log.entityId !== null && <span className="text-slate-400"> #{log.entityId}</span>}
                    </td>
                    <td className="px-3 py-2.5 text-slate-500 dark:text-slate-400">{log.ipAddress ?? '—'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {data && data.totalPages > 1 && (
          <div className="mt-4 flex items-center justify-center gap-3">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
              className="flex h-8 w-8 items-center justify-center rounded-lg border border-slate-200/70 text-slate-500 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/[0.08] dark:text-slate-400 dark:hover:bg-white/5"
            >
              <ChevronLeft size={14} />
            </button>
            <span className="text-sm text-slate-500 dark:text-slate-400">
              {page} / {data.totalPages}
            </span>
            <button
              type="button"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
              className="flex h-8 w-8 items-center justify-center rounded-lg border border-slate-200/70 text-slate-500 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/[0.08] dark:text-slate-400 dark:hover:bg-white/5"
            >
              <ChevronRight size={14} />
            </button>
          </div>
        )}
      </GlassCard>
      </motion.div>
    </motion.div>
  );
}
