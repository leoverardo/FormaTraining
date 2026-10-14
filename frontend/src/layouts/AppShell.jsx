import { useEffect, useRef, useState } from 'react';
import { NavLink } from 'react-router-dom';
import { Menu, LogOut } from 'lucide-react';
import { BrandLogo } from '../components/brand/BrandLogo';
import { NotificationBell } from '../components/ui/NotificationBell';
import { ThemeToggle } from '../components/ui/ThemeToggle';
import { useDialogFocus } from '../hooks/useDialogFocus';

function SidebarItems({ groups, onNavigate }) {
  return (
    <nav className="h-full px-3 py-4 space-y-4 overflow-y-auto">
      {groups.map((group, groupIndex) => (
        <div key={`${group.label || 'group'}-${groupIndex}`}>
          {group.label && (
            <p className="px-3 mb-1.5 text-[10px] font-semibold uppercase tracking-[0.12em] text-slate-400">
              {group.label}
            </p>
          )}
          <div className="space-y-1">
            {group.items.map(({ to, icon: Icon, label, end }) => (
              <NavLink
                key={`${group.label || 'group'}-${label}-${to}`}
                to={to}
                end={end}
                onClick={onNavigate}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    isActive
                      ? 'bg-[var(--primary-soft)] text-[var(--link)]'
                      : 'text-slate-600 hover:bg-[var(--surface-muted)] hover:text-slate-900'
                  }`
                }
              >
                <Icon size={18} className="shrink-0" />
                <span className="truncate">{label}</span>
              </NavLink>
            ))}
          </div>
        </div>
      ))}
    </nav>
  );
}

function DesktopSidebar({ groups, onLogout, user, roleLabel }) {
  return (
    <aside className="hidden md:sticky md:top-0 md:flex md:h-screen md:flex-col w-64 bg-[var(--surface)] border-r border-slate-200 shrink-0">
      <div className="px-4 py-5 border-b border-slate-200">
        <div className="min-w-0">
          <BrandLogo size="sm" />
          <p className="text-xs text-slate-500 mt-1.5 truncate">{roleLabel || user?.name}</p>
        </div>
      </div>
      <div className="flex-1 min-h-0">
        <SidebarItems groups={groups} />
      </div>
      <div className="px-3 py-3 border-t border-slate-200 mt-auto shrink-0">
        <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 hover:bg-[var(--surface-muted)] w-full transition-colors">
          <LogOut size={18} />
          Sair
        </button>
      </div>
    </aside>
  );
}

function MobileDrawer({ open, onClose, groups, onLogout, user, roleLabel }) {
  const drawerRef = useRef(null);
  useDialogFocus(drawerRef, open, onClose, 'a[href]');
  useEffect(() => {
    if (!open) return undefined;
    const desktop = window.matchMedia('(min-width: 768px)');
    const closeOnDesktop = () => { if (desktop.matches) onClose(); };
    closeOnDesktop();
    desktop.addEventListener('change', closeOnDesktop);
    return () => desktop.removeEventListener('change', closeOnDesktop);
  }, [open, onClose]);
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 md:hidden">
      <button className="absolute inset-0 bg-black/35 backdrop-blur-[1px]" onClick={onClose} aria-label="Fechar menu" />
      <aside ref={drawerRef} tabIndex={-1} role="dialog" aria-modal="true" aria-label="Menu de navegação" className="absolute left-0 top-0 bottom-0 w-72 max-w-[88vw] bg-[var(--surface)] border-r border-slate-200 flex flex-col">
        <div className="px-4 py-5 border-b border-slate-200">
          <div className="min-w-0">
            <BrandLogo size="sm" />
            <p className="text-xs text-slate-500 mt-1.5 truncate">{roleLabel || user?.name}</p>
          </div>
        </div>
        <div className="flex-1 min-h-0">
          <SidebarItems groups={groups} onNavigate={onClose} />
        </div>
        <div className="px-3 py-3 border-t border-slate-200">
          <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 hover:bg-[var(--surface-muted)] w-full transition-colors">
            <LogOut size={18} />
            Sair
          </button>
        </div>
      </aside>
    </div>
  );
}

export function AppShell({ children, user, groups, onLogout, roleLabel, contentClassName = '', useBottomNav = false }) {
  const [mobileOpen, setMobileOpen] = useState(false);
  const primaryItems = groups.flatMap(group => group.items);

  return (
    <div className="app-shell min-h-screen md:flex">
      <DesktopSidebar groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} />
      <MobileDrawer open={mobileOpen} onClose={() => setMobileOpen(false)} groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} />

      <div className="min-h-screen flex-1 min-w-0 flex flex-col">
        <header className="bg-[var(--surface)]/95 backdrop-blur border-b border-slate-200 px-4 sm:px-6 h-16 flex items-center justify-between sticky top-0 z-40">
          <div className="flex items-center gap-3 min-w-0">
            <button onClick={() => setMobileOpen(true)} className="md:hidden p-2 rounded-lg hover:bg-[var(--surface-subtle)] text-slate-500" aria-label="Abrir menu">
              <Menu size={20} />
            </button>
            <div className="md:hidden flex items-center gap-2 min-w-0">
              <BrandLogo size="xs" />
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-600 hidden md:block">{user?.name}</span>
            <ThemeToggle />
            <NotificationBell />
            <button onClick={onLogout} className="p-2 rounded-lg hover:bg-[var(--surface-subtle)] text-slate-500 md:hidden" aria-label="Sair">
              <LogOut size={18} />
            </button>
          </div>
        </header>

        <main className={`app-content flex-1 px-5 sm:px-8 py-8 ${useBottomNav ? 'pb-24 md:pb-8' : ''} ${contentClassName}`}>
          {children}
        </main>
      </div>

      {useBottomNav && (
        <nav className="fixed bottom-0 left-0 right-0 bg-[var(--surface)] border-t border-slate-200 z-40 md:hidden">
          <div className="flex">
            {primaryItems.slice(0, 5).map(({ to, icon: Icon, label, end }, index) => (
              <NavLink
                key={`${label}-${to}-${index}`}
                to={to}
                end={end}
                className={({ isActive }) =>
                  `flex-1 flex flex-col items-center py-2 text-[11px] font-medium transition-colors ${isActive ? 'text-[var(--link)]' : 'text-slate-400'}`
                }
              >
                <Icon size={19} />
                <span className="mt-0.5">{label.split(' ')[0]}</span>
              </NavLink>
            ))}
          </div>
        </nav>
      )}
    </div>
  );
}

