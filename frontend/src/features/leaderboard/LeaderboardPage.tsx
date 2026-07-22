import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Medal, Crown, Zap, Trophy } from 'lucide-react';
import { getLeaderboard, getMyRank } from '../../api/gamification';
import type { LeaderboardEntry, LeaderboardPeriod } from '../../types/gamification';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { FramedAvatar } from '../../components/ui/FramedAvatar';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const periods: LeaderboardPeriod[] = ['Daily', 'Weekly', 'Monthly', 'AllTime'];

const podiumStyle: Record<
  number,
  { icon: typeof Crown; ring: string; badge: string; wash: string; avatarSize: string; label: string }
> = {
  1: {
    icon: Crown,
    ring: 'ring-4 ring-amber-400',
    badge: 'bg-amber-400 text-amber-950',
    wash: 'bg-gradient-to-b from-amber-400/[0.12] to-transparent',
    avatarSize: 'h-24 w-24 text-2xl',
    label: 'text-amber-500 dark:text-amber-400',
  },
  2: {
    icon: Medal,
    ring: 'ring-4 ring-slate-300 dark:ring-slate-400',
    badge: 'bg-slate-300 text-slate-800',
    wash: 'bg-gradient-to-b from-slate-400/[0.1] to-transparent',
    avatarSize: 'h-20 w-20 text-xl',
    label: 'text-slate-400',
  },
  3: {
    icon: Medal,
    ring: 'ring-4 ring-orange-400',
    badge: 'bg-orange-400 text-orange-950',
    wash: 'bg-gradient-to-b from-orange-400/[0.12] to-transparent',
    avatarSize: 'h-20 w-20 text-xl',
    label: 'text-orange-500 dark:text-orange-400',
  },
};

