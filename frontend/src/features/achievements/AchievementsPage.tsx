import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Trophy, Award, Lock, Star, Coins, Pin, PinOff } from 'lucide-react';
import { getAchievements, pinAchievement, unpinAchievement } from '../../api/gamification';
import { showToast } from '../../app/toastStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { QueryErrorState } from '../../components/ui/QueryErrorState';
import { getApiErrorMessage } from '../../utils/apiError';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function AchievementsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const achievementsQuery = useQuery({ queryKey: ['achievements'], queryFn: getAchievements });
  const achievements = achievementsQuery.data ?? [];
  const unlockedCount = achievements.filter((a) => a.isUnlocked).length;
  const pinnedCount = achievements.filter((a) => a.isPinned).length;
  const progressPct = achievements.length > 0 ? (unlockedCount / achievements.length) * 100 : 0;

  const pinMutation = useMutation({
    mutationFn: pinAchievement,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['achievements'] }),
    onError: (err) => showToast({ title: getApiErrorMessage(err, t('achievements.pinError')), emoji: '⚠️' }),
  });
  const unpinMutation = useMutation({
    mutationFn: unpinAchievement,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['achievements'] }),
    onError: (err) => showToast({ title: getApiErrorMessage(err, t('achievements.pinError')), emoji: '⚠️' }),
  });

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp} className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">
            {t('achievements.title')}
          </h1>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('achievements.subtitle')}</p>
        </div>

        {achievements.length > 0 && (
          <div className="flex items-center gap-3 rounded-2xl border border-amber-400/40 bg-amber-500/10 px-5 py-3">
            <Trophy size={20} className="text-amber-600 dark:text-amber-400" />
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-wide text-amber-600/80 dark:text-amber-400/80">
                {t('achievements.unlockedLabel')}
              </p>
              <p className="text-xl font-bold text-amber-600 dark:text-amber-400">
                {unlockedCount}/{achievements.length}
              </p>
            </div>
            <div className="ml-2 h-1.5 w-24 overflow-hidden rounded-full bg-amber-500/15">
              <motion.div
                className="h-full rounded-full bg-gradient-to-r from-amber-400 to-amber-500"
                animate={{ width: `${progressPct}%` }}
                transition={{ duration: 0.6, ease: 'easeOut' }}
              />
            </div>
          </div>
        )}
      </motion.div>

      {achievementsQuery.isLoading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <AchievementCardSkeleton key={i} />
          ))}
        </div>
      ) : achievementsQuery.isError ? (
        <QueryErrorState onRetry={() => achievementsQuery.refetch()} />
      ) : achievements.length === 0 ? (
        <EmptyState icon={Trophy} tint="violet" title={t('achievements.empty')} />
      ) : (
        <motion.div variants={staggerContainer} className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {achievements.map((achievement) => (
            <motion.div key={achievement.id} variants={fadeInUp}>
              <GlassCard
                glow={achievement.isUnlocked}
                style={
                  achievement.isUnlocked
                    ? { border: '1px solid rgba(245, 158, 11, 0.35)' }
                    : undefined
                }
                className={`flex h-full flex-col p-6 ${achievement.isUnlocked ? 'bg-amber-500/[0.03]' : 'opacity-70'}`}
              >
                <div className="flex items-start justify-between gap-2">
                  <div
                    className={`flex h-14 w-14 items-center justify-center rounded-2xl ${
                      achievement.isUnlocked
                        ? 'bg-gradient-to-br from-amber-400 to-amber-500 text-white shadow-lg shadow-amber-500/30'
                        : 'bg-slate-200/70 text-slate-400 dark:bg-white/5 dark:text-slate-600'
                    }`}
                  >
                    {achievement.isUnlocked ? <Award size={26} /> : <Lock size={22} />}
                  </div>
                  {achievement.isUnlocked && (
                    <span className="flex items-center gap-1 rounded-full bg-emerald-500/15 px-2.5 py-1 text-[11px] font-semibold text-emerald-600 dark:text-emerald-400">
                      {t('achievements.unlockedBadge')}
                    </span>
                  )}
                </div>

                <h2 className="mt-4 text-lg font-semibold text-slate-900 dark:text-slate-100">{achievement.name}</h2>
                <p className="mt-1 flex-1 text-sm text-slate-600 dark:text-slate-300">{achievement.description}</p>

                <div className="mt-5 flex items-center justify-between gap-2 border-t border-slate-200/70 pt-4 text-xs dark:border-white/[0.06]">
                  <div className="flex items-center gap-3 font-medium">
                    <span className="flex items-center gap-1 text-blue-600 dark:text-cyan-400">
                      <Star size={13} />
                      {achievement.xpReward} XP
                    </span>
                    <span className="flex items-center gap-1 text-amber-600 dark:text-amber-400">
                      <Coins size={13} />
                      {achievement.coinReward}
                    </span>
                  </div>
                  {achievement.isUnlocked && achievement.unlockedAt && (
                    <span className="text-slate-400 dark:text-slate-500">
                      {new Date(achievement.unlockedAt).toLocaleDateString()}
                    </span>
                  )}
                </div>

                {achievement.isUnlocked && (
                  <button
                    type="button"
                    onClick={() =>
                      achievement.isPinned ? unpinMutation.mutate(achievement.id) : pinMutation.mutate(achievement.id)
                    }
                    disabled={
                      (pinMutation.isPending && pinMutation.variables === achievement.id) ||
                      (unpinMutation.isPending && unpinMutation.variables === achievement.id) ||
                      (!achievement.isPinned && pinnedCount >= 3)
                    }
                    title={!achievement.isPinned && pinnedCount >= 3 ? t('achievements.pinLimitReached') : undefined}
                    className={`mt-3 flex items-center justify-center gap-1.5 rounded-full px-3 py-1.5 text-[11px] font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 ${
                      achievement.isPinned
                        ? 'bg-app-accent/10 text-app-accent dark:text-app-accent-2'
                        : 'border border-slate-200/70 text-slate-500 hover:border-app-accent hover:text-app-accent dark:border-white/[0.08] dark:text-slate-400'
                    }`}
                  >
                    {achievement.isPinned ? <PinOff size={12} /> : <Pin size={12} />}
                    {achievement.isPinned ? t('achievements.unpin') : t('achievements.pin')}
                  </button>
                )}
              </GlassCard>
            </motion.div>
          ))}
        </motion.div>
      )}
    </motion.div>
  );
}

// Mirrors the real achievement card's layout (icon square, name, description, XP/coin footer) so
// the page doesn't visually "jump" once real data replaces the skeleton.
function AchievementCardSkeleton() {
  return (
    <GlassCard hoverLift={false} className="flex h-full flex-col p-6">
      <Skeleton className="h-14 w-14 rounded-2xl" />
      <Skeleton className="mt-4 h-5 w-2/3" />
      <Skeleton className="mt-2 h-3 w-full" />
      <Skeleton className="mt-1.5 h-3 w-4/5" />
      <div className="mt-5 flex items-center gap-3 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
        <Skeleton className="h-3 w-12" />
        <Skeleton className="h-3 w-10" />
      </div>
    </GlassCard>
  );
}
