import { useEffect, useRef, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import Editor from '@monaco-editor/react';
import { ArrowLeft, Send, Timer } from 'lucide-react';
import { getChallengeById } from '../../api/challenges';
import { submitCode } from '../../api/submissions';
import type { SubmissionResultDto } from '../../types/submission';
import { useThemeStore } from '../../app/themeStore';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Confetti } from '../../components/ui/Confetti';
import { ChallengeCompleteModal } from '../../components/ui/ChallengeCompleteModal';
import { SuccessSweep } from '../../components/ui/SuccessSweep';
import { Skeleton } from '../../components/ui/Skeleton';
import { playFanfareSound } from '../../utils/sounds';
import { staggerContainer, fadeInUp, buttonTap } from '../../utils/motion';

const SPEED_LIMIT_MS = 5 * 60 * 1000;

function formatClock(ms: number) {
  const totalSeconds = Math.max(0, Math.ceil(ms / 1000));
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${minutes}:${String(seconds).padStart(2, '0')}`;
}

export function SpeedChallengePage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const challengeId = Number(id);
  const queryClient = useQueryClient();
  const theme = useThemeStore((s) => s.theme);
  const updateUser = useAuthStore((s) => s.updateUser);

  const challengeQuery = useQuery({
    queryKey: ['challenge', challengeId],
    queryFn: () => getChallengeById(challengeId),
    enabled: Number.isFinite(challengeId),
  });

  const [code, setCode] = useState('');
  const codeRef = useRef('');
  const [remainingMs, setRemainingMs] = useState(SPEED_LIMIT_MS);
  const [submitResult, setSubmitResult] = useState<SubmissionResultDto | null>(null);
  const [celebrate, setCelebrate] = useState(false);
  const [showCompleteModal, setShowCompleteModal] = useState(false);
  const deadlineRef = useRef<number>(Date.now() + SPEED_LIMIT_MS);
  const hasAutoSubmittedRef = useRef(false);
  const seededRef = useRef(false);

  useEffect(() => {
    if (challengeQuery.data && !seededRef.current) {
      setCode(challengeQuery.data.starterCode);
      codeRef.current = challengeQuery.data.starterCode;
      seededRef.current = true;
      deadlineRef.current = Date.now() + SPEED_LIMIT_MS;
    }
  }, [challengeQuery.data]);

  const submitMutation = useMutation({
    mutationFn: (source: string) => submitCode(challengeId, source, SPEED_LIMIT_MS - Math.max(0, deadlineRef.current - Date.now())),
    onSuccess: (data) => {
      setSubmitResult(data);
      if (!data) return;
      updateUser({ xp: data.totalXp, coins: data.totalCoins, level: data.level });
      queryClient.invalidateQueries({ queryKey: ['submissions', 'my'] });
      if (data.verdict === 'Accepted') {
        playFanfareSound();
        setCelebrate(true);
        setShowCompleteModal(true);
        setTimeout(() => setCelebrate(false), 2500);
      }
    },
  });

  useEffect(() => {
    if (!seededRef.current) return;
    const interval = setInterval(() => {
      const remaining = deadlineRef.current - Date.now();
      setRemainingMs(remaining);
      if (remaining <= 0 && !hasAutoSubmittedRef.current && !submitMutation.isPending) {
        hasAutoSubmittedRef.current = true;
        submitMutation.mutate(codeRef.current);
      }
    }, 1000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [challengeQuery.data]);

  const handleManualSubmit = () => {
    if (hasAutoSubmittedRef.current || submitMutation.isPending) return;
    hasAutoSubmittedRef.current = true;
    submitMutation.mutate(codeRef.current);
  };

  if (challengeQuery.isLoading || !challengeQuery.data) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  const challenge = challengeQuery.data;
  const isUrgent = remainingMs < 30_000;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      {celebrate && <Confetti />}
      {celebrate && <SuccessSweep />}

      {showCompleteModal && submitResult && (
        <ChallengeCompleteModal
          xp={submitResult.xpEarned}
          coins={submitResult.coinEarned}
          passedTestCases={submitResult.passedTestCases}
          totalTestCases={submitResult.totalTestCases}
          onContinue={() => setShowCompleteModal(false)}
        />
      )}

      <Link
        to={`/challenges/${challengeId}`}
        className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400"
      >
        <ArrowLeft size={15} />
        {t('challenges.backToChallenge')}
      </Link>

      <motion.div variants={fadeInUp} className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{t('challenges.speedModeTitle')}</h1>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{challenge.title}</p>
        </div>
        <div
          className={`flex items-center gap-2 rounded-full border px-4 py-2 text-lg font-bold tabular-nums ${
            isUrgent
              ? 'border-red-400/50 bg-red-500/10 text-red-600 dark:text-red-400'
              : 'border-slate-200/70 bg-white/70 text-slate-700 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-200'
          }`}
        >
          <Timer size={18} />
          {formatClock(remainingMs)}
        </div>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <GlassCard hoverLift={false} className="p-6">
          <p className="whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{challenge.description}</p>
          {(challenge.sampleInput || challenge.sampleOutput) && (
            <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2">
              {challenge.sampleInput && (
                <pre className="overflow-x-auto rounded-xl bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                  {challenge.sampleInput}
                </pre>
              )}
              {challenge.sampleOutput && (
                <pre className="overflow-x-auto rounded-xl bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                  {challenge.sampleOutput}
                </pre>
              )}
            </div>
          )}
        </GlassCard>

        <div className="flex flex-col gap-4">
          <div className="overflow-hidden rounded-[20px] border border-slate-200/70 shadow-xl dark:border-white/[0.08]">
            <Editor
              height="400px"
              language="csharp"
              theme={theme === 'dark' ? 'vs-dark' : 'light'}
              value={code}
              onChange={(value) => {
                setCode(value ?? '');
                codeRef.current = value ?? '';
              }}
              options={{ fontSize: 14, minimap: { enabled: false }, automaticLayout: true }}
            />
          </div>

          {submitResult && (
            <p className={`text-sm font-medium ${submitResult.verdict === 'Accepted' ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'}`}>
              {t(`challenges.verdict.${submitResult.verdict}`, submitResult.verdict)} — {submitResult.passedTestCases}/{submitResult.totalTestCases}
            </p>
          )}

          <motion.button
            {...buttonTap}
            onClick={handleManualSubmit}
            disabled={submitMutation.isPending || hasAutoSubmittedRef.current}
            className="flex items-center justify-center gap-2 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-500/25 disabled:opacity-50"
          >
            <Send size={15} />
            {submitMutation.isPending ? t('challenges.submitting') : t('challenges.submit')}
          </motion.button>
        </div>
      </motion.div>
    </motion.div>
  );
}
