import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Swords, Users, DoorOpen, Hash, Plus, ArrowRight } from 'lucide-react';
import { getMyBattles, getOpenRooms, createDuelBattle, createRoomBattle, getBattleByCode, joinBattle } from '../../api/battles';
import { getFriends } from '../../api/friends';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { getApiErrorMessage } from '../../utils/apiError';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';
import type { BattleSummaryDto } from '../../types/battle';

const statusStyles: Record<string, string> = {
  Waiting: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
  InProgress: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
  Finished: 'bg-slate-500/10 text-slate-500 dark:text-slate-400',
  Cancelled: 'bg-red-500/10 text-red-600 dark:text-red-400',
};

export function BattleLobbyPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showCreateDuel, setShowCreateDuel] = useState(false);
  const [showCreateRoom, setShowCreateRoom] = useState(false);
  const [joinCode, setJoinCode] = useState('');
  const [joinError, setJoinError] = useState<string | null>(null);

  const myBattlesQuery = useQuery({ queryKey: ['battles', 'mine'], queryFn: getMyBattles, refetchInterval: 10000 });
  const openRoomsQuery = useQuery({ queryKey: ['battles', 'open-rooms'], queryFn: getOpenRooms, refetchInterval: 10000 });

  const joinRoomMutation = useMutation({
    mutationFn: async (room: BattleSummaryDto) => joinBattle(room.id),
    onSuccess: (battle) => {
      queryClient.invalidateQueries({ queryKey: ['battles'] });
      if (battle) navigate(`/battles/${battle.id}`);
    },
  });

  const joinByCodeMutation = useMutation({
    mutationFn: async () => {
      const battle = await getBattleByCode(joinCode.trim());
      if (!battle) throw new Error('not found');
      const alreadyIn = battle.participants.some((p) => p.userId === battle.hostUserId);
      return alreadyIn ? battle : await joinBattle(battle.id);
    },
    onSuccess: (battle) => {
      setJoinError(null);
      if (battle) navigate(`/battles/${battle.id}`);
    },
    onError: (err) => setJoinError(getApiErrorMessage(err, t('battles.joinCodeError'))),
  });

  const myBattles = myBattlesQuery.data ?? [];
  const openRooms = openRoomsQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp} className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('battles.title')}</h1>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('battles.subtitle')}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <motion.button
            {...buttonTap}
            onClick={() => setShowCreateDuel(true)}
            className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-2 text-sm font-medium text-white shadow-sm shadow-blue-500/30"
          >
            <Swords size={15} />
            {t('battles.newDuel')}
          </motion.button>
          <motion.button
            {...buttonTap}
            onClick={() => setShowCreateRoom(true)}
            className="flex items-center gap-1.5 rounded-full border border-slate-200/70 px-4 py-2 text-sm font-medium text-slate-700 dark:border-white/[0.08] dark:text-slate-200"
          >
            <Users size={15} />
            {t('battles.newRoom')}
          </motion.button>
        </div>
      </motion.div>

      {showCreateDuel && <CreateDuelPanel onClose={() => setShowCreateDuel(false)} onCreated={(id) => navigate(`/battles/${id}`)} />}
      {showCreateRoom && <CreateRoomPanel onClose={() => setShowCreateRoom(false)} onCreated={(id) => navigate(`/battles/${id}`)} />}

      <motion.div variants={fadeInUp} className="max-w-sm">
        <GlassCard hoverLift={false} className="p-4">
          <p className="mb-2 flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-slate-400">
            <Hash size={13} />
            {t('battles.joinByCode')}
          </p>
          <div className="flex gap-2">
            <input
              type="text"
              value={joinCode}
              onChange={(e) => setJoinCode(e.target.value.toUpperCase())}
              placeholder={t('battles.codePlaceholder')}
              maxLength={6}
              className="w-full rounded-full border border-slate-200/70 bg-white/80 px-4 py-1.5 text-sm uppercase tracking-widest text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
            />
            <button
              type="button"
              disabled={joinCode.trim().length < 4 || joinByCodeMutation.isPending}
              onClick={() => joinByCodeMutation.mutate()}
              className="shrink-0 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-1.5 text-sm font-medium text-white disabled:opacity-50"
            >
              {t('battles.join')}
            </button>
          </div>
          {joinError && <p className="mt-2 text-xs text-red-500">{joinError}</p>}
        </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp}>
        <h2 className="mb-4 text-lg font-semibold text-slate-900 dark:text-slate-100">{t('battles.myBattles')}</h2>
        {myBattlesQuery.isLoading ? (
          <Skeleton className="h-16 w-full" />
        ) : myBattles.length === 0 ? (
          <EmptyState icon={Swords} tint="blue" title={t('battles.noneActive')} />
        ) : (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {myBattles.map((b) => (
              <BattleSummaryCard key={b.id} battle={b} onClick={() => navigate(`/battles/${b.id}`)} />
            ))}
          </div>
        )}
      </motion.div>

      <motion.div variants={fadeInUp}>
        <h2 className="mb-4 text-lg font-semibold text-slate-900 dark:text-slate-100">{t('battles.openRooms')}</h2>
        {openRoomsQuery.isLoading ? (
          <Skeleton className="h-16 w-full" />
        ) : openRooms.length === 0 ? (
          <EmptyState icon={DoorOpen} tint="violet" title={t('battles.noOpenRooms')} />
        ) : (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {openRooms.map((room) => (
              <GlassCard key={room.id} hoverLift={false} className="flex items-center justify-between gap-3 p-4">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{room.challengeTitle}</p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    {room.playerCount}/{room.maxPlayers} {t('battles.players')}
                  </p>
                </div>
                <motion.button
                  {...buttonTap}
                  disabled={joinRoomMutation.isPending || room.playerCount >= room.maxPlayers}
                  onClick={() => joinRoomMutation.mutate(room)}
                  className="flex shrink-0 items-center gap-1 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white disabled:opacity-50"
                >
                  {t('battles.join')}
                  <ArrowRight size={12} />
                </motion.button>
              </GlassCard>
            ))}
          </div>
        )}
      </motion.div>
    </motion.div>
  );
}

