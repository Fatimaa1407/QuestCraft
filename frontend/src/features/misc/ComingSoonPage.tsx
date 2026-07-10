import { useTranslation } from 'react-i18next';
import { Sparkles } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';

export function ComingSoonPage({ titleKey }: { titleKey: string }) {
  const { t } = useTranslation();

  return (
    <GlassCard className="flex flex-col items-center justify-center gap-3 py-24 text-center">
      <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-blue-500/10 text-blue-600 dark:text-cyan-400">
        <Sparkles size={22} />
      </div>
      <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{t(titleKey)}</h1>
      <p className="text-sm text-slate-500 dark:text-slate-400">{t('common.comingSoon')}</p>
    </GlassCard>
  );
}
