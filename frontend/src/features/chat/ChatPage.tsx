import { useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Send, MessageCircle, ArrowLeft } from 'lucide-react';
import { getConversations, getConversation, sendChatMessage, markConversationRead } from '../../api/chat';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { FramedAvatar as Avatar } from '../../components/ui/FramedAvatar';
import { useRelativeTime } from '../../utils/useRelativeTime';
import { fadeInUp, staggerContainer } from '../../utils/motion';

export function ChatPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { userId } = useParams<{ userId: string }>();
  const activeUserId = userId ? Number(userId) : null;
  const currentUser = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const formatRelative = useRelativeTime();
  const [draft, setDraft] = useState('');
  const scrollRef = useRef<HTMLDivElement>(null);

  const conversationsQuery = useQuery({ queryKey: ['chat', 'conversations'], queryFn: getConversations });
  const messagesQuery = useQuery({
    queryKey: ['chat', 'conversation', activeUserId],
    queryFn: () => getConversation(activeUserId!, 1, 50),
    enabled: activeUserId !== null,
    refetchInterval: activeUserId !== null ? 5000 : false,
  });

  const activeFriend = conversationsQuery.data?.find((c) => c.friendUserId === activeUserId);

  const sendMutation = useMutation({
    mutationFn: (content: string) => sendChatMessage(activeUserId!, content),
    onSuccess: () => {
      setDraft('');
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversation', activeUserId] });
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
    },
  });

  useEffect(() => {
    if (activeUserId !== null) {
      markConversationRead(activeUserId).then(() => {
        queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
      });
    }
  }, [activeUserId, queryClient]);

  const orderedMessages = useMemo(() => [...(messagesQuery.data?.items ?? [])].reverse(), [messagesQuery.data]);

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight });
  }, [orderedMessages.length]);

  const handleSend = (e: React.FormEvent) => {
    e.preventDefault();
    if (!draft.trim() || activeUserId === null) return;
    sendMutation.mutate(draft.trim());
  };

  const conversations = conversationsQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('chat.title')}</h1>
        <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('chat.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-4 sm:grid-cols-[280px_1fr]" style={{ minHeight: '60vh' }}>
        {/* Conversation list — hidden on mobile once a thread is open */}
        <GlassCard hoverLift={false} className={`flex flex-col p-0 ${activeUserId !== null ? 'hidden sm:flex' : 'flex'}`}>
          <div className="border-b border-slate-200/70 px-4 py-3 dark:border-white/[0.06]">
            <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('chat.conversations')}</h2>
          </div>
          <div className="flex-1 overflow-y-auto">
            {conversationsQuery.isLoading ? (
              <div className="space-y-1 p-3">
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="flex items-center gap-3 p-2">
                    <Skeleton className="h-10 w-10 rounded-full" />
                    <Skeleton className="h-4 flex-1" />
                  </div>
                ))}
              </div>
            ) : conversations.length === 0 ? (
              <EmptyState bare icon={MessageCircle} tint="blue" title={t('chat.noConversations')} description={t('chat.noConversationsHint')} className="px-4" />
            ) : (
              conversations.map((conv) => (
                <button
                  key={conv.friendUserId}
                  type="button"
                  onClick={() => navigate(`/chat/${conv.friendUserId}`)}
                  className={`flex w-full items-center gap-3 px-4 py-3 text-left transition ${
                    activeUserId === conv.friendUserId ? 'bg-blue-500/10' : 'hover:bg-slate-50 dark:hover:bg-white/5'
                  }`}
                >
                  <Avatar username={conv.friendUsername} avatarUrl={conv.friendAvatarUrl} frameImageUrl={conv.friendFrameImageUrl} size={40} />
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center justify-between gap-2">
                      <p className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{conv.friendUsername}</p>
                      {conv.unreadCount > 0 && (
                        <span className="flex h-4 min-w-4 items-center justify-center rounded-full bg-blue-500 px-1 text-[10px] font-semibold text-white">
                          {conv.unreadCount}
                        </span>
                      )}
                    </div>
                    <p className="truncate text-xs text-slate-500 dark:text-slate-400">{conv.lastMessage ?? t('chat.noMessagesYet')}</p>
                  </div>
                </button>
              ))
            )}
          </div>
        </GlassCard>

        {/* Active thread */}
        <GlassCard hoverLift={false} className={`flex flex-col p-0 ${activeUserId === null ? 'hidden sm:flex' : 'flex'}`}>
          {activeUserId === null ? (
            <EmptyState bare icon={MessageCircle} tint="blue" title={t('chat.selectConversation')} className="m-auto" />
          ) : (
            <>
              <div className="flex items-center gap-3 border-b border-slate-200/70 px-4 py-3 dark:border-white/[0.06]">
                <button type="button" onClick={() => navigate('/chat')} className="text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 sm:hidden">
                  <ArrowLeft size={18} />
                </button>
                {activeFriend && <Avatar username={activeFriend.friendUsername} avatarUrl={activeFriend.friendAvatarUrl} frameImageUrl={activeFriend.friendFrameImageUrl} size={32} />}
                <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">{activeFriend?.friendUsername ?? '...'}</p>
              </div>

              <div ref={scrollRef} className="flex-1 space-y-2 overflow-y-auto p-4" style={{ maxHeight: '50vh' }}>
                {messagesQuery.isLoading ? (
                  <div className="space-y-2">
                    <Skeleton className="h-10 w-2/3" />
                    <Skeleton className="ml-auto h-10 w-1/2" />
                  </div>
                ) : (
                  orderedMessages.map((msg) => {
                    const isMine = msg.senderId === currentUser?.id;
                    return (
                      <div key={msg.id} className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}>
                        <div
                          className={`max-w-[75%] rounded-2xl px-3.5 py-2 text-sm ${
                            isMine
                              ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white'
                              : 'bg-slate-100 text-slate-800 dark:bg-white/10 dark:text-slate-100'
                          }`}
                        >
                          <p className="whitespace-pre-wrap break-words">{msg.content}</p>
                          <p className={`mt-1 text-[10px] ${isMine ? 'text-white/70' : 'text-slate-400'}`}>{formatRelative(msg.createdAt)}</p>
                        </div>
                      </div>
                    );
                  })
                )}
              </div>

              <form onSubmit={handleSend} className="flex items-center gap-2 border-t border-slate-200/70 p-3 dark:border-white/[0.06]">
                <input
                  type="text"
                  value={draft}
                  onChange={(e) => setDraft(e.target.value)}
                  placeholder={t('chat.messagePlaceholder')}
                  maxLength={2000}
                  className="flex-1 rounded-full border border-slate-200/70 bg-white/80 px-4 py-2 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
                />
                <button
                  type="submit"
                  disabled={!draft.trim() || sendMutation.isPending}
                  className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30 disabled:opacity-50"
                >
                  <Send size={15} />
                </button>
              </form>
            </>
          )}
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}
