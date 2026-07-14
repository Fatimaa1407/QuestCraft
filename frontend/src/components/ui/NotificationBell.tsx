import { useCallback, useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { Bell, Sparkles, Gift, TrendingUp, ShoppingBag, Info } from 'lucide-react';
import { getNotifications, markNotificationRead } from '../../api/notifications';
import type { AppNotification, NotificationType } from '../../types/notification';
import { Z_INDEX } from '../../styles/zIndex';

const iconByType: Record<NotificationType, typeof Sparkles> = {
  AchievementUnlock: Sparkles,
  DailyQuestReminder: Gift,
  LevelUp: TrendingUp,
  MarketplacePurchase: ShoppingBag,
  ChallengeAccepted: Sparkles,
  SystemNotification: Info,
};

const PANEL_WIDTH = 320;
const PANEL_GAP = 8;

interface PanelPosition {
  top: number;
  right: number;
}

function useRelativeTime() {
  const { t } = useTranslation();
  return (isoDate: string) => {
    const diffMs = Date.now() - new Date(isoDate).getTime();
    const minutes = Math.floor(diffMs / 60_000);
    if (minutes < 1) return t('notifications.justNow');
    if (minutes < 60) return t('notifications.minutesAgo', { count: minutes });
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return t('notifications.hoursAgo', { count: hours });
    const days = Math.floor(hours / 24);
    return t('notifications.daysAgo', { count: days });
  };
}

export function NotificationBell() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formatRelative = useRelativeTime();
  const [isOpen, setIsOpen] = useState(false);
  const [position, setPosition] = useState<PanelPosition | null>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  const { data: unreadCount = 0 } = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: async () => (await getNotifications({ unreadOnly: true, page: 1, pageSize: 1 })).totalCount,
    refetchInterval: 60_000,
  });

  const { data: list } = useQuery({
    queryKey: ['notifications', 'recent'],
    queryFn: () => getNotifications({ page: 1, pageSize: 10 }),
    enabled: isOpen,
    refetchInterval: isOpen ? 60_000 : false,
  });

  const updatePosition = useCallback(() => {
    const rect = buttonRef.current?.getBoundingClientRect();
    if (!rect) return;
    setPosition({
      top: rect.bottom + PANEL_GAP,
      right: Math.max(PANEL_GAP, window.innerWidth - rect.right),
    });
  }, []);

  const toggleOpen = () => {
    if (!isOpen) updatePosition();
    setIsOpen((v) => !v);
  };

  // Portaled to document.body, so the panel sits in its own top-level stacking context —
  // clicks outside must be detected against both the trigger button and the portaled panel,
  // since they're no longer DOM siblings.
  useEffect(() => {
    if (!isOpen) return;

    function handlePointerDown(event: MouseEvent) {
      const target = event.target as Node;
      if (buttonRef.current?.contains(target) || panelRef.current?.contains(target)) return;
      setIsOpen(false);
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') setIsOpen(false);
    }
    function handleReposition() {
      updatePosition();
    }
    // Closing on scroll (rather than repositioning) keeps this robust regardless of which
    // ancestor scrolls, since the panel is fixed-positioned outside the normal layout flow.
    function handleScroll() {
      setIsOpen(false);
    }

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);
    window.addEventListener('resize', handleReposition);
    window.addEventListener('scroll', handleScroll, true);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('resize', handleReposition);
      window.removeEventListener('scroll', handleScroll, true);
    };
  }, [isOpen, updatePosition]);

  const handleItemClick = async (notification: AppNotification) => {
    if (notification.isRead) return;
    await markNotificationRead(notification.id);
    queryClient.invalidateQueries({ queryKey: ['notifications'] });
  };

  return (
    <>
      <button
        ref={buttonRef}
        type="button"
        onClick={toggleOpen}
        aria-label={t('notifications.title')}
        className="relative flex h-9 w-9 items-center justify-center rounded-full border border-slate-200/70 bg-white/80 text-slate-500 transition hover:text-slate-800 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-400 dark:hover:text-slate-100"
      >
        <Bell size={16} />
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-semibold text-white">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {createPortal(
        <AnimatePresence>
          {isOpen && position && (
            <motion.div
              ref={panelRef}
              initial={{ opacity: 0, y: -8, scale: 0.98 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: -8, scale: 0.98 }}
              transition={{ duration: 0.15 }}
              style={{ position: 'fixed', top: position.top, right: position.right, width: PANEL_WIDTH, zIndex: Z_INDEX.dropdown }}
              className="overflow-hidden rounded-2xl border border-slate-200/70 bg-white/95 shadow-xl backdrop-blur-xl dark:border-white/[0.08] dark:bg-slate-900/95"
            >
              <div className="flex items-center justify-between border-b border-slate-200/70 px-4 py-3 dark:border-white/[0.06]">
                <span className="text-sm font-semibold text-slate-800 dark:text-slate-100">{t('notifications.title')}</span>
                {unreadCount > 0 && (
                  <span className="text-xs text-slate-500 dark:text-slate-400">{t('notifications.unreadBadge', { count: unreadCount })}</span>
                )}
              </div>

              <div className="max-h-96 overflow-y-auto">
                {!list || list.items.length === 0 ? (
                  <p className="px-4 py-8 text-center text-sm text-slate-500 dark:text-slate-400">{t('notifications.empty')}</p>
                ) : (
                  list.items.map((notification) => {
                    const Icon = iconByType[notification.type];
                    return (
                      <button
                        key={notification.id}
                        type="button"
                        onClick={() => handleItemClick(notification)}
                        className={`flex w-full items-start gap-3 px-4 py-3 text-left transition hover:bg-slate-50 dark:hover:bg-white/5 ${
                          notification.isRead ? '' : 'bg-indigo-50/60 dark:bg-cyan-500/[0.06]'
                        }`}
                      >
                        <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-cyan-500 text-white">
                          <Icon size={14} />
                        </span>
                        <span className="min-w-0 flex-1">
                          <span className="flex items-center gap-1.5">
                            <span className="truncate text-sm font-medium text-slate-800 dark:text-slate-100">{notification.title}</span>
                            {!notification.isRead && <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-indigo-500 dark:bg-cyan-400" />}
                          </span>
                          <span className="mt-0.5 block text-xs text-slate-500 dark:text-slate-400">{notification.message}</span>
                          <span className="mt-1 block text-[11px] text-slate-400 dark:text-slate-500">{formatRelative(notification.createdAt)}</span>
                        </span>
                      </button>
                    );
                  })
                )}
              </div>
            </motion.div>
          )}
        </AnimatePresence>,
        document.body,
      )}
    </>
  );
}
