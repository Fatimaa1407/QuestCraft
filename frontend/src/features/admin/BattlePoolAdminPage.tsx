import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Pencil, Plus, Swords, Trash2 } from 'lucide-react';
import { deleteChallenge, getBattlePoolChallenges } from '../../api/admin';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function BattlePoolAdminPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const listQuery = useQuery({ queryKey: ['admin-battle-pool'], queryFn: getBattlePoolChallenges });

  const deleteMutation = useMutation({
    mutationFn: deleteChallenge,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-battle-pool'] }),
  });

  const handleDelete = (id: number) => {
    if (!window.confirm(t('admin.confirmDelete'))) return;
    deleteMutation.mutate(id);
  };

  const items = listQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.battlePool.title')}</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('admin.battlePool.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.battlePool.title')}</h2>
            <Link
              to="/admin/challenges/new?battleOnly=1"
              className="flex items-center gap-1.5 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white shadow-lg shadow-blue-600/25 transition hover:brightness-110"
            >
              <Plus size={14} />
              {t('admin.battlePool.add')}
            </Link>
          </div>

          <div className="mt-4 max-h-[26rem] overflow-auto rounded-lg">
            <table className="w-full text-left text-sm">
              <thead className="sticky top-0 z-10 bg-white/90 backdrop-blur-sm dark:bg-slate-900/90">
                <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
                  <th className="px-3 py-2 font-medium">{t('admin.categories.name')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.sections.categories')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.sections.difficulties')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.marketplace.active')}</th>
                  <th className="px-3 py-2 font-medium">{t('admin.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {listQuery.isLoading ? (
                  Array.from({ length: 4 }).map((_, rowIndex) => (
                    <tr key={`skeleton-${rowIndex}`} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                      {Array.from({ length: 4 }).map((__, colIndex) => (
                        <td key={colIndex} className="px-3 py-2.5">
                          <Skeleton className="h-4 w-full max-w-[140px]" />
                        </td>
                      ))}
                      <td className="px-3 py-2.5">
                        <Skeleton className="h-4 w-16" />
                      </td>
                    </tr>
                  ))
                ) : items.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="px-3 py-6">
                      <EmptyState
                        bare
                        icon={Swords}
                        title={t('admin.battlePool.empty')}
                        description={t('admin.battlePool.emptyHint')}
                        action={{ label: t('admin.battlePool.add'), to: '/admin/challenges/new?battleOnly=1' }}
                      />
                    </td>
                  </tr>
                ) : (
                  items.map((item, idx) => (
                    <tr
                      key={item.id}
                      className={`border-b border-slate-100 last:border-0 transition-colors dark:border-white/[0.04] ${
                        idx % 2 === 1 ? 'bg-slate-50/60 dark:bg-white/[0.02]' : ''
                      } hover:bg-blue-50/60 dark:hover:bg-white/[0.06]`}
                    >
                      <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.title}</td>
                      <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.category}</td>
                      <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.difficulty}</td>
                      <td className="px-3 py-2.5">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                            item.isPublished
                              ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                              : 'bg-slate-200 text-slate-600 dark:bg-white/10 dark:text-slate-300'
                          }`}
                        >
                          {item.isPublished ? '✓' : '—'}
                        </span>
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="flex items-center gap-3">
                          <Link
                            to={`/admin/challenges/${item.id}`}
                            className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-blue-600 dark:text-slate-300 dark:hover:text-cyan-400"
                          >
                            <Pencil size={13} />
                            {t('admin.edit')}
                          </Link>
                          <button
                            type="button"
                            onClick={() => handleDelete(item.id)}
                            className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-red-600 dark:text-slate-300 dark:hover:text-red-400"
                          >
                            <Trash2 size={13} />
                            {t('admin.delete')}
                          </button>
                        </div>
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
