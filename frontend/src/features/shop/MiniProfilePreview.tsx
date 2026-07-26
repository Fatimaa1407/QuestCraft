import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { Eye } from 'lucide-react';
import { FramedAvatar } from '../../components/ui/FramedAvatar';
import { THEME_PALETTES, DEFAULT_ACCENT_PALETTE } from '../../utils/themePalettes';
import { Z_INDEX } from '../../styles/zIndex';
import type { EquippedCosmeticsDto, MarketplaceItemDto } from '../../types/marketplace';

interface MiniProfilePreviewProps {
  item: MarketplaceItemDto | null;
  equipped: EquippedCosmeticsDto | null | undefined;
  username: string;
  level: number;
}

// A single persistent floating panel (not one per card) that previews how the currently-hovered
// item would look equipped, by overriding just that one cosmetic slot on top of what the user has
// equipped today. Theme items are previewed with inline CSS vars scoped to this panel only —
// never document.documentElement — so hovering a theme never recolors the rest of the page.
export function MiniProfilePreview({ item, equipped, username, level }: MiniProfilePreviewProps) {
  const { t } = useTranslation();
  const isVisible = !!item;

  let avatarUrl = equipped?.avatarUrl ?? null;
  let frameImageUrl = equipped?.frameImageUrl ?? null;
  let titleText = equipped?.titleText ?? null;
  let badgeImageUrl = equipped?.badgeImageUrl ?? null;
  let badgeName = equipped?.badgeName ?? null;
  let bannerImageUrl = equipped?.bannerImageUrl ?? null;
  let palette = equipped?.themeName ? THEME_PALETTES[equipped.themeName] : undefined;

  if (item) {
    switch (item.itemType) {
      case 'Avatar':
        avatarUrl = item.imageUrl;
        break;
      case 'ProfileFrame':
        frameImageUrl = item.imageUrl;
        break;
      case 'Title':
        titleText = item.name;
        break;
      case 'Badge':
        badgeImageUrl = item.imageUrl;
        badgeName = item.name;
        break;
      case 'ProfileBanner':
        bannerImageUrl = item.imageUrl;
        break;
      case 'Theme':
        palette = THEME_PALETTES[item.name] ?? DEFAULT_ACCENT_PALETTE;
        break;
      default:
        break;
    }
  }

  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          initial={{ opacity: 0, y: 12, scale: 0.96 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 12, scale: 0.96 }}
          transition={{ type: 'spring', stiffness: 320, damping: 26 }}
          style={{ zIndex: Z_INDEX.toast, ...(palette ? { '--color-app-accent': palette.accent, '--color-app-accent-2': palette.accent2 } : {}) } as React.CSSProperties}
          className="pointer-events-none fixed bottom-5 right-5 hidden w-64 overflow-hidden rounded-2xl border border-white/20 bg-white/90 shadow-2xl backdrop-blur-xl dark:border-white/10 dark:bg-slate-900/90 sm:block"
        >
          {bannerImageUrl && <div className="h-14 w-full bg-cover bg-center" style={{ backgroundImage: `url(${bannerImageUrl})` }} />}
          <div className="flex items-center gap-2 px-2 pb-1.5 pt-2 text-[10px] font-semibold uppercase tracking-wide text-slate-400 dark:text-slate-500">
            <Eye size={11} />
            {t('shop.previewLabel')}
          </div>
          <div className="flex items-center gap-3 px-4 pb-4">
            <FramedAvatar username={username} avatarUrl={avatarUrl} frameImageUrl={frameImageUrl} size={44} />
            <div className="min-w-0">
              <p className="flex items-center gap-1.5 truncate text-sm font-semibold text-slate-900 dark:text-slate-100">
                {badgeImageUrl && <img src={badgeImageUrl} alt="" title={badgeName ?? undefined} className="h-3.5 w-3.5 shrink-0 rounded-full" />}
                {username}
              </p>
              {titleText && <p className="truncate text-xs font-medium text-app-accent dark:text-app-accent-2">{titleText}</p>}
              <p className="text-xs text-slate-500 dark:text-slate-400">Lvl {level}</p>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
