import { Link } from 'react-router-dom';
import { motion, type Transition, type Variants } from 'framer-motion';

// A restrained, duration-based easing curve (not the app-wide spring physics) —
// deliberately closer to Apple/Linear/Vercel-style motion for this pre-auth page only.
export const landingEase = [0.16, 1, 0.3, 1] as const;

// ---------------------------------------------------------------------------
// Page-load cascade: the whole landing page animates once, in one continuous
// sequence, the moment it mounts — never on scroll. `STEP` is the 150ms gap
// between consecutive items; everything below is expressed as a multiple of it
// so the full-page timeline stays easy to reason about and adjust.
// ---------------------------------------------------------------------------
export const STEP = 0.15;

// Each "card" per the spec: opacity 0→1, y 30→0, scale 0.97→1, 0.5s easeOut.
export function cascadeItemDelayed(delay: number): Variants {
  return {
    hidden: { opacity: 0, y: 30, scale: 0.97 },
    show: { opacity: 1, y: 0, scale: 1, transition: { duration: 0.5, ease: 'easeOut', delay } },
  };
}

// Same recipe, no extra delay of its own — for children of a `cascadeGroup`,
// which already spaces them out via its own `staggerChildren`.
export const cascadeItem: Variants = cascadeItemDelayed(0);

// A group whose children stagger 150ms apart, starting at `delayChildren`.
export function cascadeGroup(delayChildren: number, staggerChildren = STEP): Variants {
  return {
    hidden: {},
    show: { transition: { staggerChildren, delayChildren } },
  };
}

export const HERO_DELAYS = {
  badge: STEP * 1,
  title: STEP * 2,
  description: STEP * 3,
  cta: STEP * 4,
  editor: STEP * 5,
  stats: STEP * 6,
} as const;

const FEATURE_CARD_COUNT = 6;
const TIMELINE_STEP_COUNT = 4;
const TESTIMONIAL_COUNT = 4;
const FAQ_COUNT = 5;

export const FEATURES_START = HERO_DELAYS.stats + STEP;
export const TIMELINE_START = FEATURES_START + FEATURE_CARD_COUNT * STEP;
export const AUDIENCE_START = TIMELINE_START + TIMELINE_STEP_COUNT * STEP;
export const TESTIMONIALS_START = AUDIENCE_START + STEP;
export const FAQ_START = TESTIMONIALS_START + TESTIMONIAL_COUNT * STEP;
export const CTA_START = FAQ_START + FAQ_COUNT * STEP;
export const FOOTER_START = CTA_START + STEP;

export const cardHoverTransition: Transition = { duration: 0.25, ease: 'easeOut' };

// A motion-wrapped router Link so CTAs get scale/glow hover feedback while keeping
// client-side navigation — used for every button-styled link on the landing page.
export const MotionLink = motion.create(Link);
