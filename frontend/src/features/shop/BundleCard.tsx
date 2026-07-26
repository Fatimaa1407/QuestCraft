import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Loader2, Package } from 'lucide-react';
import type { MarketplaceBundleDto } from '../../types/marketplace';
import { GlassCard } from '../../components/ui/GlassCard';
import { buttonTap, fadeInUp } from '../../utils/motion';

interface BundleCardProps {
  bundle: MarketplaceBundleDto;
  canAfford: boolean;
  isPurchasing: boolean;
  onPurchase: () => void;
}

export function BundleCard({ bundle, canAfford, isPurchasing, onPurchase }: BundleCardProps) {
  const { t } = useTranslation();
  const savePercent = bundle.individualTotal > 0 ? Math.round((1 - bundle.bundlePrice / bundle.individualTotal) * 100) : 0;

  return (
    <motion.div variants={fadeInUp}>
      <GlassCard hoverLift={false} className="relative overflow-hidden p-6" style={{ border: '2px solid rgba(139, 92, 246, 0.4)' }}>
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-violet-500/[0.06] via-transparent to-fuchsia-500/[0.06]" />

        <div className="relative flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-violet-500/15 text-violet-600 dark:text-violet-400">
            <Package size={18} />
          </span>
          <div className="min-w-0 flex-1">
            <h3 className="truncate text-base font-bold text-slate-900 dark:text-slate-100">{bundle.name}</h3>
            {savePercent > 0 && (
              <span className="inline-flex items-center rounded-full bg-emerald-500/15 px-2 py-0.5 text-[10px] font-bold text-emerald-600 dark:text-emerald-400">
                {t('shop.bundleSave', { percent: savePercent })}
              </span>
            )}
          </div>
        </div>

        {bundle.description && <p className="relative mt-3 text-xs text-slate-500 dark:text-slate-400">{bundle.description}</p>}

        <div className="relative mt-4 flex -space-x-3">
          {bundle.items.map((i) => (
            <div
              key={i.marketplaceItemId}
              className="flex h-12 w-12 items-center justify-center overflow-hidden rounded-2xl border-2 border-white bg-blue-500/10 text-blue-600 shadow-sm dark:border-slate-900 dark:text-cyan-400"
              title={i.name}
            >
              {i.imageUrl ? <img src={i.imageUrl} alt="" className="h-full w-full object-cover" /> : <Package size={18} />}
            </div>
          ))}
        </div>

        <div className="relative mt-5 flex items-center justify-between gap-2 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
          <div>
            <span className="text-sm font-bold text-amber-600 dark:text-amber-400">🪙 {bundle.bundlePrice}</span>
            {savePercent > 0 && <span className="ml-1.5 text-xs text-slate-400 line-through">🪙 {bundle.individualTotal}</span>}
          </div>

          {bundle.isOwnedFully ? (
            <span className="rounded-full bg-slate-500/10 px-3.5 py-1.5 text-xs font-semibold text-slate-500 dark:text-slate-400">
              {t('shop.bundleOwnedFully')}
            </span>
          ) : (
            <motion.button
              {...buttonTap}
              onClick={onPurchase}
              disabled={isPurchasing || !canAfford}
              className="flex items-center gap-1 rounded-full bg-gradient-to-r from-violet-500 to-fuchsia-500 px-3.5 py-1.5 text-xs font-semibold text-white shadow-sm shadow-violet-500/30 disabled:opacity-50"
            >
              {isPurchasing && <Loader2 size={11} className="animate-spin" />}
              {t('shop.bundleBuy')}
            </motion.button>
          )}
        </div>

        {bundle.ownedCount > 0 && !bundle.isOwnedFully && (
          <p className="relative mt-2 text-center text-[11px] text-slate-400 dark:text-slate-500">
            {t('shop.bundleOwnedPartial', { owned: bundle.ownedCount, total: bundle.items.length })}
          </p>
        )}
      </GlassCard>
    </motion.div>
  );
}
