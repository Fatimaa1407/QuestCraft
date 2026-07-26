// Maps an equipped Theme marketplace item's raw (Azerbaijani) Name to an accent color pair.
// Deliberately keyed by name rather than a new backend field — Theme items stay generic
// MarketplaceItem rows, the palette is a presentation-only concern.
export interface AccentPalette {
  accent: string;
  accent2: string;
}

// Hues deliberately spread around the color wheel (teal ~165° / purple ~280° / orange ~20°,
// vs the default blue/cyan's ~205°) so each theme reads as clearly distinct at a glance, not
// just a slightly-shifted blue.
export const THEME_PALETTES: Record<string, AccentPalette> = {
  'Tünd Tema': { accent: '#0d9488', accent2: '#059669' },
  'Bənövşəyi Tema': { accent: '#9333ea', accent2: '#c026d3' },
  'Narıncı Tema': { accent: '#ea580c', accent2: '#dc2626' },
};

export const DEFAULT_ACCENT_PALETTE: AccentPalette = { accent: '#3b82f6', accent2: '#06b6d4' };

export function applyAccentPalette(themeName: string | null | undefined) {
  const palette = (themeName && THEME_PALETTES[themeName]) || DEFAULT_ACCENT_PALETTE;
  document.documentElement.style.setProperty('--color-app-accent', palette.accent);
  document.documentElement.style.setProperty('--color-app-accent-2', palette.accent2);
}