export function LeaderboardPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const [period, setPeriod] = useState<LeaderboardPeriod>('AllTime');

  const leaderboardQuery = useQuery({
    queryKey: ['leaderboard', period],
    queryFn: () => getLeaderboard(period, 20),
  });

  const myRankQuery = useQuery({
    queryKey: ['gamification', 'my-rank', period],
    queryFn: () => getMyRank(period),
  });

  const entries = leaderboardQuery.data ?? [];
  const top3 = entries.slice(0, 3);
  const rest = entries.slice(3);
  const isMeInList = entries.some((e) => e.userId === user?.id);
  const myRank = myRankQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('leaderboard.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('leaderboard.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="flex flex-wrap items-center gap-2">
        {periods.map((p) => (
          <button
            key={p}
            onClick={() => setPeriod(p)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
              period === p
                ? 'bg-gradient-to-r from-app-accent to-app-accent-2 text-white shadow-sm shadow-app-accent/30'
                : 'border border-slate-200/70 text-slate-600 hover:border-app-accent dark:border-white/[0.08] dark:text-slate-300'
            }`}
          >
            {t(`leaderboard.period.${p}`)}
          </button>
        ))}
      </motion.div>

      {leaderboardQuery.isLoading ? (
        <div className="space-y-6">
          <div className="grid grid-cols-1 items-end gap-6 sm:grid-cols-3">
            {[1, 0, 2].map((order) => (
              <PodiumSkeleton key={order} big={order === 0} />
            ))}
          </div>
          <GlassCard hoverLift={false} className="divide-y divide-slate-200/70 p-4 dark:divide-white/[0.06]">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex items-center gap-4 px-3 py-3">
                <Skeleton className="h-4 w-6 rounded" />
                <Skeleton className="h-9 w-9 shrink-0 rounded-full" />
                <Skeleton className="h-4 flex-1" />
                <Skeleton className="h-4 w-10" />
                <Skeleton className="h-4 w-12" />
              </div>
            ))}
          </GlassCard>
        </div>
      ) : entries.length === 0 ? (
        <EmptyState icon={Trophy} tint="amber" title={t('leaderboard.empty')} />
      ) : (
        <>
          {top3.length > 0 && (
            <motion.div variants={staggerContainer} className="grid grid-cols-1 items-end gap-6 sm:grid-cols-3">
              {[top3[1], top3[0], top3[2]].map((entry, i) =>
                entry ? (
                  <PodiumCard key={entry.userId} entry={entry} isMe={entry.userId === user?.id} big={i === 1} order={i} />
                ) : (
                  <div key={i} />
                ),
              )}
            </motion.div>
          )}

          {rest.length > 0 && (
            <motion.div variants={fadeInUp}>
              <GlassCard hoverLift={false} className="divide-y divide-slate-200/70 p-4 dark:divide-white/[0.06]">
                {rest.map((entry, i) => (
                  <LeaderboardRow key={entry.userId} entry={entry} isMe={entry.userId === user?.id} index={i} />
                ))}
              </GlassCard>
            </motion.div>
          )}

          {!isMeInList && user && myRank && (
            <motion.div variants={fadeInUp}>
              <p className="mb-2 text-center text-xs text-slate-400 dark:text-slate-500">···</p>
              <GlassCard hoverLift={false} className="p-4">
                <LeaderboardRow
                  entry={{
                    rank: myRank.rank,
                    userId: user.id,
                    username: user.username,
                    avatarUrl: user.avatarUrl,
                    xp: myRank.xp,
                    level: myRank.level,
                    frameImageUrl: null,
                    titleText: null,
                    badgeImageUrl: null,
                    badgeName: null,
                  }}
                  isMe
                  index={0}
                />
                <p className="mt-1 text-center text-xs text-slate-400 dark:text-slate-500">
                  {t('leaderboard.outOfTotal', { total: myRank.totalUsers })}
                </p>
              </GlassCard>
            </motion.div>
          )}
        </>
      )}
    </motion.div>
  );
}

function PodiumCard({ entry, isMe, big, order }: { entry: LeaderboardEntry; isMe: boolean; big: boolean; order: number }) {
  const { t } = useTranslation();
  const style = podiumStyle[entry.rank] ?? podiumStyle[3];
  const Icon = style.icon;

  return (
    <motion.div
      initial={{ opacity: 0, y: 24, scale: 0.9 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      transition={{ delay: 0.15 + order * 0.12, type: 'spring', stiffness: 260, damping: 22 }}
      whileHover={{ y: -6 }}
    >
      <GlassCard
        hoverLift={false}
        className={`relative flex flex-col items-center overflow-hidden p-6 text-center ${style.wash} ${
          big ? 'sm:pb-10 sm:pt-9' : ''
        } ${isMe ? 'ring-2 ring-app-accent' : ''}`}
      >
        <span className={`text-xs font-bold uppercase tracking-widest ${style.label}`}>#{entry.rank}</span>

        <div className={`relative mt-3 flex items-center justify-center rounded-full font-bold text-white ${style.ring}`}>
          <FramedAvatar username={entry.username} avatarUrl={entry.avatarUrl} frameImageUrl={entry.frameImageUrl} size={entry.rank === 1 ? 96 : 80} />
          <span className={`absolute -top-2 -right-2 flex h-8 w-8 items-center justify-center rounded-full shadow-lg ${style.badge}`}>
            <Icon size={16} />
          </span>
        </div>

        <p className="mt-4 flex items-center justify-center gap-1 truncate text-base font-semibold text-slate-900 dark:text-slate-100">
          {entry.badgeImageUrl && <img src={entry.badgeImageUrl} alt="" title={entry.badgeName ?? undefined} className="h-4 w-4 shrink-0 rounded-full" />}
          {entry.username}
          {isMe && <span className="ml-1 text-xs font-normal text-app-accent dark:text-app-accent-2">({t('leaderboard.you')})</span>}
        </p>
        {entry.titleText && <p className="text-[11px] font-medium text-app-accent dark:text-app-accent-2">{entry.titleText}</p>}
        <p className="mt-0.5 text-xs text-slate-500 dark:text-slate-400">Lvl {entry.level}</p>
        <p className="mt-2 flex items-center gap-1 text-lg font-bold text-app-accent dark:text-app-accent-2">
          <Zap size={16} />
          {entry.xp} XP
        </p>
      </GlassCard>
    </motion.div>
  );
}

// Mirrors the podium card's layout (rank label, avatar circle, name, XP) so the page doesn't
// visually "jump" once real data replaces the skeleton.
function PodiumSkeleton({ big }: { big: boolean }) {
  return (
    <GlassCard hoverLift={false} className={`flex flex-col items-center p-6 ${big ? 'sm:pb-10 sm:pt-9' : ''}`}>
      <Skeleton className="h-3 w-8" />
      <Skeleton className={`mt-3 rounded-full ${big ? 'h-24 w-24' : 'h-20 w-20'}`} />
      <Skeleton className="mt-4 h-4 w-20" />
      <Skeleton className="mt-2 h-3 w-10" />
      <Skeleton className="mt-2 h-5 w-16" />
    </GlassCard>
  );
}

function LeaderboardRow({ entry, isMe, index }: { entry: LeaderboardEntry; isMe: boolean; index: number }) {
  const { t } = useTranslation();
  return (
    <motion.div
      initial={{ opacity: 0, x: -8 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: index * 0.03 }}
      className={`flex items-center gap-4 rounded-xl px-3 py-3 ${isMe ? 'bg-app-accent/10' : ''}`}
    >
      <span className="w-6 shrink-0 text-center text-sm font-semibold text-slate-400 dark:text-slate-500">
        {entry.rank}
      </span>
      <FramedAvatar username={entry.username} avatarUrl={entry.avatarUrl} frameImageUrl={entry.frameImageUrl} size={36} />
      <span className="min-w-0 flex-1">
        <span className="flex items-center gap-1 truncate text-sm font-medium text-slate-900 dark:text-slate-100">
          {entry.badgeImageUrl && <img src={entry.badgeImageUrl} alt="" title={entry.badgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />}
          {entry.username}
          {isMe && <span className="text-xs font-normal text-app-accent dark:text-app-accent-2">({t('leaderboard.you')})</span>}
        </span>
        {entry.titleText && <span className="block truncate text-[11px] text-app-accent dark:text-app-accent-2">{entry.titleText}</span>}
      </span>
      <span className="shrink-0 text-xs text-slate-500 dark:text-slate-400">Lvl {entry.level}</span>
      <span className="flex shrink-0 items-center gap-1 text-sm font-semibold text-app-accent dark:text-app-accent-2">
        <Zap size={13} />
        {entry.xp}
      </span>
    </motion.div>
  );
}
