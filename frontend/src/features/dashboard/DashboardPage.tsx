import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Zap, Sparkles, Coins as CoinsIcon, ShieldCheck, Gift, CheckCircle2, ListChecks, Lock, TrendingUp, CalendarDays } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { showToast } from '../../app/toastStore';
import {
  getDailyQuests,
  claimDailyQuest,
  getLevelProgress,
  getDashboardAnalytics,
  getMyStreak,
  getActivityHeatmap,
  getMyStatistics,
} from '../../api/gamification';
import { getDailyPuzzle } from '../../api/challenges';
import type { LevelProgress } from '../../types/gamification';
import { getMySubmissions } from '../../api/submissions';
import { getMyQuizAttempts } from '../../api/quizzes';
import { GlassCard } from '../../components/ui/GlassCard';
import { StatCard } from '../../components/ui/StatCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { LevelUpModal } from '../../components/ui/LevelUpModal';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';
import { playSuccessSound } from '../../utils/sounds';
import { XpTrendChart } from './XpTrendChart';
import { CategoryBreakdown } from './CategoryBreakdown';
import { StreakStat } from './StreakStat';
import { ActivityFeed } from './ActivityFeed';
import { RecommendationPanel } from './RecommendationPanel';
import { PersonalGoalsPanel } from './PersonalGoalsPanel';
import { DailyPuzzleCard } from './DailyPuzzleCard';
import { ContributionHeatmap } from './ContributionHeatmap';
import { AnalyticsStatsRow } from './AnalyticsStatsRow';

interface DashboardLevelUpInfo {
  previousLevel: number;
  newLevel: number;
  xpEarned: number;
  coinsEarned: number;
}

