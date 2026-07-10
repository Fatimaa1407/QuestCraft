import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Medal, Crown, Zap } from 'lucide-react';
import { getLeaderboard } from '../../api/gamification';
import type { LeaderboardEntry, LeaderboardPeriod } from '../../types/gamification';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';

const periods: LeaderboardPeriod[] = ['Daily', 'Weekly', 'Monthly', 'AllTime'];

const podiumStyle: Record<number, { icon: typeof Crown; ring: string; badge: string }> = {
  1: { icon: Crown, ring: 'ring-2 ring-amber-400', badge: 'bg-amber-400 text-amber-950' },
  2: { icon: Medal, ring: 'ring-2 ring-slate-300', badge: 'bg-slate-300 text-slate-800' },
  3: { icon: Medal, ring: 'ring-2 ring-orange-400', badge: 'bg-orange-400 text-orange-950' },
};

export function LeaderboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const [period, setPeriod] = useState<LeaderboardPeriod>('AllTime');

  const leaderboardQuery = useQuery({
    queryKey: ['leaderboard', period],
    queryFn: () => getLeaderboard(period, 20),
  });

  const entries = leaderboardQuery.data ?? [];
  const top3 = entries.slice(0, 3);
  const rest = entries.slice(3);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t('leaderboard.title')}</h1>
        <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">{t('leaderboard.subtitle')}</p>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        {periods.map((p) => (
          <button
            key={p}
            onClick={() => setPeriod(p)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
              period === p
                ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30'
                : 'border border-slate-200/70 text-slate-600 hover:border-blue-400 dark:border-white/[0.08] dark:text-slate-300'
            }`}
          >
            {t(`leaderboard.period.${p}`)}
          </button>
        ))}
      </div>

      {leaderboardQuery.isLoading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : entries.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('leaderboard.empty')}</p>
      ) : (
        <>
          {top3.length > 0 && (
            <div className="grid grid-cols-1 items-end gap-4 sm:grid-cols-3">
              {[top3[1], top3[0], top3[2]].map((entry, i) =>
                entry ? (
                  <PodiumCard key={entry.userId} entry={entry} isMe={entry.userId === user?.id} big={i === 1} />
                ) : (
                  <div key={i} />
                ),
              )}
            </div>
          )}

          {rest.length > 0 && (
            <GlassCard className="divide-y divide-slate-200/70 p-2 dark:divide-white/[0.06]">
              {rest.map((entry) => (
                <LeaderboardRow key={entry.userId} entry={entry} isMe={entry.userId === user?.id} />
              ))}
            </GlassCard>
          )}
        </>
      )}
    </div>
  );
}

function PodiumCard({ entry, isMe, big }: { entry: LeaderboardEntry; isMe: boolean; big: boolean }) {
  const { t } = useTranslation();
  const style = podiumStyle[entry.rank] ?? podiumStyle[3];
  const Icon = style.icon;

  return (
    <GlassCard className={`flex flex-col items-center p-5 text-center ${big ? 'sm:pb-8 sm:pt-7' : ''} ${isMe ? 'ring-2 ring-blue-400' : ''}`}>
      <div className={`relative flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-xl font-bold text-white ${style.ring}`}>
        {entry.avatarUrl ? (
          <img src={entry.avatarUrl} alt="" className="h-full w-full rounded-full object-cover" />
        ) : (
          entry.username.charAt(0).toUpperCase()
        )}
        <span className={`absolute -top-2 -right-2 flex h-7 w-7 items-center justify-center rounded-full ${style.badge}`}>
          <Icon size={14} />
        </span>
      </div>
      <p className="mt-3 truncate text-sm font-semibold text-slate-900 dark:text-slate-100">
        {entry.username}
        {isMe && <span className="ml-1 text-xs font-normal text-blue-600 dark:text-cyan-400">({t('leaderboard.you')})</span>}
      </p>
      <p className="mt-0.5 text-xs text-slate-500 dark:text-slate-400">Lvl {entry.level}</p>
      <p className="mt-1 flex items-center gap-1 text-sm font-bold text-blue-600 dark:text-cyan-400">
        <Zap size={13} />
        {entry.xp} XP
      </p>
    </GlassCard>
  );
}

function LeaderboardRow({ entry, isMe }: { entry: LeaderboardEntry; isMe: boolean }) {
  const { t } = useTranslation();
  return (
    <div
      className={`flex items-center gap-4 rounded-xl px-3 py-2.5 ${isMe ? 'bg-blue-500/10' : ''}`}
    >
      <span className="w-6 shrink-0 text-center text-sm font-semibold text-slate-400 dark:text-slate-500">
        {entry.rank}
      </span>
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-sm font-semibold text-white">
        {entry.avatarUrl ? (
          <img src={entry.avatarUrl} alt="" className="h-full w-full rounded-full object-cover" />
        ) : (
          entry.username.charAt(0).toUpperCase()
        )}
      </div>
      <span className="flex-1 truncate text-sm font-medium text-slate-900 dark:text-slate-100">
        {entry.username}
        {isMe && <span className="ml-1 text-xs font-normal text-blue-600 dark:text-cyan-400">({t('leaderboard.you')})</span>}
      </span>
      <span className="shrink-0 text-xs text-slate-500 dark:text-slate-400">Lvl {entry.level}</span>
      <span className="flex shrink-0 items-center gap-1 text-sm font-semibold text-blue-600 dark:text-cyan-400">
        <Zap size={13} />
        {entry.xp}
      </span>
    </div>
  );
}
