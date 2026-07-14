import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, HelpCircle, Star, CheckCircle2, Play, RotateCcw, Lock } from 'lucide-react';
import { getQuizzes, getMyQuizAttempts } from '../../api/quizzes';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { LevelSectionHeader, type LevelSectionStatus } from '../../components/ui/LevelSectionHeader';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import type { QuizListItemDto } from '../../types/quiz';

export function QuizListPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const [search, setSearch] = useState('');
  const [expandedLevels, setExpandedLevels] = useState<Set<number>>(() => new Set([user?.level ?? 1]));

  const quizzesQuery = useQuery({
    queryKey: ['quizzes', 'list', search],
    queryFn: () => getQuizzes({ search: search || undefined, page: 1, pageSize: 100 }),
  });

  const myAttemptsQuery = useQuery({
    queryKey: ['quizzes', 'attempts', 'my', 'all'],
    queryFn: () => getMyQuizAttempts(1, 100),
  });

  const completedQuizIds = useMemo(
    () => new Set(myAttemptsQuery.data?.items.map((a) => a.quizId) ?? []),
    [myAttemptsQuery.data],
  );

  const groupedByLevel = useMemo(() => {
    const groups = new Map<number, QuizListItemDto[]>();
    for (const q of quizzesQuery.data?.items ?? []) {
      const list = groups.get(q.requiredLevel) ?? [];
      list.push(q);
      groups.set(q.requiredLevel, list);
    }
    return [...groups.entries()].sort(([a], [b]) => a - b);
  }, [quizzesQuery.data]);

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
  const data = quizzesQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('quiz.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('quiz.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="relative w-64">
        <Search size={16} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('quiz.searchPlaceholder')}
          className="w-64 rounded-full border border-slate-200/70 bg-white/80 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
      </motion.div>

      {quizzesQuery.isLoading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : !data || data.items.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('quiz.empty')}</p>
      ) : (
        <div className="space-y-4">
          {groupedByLevel.map(([level, items]) => {
            const allUnlocked = items.every((q) => !q.isLocked);
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
                  i18nNamespace="quiz"
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
                        {items.map((quiz) => {
                          const isCompleted = completedQuizIds.has(quiz.id);
                          const isLocked = quiz.isLocked;

                          const card = (
                            <GlassCard
                              glow={!isCompleted && !isLocked}
                              hoverLift={!isLocked}
                              style={isCompleted && !isLocked ? { border: '1px solid rgba(16, 185, 129, 0.35)' } : undefined}
                              className={`relative flex h-full flex-col p-6 ${isCompleted && !isLocked ? 'bg-emerald-500/[0.04]' : ''} ${isLocked ? 'opacity-60 grayscale-[40%]' : ''}`}
                            >
                              {isLocked ? (
                                <span className="absolute right-5 top-5 flex items-center gap-1 rounded-full bg-white/10 px-2.5 py-1 text-[11px] font-semibold text-slate-400 backdrop-blur-sm">
                                  <Lock size={12} />
                                  {t('quiz.requiresLevel', { level: quiz.requiredLevel })}
                                </span>
                              ) : isCompleted ? (
                                <span className="absolute right-5 top-5 flex items-center gap-1 rounded-full bg-emerald-500/15 px-2.5 py-1 text-[11px] font-semibold text-emerald-600 dark:text-emerald-400">
                                  <CheckCircle2 size={12} />
                                  {t('quiz.completedBadge')}
                                </span>
                              ) : null}

                              <h2 className="pr-24 text-lg font-semibold text-slate-900 dark:text-slate-100">{quiz.title}</h2>
                              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{quiz.category ?? t('quiz.uncategorized')}</p>

                              <div className="mt-5 flex-1" />

                              <div className="flex items-center gap-4 border-t border-slate-200/70 pt-4 text-xs text-slate-500 dark:border-white/[0.06] dark:text-slate-400">
                                <span className="flex items-center gap-1.5 font-medium">
                                  <HelpCircle size={14} className="text-blue-500 dark:text-cyan-400" />
                                  {t('quiz.questionCount', { count: quiz.questionCount })}
                                </span>
                                <span className="flex items-center gap-1.5 font-medium text-blue-600 dark:text-cyan-400">
                                  <Star size={14} />
                                  {quiz.xpReward} XP
                                </span>
                              </div>

                              {!isLocked && (
                                <div
                                  className={`mt-4 flex items-center justify-center gap-1.5 rounded-full border py-2 text-xs font-semibold transition-colors ${
                                    isCompleted
                                      ? 'border-emerald-400/40 text-emerald-600 dark:text-emerald-400'
                                      : 'border-blue-400/40 text-blue-600 dark:text-cyan-400'
                                  }`}
                                >
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
                              )}
                            </GlassCard>
                          );

                          return (
                            <motion.div key={quiz.id} variants={fadeInUp}>
                              {isLocked ? <div className="cursor-not-allowed">{card}</div> : <Link to={`/practice/${quiz.id}`}>{card}</Link>}
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
