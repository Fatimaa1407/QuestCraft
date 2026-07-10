import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import Editor from '@monaco-editor/react';
import { ArrowLeft, Play, Send, Lock, Star, Coins, CheckCircle2, XCircle, Trophy } from 'lucide-react';
import { getChallengeById, unlockHint } from '../../api/challenges';
import { runCode, submitCode } from '../../api/submissions';
import type { RunResultDto, SubmissionResultDto, SubmissionTestResultDto } from '../../types/submission';
import { useThemeStore } from '../../app/themeStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { GlassCard } from '../../components/ui/GlassCard';

const difficultyStyles: Record<string, string> = {
  Easy: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
  Medium: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  Hard: 'bg-red-500/10 text-red-600 dark:text-red-400',
};

export function ChallengeDetailPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const challengeId = Number(id);
  const queryClient = useQueryClient();
  const theme = useThemeStore((s) => s.theme);

  const challengeQuery = useQuery({
    queryKey: ['challenge', challengeId],
    queryFn: () => getChallengeById(challengeId),
    enabled: Number.isFinite(challengeId),
  });

  const [code, setCode] = useState('');
  const [runResult, setRunResult] = useState<RunResultDto | null>(null);
  const [submitResult, setSubmitResult] = useState<SubmissionResultDto | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    if (challengeQuery.data) {
      setCode(challengeQuery.data.starterCode);
    }
  }, [challengeQuery.data]);

  const runMutation = useMutation({
    mutationFn: () => runCode(challengeId, code),
    onSuccess: (data) => {
      setActionError(null);
      setSubmitResult(null);
      setRunResult(data);
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('challenges.actionError'))),
  });

  const submitMutation = useMutation({
    mutationFn: () => submitCode(challengeId, code),
    onSuccess: (data) => {
      setActionError(null);
      setRunResult(null);
      setSubmitResult(data);
      queryClient.invalidateQueries({ queryKey: ['submissions', 'my'] });
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('challenges.actionError'))),
  });

  const hintMutation = useMutation({
    mutationFn: () => unlockHint(challengeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['challenge', challengeId] });
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('challenges.actionError'))),
  });

  if (challengeQuery.isLoading) {
    return <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>;
  }

  const challenge = challengeQuery.data;
  if (!challenge) {
    return <p className="text-sm text-slate-500 dark:text-slate-500">{t('challenges.notFound')}</p>;
  }

  const isRunning = runMutation.isPending;
  const isSubmitting = submitMutation.isPending;

  return (
    <div className="space-y-4">
      <Link
        to="/challenges"
        className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400"
      >
        <ArrowLeft size={15} />
        {t('challenges.backToList')}
      </Link>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="space-y-4">
          <GlassCard className="p-6">
            <div className="flex items-start justify-between gap-2">
              <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{challenge.title}</h1>
              <span
                className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
                  difficultyStyles[challenge.difficulty] ?? 'bg-slate-500/10 text-slate-600 dark:text-slate-400'
                }`}
              >
                {challenge.difficulty}
              </span>
            </div>
            <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{challenge.category}</p>
            <div className="mt-3 flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
              <span className="flex items-center gap-1">
                <Star size={13} className="text-blue-500 dark:text-cyan-400" />
                {challenge.xpReward} XP
              </span>
              <span className="flex items-center gap-1">
                <Coins size={13} className="text-amber-500" />
                {challenge.coinReward}
              </span>
            </div>

            <p className="mt-4 whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{challenge.description}</p>

            {challenge.constraints && (
              <Section title={t('challenges.constraints')} content={challenge.constraints} />
            )}
            {challenge.inputFormat && <Section title={t('challenges.inputFormat')} content={challenge.inputFormat} />}
            {challenge.outputFormat && <Section title={t('challenges.outputFormat')} content={challenge.outputFormat} />}

            {(challenge.sampleInput || challenge.sampleOutput) && (
              <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2">
                {challenge.sampleInput && (
                  <div>
                    <p className="mb-1 text-xs font-medium text-slate-500 dark:text-slate-400">{t('challenges.sampleInput')}</p>
                    <pre className="overflow-x-auto rounded-xl bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                      {challenge.sampleInput}
                    </pre>
                  </div>
                )}
                {challenge.sampleOutput && (
                  <div>
                    <p className="mb-1 text-xs font-medium text-slate-500 dark:text-slate-400">{t('challenges.sampleOutput')}</p>
                    <pre className="overflow-x-auto rounded-xl bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                      {challenge.sampleOutput}
                    </pre>
                  </div>
                )}
              </div>
            )}

            {challenge.hasHint && (
              <div className="mt-4 rounded-2xl border border-slate-200/70 p-3.5 dark:border-white/[0.06]">
                {challenge.isHintUnlocked ? (
                  <p className="text-sm text-slate-700 dark:text-slate-300">{challenge.hint}</p>
                ) : (
                  <button
                    onClick={() => hintMutation.mutate()}
                    disabled={hintMutation.isPending}
                    className="flex items-center gap-2 text-sm font-medium text-blue-600 hover:underline disabled:opacity-50 dark:text-cyan-400"
                  >
                    <Lock size={14} />
                    {t('challenges.unlockHint')}
                  </button>
                )}
              </div>
            )}
          </GlassCard>

          {(runResult || submitResult) && (
            <ResultPanel runResult={runResult} submitResult={submitResult} />
          )}
        </div>

        <div className="flex flex-col gap-3">
          <div className="overflow-hidden rounded-[20px] border border-slate-200/70 dark:border-white/[0.08]">
            <Editor
              height="480px"
              language="csharp"
              theme={theme === 'dark' ? 'vs-dark' : 'light'}
              value={code}
              onChange={(value) => setCode(value ?? '')}
              options={{ fontSize: 14, minimap: { enabled: false }, automaticLayout: true }}
            />
          </div>

          {actionError && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/50 dark:text-red-400">
              {actionError}
            </p>
          )}

          <div className="flex items-center gap-3">
            <button
              onClick={() => runMutation.mutate()}
              disabled={isRunning || isSubmitting}
              className="flex items-center gap-2 rounded-full border border-slate-200/70 bg-white/70 px-4 py-2.5 text-sm font-medium text-slate-700 backdrop-blur-xl transition hover:border-blue-400 disabled:opacity-50 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-200 dark:hover:border-cyan-500"
            >
              <Play size={15} />
              {isRunning ? t('challenges.running') : t('challenges.run')}
            </button>
            <button
              onClick={() => submitMutation.mutate()}
              disabled={isRunning || isSubmitting}
              className="flex items-center gap-2 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-500/25 transition hover:brightness-110 disabled:opacity-50"
            >
              <Send size={15} />
              {isSubmitting ? t('challenges.submitting') : t('challenges.submit')}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function Section({ title, content }: { title: string; content: string }) {
  return (
    <div className="mt-4">
      <p className="mb-1 text-xs font-medium text-slate-500 dark:text-slate-400">{title}</p>
      <p className="whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{content}</p>
    </div>
  );
}

function VerdictBanner({ verdict, extra }: { verdict: string; extra?: React.ReactNode }) {
  const { t } = useTranslation();
  const isAccepted = verdict === 'Accepted';
  return (
    <div
      className={`flex items-center justify-between gap-2 rounded-2xl px-4 py-3 ${
        isAccepted
          ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
          : 'bg-red-500/10 text-red-600 dark:text-red-400'
      }`}
    >
      <span className="flex items-center gap-2 text-sm font-semibold">
        {isAccepted ? <CheckCircle2 size={16} /> : <XCircle size={16} />}
        {t(`challenges.verdict.${verdict}`, verdict)}
      </span>
      {extra}
    </div>
  );
}

function TestResultGroup({ title, results }: { title: string; results: SubmissionTestResultDto[] }) {
  const { t } = useTranslation();

  if (results.length === 0) return null;

  return (
    <div>
      <p className="mb-2 text-xs font-medium text-slate-500 dark:text-slate-400">{title}</p>
      <div className="space-y-2">
        {results.map((r, i) => (
          <div key={i} className="rounded-2xl border border-slate-200/70 p-3.5 text-sm dark:border-white/[0.06]">
            <div className="flex items-center justify-between">
              <span className="font-medium text-slate-700 dark:text-slate-200">
                {t('challenges.testCase')} {i + 1}
              </span>
              {r.passed ? (
                <CheckCircle2 size={16} className="text-emerald-500" />
              ) : (
                <XCircle size={16} className="text-red-500" />
              )}
            </div>
            {r.input !== null && (
              <div className="mt-2 grid grid-cols-1 gap-2 text-xs sm:grid-cols-3">
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.input')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.input}
                  </pre>
                </div>
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.expected')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.expectedOutput}
                  </pre>
                </div>
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.actual')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.actualOutput}
                  </pre>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

function ResultPanel({
  runResult,
  submitResult,
}: {
  runResult: RunResultDto | null;
  submitResult: SubmissionResultDto | null;
}) {
  const { t } = useTranslation();

  if (submitResult) {
    return (
      <div className="space-y-3 rounded-[20px] border border-slate-200/70 bg-white/70 p-5 backdrop-blur-xl dark:border-white/[0.08] dark:bg-app-card-dark/60">
        <VerdictBanner
          verdict={submitResult.verdict}
          extra={
            <span className="text-xs font-medium">
              {submitResult.passedTestCases}/{submitResult.totalTestCases}
            </span>
          }
        />

        {submitResult.verdict === 'Accepted' && (
          <div className="flex items-center gap-4 text-sm text-slate-600 dark:text-slate-300">
            <span className="flex items-center gap-1">
              <Star size={14} className="text-blue-500 dark:text-cyan-400" />+{submitResult.xpEarned} XP
            </span>
            <span className="flex items-center gap-1">
              <Coins size={14} className="text-amber-500" />+{submitResult.coinEarned}
            </span>
          </div>
        )}

        {submitResult.newAchievements.length > 0 && (
          <div className="space-y-1">
            {submitResult.newAchievements.map((name) => (
              <p key={name} className="flex items-center gap-2 text-sm text-amber-600 dark:text-amber-400">
                <Trophy size={14} />
                {name}
              </p>
            ))}
          </div>
        )}

        {submitResult.compileErrorMessage && (
          <pre className="overflow-x-auto rounded-lg bg-red-50 p-3 text-xs text-red-600 dark:bg-red-950/50 dark:text-red-400">
            {submitResult.compileErrorMessage}
          </pre>
        )}

        <div className="space-y-4">
          <TestResultGroup
            title={t('challenges.visibleTests')}
            results={submitResult.results.filter((r) => !r.isHidden)}
          />
          <TestResultGroup
            title={t('challenges.hiddenTests')}
            results={submitResult.results.filter((r) => r.isHidden)}
          />
        </div>
      </div>
    );
  }

  if (runResult) {
    return (
      <div className="space-y-3 rounded-[20px] border border-slate-200/70 bg-white/70 p-5 backdrop-blur-xl dark:border-white/[0.08] dark:bg-app-card-dark/60">
        <VerdictBanner verdict={runResult.verdict} />

        {runResult.compileErrorMessage && (
          <pre className="overflow-x-auto rounded-lg bg-red-50 p-3 text-xs text-red-600 dark:bg-red-950/50 dark:text-red-400">
            {runResult.compileErrorMessage}
          </pre>
        )}

        <div className="space-y-2">
          {runResult.results.map((r, i) => (
            <div key={i} className="rounded-2xl border border-slate-200/70 p-3.5 text-sm dark:border-white/[0.06]">
              <div className="mb-2 flex items-center justify-between">
                <span className="font-medium text-slate-700 dark:text-slate-200">
                  {t('challenges.testCase')} {i + 1}
                </span>
                {r.passed ? (
                  <CheckCircle2 size={16} className="text-emerald-500" />
                ) : (
                  <XCircle size={16} className="text-red-500" />
                )}
              </div>
              <div className="grid grid-cols-1 gap-2 text-xs sm:grid-cols-3">
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.input')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.input}
                  </pre>
                </div>
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.expected')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.expectedOutput}
                  </pre>
                </div>
                <div>
                  <p className="mb-1 text-slate-500 dark:text-slate-400">{t('challenges.actual')}</p>
                  <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2 text-slate-700 dark:bg-white/5 dark:text-slate-300">
                    {r.actualOutput}
                  </pre>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return null;
}
