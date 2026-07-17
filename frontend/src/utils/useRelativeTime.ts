import { useTranslation } from 'react-i18next';

// Shared relative-time formatter — "indicə" / "X dəq əvvəl" / "X saat əvvəl" / "X gün əvvəl".
// Originally lived inline in NotificationBell; extracted so any feed (dashboard activity,
// notifications, ...) can reuse the same i18n keys and thresholds.
export function useRelativeTime() {
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
