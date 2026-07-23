import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MessageSquare, EyeOff, Eye, CornerDownRight, Send } from 'lucide-react';
import { getChallengeComments, postChallengeComment } from '../../api/challenges';
import type { ChallengeCommentDto } from '../../types/challenge';
import { showToast } from '../../app/toastStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { GlassCard } from '../../components/ui/GlassCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Skeleton } from '../../components/ui/Skeleton';

function SpoilerContent({ content }: { content: string }) {
  const { t } = useTranslation();
  const [revealed, setRevealed] = useState(false);

  if (revealed) {
    return <p className="text-sm text-slate-700 dark:text-slate-300">{content}</p>;
  }

  return (
    <button
      type="button"
      onClick={() => setRevealed(true)}
      className="flex items-center gap-1.5 rounded-lg bg-slate-200/70 px-3 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-300/70 dark:bg-white/10 dark:text-slate-300 dark:hover:bg-white/15"
    >
      <Eye size={13} />
      {t('challenges.discussionRevealSpoiler')}
    </button>
  );
}

function CommentRow({ comment, isReply = false }: { comment: ChallengeCommentDto; isReply?: boolean }) {
  return (
    <div className={`flex gap-2.5 ${isReply ? 'ml-6' : ''}`}>
      {isReply && <CornerDownRight size={14} className="mt-1 shrink-0 text-slate-400 dark:text-slate-500" />}
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-slate-800 dark:text-slate-100">{comment.username}</span>
          <span className="text-[11px] text-slate-400 dark:text-slate-500">{new Date(comment.createdAt).toLocaleString()}</span>
          {comment.isSpoiler && (
            <span className="flex items-center gap-1 rounded-full bg-amber-500/10 px-2 py-0.5 text-[10px] font-medium text-amber-600 dark:text-amber-400">
              <EyeOff size={10} />
              Spoiler
            </span>
          )}
        </div>
        <div className="mt-1">
          {comment.isSpoiler ? <SpoilerContent content={comment.content} /> : <p className="text-sm text-slate-700 dark:text-slate-300">{comment.content}</p>}
        </div>
      </div>
    </div>
  );
}

export function ChallengeDiscussion({ challengeId }: { challengeId: number }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [content, setContent] = useState('');
  const [isSpoiler, setIsSpoiler] = useState(false);
  const [replyTo, setReplyTo] = useState<{ id: number; username: string } | null>(null);

  const commentsQuery = useQuery({
    queryKey: ['challenge', 'comments', challengeId],
    queryFn: () => getChallengeComments(challengeId),
  });

  const postMutation = useMutation({
    mutationFn: () => postChallengeComment(challengeId, { content: content.trim(), isSpoiler, parentCommentId: replyTo?.id ?? null }),
    onSuccess: () => {
      setContent('');
      setIsSpoiler(false);
      setReplyTo(null);
      queryClient.invalidateQueries({ queryKey: ['challenge', 'comments', challengeId] });
    },
    onError: (err) => showToast({ title: getApiErrorMessage(err, t('challenges.discussionError')), emoji: '⚠️' }),
  });

  const handleSubmit = () => {
    if (!content.trim() || postMutation.isPending) return;
    postMutation.mutate();
  };

  return (
    <GlassCard hoverLift={false} className="p-6">
      <div className="mb-4 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-app-accent/10 text-app-accent dark:text-app-accent-2">
          <MessageSquare size={16} />
        </div>
        <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{t('challenges.discussionTitle')}</h2>
      </div>

      <div className="space-y-2">
        {replyTo && (
          <div className="flex items-center justify-between rounded-lg bg-slate-100/70 px-3 py-1.5 text-xs text-slate-600 dark:bg-white/5 dark:text-slate-300">
            <span>{t('challenges.discussionReplyingTo', { username: replyTo.username })}</span>
            <button type="button" onClick={() => setReplyTo(null)} className="font-medium text-app-accent hover:underline dark:text-app-accent-2">
              {t('challenges.discussionCancelReply')}
            </button>
          </div>
        )}
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder={t('challenges.discussionPlaceholder')}
          rows={2}
          maxLength={1000}
          className="w-full resize-none rounded-xl border border-slate-200/70 bg-white/70 p-3 text-sm text-slate-800 placeholder:text-slate-400 focus:border-app-accent focus:outline-none dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
        <div className="flex items-center justify-between">
          <label className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
            <input type="checkbox" checked={isSpoiler} onChange={(e) => setIsSpoiler(e.target.checked)} className="rounded" />
            {t('challenges.discussionMarkSpoiler')}
          </label>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!content.trim() || postMutation.isPending}
            className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-4 py-1.5 text-xs font-medium text-white shadow-sm shadow-app-accent/25 disabled:opacity-50"
          >
            <Send size={12} />
            {t('challenges.discussionPost')}
          </button>
        </div>
      </div>

      <div className="mt-5 space-y-4">
        {commentsQuery.isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
        ) : !commentsQuery.data || commentsQuery.data.length === 0 ? (
          <EmptyState bare icon={MessageSquare} tint="slate" title={t('challenges.discussionEmpty')} />
        ) : (
          commentsQuery.data.map((thread) => (
            <div key={thread.comment.id} className="space-y-2 border-t border-slate-200/70 pt-3 first:border-t-0 first:pt-0 dark:border-white/[0.06]">
              <CommentRow comment={thread.comment} />
              <button
                type="button"
                onClick={() => setReplyTo({ id: thread.comment.id, username: thread.comment.username })}
                className="ml-6 text-xs font-medium text-app-accent hover:underline dark:text-app-accent-2"
              >
                {t('challenges.discussionReply')}
              </button>
              {thread.replies.map((reply) => (
                <CommentRow key={reply.id} comment={reply} isReply />
              ))}
            </div>
          ))
        )}
      </div>
    </GlassCard>
  );
}
