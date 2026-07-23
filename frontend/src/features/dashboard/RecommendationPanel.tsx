import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Brain, ArrowRight } from 'lucide-react';
import { getRecommendations } from '../../api/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';

export function RecommendationPanel() {
  const { t } = useTranslation();
  const recommendationsQuery = useQuery({ queryKey: ['recommendations'], queryFn: getRecommendations });

  if (recommendationsQuery.isLoading) {
    return (
      <GlassCard hoverLift={false} className="p-6">
        <Skeleton className="h-5 w-1/2" />
        <div className="mt-4 space-y-2">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
        </div>
      </GlassCard>
    );
  }

  const data = recommendationsQuery.data;
  if (!data || !data.weakCategoryName || data.challenges.length === 0) {
    return null;
  }

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-3 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-violet-500/10 text-violet-600 dark:text-violet-400">
          <Brain size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('dashboard.recommendationTitle')}</h2>
      </div>
      <p className="mb-4 text-sm text-slate-600 dark:text-slate-300">
        {t('dashboard.recommendationBody', { category: data.weakCategoryName })}
      </p>
      <div className="space-y-2">
        {data.challenges.map((challenge) => (
          <Link
            key={challenge.id}
            to={`/challenges/${challenge.id}`}
            className="flex items-center justify-between rounded-xl border border-slate-200/70 px-4 py-2.5 text-sm transition-colors hover:border-violet-400 dark:border-white/[0.08] dark:hover:border-violet-500"
          >
            <span className="font-medium text-slate-800 dark:text-slate-100">{challenge.title}</span>
            <span className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
              {challenge.difficulty} · {challenge.xpReward} XP
              <ArrowRight size={13} />
            </span>
          </Link>
        ))}
      </div>
    </GlassCard>
  );
}
