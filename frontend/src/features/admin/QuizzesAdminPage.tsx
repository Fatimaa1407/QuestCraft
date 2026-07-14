import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { getQuizzes } from '../../api/quizzes';
import { deleteQuiz } from '../../api/admin';
import { GlassCard } from '../../components/ui/GlassCard';

export function QuizzesAdminPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const listQuery = useQuery({
    queryKey: ['admin-quizzes'],
    queryFn: () => getQuizzes({ pageSize: 100 }).then((r) => r.items),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteQuiz,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-quizzes'] }),
  });

  const handleDelete = (id: number) => {
    if (!window.confirm(t('admin.confirmDelete'))) return;
    deleteMutation.mutate(id);
  };

  const items = listQuery.data ?? [];

  return (
    <GlassCard className="p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.sections.quizzes')}</h2>
        <Link
          to="/admin/quizzes/new"
          className="flex items-center gap-1.5 rounded-lg bg-gradient-to-r from-indigo-600 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white shadow-lg shadow-indigo-600/25 transition hover:brightness-110"
        >
          <Plus size={14} />
          {t('admin.add')}
        </Link>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
              <th className="px-3 py-2 font-medium">{t('admin.categories.name')}</th>
              <th className="px-3 py-2 font-medium">{t('admin.sections.categories')}</th>
              <th className="px-3 py-2 font-medium">Level</th>
              <th className="px-3 py-2 font-medium">Questions</th>
              <th className="px-3 py-2 font-medium">{t('admin.marketplace.active')}</th>
              <th className="px-3 py-2 font-medium">{t('admin.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-3 py-8 text-center text-slate-500 dark:text-slate-400">
                  {t('admin.empty')}
                </td>
              </tr>
            ) : (
              items.map((item) => (
                <tr key={item.id} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                  <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.title}</td>
                  <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.category ?? '—'}</td>
                  <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.requiredLevel}</td>
                  <td className="px-3 py-2.5 text-slate-700 dark:text-slate-200">{item.questionCount}</td>
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
                        to={`/admin/quizzes/${item.id}`}
                        className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-indigo-600 dark:text-slate-300 dark:hover:text-cyan-400"
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
  );
}
