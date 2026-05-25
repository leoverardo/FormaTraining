import { useEffect, useRef, useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { useI18n } from '../../i18n';

const languages = [
  { code: 'pt-BR', short: 'PT', labelKey: 'language.portuguese', flag: '🇧🇷' },
  { code: 'en-US', short: 'EN', labelKey: 'language.english', flag: '🇺🇸' },
];

export function LanguageSwitcher() {
  const { language, setLanguage, t } = useI18n();
  const [open, setOpen] = useState(false);
  const rootRef = useRef(null);
  const current = languages.find((item) => item.code === language) || languages[0];

  useEffect(() => {
    const onClickOutside = (event) => {
      if (!rootRef.current) return;
      if (!rootRef.current.contains(event.target)) setOpen(false);
    };
    const onEsc = (event) => {
      if (event.key === 'Escape') setOpen(false);
    };
    document.addEventListener('mousedown', onClickOutside);
    document.addEventListener('keydown', onEsc);
    return () => {
      document.removeEventListener('mousedown', onClickOutside);
      document.removeEventListener('keydown', onEsc);
    };
  }, []);

  return (
    <div className="relative" ref={rootRef}>
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        aria-label={t('language.change')}
        aria-expanded={open}
        aria-haspopup="menu"
        className="inline-flex items-center gap-2 rounded-full border border-slate-200 dark:border-white/10 bg-white/90 dark:bg-white/[0.04] px-2.5 py-1.5 text-xs font-medium text-slate-700 dark:text-slate-200 shadow-sm transition hover:bg-slate-100 dark:hover:bg-white/[0.08] hover:text-slate-900 dark:hover:text-white focus:outline-none focus:ring-2 focus:ring-violet-400/40"
      >
        <span className="flex h-5 w-5 items-center justify-center overflow-hidden rounded-full border border-black/5 dark:border-white/10 bg-white text-xs">
          {current.flag}
        </span>
        <span className="hidden sm:inline">{current.short}</span>
        <ChevronDown size={14} className={`transition ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div
          role="menu"
          className="absolute right-0 mt-2 w-44 overflow-hidden rounded-2xl border border-slate-200 dark:border-white/10 bg-white/95 dark:bg-slate-950/95 p-1 shadow-2xl shadow-black/10 dark:shadow-black/30 backdrop-blur-xl z-50"
        >
          {languages.map((item) => {
            const active = item.code === language;
            return (
              <button
                key={item.code}
                type="button"
                role="menuitem"
                onClick={() => {
                  setLanguage(item.code);
                  setOpen(false);
                }}
                className={`flex w-full items-center gap-2 rounded-xl px-3 py-2 text-sm transition ${
                  active
                    ? 'bg-violet-500/15 text-slate-900 dark:text-white'
                    : 'text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-white/[0.08] hover:text-slate-900 dark:hover:text-white'
                }`}
              >
                <span className="flex h-5 w-5 items-center justify-center overflow-hidden rounded-full border border-black/5 dark:border-white/10 bg-white text-xs">
                  {item.flag}
                </span>
                <span>{t(item.labelKey)}</span>
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}


