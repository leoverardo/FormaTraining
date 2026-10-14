import { Moon, Sun } from 'lucide-react';
import { useTheme } from '../../contexts/ThemeContext';

export function ThemeToggle({ className = '' }) {
  const { isDark, toggleTheme } = useTheme();
  return (
    <button type="button" onClick={toggleTheme} className={`uc-icon-button ${className}`}
      aria-label={isDark ? 'Ativar tema claro' : 'Ativar tema escuro'}>
      {isDark ? <Sun size={18} /> : <Moon size={18} />}
    </button>
  );
}
