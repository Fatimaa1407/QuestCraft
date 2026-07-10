import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { Zap, Sparkles, Coins as CoinsIcon, ShieldCheck, Gift, CheckCircle2, ListChecks, Swords, ClipboardList } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { getDailyQuests, claimDailyQuest } from '../../api/gamification';
import { getMySubmissions } from '../../api/submissions';
import { getMyQuizAttempts } from '../../api/quizzes';
import { VerdictBadge } from '../../components/ui/VerdictBadge';
import { GlassCard } from '../../components/ui/GlassCard';

export function DashboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  const questsQuery = useQuery({ queryKey: ['daily-quests'], queryFn: getDailyQuests });
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
      }
      queryClient.invalidateQueries({ queryKey: ['daily-quests'] });
    },
  });

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">
          {t('dashboard.welcome', { name: user?.firstName ?? user?.username })}
        </h1>
        <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">{t('dashboard.subtitle')}</p>
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard icon={ShieldCheck} label={t('dashboard.level')} value={user?.level ?? 0} tint="blue" />
        <StatCard icon={Zap} label={t('dashboard.xp')} value={user?.xp ?? 0} tint="cyan" />
        <StatCard icon={CoinsIcon} label={t('dashboard.coins')} value={user?.coins ?? 0} tint="amber" />
        <StatCard icon={Sparkles} label={t('dashboard.role')} value={user?.role ?? '-'} tint="violet" />
      </div>

      <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
        <Panel icon={ListChecks} title={t('dashboard.dailyQuests')}>
          {questsQuery.isLoading ? (
            <LoadingRow />
          ) : !questsQuery.data || questsQuery.data.length === 0 ? (
            <EmptyRow text={t('dashboard.noQuests')} />
          ) : (
            <ul className="space-y-3">
              {questsQuery.data.map((quest) => (
                <li key={quest.id} className="rounded-2xl border border-slate-200/70 p-3.5 dark:border-white/[0.06]">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{quest.title}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">
                        {quest.currentProgress}/{quest.targetValue} · +{quest.xpReward} XP · +{quest.coinReward} 🪙
                      </p>
                    </div>
                    {quest.isCompleted && !quest.rewardClaimed ? (
                      <button
                        onClick={() => claimMutation.mutate(quest.id)}
                        disabled={claimMutation.isPending}
                        className="flex shrink-0 items-center gap-1 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-3 py-1 text-xs font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-110 disabled:opacity-50"
                      >
                        <Gift size={13} />
                        {t('dashboard.claim')}
                      </button>
                    ) : quest.rewardClaimed ? (
                      <span className="flex shrink-0 items-center gap-1 text-xs text-emerald-600 dark:text-emerald-400">
                        <CheckCircle2 size={13} />
                        {t('dashboard.claimed')}
                      </span>
                    ) : null}
                  </div>
                  <div className="mt-2.5 h-1.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
                    <div
                      className="h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-500"
                      style={{ width: `${Math.min(100, (quest.currentProgress / Math.max(1, quest.targetValue)) * 100)}%` }}
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
                  className="flex items-center justify-between gap-2 rounded-2xl border border-slate-200/70 p-3.5 dark:border-white/[0.06]"
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
                  className="flex items-center justify-between gap-2 rounded-2xl border border-slate-200/70 p-3.5 dark:border-white/[0.06]"
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
      </div>
    </div>
  );
}

const tintStyles = {
  blue: 'bg-blue-500/10 text-blue-600 dark:text-blue-400',
  cyan: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-400',
  amber: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  violet: 'bg-violet-500/10 text-violet-600 dark:text-violet-400',
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
    <GlassCard className="p-5 transition hover:-translate-y-0.5 hover:shadow-xl">
      <div className={`mb-3 flex h-10 w-10 items-center justify-center rounded-xl ${tintStyles[tint]}`}>
        <Icon size={19} />
      </div>
      <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
      <p className="mt-0.5 text-2xl font-bold text-slate-900 dark:text-white">{value}</p>
    </GlassCard>
  );
}

function Panel({ icon: Icon, title, children }: { icon: typeof Zap; title: string; children: React.ReactNode }) {
  return (
    <GlassCard className="p-5">
      <div className="mb-4 flex items-center gap-2.5">
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
