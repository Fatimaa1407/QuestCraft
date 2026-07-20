// Maps an equipped Theme marketplace item's raw (Azerbaijani) Name to an accent color pair.
// Deliberately keyed by name rather than a new backend field — Theme items stay generic
// MarketplaceItem rows, the palette is a presentation-only concern.
export interface AccentPalette {
  accent: string;
  accent2: string;
}

export const THEME_PALETTES: Record<string, AccentPalette> = {
  'Tünd Tema': { accent: '#6366f1', accent2: '#8b5cf6' },
  'Bənövşəyi Tema': { accent: '#a855f7', accent2: '#d946ef' },
  'Narıncı Tema': { accent: '#f97316', accent2: '#ef4444' },
};

export const DEFAULT_ACCENT_PALETTE: AccentPalette = { accent: '#3b82f6', accent2: '#06b6d4' };

export function applyAccentPalette(themeName: string | null | undefined) {
  const palette = (themeName && THEME_PALETTES[themeName]) || DEFAULT_ACCENT_PALETTE;
  document.documentElement.style.setProperty('--color-app-accent', palette.accent);
  document.documentElement.style.setProperty('--color-app-accent-2', palette.accent2);
}
