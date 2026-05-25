import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import ptBR from './locales/pt-BR.json';
import enUS from './locales/en-US.json';

export const LANGUAGE_STORAGE_KEY = 'formaTraining.language';
const DEFAULT_LANGUAGE = 'pt-BR';

const dictionaries = {
  'pt-BR': ptBR,
  'en-US': enUS
};

function getByPath(obj, path) {
  return path.split('.').reduce((acc, key) => (acc && acc[key] !== undefined ? acc[key] : undefined), obj);
}

function formatTemplate(text, params = {}) {
  return Object.entries(params).reduce(
    (acc, [key, value]) => acc.replaceAll(`{{${key}}}`, String(value)),
    text
  );
}

function getInitialLanguage() {
  const saved = window.localStorage.getItem(LANGUAGE_STORAGE_KEY);
  return saved && dictionaries[saved] ? saved : DEFAULT_LANGUAGE;
}

const I18nContext = createContext({
  language: DEFAULT_LANGUAGE,
  setLanguage: () => {},
  t: (key) => key
});

export function I18nProvider({ children }) {
  const [language, setLanguage] = useState(getInitialLanguage);

  useEffect(() => {
    window.localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
    document.documentElement.lang = language;
  }, [language]);

  const value = useMemo(() => {
    const dictionary = dictionaries[language] || dictionaries[DEFAULT_LANGUAGE];
    const fallback = dictionaries[DEFAULT_LANGUAGE];

    const t = (key, params) => {
      const fromCurrent = getByPath(dictionary, key);
      const fromFallback = getByPath(fallback, key);
      const raw = fromCurrent ?? fromFallback ?? key;
      return typeof raw === 'string' ? formatTemplate(raw, params) : raw;
    };

    return { language, setLanguage, t };
  }, [language]);

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n() {
  return useContext(I18nContext);
}
