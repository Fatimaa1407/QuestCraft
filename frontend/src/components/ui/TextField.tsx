import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  icon?: ReactNode;
  error?: string;
}

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  ({ label, icon, error, id, className, ...props }, ref) => (
    <div className="space-y-1.5">
      <label htmlFor={id} className="text-sm font-medium text-slate-700 dark:text-slate-300">
        {label}
      </label>
      <div className="relative">
        {icon && <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400">{icon}</span>}
        <input
          id={id}
          ref={ref}
          className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-cyan-500/20 dark:bg-slate-800/60 dark:text-slate-100 dark:placeholder:text-slate-500 ${
            icon ? 'pl-10' : ''
          } ${error ? 'border-red-400 dark:border-red-500' : 'border-slate-300 dark:border-slate-700'} ${className ?? ''}`}
          {...props}
        />
      </div>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  ),
);
TextField.displayName = 'TextField';
