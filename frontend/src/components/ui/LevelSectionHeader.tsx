import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { CheckCircle2, Unlock, Lock, ChevronDown } from 'lucide-react';

export type LevelSectionStatus = 'Completed' | 'Current' | 'Locked';

const levelSectionStyles: Record<LevelSectionStatus, { icon: typeof Lock; badge: string; border: string }> = {
  Completed: {
    icon: CheckCircle2,
    badge: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
    border: 'border-emerald-400/30',
  },
  Current: {
    icon: Unlock,
    badge: 'bg-blue-500/10 text-blue-600 dark:text-cyan-400',
    border: 'border-blue-400/40',
  },
  Locked: {
    icon: Lock,
    badge: 'bg-slate-500/10 text-slate-500 dark:text-slate-400',
    border: 'border-slate-200/70 dark:border-white/[0.06]',
  },
};

export function LevelSectionHeader({
  level,
  status,
  isExpanded,
  onToggle,
  i18nNamespace,
}: {
  level: number;
  status: LevelSectionStatus;
  isExpanded: boolean;
  onToggle: () => void;
  /** Which i18n key group to read "levelSection"/"levelSectionStatus" from — e.g. "challenges" or "quiz". */
  i18nNamespace: string;
}) {
  const { t } = useTranslation();
  const style = levelSectionStyles[status];
  const Icon = style.icon;

  return (
    <button
      onClick={onToggle}
      className={`flex w-full items-center justify-between gap-3 rounded-2xl border ${style.border} bg-white/60 px-5 py-3.5 backdrop-blur-xl transition-colors hover:bg-white/80 dark:bg-white/5 dark:hover:bg-white/[0.08]`}
    >
      <div className="flex items-center gap-3">
        <span className="text-sm font-semibold text-slate-900 dark:text-slate-100">
          {t(`${i18nNamespace}.levelSection`, { level })}
        </span>
        <span className={`flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-semibold ${style.badge}`}>
          <Icon size={12} />
          {t(`${i18nNamespace}.levelSectionStatus.${status}`)}
        </span>
      </div>
      <motion.div animate={{ rotate: isExpanded ? 180 : 0 }} transition={{ duration: 0.2 }}>
        <ChevronDown size={18} className="text-slate-400" />
      </motion.div>
    </button>
  );
}
