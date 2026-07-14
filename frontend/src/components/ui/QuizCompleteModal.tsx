import { useTranslation } from 'react-i18next';
import { GlassCard } from './GlassCard';
import { Z_INDEX } from '../../styles/zIndex';

export function QuizCompleteModal({
  xp,
  score,
  totalQuestions,
  onContinue,
}: {
  xp: number;
  score: number;
  totalQuestions: number;
  onContinue: () => void;
}) {
  const { t } = useTranslation();
  const ratio = totalQuestions > 0 ? score / totalQuestions : 0;
  const stars = ratio === 1 ? 3 : ratio >= 0.66 ? 2 : ratio > 0 ? 1 : 0;
  const isPerfect = stars === 3;

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-slate-950/50 backdrop-blur-sm" style={{ zIndex: Z_INDEX.modal }}>
      <GlassCard className="animate-modal-in mx-4 w-full max-w-sm p-8 text-center">
        <p className="text-4xl">🎉</p>
        <h2 className="mt-2 text-xl font-bold text-slate-900 dark:text-white">{t('quiz.completed')}</h2>

        <p className="mt-4 flex items-center justify-center gap-1.5 text-2xl font-bold text-blue-600 dark:text-cyan-400">
          ⭐ +{xp} XP
        </p>

        <p className="mt-4 text-2xl tracking-widest">
          {Array.from({ length: 3 }, (_, i) => (
            <span key={i} className={i < stars ? 'text-amber-400' : 'text-slate-300 dark:text-slate-700'}>
              ★
            </span>
          ))}
        </p>

        {isPerfect && (
          <p className="mt-1 text-sm font-medium text-amber-500">{t('quiz.perfectScore')}</p>
        )}

        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          {score}/{totalQuestions}
        </p>

        <button
          onClick={onContinue}
          className="mt-6 w-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-5 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-500/25 transition hover:brightness-110"
        >
          {t('quiz.continue')}
        </button>
      </GlassCard>
    </div>
  );
}