export function DashboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [levelUpInfo, setLevelUpInfo] = useState<DashboardLevelUpInfo | null>(null);

  const questsQuery = useQuery({ queryKey: ['daily-quests'], queryFn: getDailyQuests });
  const levelProgressQuery = useQuery({ queryKey: ['level-progress'], queryFn: getLevelProgress });
  const submissionsQuery = useQuery({
    queryKey: ['submissions', 'my', 1, 5],
    queryFn: () => getMySubmissions(1, 5),
  });
  const quizzesQuery = useQuery({
    queryKey: ['quizzes', 'attempts', 'my', 1, 5],
    queryFn: () => getMyQuizAttempts(1, 5),
  });
  const analyticsQuery = useQuery({ queryKey: ['dashboard-analytics'], queryFn: getDashboardAnalytics });
  const streakQuery = useQuery({ queryKey: ['streak', 'my'], queryFn: getMyStreak });
  const heatmapQuery = useQuery({ queryKey: ['gamification', 'heatmap', 180], queryFn: () => getActivityHeatmap(180) });
  const statisticsQuery = useQuery({ queryKey: ['gamification', 'statistics'], queryFn: getMyStatistics });
  const dailyPuzzleQuery = useQuery({ queryKey: ['daily-puzzle'], queryFn: getDailyPuzzle });

  // Fires once whenever the streak counter has grown since the last time this component saw it
  // (e.g. after solving a challenge/quiz elsewhere and coming back to the dashboard) — never on
  // first load, since previousStreakRef starts null.
  const previousStreakRef = useRef<number | null>(null);
  useEffect(() => {
    const current = streakQuery.data?.currentStreak;
    if (current === undefined) return;
    if (previousStreakRef.current !== null && current > previousStreakRef.current) {
      showToast({ title: t('dashboard.streakToastTitle'), message: t('dashboard.streakToastBody', { count: current }), emoji: '🔥' });
    }
    previousStreakRef.current = current;
  }, [streakQuery.data?.currentStreak, t]);

  const updateUser = useAuthStore((s) => s.updateUser);

  const claimMutation = useMutation({
    mutationFn: claimDailyQuest,
    onSuccess: (result) => {
      if (result) {
        const previousLevel = user?.level ?? 1;
        const previousCoins = user?.coins ?? 0;
        const previousXp = user?.xp ?? 0;
        updateUser({ xp: result.totalXp, coins: result.totalCoins, level: result.level });

        playSuccessSound();
        showToast({
          title: t('dashboard.questRewardToastTitle'),
          message: t('dashboard.questRewardToastBody', { xp: result.totalXp - previousXp, coins: result.totalCoins - previousCoins }),
          emoji: '🎁',
        });

        if (result.newAchievements.length > 0) {
          result.newAchievements.forEach((name) => {
            showToast({ title: t('dashboard.achievementUnlocked', { name }), emoji: '🏆' });
          });
        }

        if (result.level > previousLevel) {
          setLevelUpInfo({
            previousLevel,
            newLevel: result.level,
            xpEarned: result.totalXp - previousXp,
            coinsEarned: result.totalCoins - previousCoins,
          });
        }
      }
      queryClient.invalidateQueries({ queryKey: ['daily-quests'] });
      queryClient.invalidateQueries({ queryKey: ['achievements'] });
    },
  });

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-10">
      {levelUpInfo && (
        <LevelUpModal
          isOpen
          previousLevel={levelUpInfo.previousLevel}
          newLevel={levelUpInfo.newLevel}
          xpEarned={levelUpInfo.xpEarned}
          coinsEarned={levelUpInfo.coinsEarned}
          newChallengesUnlocked={0}
          newQuizzesUnlocked={0}
          onContinue={() => setLevelUpInfo(null)}
        />
      )}

      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">
          {t('dashboard.welcome', { name: user?.firstName ?? user?.username })}
        </h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('dashboard.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-5">
        <StatCard icon={ShieldCheck} label={t('dashboard.level')} value={user?.level ?? 1} tint="blue" />
        <StatCard icon={Zap} label={t('dashboard.xp')} value={user?.xp ?? 0} tint="cyan" />
        <StatCard icon={CoinsIcon} label={t('dashboard.coins')} value={user?.coins ?? 0} tint="amber" />
        <StatCard icon={Sparkles} label={t('dashboard.role')} value={user?.role ?? '-'} tint="violet" />
        <StreakStat currentStreak={streakQuery.data?.currentStreak ?? 0} longestStreak={streakQuery.data?.longestStreak ?? 0} />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <LevelProgressPanel data={levelProgressQuery.data} isLoading={levelProgressQuery.isLoading} />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <XpTrendChart data={analyticsQuery.data?.xpLast30Days} isLoading={analyticsQuery.isLoading} />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard hoverLift={false} glow className="p-6">
          <div className="mb-5 flex items-center justify-between gap-3">
            <div className="flex items-center gap-2.5">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
                <CalendarDays size={16} />
              </div>
              <div>
                <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.activityHeatmapTitle')}</h2>
                <p className="text-xs text-slate-400 dark:text-slate-500">{t('dashboard.heatmapSubtitle')}</p>
              </div>
            </div>
          </div>

          <AnalyticsStatsRow
            currentStreak={streakQuery.data?.currentStreak ?? 0}
            longestStreak={streakQuery.data?.longestStreak ?? 0}
            totalSolved={statisticsQuery.data?.challengesSolved ?? 0}
            totalCodingTimeMs={statisticsQuery.data?.totalCodingTimeMs ?? 0}
            dailyPuzzleSolved={dailyPuzzleQuery.data?.challengeId ? dailyPuzzleQuery.data.solvedToday : undefined}
          />

          <div className="mt-6">
            <ContributionHeatmap days={heatmapQuery.data} isLoading={heatmapQuery.isLoading} />
          </div>
        </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp}>
        <CategoryBreakdown data={analyticsQuery.data?.categoryProgress} isLoading={analyticsQuery.isLoading} />
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <DailyPuzzleCard />
        <RecommendationPanel />
        <PersonalGoalsPanel />
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Panel icon={ListChecks} title={t('dashboard.dailyQuests')}>
          {questsQuery.isLoading ? (
            <div className="space-y-3">
              <Skeleton className="h-16 w-full" />
              <Skeleton className="h-16 w-full" />
            </div>
          ) : !questsQuery.data || questsQuery.data.length === 0 ? (
            <EmptyRow text={t('dashboard.noQuests')} />
          ) : (
            <ul className="space-y-3">
              {questsQuery.data.map((quest) => (
                <li key={quest.id} className="rounded-2xl border border-slate-200/70 p-4 dark:border-white/[0.06]">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{quest.title}</p>
                      <p className="mt-0.5 text-xs text-slate-500 dark:text-slate-400">
                        {quest.currentProgress}/{quest.targetValue} · +{quest.xpReward} XP · +{quest.coinReward} 🪙
                      </p>
                    </div>
                    {quest.isCompleted && !quest.rewardClaimed ? (
                      <motion.button
                        {...buttonTap}
                        onClick={() => claimMutation.mutate(quest.id)}
                        disabled={claimMutation.isPending}
                        className="flex shrink-0 items-center gap-1 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-3 py-1 text-xs font-medium text-white shadow-sm shadow-app-accent/30 disabled:opacity-50"
                      >
                        <Gift size={13} />
                        {t('dashboard.claim')}
                      </motion.button>
                    ) : quest.rewardClaimed ? (
                      <span className="flex shrink-0 items-center gap-1 text-xs text-emerald-600 dark:text-emerald-400">
                        <CheckCircle2 size={13} />
                        {t('dashboard.claimed')}
                      </span>
                    ) : null}
                  </div>
                  <div className="mt-3 h-1.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
                    <motion.div
                      className="h-full rounded-full bg-gradient-to-r from-app-accent to-app-accent-2"
                      initial={{ width: 0 }}
                      animate={{ width: `${Math.min(100, (quest.currentProgress / Math.max(1, quest.targetValue)) * 100)}%` }}
                      transition={{ duration: 0.6, ease: 'easeOut' }}
                    />
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Panel>

        <ActivityFeed
          submissions={submissionsQuery.data?.items}
          quizzes={quizzesQuery.data?.items}
          isLoading={submissionsQuery.isLoading || quizzesQuery.isLoading}
        />
      </motion.div>
    </motion.div>
  );
}

function ProgressBar({ completed, total, colorClass }: { completed: number; total: number; colorClass: string }) {
  const percent = total > 0 ? Math.min(100, (completed / total) * 100) : 0;
  return (
    <div className="h-2 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
      <motion.div
        className={`h-full rounded-full ${colorClass}`}
        initial={{ width: 0 }}
        animate={{ width: `${percent}%` }}
        transition={{ duration: 0.6, ease: 'easeOut' }}
      />
    </div>
  );
}

function LevelProgressPanel({ data, isLoading }: { data: LevelProgress | null | undefined; isLoading: boolean }) {
  const { t } = useTranslation();

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
          <TrendingUp size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">
          {isLoading || !data ? t('dashboard.level') : t('dashboard.levelProgressTitle', { level: data.level })}
        </h2>
      </div>

      {isLoading || !data ? (
        <div className="space-y-4">
          <Skeleton className="h-2 w-full" />
          <Skeleton className="h-2 w-full" />
          <Skeleton className="h-2 w-full" />
        </div>
      ) : data.isMaxLevel ? (
        <p className="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
          <CheckCircle2 size={15} />
          {t('dashboard.levelProgressMax')}
        </p>
      ) : (
        <div className="space-y-4">
          <div>
            <div className="mb-1.5 flex items-center justify-between text-xs text-slate-500 dark:text-slate-400">
              <span>{t('dashboard.levelProgressChallenges')}</span>
              <span className="font-medium text-slate-700 dark:text-slate-200">
                {data.challengesCompleted}/{data.challengesTotal}
              </span>
            </div>
            <ProgressBar completed={data.challengesCompleted} total={data.challengesTotal} colorClass="bg-gradient-to-r from-app-accent to-app-accent-2" />
          </div>

          <div>
            <div className="mb-1.5 flex items-center justify-between text-xs text-slate-500 dark:text-slate-400">
              <span>{t('dashboard.levelProgressQuizzes')}</span>
              <span className="font-medium text-slate-700 dark:text-slate-200">
                {data.quizzesCompleted}/{data.quizzesTotal}
              </span>
            </div>
            <ProgressBar completed={data.quizzesCompleted} total={data.quizzesTotal} colorClass="bg-gradient-to-r from-violet-500 to-fuchsia-500" />
          </div>

          <div className="border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
            <div className="mb-1.5 flex items-center justify-between text-xs">
              <span className="font-medium text-slate-700 dark:text-slate-200">{t('dashboard.levelProgressOverall')}</span>
              <span className="font-semibold text-slate-900 dark:text-white">
                {data.overallCompleted}/{data.overallTotal}
              </span>
            </div>
            <ProgressBar completed={data.overallCompleted} total={data.overallTotal} colorClass="bg-gradient-to-r from-emerald-500 to-teal-500" />
            <p className="mt-2 flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
              <Lock size={12} />
              {t('dashboard.levelProgressUnlockHint', { level: data.level + 1, total: data.overallTotal })}
            </p>
          </div>
        </div>
      )}
    </GlassCard>
  );
}

function Panel({ icon: Icon, title, children }: { icon: typeof Zap; title: string; children: React.ReactNode }) {
  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
          <Icon size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
      </div>
      {children}
    </GlassCard>
  );
}

function EmptyRow({ text }: { text: string }) {
  return <p className="text-sm text-slate-500 dark:text-slate-500">{text}</p>;
}
