import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft, Receipt, Check } from 'lucide-react';
import { getMyPurchases } from '../../api/marketplace';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { useDateBucketLabel } from '../../utils/relativeDateBucket';
import type { MyPurchaseDto } from '../../types/marketplace';

export function PurchaseHistoryPage() {
  const { t } = useTranslation();
  const bucketLabel = useDateBucketLabel();
  const purchasesQuery = useQuery({ queryKey: ['marketplace', 'my-purchases'], queryFn: getMyPurchases });
  const purchases = purchasesQuery.data ?? [];

  const groups = useMemo(() => {
    const map = new Map<string, MyPurchaseDto[]>();
    for (const p of purchases) {
      const label = bucketLabel(p.purchasedAt);
      const list = map.get(label) ?? [];
      list.push(p);
      map.set(label, list);
    }
    return [...map.entries()];
    // purchases is already sorted newest-first by the backend, so Map insertion order is correct.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [purchases]);

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp} className="flex items-center gap-3">
        <Link
          to="/shop"
          className="flex h-9 w-9 items-center justify-center rounded-full text-slate-400 transition hover:bg-slate-500/10 hover:text-slate-700 dark:hover:text-slate-200"
        >
          <ArrowLeft size={17} />
        </Link>
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('shop.historyTitle')}</h1>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('shop.historySubtitle')}</p>
        </div>
      </motion.div>

      {purchasesQuery.isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-2xl" />
          ))}
        </div>
      ) : purchases.length === 0 ? (
        <EmptyState icon={Receipt} tint="amber" title={t('shop.historyEmpty')} action={{ label: t('shop.title'), to: '/shop' }} />
      ) : (
        <div className="space-y-6">
          {groups.map(([label, items]) => (
            <motion.div key={label} variants={fadeInUp}>
              <h2 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-400 dark:text-slate-500">{label}</h2>
              <GlassCard hoverLift={false} className="divide-y divide-slate-200/70 p-2 dark:divide-white/[0.06]">
                {items.map((p) => (
                  <div key={p.id} className="flex items-center gap-3 px-3 py-3">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400">
                      {p.imageUrl ? (
                        <img src={p.imageUrl} alt="" className="h-full w-full rounded-xl object-cover" />
                      ) : (
                        <Receipt size={17} />
                      )}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{p.itemName}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">{p.itemType}</p>
                    </div>
                    {p.isEquipped && (
                      <span className="flex shrink-0 items-center gap-1 rounded-full bg-emerald-500/15 px-2 py-0.5 text-[10px] font-semibold text-emerald-600 dark:text-emerald-400">
                        <Check size={10} />
                        {t('shop.equipped')}
                      </span>
                    )}
                    <span className="shrink-0 text-sm font-semibold text-amber-600 dark:text-amber-400">
                      {p.pricePaid === 0 ? t('shop.historyFree') : `🪙 ${p.pricePaid}`}
                    </span>
                  </div>
                ))}
              </GlassCard>
            </motion.div>
          ))}
        </div>
      )}
    </motion.div>
  );
}
