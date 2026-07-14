import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import az from './locales/az.json';
import en from './locales/en.json';
import { queryClient } from '../app/queryClient';

const STORAGE_KEY = 'questcraft-language';

const storedLanguage = localStorage.getItem(STORAGE_KEY);

i18n.use(initReactI18next).init({
  resources: {
    az: { translation: az },
    en: { translation: en },
  },
  lng: storedLanguage === 'en' ? 'en' : 'az',
  fallbackLng: 'az',
  interpolation: { escapeValue: false },
});

i18n.on('languageChanged', (language) => {
  localStorage.setItem(STORAGE_KEY, language);
  document.documentElement.lang = language;
  // Server-rendered content (challenge/quiz/achievement/etc. text) is localized via the X-Language
  // header, so switching AZ/EN needs a refetch — the query keys themselves don't change.
  queryClient.invalidateQueries();
});

document.documentElement.lang = i18n.language;

export default i18n;
