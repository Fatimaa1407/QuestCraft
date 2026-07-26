import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Loader2, Lock, Sparkles, Heart, type LucideIcon } from 'lucide-react';
import type { MarketplaceItemDto } from '../../types/marketplace';
import { getRarity, RARITY_STYLES } from '../../utils/rarity';
import { THEME_PALETTES, DEFAULT_ACCENT_PALETTE } from '../../utils/themePalettes';
import { GlassCard } from '../../components/ui/GlassCard';
import { buttonTap } from '../../utils/motion';

interface FeaturedItemProps {
  item: MarketplaceItemDto;
  icon: LucideIcon;
  canAfford: boolean;
  isPurchasing: boolean;
  isTogglingWishlist?: boolean;
  onPurchase: () => void;
  onLockedClick: () => void;
  onToggleWishlist?: () => void;
}

export function FeaturedItem({ item, icon: Icon, canAfford, isPurchasing, isTogglingWishlist, onPurchase, onLockedClick, onToggleWishlist }: FeaturedItemProps) {
  const { t } = useTranslation();
  const rarity = getRarity(item.price);
  const rarityStyle = RARITY_STYLES[rarity];

  return (
    <GlassCard
      hoverLift={false}
      style={{ border: `2px solid ${rarityStyle.borderColor}` }}
      className="relative overflow-hidden p-0"
    >
      {rarityStyle.ringClass && <div className={`pointer-events-none absolute inset-0 z-[1] ${rarityStyle.ringClass}`} />}
      <div className="pointer-events-none absolute inset-0 z-[1] bg-gradient-to-br from-app-accent/[0.06] via-transparent to-app-accent-2/[0.08]" />

      <div className="relative z-[2] flex flex-col items-center gap-6 p-8 sm:flex-row">
        <div className="flex h-28 w-28 shrink-0 items-center justify-center rounded-3xl bg-blue-500/10 text-blue-600 shadow-lg dark:text-cyan-400">
          {item.itemType === 'Theme' ? (
            <div
              className="h-full w-full rounded-3xl"
              style={{ background: `linear-gradient(135deg, ${(THEME_PALETTES[item.name] ?? DEFAULT_ACCENT_PALETTE).accent} 50%, ${(THEME_PALETTES[item.name] ?? DEFAULT_ACCENT_PALETTE).accent2} 50%)` }}
            />
          ) : item.imageUrl ? (
            <img src={item.imageUrl} alt="" className="h-full w-full rounded-3xl object-cover" />
          ) : (
            <Icon size={48} />
          )}
        </div>

        <div className="min-w-0 flex-1 text-center sm:text-left">
          <span className="inline-flex items-center gap-1 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-3 py-1 text-[11px] font-bold text-white shadow-sm">
            <Sparkles size={12} />
            {t('shop.featuredBadge')}
          </span>
          <h2 className="mt-2 text-2xl font-bold text-slate-900 dark:text-white">{item.name}</h2>
          <div className="mt-1 flex items-center justify-center gap-2 text-xs text-slate-500 dark:text-slate-400 sm:justify-start">
            <span>{item.itemType}</span>
            <span className="text-slate-300 dark:text-slate-700">·</span>
            <span className={`flex items-center gap-1 font-medium ${rarityStyle.text}`}>
              <span className={`h-2 w-2 rounded-full ${rarityStyle.dot}`} />
              {rarityStyle.labelAz}
            </span>
          </div>
          {item.description && <p className="mt-2 max-w-md text-sm text-slate-600 dark:text-slate-300">{item.description}</p>}
        </div>

        <div className="flex shrink-0 items-center gap-2">
          {onToggleWishlist && (
            <motion.button
              {...buttonTap}
              type="button"
              onClick={onToggleWishlist}
              disabled={isTogglingWishlist}
              className="flex h-10 w-10 items-center justify-center rounded-full border border-slate-200/70 text-slate-400 transition-colors hover:text-red-500 disabled:opacity-60 dark:border-white/10"
            >
              <Heart size={16} className={item.isWishlisted ? 'fill-red-500 text-red-500' : ''} />
            </motion.button>
          )}
          {item.isOwned ? (
            <span className="rounded-full bg-slate-500/10 px-5 py-2.5 text-sm font-semibold text-slate-500 dark:text-slate-400">{t('shop.owned')}</span>
          ) : canAfford ? (
            <motion.button
              {...buttonTap}
              onClick={onPurchase}
              disabled={isPurchasing}
              className="flex items-center gap-1.5 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-6 py-2.5 text-sm font-semibold text-white shadow-lg shadow-app-accent/30"
            >
              {isPurchasing && <Loader2 size={14} className="animate-spin" />}
              🪙 {item.price} · {t('shop.buy')}
            </motion.button>
          ) : (
            <motion.button
              {...buttonTap}
              onClick={onLockedClick}
              className="flex items-center gap-1.5 rounded-full border border-white/10 bg-white/5 px-5 py-2.5 text-sm font-medium text-slate-400 backdrop-blur-sm dark:text-slate-400"
            >
              <Lock size={14} />
              {t('shop.locked')}
            </motion.button>
          )}
        </div>
      </div>
    </GlassCard>
  );
}
