import { Check, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const rules: Array<{ key: string; test: (password: string) => boolean }> = [
  { key: 'minLength', test: (p) => p.length >= 8 },
  { key: 'uppercase', test: (p) => /[A-Z]/.test(p) },
  { key: 'lowercase', test: (p) => /[a-z]/.test(p) },
  { key: 'digit', test: (p) => /[0-9]/.test(p) },
];

export function PasswordStrengthChecklist({ password }: { password: string }) {
  const { t } = useTranslation();

  return (
    <ul className="grid grid-cols-2 gap-x-3 gap-y-1 text-xs">
      {rules.map((rule) => {
        const passed = rule.test(password);
        return (
          <li
            key={rule.key}
            className={`flex items-center gap-1.5 transition-colors ${
              passed ? 'text-emerald-600 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'
            }`}
          >
            {passed ? <Check size={13} /> : <X size={13} />}
            {t(`auth.register.passwordRules.${rule.key}`)}
          </li>
        );
      })}
    </ul>
  );
}
