import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Search, HelpCircle, Star, CheckCircle2, Play, RotateCcw } from 'lucide-react';
import { getQuizzes, getMyQuizAttempts } from '../../api/quizzes';
import { GlassCard } from '../../components/ui/GlassCard';

export function QuizListPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const quizzesQuery = useQuery({
    queryKey: ['quizzes', 'list', search, page],
    queryFn: () => getQuizzes({ search: search || undefined, page, pageSize: 12 }),
  });

  const myAttemptsQuery = useQuery({
    queryKey: ['quizzes', 'attempts', 'my', 'all'],
    queryFn: () => getMyQuizAttempts(1, 100),
  });

  const completedQuizIds = useMemo(
    () => new Set(myAttemptsQuery.data?.items.map((a) => a.quizId) ?? []),
    [myAttemptsQuery.data],
  );

  const data = quizzesQuery.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t('quiz.title')}</h1>
        <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">{t('quiz.subtitle')}</p>
      </div>

      <div className="relative w-64">
        <Search size={16} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder={t('quiz.searchPlaceholder')}
          className="w-64 rounded-full border border-slate-200/70 bg-white/80 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
      </div>

      {quizzesQuery.isLoading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : !data || data.items.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('quiz.empty')}</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((quiz) => {
            const isCompleted = completedQuizIds.has(quiz.id);
            return (
              <Link key={quiz.id} to={`/practice/${quiz.id}`}>
                <GlassCard className="relative h-full p-5 transition hover:-translate-y-0.5 hover:shadow-xl">
                  {isCompleted && (
                    <span className="absolute right-4 top-4 flex items-center gap-1 rounded-full bg-emerald-500/10 px-2 py-0.5 text-[11px] font-medium text-emerald-600 dark:text-emerald-400">
                      <CheckCircle2 size={12} />
                      {t('quiz.completedBadge')}
                    </span>
                  )}

                  <h2 className="pr-24 font-semibold text-slate-900 dark:text-slate-100">{quiz.title}</h2>
                  <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{quiz.category ?? t('quiz.uncategorized')}</p>
                  <div className="mt-4 flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
                    <span className="flex items-center gap-1">
                      <HelpCircle size={13} className="text-blue-500 dark:text-cyan-400" />
                      {t('quiz.questionCount', { count: quiz.questionCount })}
                    </span>
                    <span className="flex items-center gap-1">
                      <Star size={13} className="text-blue-500 dark:text-cyan-400" />
                      {quiz.xpReward} XP
                    </span>
                  </div>

                  <div className="mt-4 flex items-center gap-1.5 text-xs font-medium text-blue-600 dark:text-cyan-400">
                    {isCompleted ? (
                      <>
                        <RotateCcw size={13} />
                        {t('quiz.practiceAgain')}
                      </>
                    ) : (
                      <>
                        <Play size={13} />
                        {t('quiz.start')}
                      </>
                    )}
                  </div>
                </GlassCard>
              </Link>
            );
          })}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-2">
          {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((p) => (
            <button
              key={p}
              onClick={() => setPage(p)}
              className={`h-8 w-8 rounded-full text-sm font-medium transition ${
                p === page
                  ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30'
                  : 'border border-slate-200/70 text-slate-600 hover:border-blue-400 dark:border-white/[0.08] dark:text-slate-300'
              }`}
            >
              {p}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
