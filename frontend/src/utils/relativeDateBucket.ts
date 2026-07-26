import { useTranslation } from 'react-i18next';

// Buckets a timestamp into a relative-day group label for list headers (distinct from
// useRelativeTime, which formats a single row's own timestamp rather than a group heading).
export function useDateBucketLabel() {
  const { t } = useTranslation();
  return (isoDate: string): string => {
    const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
    const diffDays = Math.round((startOfDay(new Date()) - startOfDay(new Date(isoDate))) / 86_400_000);

    if (diffDays <= 0) return t('shop.historyToday');
    if (diffDays === 1) return t('shop.historyYesterday');
    if (diffDays < 7) return t('shop.historyDaysAgo', { count: diffDays });
    const weeks = Math.floor(diffDays / 7);
    return t('shop.historyWeeksAgo', { count: weeks });
  };
}
