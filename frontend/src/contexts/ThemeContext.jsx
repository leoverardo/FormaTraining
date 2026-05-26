import { createContext, useContext, useEffect, useMemo, useState } from 'react';

/**
 * Chave canônica do localStorage para tema da Forma Training.
 * Chaves antigas (fitplatform-theme, theme, darkMode, vite-ui-theme,
 * color-theme, appearance) são lidas uma vez para migração e descartadas.
 */
const THEME_KEY = 'forma-training-theme';

/** Chaves legadas que podem conter um valor de tema anterior. */
const LEGACY_KEYS = [
  'fitplatform-theme',
  'theme',
  'darkMode',
  'vite-ui-theme',
  'color-theme',
  'appearance',
];

/** Valores válidos de tema. Qualquer outro valor é normalizado para 'dark'. */
const VALID_THEMES = ['dark', 'light'];

function normalizeTheme(value) {
  return VALID_THEMES.includes(value) ? value : 'dark';
}

/**
 * Tenta recuperar o tema salvo — primeiro na chave canônica,
 * depois nas chaves legadas (migração única).
 * Padrão: 'dark'.
 */
function getInitialTheme() {
  if (typeof window === 'undefined') return 'dark';

  // 1. Chave canônica
  const stored = window.localStorage.getItem(THEME_KEY);
  if (stored) return normalizeTheme(stored);

  // 2. Migração: chaves legadas
  for (const key of LEGACY_KEYS) {
    const legacy = window.localStorage.getItem(key);
    if (legacy) return normalizeTheme(legacy);
  }

  // 3. Padrão: dark
  return 'dark';
}

/**
 * Aplica o tema no document.documentElement de forma atômica:
 * - remove .dark e .light antes de adicionar o correto (evita flash duplo)
 * - define data-theme para seletores CSS legados
 * - define color-scheme para o navegador (scrollbar, inputs nativos)
 */
function applyTheme(theme) {
  const root = document.documentElement;
  root.classList.remove('dark', 'light');
  root.classList.add(theme);
  root.dataset.theme = theme;
  root.style.colorScheme = theme;
}

const ThemeContext = createContext(null);

export function ThemeProvider({ children }) {
  const [theme, setThemeState] = useState(getInitialTheme);

  // Aplica no DOM e persiste sempre que o tema muda
  useEffect(() => {
    applyTheme(theme);
    window.localStorage.setItem(THEME_KEY, theme);

    // Remove chaves legadas para evitar confusão em sessões futuras
    LEGACY_KEYS.forEach((key) => window.localStorage.removeItem(key));
  }, [theme]);

  const setTheme = (value) => {
    setThemeState(normalizeTheme(value));
  };

  const value = useMemo(
    () => ({
      theme,
      isDark: theme === 'dark',
      setTheme,
      toggleTheme: () => setThemeState((current) => (current === 'dark' ? 'light' : 'dark')),
    }),
    [theme]
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}
