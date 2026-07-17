import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Search, ShieldCheck, UserX, UserCheck, Users } from 'lucide-react';
import { getAdminUsers, updateUserActive, updateUserRole } from '../../api/admin';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function UsersAdminPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const currentUser = useAuthStore((s) => s.user);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const usersQuery = useQuery({
    queryKey: ['admin-users', page, search],
    queryFn: () => getAdminUsers(page, 20, search),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin-users'] });
  const roleMutation = useMutation({ mutationFn: ({ id, role }: { id: number; role: string }) => updateUserRole(id, role), onSuccess: invalidate });
  const activeMutation = useMutation({ mutationFn: ({ id, isActive }: { id: number; isActive: boolean }) => updateUserActive(id, isActive), onSuccess: invalidate });

  const data = usersQuery.data;
  const items = data?.items ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.users.title')}</h1>
      </motion.div>

      <motion.div variants={fadeInUp} className="relative max-w-sm">
        <Search size={15} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder={t('admin.users.searchPlaceholder')}
          className="w-full rounded-full border border-slate-200/70 bg-white/80 py-2 pl-9 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="max-h-[30rem] overflow-auto rounded-lg">
            <table className="w-full text-left text-sm">
              <thead className="sticky top-0 z-10 bg-white/90 backdrop-blur-sm dark:bg-slate-900/90">
                <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnUser')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnRole')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnLevel')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnStatus')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnLastLogin')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.users.columnJoined')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {usersQuery.isLoading ? (
                  Array.from({ length: 6 }).map((_, rowIndex) => (
                    <tr key={`skeleton-${rowIndex}`} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                      {Array.from({ length: 6 }).map((__, colIndex) => (
                        <td key={colIndex} className="px-3 py-2.5">
                          <Skeleton className="h-4 w-full max-w-[140px]" />
                        </td>
                      ))}
                      <td className="px-3 py-2.5">
                        <Skeleton className="h-4 w-20" />
                      </td>
                    </tr>
                  ))
                ) : items.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-3 py-6">
                      <EmptyState bare icon={Users} title={t('admin.empty')} />
                    </td>
                  </tr>
                ) : (
                  items.map((item, idx) => {
                    const isSelf = item.id === currentUser?.id;
                    return (
                      <tr
                        key={item.id}
                        className={`border-b border-slate-100 last:border-0 transition-colors dark:border-white/[0.04] ${
                          idx % 2 === 1 ? 'bg-slate-50/60 dark:bg-white/[0.02]' : ''
                        } hover:bg-blue-50/60 dark:hover:bg-white/[0.06]`}
                      >
                        <td className="px-3 py-2.5">
                          <p className="font-medium text-slate-800 dark:text-slate-100">
                            {item.username}
                            {isSelf && <span className="ml-1.5 text-xs font-normal text-blue-500 dark:text-cyan-400">({t('admin.users.you')})</span>}
                          </p>
                          <p className="text-xs text-slate-500 dark:text-slate-400">{item.email}</p>
                        </td>
                        <td className="px-3 py-2.5">
                          <select
                            value={item.role}
                            disabled={isSelf || roleMutation.isPending}
                            onChange={(e) => roleMutation.mutate({ id: item.id, role: e.target.value })}
                            className="rounded-lg border border-slate-200/70 bg-white/80 px-2 py-1 text-xs text-slate-700 outline-none transition focus:border-blue-400 disabled:cursor-not-allowed disabled:opacity-50 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-200"
                          >
                            <option value="Admin">{t('admin.users.roleAdmin')}</option>
                            <option value="Student">{t('admin.users.roleStudent')}</option>
                          </select>
                        </td>
                        <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">
                          Lvl {item.level} · {item.xp} XP
                        </td>
                        <td className="px-3 py-2.5">
                          <span
                            className={`flex w-fit items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${
                              item.isActive
                                ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                                : 'bg-red-500/10 text-red-600 dark:text-red-400'
                            }`}
                          >
                            {item.isActive ? <ShieldCheck size={12} /> : <UserX size={12} />}
                            {item.isActive ? t('admin.users.active') : t('admin.users.inactive')}
                          </span>
                        </td>
                        <td className="px-3 py-2.5 whitespace-nowrap text-slate-500 dark:text-slate-400">
                          {item.lastLoginAt ? new Date(item.lastLoginAt).toLocaleString() : t('admin.users.never')}
                        </td>
                        <td className="px-3 py-2.5 whitespace-nowrap text-slate-500 dark:text-slate-400">
                          {new Date(item.createdAt).toLocaleDateString()}
                        </td>
                        <td className="px-3 py-2.5">
                          <button
                            type="button"
                            disabled={isSelf || activeMutation.isPending}
                            onClick={() => activeMutation.mutate({ id: item.id, isActive: !item.isActive })}
                            className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-blue-600 disabled:cursor-not-allowed disabled:opacity-50 dark:text-slate-300 dark:hover:text-cyan-400"
                          >
                            {item.isActive ? <UserX size={13} /> : <UserCheck size={13} />}
                            {item.isActive ? t('admin.users.deactivate') : t('admin.users.activate')}
                          </button>
                        </td>
                      </tr>
                    );
                  })
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
                ‹
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
                ›
              </button>
            </div>
          )}
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}
