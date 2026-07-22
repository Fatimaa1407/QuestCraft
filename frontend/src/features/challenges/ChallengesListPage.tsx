import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, Star, Coins, Lock, SearchX } from 'lucide-react';
import { getCategories, getChallenges, getDifficulties } from '../../api/challenges';
import { getMySubmissions } from '../../api/submissions';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { LevelSectionHeader, type LevelSectionStatus } from '../../components/ui/LevelSectionHeader';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { QueryErrorState } from '../../components/ui/QueryErrorState';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import type { ChallengeListItemDto } from '../../types/challenge';

type ChallengeStatus = 'Solved' | 'Attempted' | 'NotStarted';

export function ChallengesListPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);

  // Filters live in the URL (not local state) so they survive a reload and the back button —
  // reading/writing search params directly instead of mirroring them into useState.
  const [searchParams, setSearchParams] = useSearchParams();
  const categoryId = searchParams.get('category') ? Number(searchParams.get('category')) : undefined;
  const difficultyId = searchParams.get('difficulty') ? Number(searchParams.get('difficulty')) : undefined;
  const search = searchParams.get('q') ?? '';

  const setCategoryId = (value: number | undefined) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (value === undefined) next.delete('category');
        else next.set('category', String(value));
        return next;
      },
      { replace: true },
    );
  };
  const setDifficultyId = (value: number | undefined) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (value === undefined) next.delete('difficulty');
        else next.set('difficulty', String(value));
        return next;
      },
      { replace: true },
    );
  };
  const setSearch = (value: string) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (!value) next.delete('q');
        else next.set('q', value);
        return next;
      },
      { replace: true },
    );
  };

  const [expandedLevels, setExpandedLevels] = useState<Set<number>>(() => new Set([user?.level ?? 1]));

  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const difficultiesQuery = useQuery({ queryKey: ['difficulties'], queryFn: getDifficulties });
  const challengesQuery = useQuery({
    queryKey: ['challenges', categoryId, difficultyId, search],
    queryFn: () => getChallenges({ categoryId, difficultyId, search: search || undefined, page: 1, pageSize: 100 }),
  });

  const mySubmissionsQuery = useQuery({
    queryKey: ['submissions', 'my', 'all'],
    queryFn: () => getMySubmissions(1, 100),
  });

  const statusByChallengeId = useMemo(() => {
    const map = new Map<number, ChallengeStatus>();
    for (const s of mySubmissionsQuery.data?.items ?? []) {
      if (s.verdict === 'Accepted') {
        map.set(s.challengeId, 'Solved');
      } else if (!map.has(s.challengeId)) {
        map.set(s.challengeId, 'Attempted');
      }
    }
    return map;
  }, [mySubmissionsQuery.data]);

  const groupedByLevel = useMemo(() => {
    const groups = new Map<number, ChallengeListItemDto[]>();
    for (const c of challengesQuery.data?.items ?? []) {
      const list = groups.get(c.requiredLevel) ?? [];
      list.push(c);
      groups.set(c.requiredLevel, list);
    }
    return [...groups.entries()].sort(([a], [b]) => a - b);
  }, [challengesQuery.data]);

  // Keep the current level expanded by default once we know it (e.g. after auth hydrates).
  useEffect(() => {
    if (user?.level) {
      setExpandedLevels((prev) => (prev.has(user.level) ? prev : new Set(prev).add(user.level)));
    }
  }, [user?.level]);

  const toggleLevel = (level: number) => {
    setExpandedLevels((prev) => {
      const next = new Set(prev);
      if (next.has(level)) {
        next.delete(level);
      } else {
        next.add(level);
      }
      return next;
    });
  };

  const userLevel = user?.level ?? 1;
  const data = challengesQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('challenges.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('challenges.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="flex flex-wrap items-center gap-3">
        <div className="relative">
          <Search size={16} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('challenges.searchPlaceholder')}
            className="w-64 rounded-full border border-slate-200/70 bg-white/80 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
          />
        </div>

        <select
          value={categoryId ?? ''}
          onChange={(e) => setCategoryId(e.target.value ? Number(e.target.value) : undefined)}
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
          onChange={(e) => setDifficultyId(e.target.value ? Number(e.target.value) : undefined)}
          className="rounded-full border border-slate-200/70 bg-white/80 px-4 py-2.5 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        >
          <option value="">{t('challenges.allDifficulties')}</option>
          {difficultiesQuery.data?.map((d) => (
            <option key={d.id} value={d.id}>
              {d.name}
            </option>
          ))}
        </select>
      </motion.div>

      {challengesQuery.isLoading ? (
        <div className="space-y-4">
          {[0, 1].map((section) => (
            <div key={section} className="space-y-4">
              <Skeleton className="h-14 w-full rounded-2xl" />
              <div className="grid grid-cols-1 gap-6 pt-4 sm:grid-cols-2 lg:grid-cols-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <ChallengeCardSkeleton key={i} />
                ))}
              </div>
            </div>
          ))}
        </div>
      ) : challengesQuery.isError ? (
        <QueryErrorState onRetry={() => challengesQuery.refetch()} />
      ) : !data || data.items.length === 0 ? (
        <EmptyState
          icon={SearchX}
          tint="blue"
          title={t('challenges.empty')}
          action={
            search || categoryId !== undefined || difficultyId !== undefined
              ? {
                  label: t('challenges.clearFilters'),
                  onClick: () => {
                    setSearch('');
                    setCategoryId(undefined);
                    setDifficultyId(undefined);
                  },
                }
              : undefined
          }
        />
      ) : (
        <div className="space-y-4">
          {groupedByLevel.map(([level, items]) => {
            const allUnlocked = items.every((c) => !c.isLocked);
            const sectionStatus: LevelSectionStatus =
              level < userLevel ? 'Completed' : level === userLevel || allUnlocked ? 'Current' : 'Locked';
            const isExpanded = expandedLevels.has(level);

            return (
              <motion.div key={level} variants={fadeInUp}>
                <LevelSectionHeader
                  level={level}
                  status={sectionStatus}
                  isExpanded={isExpanded}
                  onToggle={() => toggleLevel(level)}
                  i18nNamespace="challenges"
                />

                <AnimatePresence initial={false}>
                  {isExpanded && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: 'auto', opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.25 }}
                      className="overflow-hidden"
                    >
                      <motion.div
                        variants={staggerContainer}
                        initial="hidden"
                        animate="show"
                        className="grid grid-cols-1 gap-6 pb-2 pt-4 sm:grid-cols-2 lg:grid-cols-3"
                      >
                        {items.map((challenge) => {
                          const status = statusByChallengeId.get(challenge.id) ?? 'NotStarted';
                          const card = (
                            <GlassCard
                              glow={!challenge.isLocked}
                              hoverLift={!challenge.isLocked}
                              className={`relative flex h-full flex-col p-6 ${challenge.isLocked ? 'opacity-60 grayscale-[40%]' : ''}`}
                            >
                              {challenge.isLocked && (
                                <span className="absolute right-5 top-5 flex items-center gap-1 rounded-full bg-white/10 px-2.5 py-1 text-[11px] font-semibold text-slate-400 backdrop-blur-sm">
                                  <Lock size={12} />
                                  {t('challenges.requiresLevel', { level: challenge.requiredLevel })}
                                </span>
                              )}
                              <div className="flex items-start justify-between gap-2 pr-2">
                                <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">{challenge.title}</h2>
                                {!challenge.isLocked && <DifficultyBadge name={challenge.difficulty} />}
                              </div>
                              <div className="mt-1 flex items-center justify-between gap-2">
                                <p className="text-xs text-slate-500 dark:text-slate-400">{challenge.category}</p>
                                {!challenge.isLocked && <StatusBadge status={status} />}
                              </div>

                              {challenge.tags && (
                                <div className="mt-3 flex flex-wrap gap-1.5">
                                  {challenge.tags.split(',').map((tag) => tag.trim()).filter(Boolean).map((tag) => (
                                    <span key={tag} className="rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-medium text-slate-500 dark:bg-white/5 dark:text-slate-400">
                                      {tag}
                                    </span>
                                  ))}
                                </div>
                              )}

                              <div className="mt-5 flex-1" />

                              <div className="flex items-center gap-4 border-t border-slate-200/70 pt-4 text-xs text-slate-500 dark:border-white/[0.06] dark:text-slate-400">
                                <span className="flex items-center gap-1.5 font-medium text-blue-600 dark:text-cyan-400">
                                  <Star size={14} />
                                  {challenge.xpReward} XP
                                </span>
                                <span className="flex items-center gap-1.5 font-medium text-amber-600 dark:text-amber-400">
                                  <Coins size={14} />
                                  {challenge.coinReward}
                                </span>
                              </div>
                            </GlassCard>
                          );

                          return (
                            <motion.div key={challenge.id} variants={fadeInUp}>
                              {challenge.isLocked ? (
                                <div className="cursor-not-allowed">{card}</div>
                              ) : (
                                <Link to={`/challenges/${challenge.id}`}>{card}</Link>
                              )}
                            </motion.div>
                          );
                        })}
                      </motion.div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            );
          })}
        </div>
      )}
    </motion.div>
  );
}

