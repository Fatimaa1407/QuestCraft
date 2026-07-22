import { forwardRef, type ButtonHTMLAttributes } from 'react';

export const Button = forwardRef<HTMLButtonElement, ButtonHTMLAttributes<HTMLButtonElement>>(
  ({ className = '', children, disabled, ...props }, ref) => (
    <button
      ref={ref}
      disabled={disabled}
      className={`group relative w-full overflow-hidden rounded-lg bg-gradient-to-r from-app-accent to-app-accent-2 px-3 py-2.5 text-sm font-medium text-white shadow-lg shadow-app-accent/25 transition-all duration-200 ease-out hover:-translate-y-0.5 hover:shadow-xl hover:shadow-app-accent-2/35 hover:brightness-110 active:translate-y-0 active:scale-[0.98] active:duration-100 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:translate-y-0 disabled:hover:brightness-100 ${className}`}
      {...props}
    >
      <span className="pointer-events-none absolute inset-0 -translate-x-full bg-gradient-to-r from-transparent via-white/20 to-transparent transition-transform duration-700 group-hover:translate-x-full" />
      <span className="relative">{children}</span>
    </button>
  ),
);
Button.displayName = 'Button';