function BattleSummaryCard({ battle, onClick }: { battle: BattleSummaryDto; onClick: () => void }) {
  const { t } = useTranslation();
  return (
    <motion.div variants={fadeInUp}>
      <GlassCard hoverLift={false} className="cursor-pointer p-4" onClick={onClick}>
        <div className="flex items-center justify-between gap-2">
          <span className={`rounded-full px-2 py-0.5 text-[11px] font-medium ${statusStyles[battle.status]}`}>{t(`battles.status.${battle.status}`)}</span>
          <span className="text-xs text-slate-400">{battle.mode === 'Duel' ? '⚔️' : '🏟️'} {t(`battles.mode.${battle.mode}`)}</span>
        </div>
        <p className="mt-2 truncate text-sm font-medium text-slate-900 dark:text-slate-100">{battle.challengeTitle}</p>
        <p className="mt-0.5 text-xs text-slate-500 dark:text-slate-400">
          {battle.playerCount}/{battle.maxPlayers} {t('battles.players')}
        </p>
      </GlassCard>
    </motion.div>
  );
}

function CreateDuelPanel({ onClose, onCreated }: { onClose: () => void; onCreated: (id: number) => void }) {
  const { t } = useTranslation();
  const [friendId, setFriendId] = useState<number | null>(null);

  const friendsQuery = useQuery({ queryKey: ['friends', 'list'], queryFn: getFriends });

  const createMutation = useMutation({
    mutationFn: () => createDuelBattle(friendId!),
    onSuccess: (battle) => {
      if (battle) onCreated(battle.id);
    },
  });

  return (
    <motion.div initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: 'auto' }} exit={{ opacity: 0, height: 0 }}>
      <GlassCard hoverLift={false} className="space-y-3 p-6">
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('battles.newDuel')}</h2>
        {friendsQuery.data?.length === 0 ? (
          <p className="text-sm text-slate-500 dark:text-slate-400">{t('battles.noFriendsHint')}</p>
        ) : (
          <>
            <select
              value={friendId ?? ''}
              onChange={(e) => setFriendId(e.target.value ? Number(e.target.value) : null)}
              className="w-full rounded-full border border-slate-200/70 bg-white/80 px-4 py-2 text-sm text-slate-900 outline-none dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
            >
              <option value="">{t('battles.selectFriend')}</option>
              {friendsQuery.data?.map((f) => (
                <option key={f.userId} value={f.userId}>
                  {f.username}
                </option>
              ))}
            </select>
            <div className="flex items-center gap-2">
              <motion.button
                {...buttonTap}
                disabled={!friendId || createMutation.isPending}
                onClick={() => createMutation.mutate()}
                className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              >
                <Plus size={14} />
                {t('battles.sendInvite')}
              </motion.button>
              <button type="button" onClick={onClose} className="text-sm text-slate-500 dark:text-slate-400">
                {t('common.cancel')}
              </button>
            </div>
          </>
        )}
      </GlassCard>
    </motion.div>
  );
}

function CreateRoomPanel({ onClose, onCreated }: { onClose: () => void; onCreated: (id: number) => void }) {
  const { t } = useTranslation();
  const [maxPlayers, setMaxPlayers] = useState(4);

  const createMutation = useMutation({
    mutationFn: () => createRoomBattle(maxPlayers),
    onSuccess: (battle) => {
      if (battle) onCreated(battle.id);
    },
  });

  return (
    <motion.div initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: 'auto' }} exit={{ opacity: 0, height: 0 }}>
      <GlassCard hoverLift={false} className="space-y-3 p-6">
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('battles.newRoom')}</h2>
        <div className="flex items-center gap-3">
          <label className="text-sm text-slate-600 dark:text-slate-300">{t('battles.maxPlayers')}</label>
          <input
            type="number"
            min={2}
            max={10}
            value={maxPlayers}
            onChange={(e) => setMaxPlayers(Number(e.target.value))}
            className="w-20 rounded-full border border-slate-200/70 bg-white/80 px-3 py-1.5 text-sm text-slate-900 outline-none dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
          />
        </div>
        <div className="flex items-center gap-2">
          <motion.button
            {...buttonTap}
            disabled={createMutation.isPending}
            onClick={() => createMutation.mutate()}
            className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            <Plus size={14} />
            {t('battles.createRoom')}
          </motion.button>
          <button type="button" onClick={onClose} className="text-sm text-slate-500 dark:text-slate-400">
            {t('common.cancel')}
          </button>
        </div>
      </GlassCard>
    </motion.div>
  );
}
