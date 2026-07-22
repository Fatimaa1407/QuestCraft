import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import Editor from '@monaco-editor/react';
import { ArrowLeft, Play, Trophy, Crown, Clock, Users as UsersIcon, Hash, Copy, Check } from 'lucide-react';
import { getBattle, startBattle, cancelBattle, submitBattleSolution, joinBattle } from '../../api/battles';
import { getChallengeById } from '../../api/challenges';
import { useBattleHub } from '../../utils/useBattleHub';
import { useAuthStore } from '../../app/authStore';
import { useThemeStore } from '../../app/themeStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { Confetti } from '../../components/ui/Confetti';
import { FramedAvatar as Avatar } from '../../components/ui/FramedAvatar';
import { getApiErrorMessage } from '../../utils/apiError';
import { playErrorSound, playFanfareSound } from '../../utils/sounds';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';
import type { BattleParticipantDto } from '../../types/battle';

export function BattleRoomPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const battleId = Number(id);
  const currentUser = useAuthStore((s) => s.user);
  const theme = useThemeStore((s) => s.theme);
  const queryClient = useQueryClient();

  useBattleHub(Number.isFinite(battleId) ? battleId : null);

  const battleQuery = useQuery({
    queryKey: ['battle', battleId],
    queryFn: () => getBattle(battleId),
    enabled: Number.isFinite(battleId),
    refetchInterval: 8000,
  });

  const battle = battleQuery.data;

  const challengeQuery = useQuery({
    queryKey: ['challenge', battle?.challengeId],
    queryFn: () => getChallengeById(battle!.challengeId),
    enabled: !!battle,
  });

  const [code, setCode] = useState('');
  const [actionError, setActionError] = useState<string | null>(null);
  const [codeCopied, setCodeCopied] = useState(false);
  const seededRef = useRef(false);

  useEffect(() => {
    if (challengeQuery.data && !seededRef.current) {
      setCode(challengeQuery.data.starterCode);
      seededRef.current = true;
    }
  }, [challengeQuery.data]);

  const startMutation = useMutation({
    mutationFn: () => startBattle(battleId),
    onSuccess: (updated) => {
      if (updated) queryClient.setQueryData(['battle', battleId], updated);
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('battles.actionError'))),
  });

  const cancelMutation = useMutation({
    mutationFn: () => cancelBattle(battleId),
    onSuccess: () => navigate('/battles'),
    onError: (err) => setActionError(getApiErrorMessage(err, t('battles.actionError'))),
  });

  const joinMutation = useMutation({
    mutationFn: () => joinBattle(battleId),
    onSuccess: (updated) => {
      if (updated) queryClient.setQueryData(['battle', battleId], updated);
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('battles.actionError'))),
  });

  const submitMutation = useMutation({
    mutationFn: () => submitBattleSolution(battleId, code),
    onSuccess: (result) => {
      setActionError(null);
      if (!result) return;
      queryClient.setQueryData(['battle', battleId], result.battle);
      if (result.allPassed) {
        playFanfareSound();
      } else {
        playErrorSound();
      }
    },
    onError: (err) => setActionError(getApiErrorMessage(err, t('battles.actionError'))),
  });

  if (battleQuery.isLoading || !battle) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-1/3" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  const me = battle.participants.find((p) => p.userId === currentUser?.id);
  const isHost = battle.hostUserId === currentUser?.id;
  const isInvitedPending = battle.mode === 'Duel' && battle.status === 'Waiting' && battle.invitedUserId === currentUser?.id && !me;
  const iWon = battle.status === 'Finished' && me?.rank === 1;
  const submitResult = submitMutation.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      {iWon && <Confetti />}

      <motion.div variants={fadeInUp} className="flex items-center justify-between gap-3">
        <Link to="/battles" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-app-accent dark:text-slate-400 dark:hover:text-app-accent-2">
          <ArrowLeft size={15} />
          {t('battles.backToLobby')}
        </Link>
        {battle.joinCode && battle.status === 'Waiting' && (
          <button
            type="button"
            onClick={() => {
              navigator.clipboard.writeText(battle.joinCode!);
              setCodeCopied(true);
              setTimeout(() => setCodeCopied(false), 1500);
            }}
            className="flex items-center gap-2 rounded-full border border-slate-200/70 px-3 py-1.5 text-xs font-medium text-slate-600 dark:border-white/[0.08] dark:text-slate-300"
          >
            <Hash size={13} />
            {battle.joinCode}
            {codeCopied ? <Check size={13} className="text-emerald-500" /> : <Copy size={13} />}
          </button>
        )}
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard hoverLift={false} className="p-6">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{battle.challengeTitle}</h1>
              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                {t(`battles.mode.${battle.mode}`)} · {t(`battles.status.${battle.status}`)}
              </p>
            </div>
            {battle.status === 'Waiting' && isHost && battle.mode === 'Room' && (
              <div className="flex items-center gap-2">
                <motion.button
                  {...buttonTap}
                  disabled={battle.participants.length < 2 || startMutation.isPending}
                  onClick={() => startMutation.mutate()}
                  className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                >
                  <Play size={14} />
                  {t('battles.start')}
                </motion.button>
                <button
                  type="button"
                  onClick={() => cancelMutation.mutate()}
                  className="text-xs text-slate-500 hover:text-red-500 dark:text-slate-400"
                >
                  {t('battles.cancel')}
                </button>
              </div>
            )}
            {battle.status === 'Waiting' && battle.mode === 'Duel' && isInvitedPending && (
              <div className="flex items-center gap-2">
                <motion.button
                  {...buttonTap}
                  disabled={joinMutation.isPending}
                  onClick={() => joinMutation.mutate()}
                  className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                >
                  <Play size={14} />
                  {t('battles.acceptDuel')}
                </motion.button>
                <button
                  type="button"
                  onClick={() => navigate('/battles')}
                  className="text-xs text-slate-500 hover:text-red-500 dark:text-slate-400"
                >
                  {t('battles.decline')}
                </button>
              </div>
            )}
            {battle.status === 'Waiting' && battle.mode === 'Duel' && !isInvitedPending && (
              <span className="flex items-center gap-1.5 text-sm text-amber-500">
                <Clock size={14} />
                {t('battles.waitingForOpponent')}
              </span>
            )}
          </div>

          {actionError && <p className="mt-3 text-sm text-red-500">{actionError}</p>}
        </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-4 sm:grid-cols-[1fr_260px]">
        {/* Main area: waiting room / editor / results */}
        <div className="space-y-4">
          {battle.status === 'Waiting' && (
            <GlassCard hoverLift={false} className="p-6">
              <p className="mb-4 flex items-center gap-1.5 text-sm font-semibold text-slate-900 dark:text-slate-100">
                <UsersIcon size={15} />
                {t('battles.participants')} ({battle.participants.length}/{battle.maxPlayers})
              </p>
              <div className="space-y-2">
                {battle.participants.map((p) => (
                  <div key={p.userId} className="flex items-center gap-3 rounded-xl border border-slate-200/70 p-2.5 dark:border-white/[0.06]">
                    <Avatar username={p.username} avatarUrl={p.avatarUrl} frameImageUrl={p.frameImageUrl} size={32} />
                    <span className="min-w-0 flex-1">
                      <span className="flex items-center gap-1.5 text-sm font-medium text-slate-800 dark:text-slate-100">
                        {p.badgeImageUrl && <img src={p.badgeImageUrl} alt="" title={p.badgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />}
                        {p.username}
                        {p.userId === battle.hostUserId && (
                          <span className="text-xs font-normal text-app-accent dark:text-app-accent-2">({t('battles.host')})</span>
                        )}
                      </span>
                      {p.titleText && <span className="block truncate text-[10px] text-app-accent dark:text-app-accent-2">{p.titleText}</span>}
                    </span>
                  </div>
                ))}
              </div>
            </GlassCard>
          )}

          {battle.status === 'InProgress' && challengeQuery.isLoading && (
            <div className="space-y-4">
              <Skeleton className="h-24 w-full" />
              <Skeleton className="h-64 w-full" />
            </div>
          )}

          {battle.status === 'InProgress' && challengeQuery.data && (
            <>
              <GlassCard hoverLift={false} className="p-5">
                <p className="whitespace-pre-line text-sm text-slate-700 dark:text-slate-300">{challengeQuery.data.description}</p>
                {(challengeQuery.data.sampleInput || challengeQuery.data.sampleOutput) && (
                  <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
                    {challengeQuery.data.sampleInput && (
                      <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                        {challengeQuery.data.sampleInput}
                      </pre>
                    )}
                    {challengeQuery.data.sampleOutput && (
                      <pre className="overflow-x-auto rounded-lg bg-slate-100/80 p-2.5 text-xs text-slate-800 dark:bg-white/5 dark:text-slate-200">
                        {challengeQuery.data.sampleOutput}
                      </pre>
                    )}
                  </div>
                )}
              </GlassCard>

              <div className="overflow-hidden rounded-[20px] border border-slate-200/70 shadow-xl dark:border-white/[0.08]">
                <Editor
                  height="360px"
                  language="csharp"
                  theme={theme === 'dark' ? 'vs-dark' : 'light'}
                  value={code}
                  onChange={(value) => setCode(value ?? '')}
                  options={{ fontSize: 14, minimap: { enabled: false }, automaticLayout: true }}
                />
              </div>

              {submitResult && !submitResult.allPassed && (
                <GlassCard hoverLift={false} className="p-4">
                  <p className="text-sm text-red-500">
                    {t('battles.testsFailed', { passed: submitResult.passedTestCases, total: submitResult.totalTestCases })}
                  </p>
                  {submitResult.compileErrorMessage && (
                    <pre className="mt-2 overflow-x-auto rounded-lg bg-red-50 p-3 text-xs text-red-600 dark:bg-red-950/50 dark:text-red-400">
                      {submitResult.compileErrorMessage}
                    </pre>
                  )}
                </GlassCard>
              )}

              {!me?.hasFinished && (
                <motion.button
                  {...buttonTap}
                  disabled={submitMutation.isPending}
                  onClick={() => submitMutation.mutate()}
                  className="flex items-center gap-2 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2.5 text-sm font-medium text-white shadow-lg shadow-app-accent/25 disabled:opacity-50"
                >
                  <Play size={15} />
                  {submitMutation.isPending ? t('battles.submitting') : t('battles.submit')}
                </motion.button>
              )}
            </>
          )}

          {(battle.status === 'Finished' || battle.status === 'Cancelled') && (
            <GlassCard hoverLift={false} className="p-8 text-center">
              {battle.status === 'Cancelled' ? (
                <p className="text-slate-500 dark:text-slate-400">{t('battles.wasCancelled')}</p>
              ) : (
                <>
                  <p className="text-4xl">{iWon ? '🏆' : '🏁'}</p>
                  <h2 className="mt-2 text-xl font-bold text-slate-900 dark:text-white">
                    {iWon ? t('battles.youWon') : t('battles.battleOver')}
                  </h2>
                  <div className="mx-auto mt-5 max-w-sm space-y-2 text-left">
                    {battle.participants
                      .slice()
                      .sort((a, b) => (a.rank ?? 99) - (b.rank ?? 99))
                      .map((p) => (
                        <RankRow key={p.userId} participant={p} />
                      ))}
                  </div>
                </>
              )}
              <Link
                to="/battles"
                className="mt-6 inline-flex items-center gap-1.5 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2.5 text-sm font-medium text-white"
              >
                {t('battles.backToLobby')}
              </Link>
            </GlassCard>
          )}
        </div>

        {/* Live scoreboard sidebar */}
        {battle.status === 'InProgress' && (
          <GlassCard hoverLift={false} className="h-fit p-4">
            <p className="mb-3 flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-slate-400">
              <Trophy size={13} />
              {t('battles.liveScoreboard')}
            </p>
            <div className="space-y-2">
              {battle.participants.map((p) => (
                <div key={p.userId} className="rounded-xl border border-slate-200/70 p-2.5 dark:border-white/[0.06]">
                  <div className="flex items-center gap-2">
                    <Avatar username={p.username} avatarUrl={p.avatarUrl} frameImageUrl={p.frameImageUrl} size={24} />
                    {p.badgeImageUrl && <img src={p.badgeImageUrl} alt="" title={p.badgeName ?? undefined} className="h-3 w-3 shrink-0 rounded-full" />}
                    <span className="truncate text-xs font-medium text-slate-800 dark:text-slate-100">{p.username}</span>
                    {p.rank === 1 && <Crown size={13} className="ml-auto text-amber-500" />}
                  </div>
                  <div className="mt-1.5 h-1.5 w-full overflow-hidden rounded-full bg-slate-200/70 dark:bg-white/[0.06]">
                    <motion.div
                      className={`h-full rounded-full ${p.hasFinished ? 'bg-emerald-500' : 'bg-gradient-to-r from-app-accent to-app-accent-2'}`}
                      animate={{ width: p.totalTestCases > 0 ? `${(p.passedTestCases / p.totalTestCases) * 100}%` : '0%' }}
                      transition={{ duration: 0.4 }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </GlassCard>
        )}
      </motion.div>
    </motion.div>
  );
}

function RankRow({ participant }: { participant: BattleParticipantDto }) {
  const { t } = useTranslation();
  return (
    <div className="flex items-center gap-3 rounded-xl border border-slate-200/70 p-2.5 dark:border-white/[0.06]">
      <span className="w-5 text-center text-sm font-bold text-slate-400">{participant.rank ?? '-'}</span>
      <Avatar username={participant.username} avatarUrl={participant.avatarUrl} frameImageUrl={participant.frameImageUrl} size={28} />
      {participant.badgeImageUrl && <img src={participant.badgeImageUrl} alt="" title={participant.badgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />}
      <span className="flex-1 truncate text-sm font-medium text-slate-800 dark:text-slate-100">{participant.username}</span>
      <span className="text-xs text-slate-500 dark:text-slate-400">
        {participant.hasFinished
          ? t('battles.solved')
          : `${participant.passedTestCases}/${participant.totalTestCases}`}
      </span>
    </div>
  );
}
