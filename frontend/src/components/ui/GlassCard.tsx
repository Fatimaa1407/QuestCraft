import type { HTMLAttributes } from 'react';

export function GlassCard({ className = '', children, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div className={`glass-card ${className}`} {...props}>
      {children}
    </div>
  );
}
