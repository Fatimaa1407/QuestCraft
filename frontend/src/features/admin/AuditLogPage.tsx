import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { getAuditLogs } from '../../api/admin';
import { GlassCard } from '../../components/ui/GlassCard';

export function AuditLogPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);

  const logsQuery = useQuery({
    queryKey: ['admin-audit-logs', page],
    queryFn: () => getAuditLogs(page, 20),
  });

  const data = logsQuery.data;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('admin.sections.auditLog')}</h1>

      <GlassCard className="p-6">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
                <th className="px-3 py-2 font-medium">Timestamp</th>
                <th className="px-3 py-2 font-medium">User</th>
                <th className="px-3 py-2 font-medium">Action</th>
                <th className="px-3 py-2 font-medium">Entity</th>
                <th className="px-3 py-2 font-medium">IP</th>
              </tr>
            </thead>
            <tbody>
              {!data || data.items.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-3 py-8 text-center text-slate-500 dark:text-slate-400">
                    {t('admin.empty')}
                  </td>
                </tr>
              ) : (
                data.items.map((log) => (
                  <tr key={log.id} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
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
    </div>
  );
}
