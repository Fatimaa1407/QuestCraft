export type Rarity = 'Common' | 'Rare' | 'Epic' | 'Legendary' | 'Mythic';

export function getRarity(price: number): Rarity {
  if (price >= 200) return 'Mythic';
  if (price >= 150) return 'Legendary';
  if (price >= 100) return 'Epic';
  if (price >= 50) return 'Rare';
  return 'Common';
}

// Common -> Mythic ordinal, for sort-by-rarity controls.
export const RARITY_ORDER: Rarity[] = ['Common', 'Rare', 'Epic', 'Legendary', 'Mythic'];

export const RARITY_STYLES: Record<
  Rarity,
  { dot: string; text: string; borderColor: string; glow: string; labelAz: string; ringClass?: string }
> = {
  Common: {
    dot: 'bg-emerald-500',
    text: 'text-emerald-600 dark:text-emerald-400',
    borderColor: 'rgba(16, 185, 129, 0.45)',
    glow: 'hover:shadow-emerald-500/20',
    labelAz: 'Common',
  },
  Rare: {
    dot: 'bg-blue-500',
    text: 'text-blue-600 dark:text-blue-400',
    borderColor: 'rgba(59, 130, 246, 0.5)',
    glow: 'hover:shadow-blue-500/25',
    labelAz: 'Rare',
  },
  Epic: {
    dot: 'bg-violet-500',
    text: 'text-violet-600 dark:text-violet-400',
    borderColor: 'rgba(139, 92, 246, 0.5)',
    glow: 'hover:shadow-violet-500/25',
    labelAz: 'Epic',
  },
  Legendary: {
    dot: 'bg-amber-500',
    text: 'text-amber-600 dark:text-amber-400',
    borderColor: 'rgba(245, 158, 11, 0.55)',
    glow: 'hover:shadow-amber-500/30',
    labelAz: 'Legendary',
    // Animated gold ring — see .rarity-ring-legendary in index.css.
    ringClass: 'rarity-ring-legendary',
  },
  Mythic: {
    dot: 'bg-fuchsia-500',
    text: 'text-fuchsia-600 dark:text-fuchsia-400',
    borderColor: 'rgba(217, 70, 239, 0.6)',
    glow: 'hover:shadow-fuchsia-500/40',
    labelAz: 'Mythic',
    // Animated fuchsia/pink ring — see .rarity-ring-mythic in index.css.
    ringClass: 'rarity-ring-mythic',
  },
};
