import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { Target } from 'lucide-react';
import { getPersonalGoals } from '../../api/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Skeleton } from '../../components/ui/Skeleton';

function GoalBar({ label, current, target, colorClass }: { label: string; current: number; target: number; colorClass: string }) {
  const percent = target > 0 ? Math.min(100, (current / target) * 100) : 0;
  return (
    <div>
      <div className="mb-1.5 flex items-center justify-between text-xs text-slate-500 dark:text-slate-400">
        <span>{label}</span>
        <span className="font-medium text-slate-700 dark:text-slate-200">
          {current}/{target}
        </span>
      </div>
      <div className="h-2 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
        <motion.div
          className={`h-full rounded-full ${colorClass}`}
          initial={{ width: 0 }}
          animate={{ width: `${percent}%` }}
          transition={{ duration: 0.6, ease: 'easeOut' }}
        />
      </div>
    </div>
  );
}

export function PersonalGoalsPanel() {
  const { t } = useTranslation();
  const goalsQuery = useQuery({ queryKey: ['personal-goals'], queryFn: getPersonalGoals });

  const data = goalsQuery.data;
  const hasAnyGoal = data && (data.challengeGoal !== null || data.xpGoal !== null || data.battleGoal !== null);

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-5 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
          <Target size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.goalsTitle')}</h2>
      </div>

      {goalsQuery.isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-2 w-full" />
          <Skeleton className="h-2 w-full" />
        </div>
      ) : !hasAnyGoal ? (
        <EmptyState
          bare
          icon={Target}
          tint="blue"
          title={t('dashboard.goalsEmpty')}
          action={{ label: t('dashboard.goalsSetAction'), to: '/settings' }}
        />
      ) : (
        <div className="space-y-4">
          {data!.challengeGoal !== null && (
            <GoalBar
              label={t('dashboard.goalChallenges')}
              current={data!.challengesDoneToday}
              target={data!.challengeGoal}
              colorClass="bg-gradient-to-r from-app-accent to-app-accent-2"
            />
          )}
          {data!.xpGoal !== null && (
            <GoalBar
              label={t('dashboard.goalXp')}
              current={data!.xpToday}
              target={data!.xpGoal}
              colorClass="bg-gradient-to-r from-amber-500 to-orange-400"
            />
          )}
          {data!.battleGoal !== null && (
            <GoalBar
              label={t('dashboard.goalBattles')}
              current={data!.battlesToday}
              target={data!.battleGoal}
              colorClass="bg-gradient-to-r from-violet-500 to-fuchsia-500"
            />
          )}
          <Link to="/settings" className="inline-block text-xs font-medium text-app-accent hover:underline dark:text-app-accent-2">
            {t('dashboard.goalsEdit')}
          </Link>
        </div>
      )}
    </GlassCard>
  );
}
