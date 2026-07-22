import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { ArrowLeft, CheckCircle2, XCircle, Star, Trophy, Sparkles } from 'lucide-react';
import { getQuizForAttempt, submitQuizAttempt } from '../../api/quizzes';
import type { QuizAttemptResultDto } from '../../types/quiz';
import { useAuthStore } from '../../app/authStore';
import { showToast } from '../../app/toastStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Confetti } from '../../components/ui/Confetti';
import { FloatingXp } from '../../components/ui/FloatingXp';
import { QuizCompleteModal } from '../../components/ui/QuizCompleteModal';
import { LevelUpModal } from '../../components/ui/LevelUpModal';
import { Skeleton } from '../../components/ui/Skeleton';
import { getApiErrorMessage } from '../../utils/apiError';
import { playSuccessSound, playErrorSound, playFanfareSound } from '../../utils/sounds';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';

const REVEAL_STEP_MS = 450;

export function QuizAttemptPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const quizId = Number(id);
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const updateUser = useAuthStore((s) => s.updateUser);

  const quizQuery = useQuery({
    queryKey: ['quiz-attempt-view', quizId],
    queryFn: () => getQuizForAttempt(quizId),
    enabled: Number.isFinite(quizId),
  });

  const [answers, setAnswers] = useState<Record<number, number | null>>({});
  const [result, setResult] = useState<QuizAttemptResultDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [revealedIds, setRevealedIds] = useState<Set<number>>(new Set());
  const [showModal, setShowModal] = useState(false);
  const [celebrate, setCelebrate] = useState(false);
  const [showLevelUpModal, setShowLevelUpModal] = useState(false);
  const timeouts = useRef<ReturnType<typeof setTimeout>[]>([]);

  const submitMutation = useMutation({
    mutationFn: () =>
      submitQuizAttempt(
        quizId,
        Object.entries(answers).map(([questionId, selectedOptionId]) => ({
          questionId: Number(questionId),
          selectedOptionId,
        })),
      ),
    onSuccess: (data) => {
      setError(null);
      setResult(data);
      if (!data) return;
      updateUser({ xp: data.totalXp, coins: data.totalCoins, level: data.level });
      queryClient.invalidateQueries({ queryKey: ['quizzes', 'attempts', 'my'] });
      queryClient.invalidateQueries({ queryKey: ['quiz-attempt-view', quizId] });
      queryClient.invalidateQueries({ queryKey: ['level-progress'] });
      data.newAchievements.forEach((name) => {
        showToast({ title: t('dashboard.achievementUnlocked', { name }), emoji: '🏆' });
      });
    },
    onError: (err) => setError(getApiErrorMessage(err, t('quiz.actionError'))),
  });

  useEffect(() => {
    if (!result) return;

    result.questions.forEach((q, index) => {
      const timeoutId = setTimeout(() => {
        setRevealedIds((prev) => new Set(prev).add(q.questionId));
        if (q.isCorrect) {
          playSuccessSound();
        } else {
          playErrorSound();
        }
      }, index * REVEAL_STEP_MS);
      timeouts.current.push(timeoutId);
    });

    const revealDoneAt = result.questions.length * REVEAL_STEP_MS + 400;
    const modalTimeout = setTimeout(() => {
      setShowModal(true);
      setCelebrate(true);
      playFanfareSound();
    }, revealDoneAt);
    timeouts.current.push(modalTimeout);

    const celebrateEndTimeout = setTimeout(() => setCelebrate(false), revealDoneAt + 2000);
    timeouts.current.push(celebrateEndTimeout);

    return () => {
      timeouts.current.forEach(clearTimeout);
      timeouts.current = [];
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [result]);

  const optionTextById = useMemo(() => {
    const map = new Map<number, string>();
    quizQuery.data?.questions.forEach((q) => q.options.forEach((o) => map.set(o.id, o.text)));
    return map;
  }, [quizQuery.data]);

  if (quizQuery.isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-40 w-full" />
      </div>
    );
  }

  const quiz = quizQuery.data;
  if (!quiz) {
    return <p className="text-sm text-slate-500 dark:text-slate-500">{t('quiz.notFound')}</p>;
  }

  const answeredCount = Object.values(answers).filter((v) => v !== null && v !== undefined).length;
  const allAnswered = quiz.questions.every((q) => answers[q.id] !== undefined && answers[q.id] !== null);

  if (result) {
    const isPerfect = result.score === result.totalQuestions;

    return (
      <div className="space-y-6">
        {celebrate && isPerfect && <Confetti />}
        {celebrate && <FloatingXp xp={result.xpEarned} />}
        {showModal && (
          <QuizCompleteModal
            xp={result.xpEarned}
            score={result.score}
            totalQuestions={result.totalQuestions}
            onContinue={() => {
              setShowModal(false);
              if (result.level > result.previousLevel) {
                setShowLevelUpModal(true);
              }
            }}
          />
        )}

        {showLevelUpModal && (
          <LevelUpModal
            isOpen
            previousLevel={result.previousLevel}
            newLevel={result.level}
            xpEarned={result.xpEarned}
            coinsEarned={0}
            newChallengesUnlocked={result.newChallengesUnlocked}
            newQuizzesUnlocked={result.newQuizzesUnlocked}
            onContinue={() => {
              setShowLevelUpModal(false);
              navigate('/challenges');
            }}
          />
        )}

        <Link
          to="/practice"
          className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400"
        >
          <ArrowLeft size={15} />
          {t('quiz.backToList')}
        </Link>

        <GlassCard hoverLift={false} className="p-6">
          <div className="flex items-center justify-between gap-2">
            <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{quiz.title}</h1>
            <span className="text-lg font-bold text-blue-600 dark:text-cyan-400">
              {result.score}/{result.totalQuestions}
            </span>
          </div>

          {result.xpEarned > 0 && (
            <div className="mt-3 flex items-center gap-1 text-sm text-slate-600 dark:text-slate-300">
              <Star size={14} className="text-blue-500 dark:text-cyan-400" />+{result.xpEarned} XP
            </div>
          )}

          {result.newAchievements.length > 0 && (
            <div className="mt-2 space-y-1">
              {result.newAchievements.map((name) => (
                <p key={name} className="flex items-center gap-2 text-sm text-amber-600 dark:text-amber-400">
                  <Trophy size={14} />
                  {name}
                </p>
              ))}
            </div>
          )}
        </GlassCard>

        <div className="space-y-4">
          {result.questions.map((q) => {
            const isRevealed = revealedIds.has(q.questionId);
            return (
              <GlassCard
                key={q.questionId}
                hoverLift={false}
                className={`p-6 transition-opacity duration-200 ${isRevealed ? 'opacity-100' : 'opacity-40'} ${
                  isRevealed && !q.isCorrect ? 'animate-shake' : ''
                }`}
              >
                <div className="flex items-start justify-between gap-2">
                  <p className="font-medium text-slate-900 dark:text-slate-100">{q.text}</p>
                  {isRevealed &&
                    (q.isCorrect ? (
                      <CheckCircle2 size={18} className="animate-pop-in shrink-0 text-emerald-500" />
                    ) : (
                      <XCircle size={18} className="animate-pop-in shrink-0 text-red-500" />
                    ))}
                </div>

                {isRevealed && !q.isCorrect && (
                  <div className="mt-2 space-y-0.5 text-sm">
                    {q.selectedOptionId !== null && (
                      <p className="text-red-500 line-through decoration-red-400">
                        {optionTextById.get(q.selectedOptionId)}
                      </p>
                    )}
                    <p className="font-medium text-emerald-600 dark:text-emerald-400">
                      {optionTextById.get(q.correctOptionId)}
                    </p>
                  </div>
                )}

                {isRevealed && q.explanation && (
                  <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{q.explanation}</p>
                )}
              </GlassCard>
            );
          })}
        </div>
      </div>
    );
  }

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <Link
        to="/practice"
        className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400"
      >
        <ArrowLeft size={15} />
        {t('quiz.backToList')}
      </Link>

      {quiz.isAlreadyCompleted && (
        <motion.div
          variants={fadeInUp}
          className="flex items-center gap-2.5 rounded-2xl border border-violet-400/30 bg-violet-500/10 px-4 py-3 text-sm text-violet-600 dark:text-violet-400"
        >
          <Sparkles size={16} className="shrink-0" />
          <div>
            <span className="font-semibold">{t('quiz.practiceMode')}</span> — {t('quiz.practiceModeHint')}
          </div>
        </motion.div>
      )}

      <motion.div variants={fadeInUp}>
        <GlassCard hoverLift={false} className="p-6">
          <div className="flex items-center justify-between gap-2">
            <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{quiz.title}</h1>
            <span className="text-sm font-medium text-slate-500 dark:text-slate-400">
              {answeredCount}/{quiz.questions.length}
            </span>
          </div>
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
            {t('quiz.questionCount', { count: quiz.questions.length })} · {quiz.xpReward} XP
          </p>
          <div className="mt-3 h-1.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
            <motion.div
              className="h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-500"
              animate={{ width: `${(answeredCount / quiz.questions.length) * 100}%` }}
              transition={{ duration: 0.4, ease: 'easeOut' }}
            />
          </div>
        </GlassCard>
      </motion.div>

      <motion.div variants={staggerContainer} className="space-y-4">
        {quiz.questions.map((q, index) => (
          <motion.div key={q.id} variants={fadeInUp}>
            <GlassCard hoverLift={false} className="p-6">
              <p className="font-medium text-slate-900 dark:text-slate-100">
                {index + 1}. {q.text}
              </p>
              <div className="mt-4 space-y-2">
                {q.options.map((option) => (
                  <label
                    key={option.id}
                    className={`flex cursor-pointer items-center gap-3 rounded-2xl border px-4 py-2.5 text-sm transition-colors ${
                      answers[q.id] === option.id
                        ? 'border-blue-400 bg-blue-500/10 text-slate-900 dark:text-slate-100'
                        : 'border-slate-200/70 text-slate-700 hover:border-blue-300 dark:border-white/[0.08] dark:text-slate-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name={`question-${q.id}`}
                      checked={answers[q.id] === option.id}
                      onChange={() => setAnswers((prev) => ({ ...prev, [q.id]: option.id }))}
                      className="accent-blue-500"
                    />
                    {option.text}
                  </label>
                ))}
              </div>
            </GlassCard>
          </motion.div>
        ))}
      </motion.div>

      {error && (
        <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/50 dark:text-red-400">{error}</p>
      )}

      <motion.button
        {...buttonTap}
        onClick={() => submitMutation.mutate()}
        disabled={!allAnswered || submitMutation.isPending}
        className="flex items-center gap-2 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-5 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-500/25 disabled:opacity-50"
      >
        {submitMutation.isPending ? t('quiz.submitting') : t('quiz.submit')}
      </motion.button>
    </motion.div>
  );
}
