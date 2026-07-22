import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Volume2, VolumeX } from 'lucide-react';
import { useSoundStore } from '../../app/soundStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { playSuccessSound } from '../../utils/sounds';

export function SettingsPage() {
  const { t } = useTranslation();
  const soundEnabled = useSoundStore((s) => s.soundEnabled);
  const toggleSound = useSoundStore((s) => s.toggleSound);

  const handleToggleSound = () => {
    const wasEnabled = soundEnabled;
    toggleSound();
    // Play the confirmation chime only when turning sound ON, so muting stays silent immediately.
    if (!wasEnabled) playSuccessSound();
  };

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('settings.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('settings.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
            {t('settings.audioSection')}
          </h2>

          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-app-accent/10 text-app-accent dark:text-app-accent-2">
                {soundEnabled ? <Volume2 size={18} /> : <VolumeX size={18} />}
              </span>
              <div>
                <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{t('settings.soundEffects')}</p>
                <p className="text-xs text-slate-500 dark:text-slate-400">{t('settings.soundEffectsHint')}</p>
              </div>
            </div>

            <button
              type="button"
              role="switch"
              aria-checked={soundEnabled}
              aria-label={t('settings.soundEffects')}
              onClick={handleToggleSound}
              className={`relative h-7 w-12 shrink-0 rounded-full transition-colors ${
                soundEnabled ? 'bg-gradient-to-r from-app-accent to-app-accent-2' : 'bg-slate-300 dark:bg-white/10'
              }`}
            >
              <motion.span
                className="absolute top-1 h-5 w-5 rounded-full bg-white shadow"
                animate={{ left: soundEnabled ? 26 : 4 }}
                transition={{ type: 'spring', stiffness: 500, damping: 30 }}
              />
            </button>
          </div>
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}
