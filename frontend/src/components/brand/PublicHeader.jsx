import { Link } from 'react-router-dom';
import { BrandLogo } from './BrandLogo';
import { ThemeToggle } from '../ui/ThemeToggle';

export function PublicHeader() {
  return (
    <header className="border-b border-[var(--border)] bg-[var(--surface)]">
      <div className="uc-container flex min-h-16 items-center justify-between gap-4">
        <Link to="/landing" aria-label="UpCoach — início"><BrandLogo size="sm" /></Link>
        <div className="flex items-center gap-4">
          <Link to="/login" className="text-sm text-[var(--link)]">Entrar</Link>
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}
