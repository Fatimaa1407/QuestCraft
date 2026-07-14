import type { ReactNode } from 'react';
import { motion, type HTMLMotionProps } from 'framer-motion';
import { spring } from '../../utils/motion';

interface GlassCardProps extends Omit<HTMLMotionProps<'div'>, 'children'> {
  hoverLift?: boolean;
  glow?: boolean;
  children?: ReactNode;
}

export function GlassCard({ className = '', hoverLift = true, glow = false, children, ...props }: GlassCardProps) {
  return (
    <motion.div
      className={`glass-card group/card relative ${glow ? 'glow-card' : ''} ${className}`}
      whileHover={hoverLift ? { y: -4, boxShadow: '0 24px 48px -20px rgba(0,0,0,0.35)' } : undefined}
      transition={spring}
      {...props}
    >
      {glow && (
        <div className="glow-card-border pointer-events-none absolute inset-0 rounded-[20px] opacity-0 transition-opacity duration-300 group-hover/card:opacity-100" />
      )}
      {children}
    </motion.div>
  );
}
