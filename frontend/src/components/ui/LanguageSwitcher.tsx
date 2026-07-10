import { useTranslation } from 'react-i18next';

const languages = [
  { code: 'az', label: 'AZ' },
  { code: 'en', label: 'EN' },
] as const;

export function LanguageSwitcher() {
  const { i18n, t } = useTranslation();

  return (
    <div
      role="group"
      aria-label={t('common.toggleLanguage')}
      className="flex items-center rounded-full border border-slate-200 bg-white p-0.5 text-xs font-medium dark:border-slate-700 dark:bg-slate-800"
    >
      {languages.map((lang) => (
        <button
          key={lang.code}
          type="button"
          onClick={() => i18n.changeLanguage(lang.code)}
          className={`rounded-full px-2.5 py-1 transition ${
            i18n.language === lang.code
              ? 'bg-indigo-600 text-white'
              : 'text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-100'
          }`}
        >
          {lang.label}
        </button>
      ))}
    </div>
  );
}
