import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Volume2, VolumeX, Target } from 'lucide-react';
import { useSoundStore } from '../../app/soundStore';
import { getPersonalGoals, updatePersonalGoals } from '../../api/gamification';
import { showToast } from '../../app/toastStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { playSuccessSound } from '../../utils/sounds';

function toInputValue(value: number | null): string {
  return value === null ? '' : String(value);
}

function toGoalValue(text: string): number | null {
  const trimmed = text.trim();
  if (trimmed === '') return null;
  const parsed = Number(trimmed);
  return Number.isFinite(parsed) && parsed > 0 ? Math.round(parsed) : null;
}

export function SettingsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const soundEnabled = useSoundStore((s) => s.soundEnabled);
  const toggleSound = useSoundStore((s) => s.toggleSound);

  const handleToggleSound = () => {
    const wasEnabled = soundEnabled;
    toggleSound();
    // Play the confirmation chime only when turning sound ON, so muting stays silent immediately.
    if (!wasEnabled) playSuccessSound();
  };

  const goalsQuery = useQuery({ queryKey: ['personal-goals'], queryFn: getPersonalGoals });
  const [challengeGoal, setChallengeGoal] = useState('');
  const [xpGoal, setXpGoal] = useState('');
  const [battleGoal, setBattleGoal] = useState('');
  const [isDirty, setIsDirty] = useState(false);

  useEffect(() => {
    if (goalsQuery.data && !isDirty) {
      setChallengeGoal(toInputValue(goalsQuery.data.challengeGoal));
      setXpGoal(toInputValue(goalsQuery.data.xpGoal));
      setBattleGoal(toInputValue(goalsQuery.data.battleGoal));
    }
  }, [goalsQuery.data, isDirty]);

  const saveGoalsMutation = useMutation({
    mutationFn: () =>
      updatePersonalGoals({
        dailyGoalChallenges: toGoalValue(challengeGoal),
        dailyGoalXp: toGoalValue(xpGoal),
        dailyGoalBattles: toGoalValue(battleGoal),
      }),
    onSuccess: () => {
      setIsDirty(false);
      queryClient.invalidateQueries({ queryKey: ['personal-goals'] });
      showToast({ title: t('settings.goalsSaved'), emoji: '🎯' });
    },
    onError: (err) => showToast({ title: getApiErrorMessage(err, t('settings.goalsError')), emoji: '⚠️' }),
  });

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

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="mb-4 flex items-center gap-2.5">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
              <Target size={16} />
            </span>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
              {t('settings.goalsSection')}
            </h2>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <GoalInput
              label={t('dashboard.goalChallenges')}
              value={challengeGoal}
              onChange={(v) => {
                setChallengeGoal(v);
                setIsDirty(true);
              }}
            />
            <GoalInput
              label={t('dashboard.goalXp')}
              value={xpGoal}
              onChange={(v) => {
                setXpGoal(v);
                setIsDirty(true);
              }}
            />
            <GoalInput
              label={t('dashboard.goalBattles')}
              value={battleGoal}
              onChange={(v) => {
                setBattleGoal(v);
                setIsDirty(true);
              }}
            />
          </div>

          <button
            type="button"
            onClick={() => saveGoalsMutation.mutate()}
            disabled={!isDirty || saveGoalsMutation.isPending}
            className="mt-5 rounded-lg bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2 text-sm font-medium text-white shadow-lg shadow-app-accent/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {saveGoalsMutation.isPending ? t('profile.saving') : t('profile.save')}
          </button>
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}

function GoalInput({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-xs font-medium text-slate-500 dark:text-slate-400">{label}</span>
      <input
        type="number"
        min={1}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="—"
        className="w-full rounded-lg border border-slate-200/70 bg-white/70 px-3 py-2 text-sm text-slate-800 focus:border-app-accent focus:outline-none dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
      />
    </label>
  );
}
