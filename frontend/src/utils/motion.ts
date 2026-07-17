import type { Variants, Transition } from 'framer-motion';

export const spring: Transition = { type: 'spring', stiffness: 340, damping: 28 };

// Softer and slower than `spring` on purpose — used only for card entrances, so
// hover/tap feedback (which still uses `spring`) stays snappy and responsive.
export const entranceSpring: Transition = { type: 'spring', stiffness: 120, damping: 20, mass: 0.9 };

export const fadeInUp: Variants = {
  hidden: { opacity: 0, y: 16 },
  show: { opacity: 1, y: 0, transition: entranceSpring },
};

export const staggerContainer: Variants = {
  hidden: {},
  show: {
    transition: { staggerChildren: 0.12, delayChildren: 0.08 },
  },
};

export const pageTransition: Variants = {
  hidden: { opacity: 0, y: 8 },
  show: { opacity: 1, y: 0, transition: { duration: 0.25, ease: 'easeOut' } },
  exit: { opacity: 0, y: -8, transition: { duration: 0.15, ease: 'easeIn' } },
};

export const cardHover = {
  whileHover: { y: -4, transition: spring },
  whileTap: { scale: 0.98 },
};

export const buttonTap = {
  whileHover: { scale: 1.02 },
  whileTap: { scale: 0.96 },
};
