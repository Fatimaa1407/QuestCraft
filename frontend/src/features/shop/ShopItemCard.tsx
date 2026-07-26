import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Check, Lock, X, Loader2, Heart, type LucideIcon } from 'lucide-react';
import type { MarketplaceItemDto } from '../../types/marketplace';
import { getRarity, RARITY_STYLES } from '../../utils/rarity';
import { THEME_PALETTES, DEFAULT_ACCENT_PALETTE } from '../../utils/themePalettes';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, cardHover, buttonTap } from '../../utils/motion';

interface ShopItemCardProps {
  item: MarketplaceItemDto;
  icon: LucideIcon;
  isEquipable: boolean;
  isEquipped: boolean;
  canAfford: boolean;
  isCelebrating: boolean;
  isPurchasing: boolean;
  isEquipping: boolean;
  isUnequipping: boolean;
  isTogglingWishlist?: boolean;
  onPurchase: () => void;
  onEquip: () => void;
  onUnequip: () => void;
  onLockedClick: () => void;
  onToggleWishlist?: () => void;
  onHoverStart?: () => void;
  onHoverEnd?: () => void;
}

export function ShopItemCard({
  item,
  icon: Icon,
  isEquipable,
  isEquipped,
  canAfford,
  isCelebrating,
  isPurchasing,
  isEquipping,
  isUnequipping,
  isTogglingWishlist,
  onPurchase,
  onEquip,
  onUnequip,
  onLockedClick,
  onToggleWishlist,
  onHoverStart,
  onHoverEnd,
}: ShopItemCardProps) {
  const { t } = useTranslation();
  const rarity = getRarity(item.price);
  const rarityStyle = RARITY_STYLES[rarity];
  const isOwnedUnequipped = item.isOwned && !isEquipped;

  return (
    <motion.div
      variants={fadeInUp}
      {...cardHover}
      onHoverStart={onHoverStart}
      onHoverEnd={onHoverEnd}
      className="group"
    >
      <GlassCard
        hoverLift={false}
        style={{ border: `2px solid ${rarityStyle.borderColor}` }}
        className={`relative flex flex-col overflow-hidden p-0 shadow-sm transition-shadow duration-300 group-hover:shadow-xl ${rarityStyle.glow} ${isCelebrating ? 'animate-card-pulse' : ''} ${isOwnedUnequipped ? 'opacity-90 grayscale-[15%]' : ''}`}
      >
        {rarityStyle.ringClass && (
          <div className={`pointer-events-none absolute inset-0 z-[1] ${rarityStyle.ringClass}`} />
        )}

        {isCelebrating && (
          <span className="animate-shimmer-sweep pointer-events-none absolute inset-y-0 left-0 z-10 w-1/3 bg-gradient-to-r from-transparent via-white/40 to-transparent" />
        )}
        {isCelebrating && (
          <span className="animate-toast-pop pointer-events-none absolute left-1/2 top-1/2 z-20 -translate-x-1/2 -translate-y-1/2 rounded-full bg-slate-900/90 px-4 py-2 text-sm font-semibold text-white shadow-xl">
            🎉 {t('shop.purchased')}
          </span>
        )}

        {item.isOwned && (
          <div
            className={`relative z-[2] flex items-center justify-center gap-1.5 py-2 text-xs font-semibold ${
              isEquipped
                ? 'bg-gradient-to-r from-emerald-500/25 to-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                : 'bg-slate-500/10 text-slate-500 dark:bg-white/[0.04] dark:text-slate-400'
            }`}
          >
            <Check size={13} />
            {isEquipped ? t('shop.equipped') : t('shop.owned')}
          </div>
        )}

        <div className="relative z-[2] flex flex-1 flex-col p-6">
          {onToggleWishlist && (
            <motion.button
              {...buttonTap}
              type="button"
              onClick={onToggleWishlist}
              disabled={isTogglingWishlist}
              title={item.isWishlisted ? t('shop.wishlistRemoved') : t('shop.wishlistAdded')}
              className="absolute right-4 top-4 z-[3] flex h-8 w-8 items-center justify-center rounded-full bg-white/70 text-slate-400 shadow-sm backdrop-blur-sm transition-colors hover:text-red-500 disabled:opacity-60 dark:bg-slate-900/60 dark:text-slate-400"
            >
              <Heart size={15} className={item.isWishlisted ? 'fill-red-500 text-red-500' : ''} />
            </motion.button>
          )}

          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-500/10 text-blue-600 transition-transform duration-300 group-hover:scale-110 dark:text-cyan-400">
            {item.itemType === 'Theme' ? (
              // Themes only recolor accent elements app-wide rather than the whole page, so the
              // effect can be easy to miss — showing the actual resulting colors here (and title
              // "requires a closer look" isn't enough) makes clear what buying/equipping it changes.
              <ThemeSwatch name={item.name} />
            ) : item.imageUrl ? (
              <img src={item.imageUrl} alt="" className="h-full w-full rounded-2xl object-cover" />
            ) : (
              <Icon size={26} />
            )}
          </div>

          <h2 className="mt-4 text-lg font-semibold text-slate-900 dark:text-slate-100">{item.name}</h2>
          <div className="mt-1 flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400">
            <span>{item.itemType}</span>
            <span className="text-slate-300 dark:text-slate-700">·</span>
            <span className={`flex items-center gap-1 font-medium ${rarityStyle.text}`}>
              <span className={`h-2 w-2 rounded-full ${rarityStyle.dot}`} />
              {rarityStyle.labelAz}
            </span>
          </div>

          {item.description && (
            <p className="mt-2 flex-1 text-sm text-slate-600 dark:text-slate-300">{item.description}</p>
          )}
          {!item.description && <div className="flex-1" />}

          <div className="mt-5 flex items-center justify-between gap-2 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
            <span className="flex items-center gap-1 text-sm font-semibold text-amber-600 dark:text-amber-400">
              🪙 {item.price}
            </span>

            {item.isOwned ? (
              isEquipable ? (
                isEquipped ? (
                  <motion.button
                    {...buttonTap}
                    onClick={onUnequip}
                    disabled={isUnequipping}
                    className="flex items-center gap-1 rounded-full border border-slate-300 px-3.5 py-1.5 text-xs font-medium text-slate-600 transition-colors hover:bg-slate-500/10 disabled:opacity-50 dark:border-white/20 dark:text-slate-300"
                  >
                    {isUnequipping ? <Loader2 size={11} className="animate-spin" /> : <X size={11} />}
                    {t('shop.unequip')}
                  </motion.button>
                ) : (
                  <motion.button
                    {...buttonTap}
                    onClick={onEquip}
                    disabled={isEquipping}
                    className="flex items-center gap-1 rounded-full border border-blue-400 px-3.5 py-1.5 text-xs font-medium text-blue-600 transition-colors hover:bg-blue-500/10 disabled:opacity-50 dark:text-cyan-400"
                  >
                    {isEquipping && <Loader2 size={11} className="animate-spin" />}
                    {t('shop.equip')}
                  </motion.button>
                )
              ) : null
            ) : canAfford ? (
              <motion.button
                {...buttonTap}
                onClick={onPurchase}
                disabled={isPurchasing}
                className="flex items-center gap-1 rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-3.5 py-1.5 text-xs font-medium text-white shadow-sm shadow-app-accent/30"
              >
                {isPurchasing && <Loader2 size={11} className="animate-spin" />}
                {t('shop.buy')}
              </motion.button>
            ) : (
              <motion.button
                {...buttonTap}
                onClick={onLockedClick}
                className="flex items-center gap-1.5 rounded-full border border-white/10 bg-white/5 px-3.5 py-1.5 text-xs font-medium text-slate-400 backdrop-blur-sm transition-colors hover:border-white/20 hover:text-slate-300 dark:text-slate-400"
              >
                <Lock size={11} />
                {t('shop.locked')}
              </motion.button>
            )}
          </div>
        </div>
      </GlassCard>
    </motion.div>
  );
}

// Split-diagonal preview of the two accent colors a Theme item applies — same lookup
// `applyAccentPalette` uses, so what's shown here always matches the real effect.
function ThemeSwatch({ name }: { name: string }) {
  const palette = THEME_PALETTES[name] ?? DEFAULT_ACCENT_PALETTE;
  return (
    <div
      className="h-full w-full rounded-2xl"
      style={{ background: `linear-gradient(135deg, ${palette.accent} 50%, ${palette.accent2} 50%)` }}
    />
  );
}
