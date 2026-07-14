import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Zap, Sparkles, Coins as CoinsIcon, ShieldCheck, Gift, CheckCircle2, ListChecks, Swords, ClipboardList, Trophy, Lock, TrendingUp } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { getDailyQuests, claimDailyQuest, getLevelProgress } from '../../api/gamification';
import type { LevelProgress } from '../../types/gamification';
import { getMySubmissions } from '../../api/submissions';
import { getMyQuizAttempts } from '../../api/quizzes';
import { VerdictBadge } from '../../components/ui/VerdictBadge';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';
import { playSuccessSound } from '../../utils/sounds';

export function DashboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [newAchievements, setNewAchievements] = useState<string[]>([]);

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

  const updateUser = useAuthStore((s) => s.updateUser);

  const claimMutation = useMutation({
    mutationFn: claimDailyQuest,
    onSuccess: (result) => {
      if (result) {
        updateUser({ xp: result.totalXp, coins: result.totalCoins, level: result.level });
        if (result.newAchievements.length > 0) {
          playSuccessSound();
          setNewAchievements(result.newAchievements);
          setTimeout(() => setNewAchievements([]), 4000);
        }
      }
      queryClient.invalidateQueries({ queryKey: ['daily-quests'] });
      queryClient.invalidateQueries({ queryKey: ['achievements'] });
    },
  });

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-10">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">
          {t('dashboard.welcome', { name: user?.firstName ?? user?.username })}
        </h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('dashboard.subtitle')}</p>
      </motion.div>

      {newAchievements.length > 0 && (
        <motion.div
          initial={{ opacity: 0, y: -8 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col gap-1.5 rounded-2xl border border-amber-400/40 bg-amber-500/10 px-4 py-3"
        >
          {newAchievements.map((name) => (
            <p key={name} className="flex items-center gap-2 text-sm font-medium text-amber-600 dark:text-amber-400">
              <Trophy size={15} />
              {t('dashboard.achievementUnlocked', { name })}
            </p>
          ))}
        </motion.div>
      )}

      <motion.div variants={fadeInUp} className="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <StatCard icon={ShieldCheck} label={t('dashboard.level')} value={user?.level ?? 1} tint="blue" />
        <StatCard icon={Zap} label={t('dashboard.xp')} value={user?.xp ?? 0} tint="cyan" />
        <StatCard icon={CoinsIcon} label={t('dashboard.coins')} value={user?.coins ?? 0} tint="amber" />
        <StatCard icon={Sparkles} label={t('dashboard.role')} value={user?.role ?? '-'} tint="violet" />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <LevelProgressPanel data={levelProgressQuery.data} isLoading={levelProgressQuery.isLoading} />
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Panel icon={ListChecks} title={t('dashboard.dailyQuests')}>
          {questsQuery.isLoading ? (
            <LoadingRow />
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
                        className="flex shrink-0 items-center gap-1 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-3 py-1 text-xs font-medium text-white shadow-sm shadow-blue-500/30 disabled:opacity-50"
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
                      className="h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-500"
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

        <Panel icon={Swords} title={t('dashboard.recentSubmissions')}>
          {submissionsQuery.isLoading ? (
            <LoadingRow />
          ) : !submissionsQuery.data || submissionsQuery.data.items.length === 0 ? (
            <EmptyRow text={t('dashboard.noSubmissions')} />
          ) : (
            <ul className="space-y-2.5">
              {submissionsQuery.data.items.map((s) => (
                <li
                  key={s.id}
                  className="flex items-center justify-between gap-2 rounded-2xl border border-slate-200/70 p-4 dark:border-white/[0.06]"
                >
                  <span className="truncate text-sm text-slate-900 dark:text-slate-100">{s.challengeTitle}</span>
                  <VerdictBadge verdict={s.verdict} />
                </li>
              ))}
            </ul>
          )}
        </Panel>

        <Panel icon={ClipboardList} title={t('dashboard.recentQuizzes')}>
          {quizzesQuery.isLoading ? (
            <LoadingRow />
          ) : !quizzesQuery.data || quizzesQuery.data.items.length === 0 ? (
            <EmptyRow text={t('dashboard.noQuizzes')} />
          ) : (
            <ul className="space-y-2.5">
              {quizzesQuery.data.items.map((q) => (
                <li
                  key={q.id}
                  className="flex items-center justify-between gap-2 rounded-2xl border border-slate-200/70 p-4 dark:border-white/[0.06]"
                >
                  <span className="truncate text-sm text-slate-900 dark:text-slate-100">{q.quizTitle}</span>
                  <span className="shrink-0 text-xs font-semibold text-blue-600 dark:text-cyan-400">
                    {q.score}/{q.totalQuestions}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </Panel>
      </motion.div>
    </motion.div>
  );
}

const tintStyles = {
  blue: 'bg-blue-500/10 text-blue-600 shadow-blue-500/20 dark:text-blue-400',
  cyan: 'bg-cyan-500/10 text-cyan-600 shadow-cyan-500/20 dark:text-cyan-400',
  amber: 'bg-amber-500/10 text-amber-600 shadow-amber-500/20 dark:text-amber-400',
  violet: 'bg-violet-500/10 text-violet-600 shadow-violet-500/20 dark:text-violet-400',
} as const;

function StatCard({
  icon: Icon,
  label,
  value,
  tint,
}: {
  icon: typeof Zap;
  label: string;
  value: string | number;
  tint: keyof typeof tintStyles;
}) {
  return (
    <GlassCard className="p-6">
      <div className={`mb-4 flex h-11 w-11 items-center justify-center rounded-xl shadow-lg ${tintStyles[tint]}`}>
        <Icon size={20} />
      </div>
      <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
      <p className="mt-1 text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{value}</p>
    </GlassCard>
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
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <TrendingUp size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">
          {isLoading || !data ? t('dashboard.level') : t('dashboard.levelProgressTitle', { level: data.level })}
        </h2>
      </div>

      {isLoading || !data ? (
        <LoadingRow />
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
            <ProgressBar completed={data.challengesCompleted} total={data.challengesTotal} colorClass="bg-gradient-to-r from-blue-500 to-cyan-500" />
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
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600 dark:text-cyan-400">
          <Icon size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
      </div>
      {children}
    </GlassCard>
  );
}

function LoadingRow() {
  return <p className="text-sm text-slate-400 dark:text-slate-500">…</p>;
}

function EmptyRow({ text }: { text: string }) {
  return <p className="text-sm text-slate-500 dark:text-slate-500">{text}</p>;
}
