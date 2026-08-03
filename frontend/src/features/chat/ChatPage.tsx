import { useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Send, MessageCircle, ArrowLeft, Image as ImageIcon, X, Trash2 } from 'lucide-react';
import { getConversations, getConversation, sendChatMessage, markConversationRead, deleteChatMessage, clearConversation } from '../../api/chat';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { FramedAvatar as Avatar } from '../../components/ui/FramedAvatar';
import { UserProfileModal } from '../../components/ui/UserProfileModal';
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
  const [imageDataUrl, setImageDataUrl] = useState<string | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [viewingUserId, setViewingUserId] = useState<number | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const conversationsQuery = useQuery({ queryKey: ['chat', 'conversations'], queryFn: getConversations });
  const messagesQuery = useQuery({
    queryKey: ['chat', 'conversation', activeUserId],
    queryFn: () => getConversation(activeUserId!, 1, 50),
    enabled: activeUserId !== null,
    refetchInterval: activeUserId !== null ? 5000 : false,
  });

  const activeFriend = conversationsQuery.data?.find((c) => c.friendUserId === activeUserId);

  const sendMutation = useMutation({
    mutationFn: ({ content, image }: { content: string; image: string | null }) =>
      sendChatMessage(activeUserId!, content, image),
    onSuccess: () => {
      setDraft('');
      setImageDataUrl(null);
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversation', activeUserId] });
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
    },
  });

  const deleteMessageMutation = useMutation({
    mutationFn: deleteChatMessage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversation', activeUserId] });
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
    },
  });

  const clearConversationMutation = useMutation({
    mutationFn: () => clearConversation(activeUserId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversation', activeUserId] });
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
    },
  });

  const handleDeleteMessage = (messageId: number) => {
    if (!window.confirm(t('chat.confirmDeleteMessage'))) return;
    deleteMessageMutation.mutate(messageId);
  };

  const handleClearConversation = () => {
    if (!window.confirm(t('chat.confirmClearConversation'))) return;
    clearConversationMutation.mutate();
  };

  // 3MB raw-file cap keeps the base64 payload (~4/3 larger) comfortably under the server's
  // MaxImageDataUrlLength backstop, and keeps message history/DB rows from bloating.
  const MAX_IMAGE_BYTES = 3 * 1024 * 1024;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) return;
    setImageError(null);
    if (!file.type.startsWith('image/')) {
      setImageError(t('chat.invalidImageType'));
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      setImageError(t('chat.imageTooLarge'));
      return;
    }
    const reader = new FileReader();
    reader.onload = () => setImageDataUrl(reader.result as string);
    reader.readAsDataURL(file);
  };

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
    if ((!draft.trim() && !imageDataUrl) || activeUserId === null) return;
    sendMutation.mutate({ content: draft.trim(), image: imageDataUrl });
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
                  <span
                    role="button"
                    tabIndex={0}
                    onClick={(e) => {
                      e.stopPropagation();
                      setViewingUserId(conv.friendUserId);
                    }}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.stopPropagation();
                        setViewingUserId(conv.friendUserId);
                      }
                    }}
                  >
                    <Avatar username={conv.friendUsername} avatarUrl={conv.friendAvatarUrl} frameImageUrl={conv.friendFrameImageUrl} size={40} />
                  </span>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center justify-between gap-2">
                      <p className="flex min-w-0 items-center gap-1.5 truncate text-sm font-medium text-slate-900 dark:text-slate-100">
                        {conv.friendBadgeImageUrl && (
                          <img src={conv.friendBadgeImageUrl} alt="" title={conv.friendBadgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />
                        )}
                        <span className="truncate">{conv.friendUsername}</span>
                      </p>
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
                <button
                  type="button"
                  disabled={!activeFriend}
                  onClick={() => activeFriend && setViewingUserId(activeFriend.friendUserId)}
                  className="flex min-w-0 flex-1 items-center gap-3 text-left disabled:cursor-default"
                >
                  {activeFriend && <Avatar username={activeFriend.friendUsername} avatarUrl={activeFriend.friendAvatarUrl} frameImageUrl={activeFriend.friendFrameImageUrl} size={32} />}
                  <div className="min-w-0">
                    <p className="flex items-center gap-1.5 text-sm font-semibold text-slate-900 dark:text-slate-100">
                      {activeFriend?.friendBadgeImageUrl && (
                        <img src={activeFriend.friendBadgeImageUrl} alt="" title={activeFriend.friendBadgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />
                      )}
                      {activeFriend?.friendUsername ?? '...'}
                    </p>
                    {activeFriend?.friendTitleText && (
                      <p className="text-[11px] font-medium text-app-accent dark:text-app-accent-2">{activeFriend.friendTitleText}</p>
                    )}
                  </div>
                </button>
                <button
                  type="button"
                  onClick={handleClearConversation}
                  disabled={clearConversationMutation.isPending || orderedMessages.length === 0}
                  className="shrink-0 text-slate-400 transition hover:text-red-500 disabled:cursor-not-allowed disabled:opacity-40"
                  aria-label={t('chat.clearConversation')}
                  title={t('chat.clearConversation')}
                >
                  <Trash2 size={16} />
                </button>
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
                      <div key={msg.id} className={`group flex items-center gap-1.5 ${isMine ? 'justify-end' : 'justify-start'}`}>
                        {isMine && (
                          <button
                            type="button"
                            onClick={() => handleDeleteMessage(msg.id)}
                            className="shrink-0 text-slate-300 opacity-0 transition hover:text-red-500 group-hover:opacity-100 dark:text-slate-600"
                            aria-label={t('chat.deleteMessage')}
                            title={t('chat.deleteMessage')}
                          >
                            <Trash2 size={13} />
                          </button>
                        )}
                        <div
                          className={`max-w-[75%] rounded-2xl px-3.5 py-2 text-sm ${
                            isMine
                              ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white'
                              : 'bg-slate-100 text-slate-800 dark:bg-white/10 dark:text-slate-100'
                          }`}
                        >
                          {msg.imageDataUrl && (
                            <img
                              src={msg.imageDataUrl}
                              alt={t('chat.imageAlt')}
                              className="mb-1.5 max-h-60 max-w-full rounded-lg object-cover"
                            />
                          )}
                          {msg.content && <p className="whitespace-pre-wrap break-words">{msg.content}</p>}
                          <p className={`mt-1 text-[10px] ${isMine ? 'text-white/70' : 'text-slate-400'}`}>{formatRelative(msg.createdAt)}</p>
                        </div>
                      </div>
                    );
                  })
                )}
              </div>

              <div className="border-t border-slate-200/70 dark:border-white/[0.06]">
                {imageError && <p className="px-3 pt-2 text-xs text-red-500">{imageError}</p>}
                {imageDataUrl && (
                  <div className="flex items-center gap-2 px-3 pt-3">
                    <div className="relative">
                      <img src={imageDataUrl} alt={t('chat.imageAlt')} className="h-14 w-14 rounded-lg object-cover" />
                      <button
                        type="button"
                        onClick={() => setImageDataUrl(null)}
                        className="absolute -right-1.5 -top-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-slate-900 text-white shadow-sm"
                        aria-label={t('chat.removeImage')}
                      >
                        <X size={12} />
                      </button>
                    </div>
                  </div>
                )}
                <form onSubmit={handleSend} className="flex items-center gap-2 p-3">
                  <input ref={fileInputRef} type="file" accept="image/*" onChange={handleFileChange} className="hidden" />
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-slate-400 transition hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-white/10 dark:hover:text-slate-200"
                    aria-label={t('chat.attachImage')}
                  >
                    <ImageIcon size={17} />
                  </button>
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
                    disabled={(!draft.trim() && !imageDataUrl) || sendMutation.isPending}
                    className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30 disabled:opacity-50"
                  >
                    <Send size={15} />
                  </button>
                </form>
              </div>
            </>
          )}
        </GlassCard>
      </motion.div>

      <UserProfileModal userId={viewingUserId} onClose={() => setViewingUserId(null)} />
    </motion.div>
  );
}
