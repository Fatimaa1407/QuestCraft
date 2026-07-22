import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { Search, UserPlus, UserCheck, UserX, UserMinus, Users, Clock, MessageCircle } from 'lucide-react';
import { getFriends, getIncomingFriendRequests, searchUsers, sendFriendRequest, respondFriendRequest, removeFriend } from '../../api/friends';
import type { FriendDto } from '../../types/friends';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { FramedAvatar as Avatar } from '../../components/ui/FramedAvatar';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';

export function FriendsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');

  const friendsQuery = useQuery({ queryKey: ['friends', 'list'], queryFn: getFriends });
  const requestsQuery = useQuery({ queryKey: ['friends', 'requests'], queryFn: getIncomingFriendRequests });
  const searchQuery = useQuery({
    queryKey: ['friends', 'search', search],
    queryFn: () => searchUsers(search),
    enabled: search.trim().length >= 2,
  });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ['friends'] });
  };

  const sendMutation = useMutation({ mutationFn: sendFriendRequest, onSuccess: invalidateAll });
  const respondMutation = useMutation({
    mutationFn: ({ id, accept }: { id: number; accept: boolean }) => respondFriendRequest(id, accept),
    onSuccess: invalidateAll,
  });
  const removeMutation = useMutation({ mutationFn: removeFriend, onSuccess: invalidateAll });

  const friends = friendsQuery.data ?? [];
  const requests = requestsQuery.data ?? [];
  const results = searchQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('friends.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('friends.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="relative max-w-md">
        <Search size={16} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('friends.searchPlaceholder')}
          className="w-full rounded-full border border-slate-200/70 bg-white/80 py-2.5 pl-10 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
      </motion.div>

      {search.trim().length >= 2 && (
        <motion.div variants={fadeInUp}>
          <GlassCard hoverLift={false} className="p-6">
            <h2 className="mb-4 text-sm font-semibold text-slate-900 dark:text-slate-100">{t('friends.searchResults')}</h2>
            {searchQuery.isLoading ? (
              <div className="space-y-3">
                {Array.from({ length: 2 }).map((_, i) => (
                  <Skeleton key={i} className="h-12 w-full" />
                ))}
              </div>
            ) : results.length === 0 ? (
              <p className="text-sm text-slate-500 dark:text-slate-400">{t('friends.noResults')}</p>
            ) : (
              <ul className="space-y-2.5">
                {results.map((r) => (
                  <li key={r.userId} className="flex items-center gap-3 rounded-2xl border border-slate-200/70 p-3 dark:border-white/[0.06]">
                    <Avatar username={r.username} avatarUrl={r.avatarUrl} frameImageUrl={r.frameImageUrl} size={36} />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{r.username}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">Lvl {r.level}</p>
                    </div>
                    {r.friendStatus === 'None' && (
                      <motion.button
                        {...buttonTap}
                        onClick={() => sendMutation.mutate(r.userId)}
                        disabled={sendMutation.isPending}
                        className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white shadow-sm shadow-blue-500/30 disabled:opacity-50"
                      >
                        <UserPlus size={13} />
                        {t('friends.add')}
                      </motion.button>
                    )}
                    {r.friendStatus === 'PendingSent' && (
                      <span className="flex items-center gap-1.5 text-xs font-medium text-slate-400">
                        <Clock size={13} />
                        {t('friends.pendingSent')}
                      </span>
                    )}
                    {r.friendStatus === 'PendingReceived' && (
                      <span className="flex items-center gap-1.5 text-xs font-medium text-amber-500">
                        <Clock size={13} />
                        {t('friends.pendingReceived')}
                      </span>
                    )}
                    {r.friendStatus === 'Friends' && (
                      <span className="flex items-center gap-1.5 text-xs font-medium text-emerald-600 dark:text-emerald-400">
                        <UserCheck size={13} />
                        {t('friends.alreadyFriends')}
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </GlassCard>
        </motion.div>
      )}

      {(requestsQuery.isLoading || requests.length > 0) && (
        <motion.div variants={fadeInUp}>
          <GlassCard hoverLift={false} className="p-6">
            <h2 className="mb-4 text-sm font-semibold text-slate-900 dark:text-slate-100">
              {t('friends.incomingRequests')} {requests.length > 0 && `(${requests.length})`}
            </h2>
            {requestsQuery.isLoading ? (
              <Skeleton className="h-12 w-full" />
            ) : (
              <ul className="space-y-2.5">
                {requests.map((req) => (
                  <li key={req.id} className="flex items-center gap-3 rounded-2xl border border-blue-400/30 bg-blue-500/[0.04] p-3">
                    <Avatar username={req.requesterUsername} avatarUrl={req.requesterAvatarUrl} frameImageUrl={req.requesterFrameImageUrl} size={36} />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{req.requesterUsername}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">Lvl {req.requesterLevel}</p>
                    </div>
                    <motion.button
                      {...buttonTap}
                      onClick={() => respondMutation.mutate({ id: req.id, accept: true })}
                      disabled={respondMutation.isPending}
                      className="flex items-center gap-1 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500 px-3 py-1.5 text-xs font-medium text-white shadow-sm disabled:opacity-50"
                    >
                      <UserCheck size={13} />
                      {t('friends.accept')}
                    </motion.button>
                    <motion.button
                      {...buttonTap}
                      onClick={() => respondMutation.mutate({ id: req.id, accept: false })}
                      disabled={respondMutation.isPending}
                      className="flex items-center gap-1 rounded-full border border-slate-200/70 px-3 py-1.5 text-xs font-medium text-slate-500 disabled:opacity-50 dark:border-white/[0.08] dark:text-slate-400"
                    >
                      <UserX size={13} />
                      {t('friends.decline')}
                    </motion.button>
                  </li>
                ))}
              </ul>
            )}
          </GlassCard>
        </motion.div>
      )}

      <motion.div variants={fadeInUp}>
        <h2 className="mb-4 text-lg font-semibold text-slate-900 dark:text-slate-100">
          {t('friends.myFriends')} {friends.length > 0 && `(${friends.length})`}
        </h2>

        {friendsQuery.isLoading ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <GlassCard key={i} hoverLift={false} className="flex items-center gap-3 p-4">
                <Skeleton className="h-11 w-11 rounded-full" />
                <div className="flex-1 space-y-1.5">
                  <Skeleton className="h-4 w-2/3" />
                  <Skeleton className="h-3 w-1/3" />
                </div>
              </GlassCard>
            ))}
          </div>
        ) : friends.length === 0 ? (
          <EmptyState icon={Users} tint="blue" title={t('friends.empty')} description={t('friends.emptyHint')} />
        ) : (
          <motion.div variants={staggerContainer} className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {friends.map((friend) => (
              <FriendCard key={friend.userId} friend={friend} onRemove={() => removeMutation.mutate(friend.userId)} />
            ))}
          </motion.div>
        )}
      </motion.div>
    </motion.div>
  );
}

function FriendCard({ friend, onRemove }: { friend: FriendDto; onRemove: () => void }) {
  const { t } = useTranslation();
  return (
    <motion.div variants={fadeInUp}>
      <GlassCard hoverLift={false} className="flex items-center gap-3 p-4">
        <Avatar username={friend.username} avatarUrl={friend.avatarUrl} frameImageUrl={friend.frameImageUrl} size={44} />
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold text-slate-900 dark:text-slate-100">{friend.username}</p>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            Lvl {friend.level} · {friend.xp} XP
          </p>
        </div>
        <div className="flex shrink-0 items-center gap-1.5">
          <Link
            to={`/chat/${friend.userId}`}
            title={t('friends.message')}
            className="flex h-8 w-8 items-center justify-center rounded-full text-slate-400 transition hover:bg-blue-500/10 hover:text-blue-600 dark:hover:text-cyan-400"
          >
            <MessageCircle size={15} />
          </Link>
          <button
            type="button"
            title={t('friends.remove')}
            onClick={onRemove}
            className="flex h-8 w-8 items-center justify-center rounded-full text-slate-400 transition hover:bg-red-500/10 hover:text-red-600 dark:hover:text-red-400"
          >
            <UserMinus size={15} />
          </button>
        </div>
      </GlassCard>
    </motion.div>
  );
}