const difficultyStyles: Record<string, string> = {
  Easy: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
  Medium: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  Hard: 'bg-red-500/10 text-red-600 dark:text-red-400',
};

function DifficultyBadge({ name }: { name: string }) {
  const style = difficultyStyles[name] ?? 'bg-slate-500/10 text-slate-600 dark:text-slate-400';
  return <span className={`shrink-0 rounded-full px-2.5 py-1 text-xs font-semibold ${style}`}>{name}</span>;
}

// Mirrors the real challenge card's layout (title + badge, category line, XP/coin footer) so the
// page doesn't visually "jump" once real data replaces the skeleton.
function ChallengeCardSkeleton() {
  return (
    <GlassCard hoverLift={false} className="flex h-full flex-col p-6">
      <div className="flex items-start justify-between gap-2 pr-2">
        <Skeleton className="h-5 w-2/3" />
        <Skeleton className="h-5 w-14 shrink-0 rounded-full" />
      </div>
      <Skeleton className="mt-2 h-3 w-1/3" />
      <div className="mt-5 flex-1" />
      <div className="flex items-center gap-4 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-4 w-10" />
      </div>
    </GlassCard>
  );
}

const statusStyles: Record<ChallengeStatus, { dot: string; text: string }> = {
  Solved: { dot: 'bg-emerald-500', text: 'text-emerald-600 dark:text-emerald-400' },
  Attempted: { dot: 'bg-amber-500', text: 'text-amber-600 dark:text-amber-400' },
  NotStarted: { dot: 'bg-slate-400 dark:bg-slate-500', text: 'text-slate-500 dark:text-slate-400' },
};

function StatusBadge({ status }: { status: ChallengeStatus }) {
  const { t } = useTranslation();
  const style = statusStyles[status];
  return (
    <span className={`flex shrink-0 items-center gap-1.5 text-[11px] font-medium ${style.text}`}>
      <span className={`h-2 w-2 rounded-full ${style.dot}`} />
      {t(`challenges.status.${status}`)}
    </span>
  );
}

