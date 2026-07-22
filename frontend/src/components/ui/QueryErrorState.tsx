import { useTranslation } from 'react-i18next';
import { AlertTriangle } from 'lucide-react';
import { EmptyState, type EmptyStateTint } from './EmptyState';

interface QueryErrorStateProps {
  onRetry: () => void;
  title?: string;
  bare?: boolean;
  tint?: EmptyStateTint;
  className?: string;
}

// Shared "this list failed to load" state — pairs with a query's `isError` + `refetch()` so a
// failed fetch never just renders nothing (previously indistinguishable from a genuinely empty
// list). Deliberately distinct from EmptyState's icon/tint defaults so a real failure never
// looks like "there's nothing here yet".
export function QueryErrorState({ onRetry, title, bare = false, tint = 'amber', className = '' }: QueryErrorStateProps) {
  const { t } = useTranslation();

  return (
    <EmptyState
      bare={bare}
      icon={AlertTriangle}
      tint={tint}
      title={title ?? t('common.loadError')}
      className={className}
      action={{ label: t('common.retry'), onClick: onRetry }}
    />
  );
}
