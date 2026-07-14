import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Trophy, Award, Lock, Star, Coins } from 'lucide-react';
import { getAchievements } from '../../api/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function AchievementsPage() {
  const { t } = useTranslation();

  const achievementsQuery = useQuery({ queryKey: ['achievements'], queryFn: getAchievements });
  const achievements = achievementsQuery.data ?? [];
  const unlockedCount = achievements.filter((a) => a.isUnlocked).length;
  const progressPct = achievements.length > 0 ? (unlockedCount / achievements.length) * 100 : 0;

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
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : achievements.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('achievements.empty')}</p>
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
              </GlassCard>
            </motion.div>
          ))}
        </motion.div>
      )}
    </motion.div>
  );
}
