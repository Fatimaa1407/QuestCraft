// Centralized z-index scale. Anything that must float above normal page content
// (cards, grids, page transitions with their own stacking contexts) should reference
// this instead of picking an arbitrary Tailwind z-* class, so overlay layering stays
// predictable as more floating UI (menus, toasts, modals) gets added.
export const Z_INDEX = {
  header: 10,
  // Fixed bottom tab bar on mobile — above page content, below dropdowns/modals.
  mobileNav: 20,
  dropdown: 9999,
  // Full-screen dialogs (e.g. QuizCompleteModal) must win over any open dropdown/menu.
  modal: 10000,
} as const;
