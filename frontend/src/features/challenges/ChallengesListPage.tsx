import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Search, Star, Coins } from 'lucide-react';
import { getCategories, getChallenges, getDifficulties } from '../../api/challenges';
import { GlassCard } from '../../components/ui/GlassCard';

export function ChallengesListPage() {
  const { t } = useTranslation();
  const [categoryId, setCategoryId] = useState<number | undefined>(undefined);
  const [difficultyId, setDifficultyId] = useState<number | undefined>(undefined);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const difficultiesQuery = useQuery({ queryKey: ['difficulties'], queryFn: getDifficulties });
  const challengesQuery = useQuery({
    queryKey: ['challenges', categoryId, difficultyId, search, page],
    queryFn: () => getChallenges({ categoryId, difficultyId, search: search || undefined, page, pageSize: 12 }),
  });

  const data = challengesQuery.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t('challenges.title')}</h1>
        <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">{t('challenges.subtitle')}</p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <div className="relative">
          <Search size={16} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            placeholder={t('challenges.searchPlaceholder')}
            className="w-64 rounded-full border border-slate-200/70 bg-white/80 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
          />
        </div>

        <select
          value={categoryId ?? ''}
          onChange={(e) => {
            setCategoryId(e.target.value ? Number(e.target.value) : undefined);
            setPage(1);
          }}
          className="rounded-full border border-slate-200/70 bg-white/80 px-4 py-2.5 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        >
          <option value="">{t('challenges.allCategories')}</option>
          {categoriesQuery.data?.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>

        <select
          value={difficultyId ?? ''}
          onChange={(e) => {
            setDifficultyId(e.target.value ? Number(e.target.value) : undefined);
            setPage(1);
          }}
          className="rounded-full border border-slate-200/70 bg-white/80 px-4 py-2.5 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        >
          <option value="">{t('challenges.allDifficulties')}</option>
          {difficultiesQuery.data?.map((d) => (
            <option key={d.id} value={d.id}>
              {d.name}
            </option>
          ))}
        </select>
      </div>

      {challengesQuery.isLoading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : !data || data.items.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('challenges.empty')}</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((challenge) => (
            <Link key={challenge.id} to={`/challenges/${challenge.id}`}>
              <GlassCard className="h-full p-5 transition hover:-translate-y-0.5 hover:shadow-xl">
                <div className="flex items-start justify-between gap-2">
                  <h2 className="font-semibold text-slate-900 dark:text-slate-100">{challenge.title}</h2>
                  <DifficultyBadge name={challenge.difficulty} />
                </div>
                <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{challenge.category}</p>
                <div className="mt-4 flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
                  <span className="flex items-center gap-1">
                    <Star size={13} className="text-blue-500 dark:text-cyan-400" />
                    {challenge.xpReward} XP
                  </span>
                  <span className="flex items-center gap-1">
                    <Coins size={13} className="text-amber-500" />
                    {challenge.coinReward}
                  </span>
                </div>
              </GlassCard>
            </Link>
          ))}
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

const difficultyStyles: Record<string, string> = {
  Easy: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
  Medium: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  Hard: 'bg-red-500/10 text-red-600 dark:text-red-400',
};

function DifficultyBadge({ name }: { name: string }) {
  const style = difficultyStyles[name] ?? 'bg-slate-500/10 text-slate-600 dark:text-slate-400';
  return <span className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${style}`}>{name}</span>;
}
