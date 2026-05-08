import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { Menu, LogOut } from 'lucide-react';
import { NotificationBell } from '../components/ui/NotificationBell';

function SidebarItems({ groups, onNavigate }) {
  return (
    <nav className="px-3 py-4 space-y-4 overflow-y-auto">
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
                key={to}
                to={to}
                end={end}
                onClick={onNavigate}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    isActive
                      ? 'bg-indigo-50 text-indigo-700 shadow-[inset_0_0_0_1px_rgba(99,102,241,0.16)]'
                      : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
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
    <aside className="hidden md:flex md:flex-col w-64 bg-white border-r border-slate-200 shrink-0">
      <div className="px-4 py-5 border-b border-slate-200">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-indigo-600 flex items-center justify-center text-white font-bold text-xs">FP</div>
          <div className="min-w-0">
            <p className="font-semibold text-slate-900 text-sm leading-none">FitPlatform</p>
            <p className="text-xs text-slate-500 mt-1 truncate">{roleLabel || user?.name}</p>
          </div>
        </div>
      </div>
      <div className="flex-1 min-h-0">
        <SidebarItems groups={groups} />
      </div>
      <div className="px-3 py-3 border-t border-slate-200">
        <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 hover:bg-slate-50 w-full transition-colors">
          <LogOut size={18} />
          Sair
        </button>
      </div>
    </aside>
  );
}

function MobileDrawer({ open, onClose, groups, onLogout, user, roleLabel }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 sm:hidden">
      <button className="absolute inset-0 bg-slate-900/35 backdrop-blur-[1px]" onClick={onClose} aria-label="Fechar menu" />
      <aside className="absolute left-0 top-0 bottom-0 w-72 max-w-[88vw] bg-white border-r border-slate-200 flex flex-col">
        <div className="px-4 py-5 border-b border-slate-200">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-xl bg-indigo-600 flex items-center justify-center text-white font-bold text-xs">FP</div>
            <div className="min-w-0">
              <p className="font-semibold text-slate-900 text-sm leading-none">FitPlatform</p>
              <p className="text-xs text-slate-500 mt-1 truncate">{roleLabel || user?.name}</p>
            </div>
          </div>
        </div>
        <div className="flex-1 min-h-0">
          <SidebarItems groups={groups} onNavigate={onClose} />
        </div>
        <div className="px-3 py-3 border-t border-slate-200">
          <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 hover:bg-slate-50 w-full transition-colors">
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
    <div className="min-h-screen bg-slate-50 md:flex">
      <DesktopSidebar groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} />
      <MobileDrawer open={mobileOpen} onClose={() => setMobileOpen(false)} groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} />

      <div className="min-h-screen flex-1 min-w-0 flex flex-col">
        <header className="bg-white/95 backdrop-blur border-b border-slate-200 px-4 sm:px-6 h-16 flex items-center justify-between sticky top-0 z-40">
          <div className="flex items-center gap-3 min-w-0">
            <button onClick={() => setMobileOpen(true)} className="md:hidden p-2 rounded-lg hover:bg-slate-100 text-slate-500" aria-label="Abrir menu">
              <Menu size={20} />
            </button>
            <div className="md:hidden flex items-center gap-2 min-w-0">
              <div className="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white font-bold text-xs">FP</div>
              <span className="font-semibold text-slate-900 text-sm truncate">FitPlatform</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-600 hidden md:block">{user?.name}</span>
            <NotificationBell />
            <button onClick={onLogout} className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 md:hidden" aria-label="Sair">
              <LogOut size={18} />
            </button>
          </div>
        </header>

        <main className={`flex-1 px-4 sm:px-6 py-5 sm:py-6 ${useBottomNav ? 'pb-24 md:pb-6' : ''} ${contentClassName}`}>
          {children}
        </main>
      </div>

      {useBottomNav && (
        <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-slate-200 z-40 md:hidden">
          <div className="flex">
            {primaryItems.slice(0, 5).map(({ to, icon: Icon, label, end }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                className={({ isActive }) =>
                  `flex-1 flex flex-col items-center py-2 text-[11px] font-medium transition-colors ${isActive ? 'text-indigo-600' : 'text-slate-400'}`
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

