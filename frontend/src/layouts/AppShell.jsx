import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { Menu, LogOut, Moon, Sun } from 'lucide-react';
import { NotificationBell } from '../components/ui/NotificationBell';
import { LanguageSwitcher } from '../components/ui/LanguageSwitcher';
import { BrandLogo } from '../components/brand/BrandLogo';
import { useTheme } from '../contexts/ThemeContext';
import { useI18n } from '../i18n';
import { brand } from '../config/brand';

function SidebarItems({ groups, onNavigate }) {
  return (
    <nav className="px-3 py-4 space-y-4 overflow-y-auto">
      {groups.map((group, groupIndex) => (
        <div key={`${group.label || 'group'}-${groupIndex}`}>
          {group.label && (
            <p className="px-3 mb-1.5 text-[10px] font-semibold uppercase tracking-[0.12em] text-slate-400 dark:text-slate-500">
              {group.label}
            </p>
          )}
          <div className="space-y-1">
            {group.items.map(({ to, icon: Icon, label, end, labelKey }) => (
              <NavLink
                key={`${group.label || 'group'}-${label || labelKey}-${to}`}
                to={to}
                end={end}
                onClick={onNavigate}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    isActive
                      ? 'bg-indigo-50 dark:bg-indigo-500/15 text-indigo-700 dark:text-indigo-200 shadow-[inset_0_0_0_1px_rgba(99,102,241,0.16)]'
                      : 'text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-white/5 hover:text-slate-900 dark:hover:text-white'
                  }`
                }
              >
                <Icon size={18} className="shrink-0" />
                <span className="truncate">{label || labelKey}</span>
              </NavLink>
            ))}
          </div>
        </div>
      ))}
    </nav>
  );
}

function DesktopSidebar({ groups, onLogout, user, roleLabel, appName, logoutLabel }) {
  return (
    <aside className="hidden md:sticky md:top-0 md:flex md:h-screen md:flex-col w-64 bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-white/10 shrink-0">
      <div className="px-4 py-5 border-b border-slate-200 dark:border-white/10">
        <div className="flex items-center gap-3">
          <BrandLogo showText={false} />
          <div className="min-w-0">
            <p className="font-semibold text-slate-900 dark:text-white text-sm leading-none">{appName}</p>
            <p className="text-xs text-slate-500 dark:text-slate-400 mt-1 truncate">{roleLabel || user?.name}</p>
          </div>
        </div>
      </div>
      <div className="flex-1 min-h-0">
        <SidebarItems groups={groups} />
      </div>
      <div className="px-3 py-3 border-t border-slate-200 dark:border-white/10 mt-auto shrink-0">
        <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-white/5 w-full transition-colors">
          <LogOut size={18} />
          {logoutLabel}
        </button>
      </div>
    </aside>
  );
}

function MobileDrawer({ open, onClose, groups, onLogout, user, roleLabel, appName, logoutLabel, closeMenuLabel }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 sm:hidden">
      <button className="absolute inset-0 bg-slate-900/35 backdrop-blur-[1px]" onClick={onClose} aria-label={closeMenuLabel} />
      <aside className="absolute left-0 top-0 bottom-0 w-72 max-w-[88vw] bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-white/10 flex flex-col">
        <div className="px-4 py-5 border-b border-slate-200 dark:border-white/10">
          <div className="flex items-center gap-3">
            <BrandLogo showText={false} />
            <div className="min-w-0">
              <p className="font-semibold text-slate-900 dark:text-white text-sm leading-none">{appName}</p>
              <p className="text-xs text-slate-500 dark:text-slate-400 mt-1 truncate">{roleLabel || user?.name}</p>
            </div>
          </div>
        </div>
        <div className="flex-1 min-h-0">
          <SidebarItems groups={groups} onNavigate={onClose} />
        </div>
        <div className="px-3 py-3 border-t border-slate-200 dark:border-white/10">
          <button onClick={onLogout} className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-500 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-white/5 w-full transition-colors">
            <LogOut size={18} />
            {logoutLabel}
          </button>
        </div>
      </aside>
    </div>
  );
}

export function AppShell({ children, user, groups, onLogout, roleLabel, contentClassName = '', useBottomNav = false }) {
  const [mobileOpen, setMobileOpen] = useState(false);
  const primaryItems = groups.flatMap((group) => group.items);
  const { isDark, toggleTheme } = useTheme();
  const { t } = useI18n();

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 md:flex">
      <DesktopSidebar groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} appName={brand.name || t('common.appName')} logoutLabel={t('common.logout')} />
      <MobileDrawer open={mobileOpen} onClose={() => setMobileOpen(false)} groups={groups} onLogout={onLogout} user={user} roleLabel={roleLabel} appName={brand.name || t('common.appName')} logoutLabel={t('common.logout')} closeMenuLabel={t('common.closeMenu')} />

      <div className="min-h-screen flex-1 min-w-0 flex flex-col">
        <header className="bg-white/95 dark:bg-slate-950/95 backdrop-blur border-b border-slate-200 dark:border-white/10 px-4 sm:px-6 h-16 flex items-center justify-between sticky top-0 z-40">
          <div className="flex items-center gap-3 min-w-0">
            <button onClick={() => setMobileOpen(true)} className="md:hidden p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-white/5 text-slate-500 dark:text-slate-300" aria-label={t('common.openMenu')}>
              <Menu size={20} />
            </button>
            <div className="md:hidden flex items-center gap-2 min-w-0">
              <BrandLogo size="sm" showText={false} />
              <span className="font-semibold text-slate-900 dark:text-white text-sm truncate max-w-[150px]" title={brand.name || t('common.appName')}>
                {brand.name || t('common.appName')}
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-600 dark:text-slate-300 hidden md:block">{user?.name}</span>
            <LanguageSwitcher />
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-white/5 text-slate-500 dark:text-slate-300"
              aria-label={isDark ? t('common.activateLightTheme') : t('common.activateDarkTheme')}
              title={isDark ? t('common.lightTheme') : t('common.darkTheme')}
            >
              {isDark ? <Sun size={18} /> : <Moon size={18} />}
            </button>
            <NotificationBell />
            <button onClick={onLogout} className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-white/5 text-slate-500 dark:text-slate-300 md:hidden" aria-label={t('common.logout')}>
              <LogOut size={18} />
            </button>
          </div>
        </header>

        <main className={`flex-1 px-4 sm:px-6 py-5 sm:py-6 ${useBottomNav ? 'pb-24 md:pb-6' : ''} ${contentClassName}`}>
          {children}
        </main>
      </div>

      {useBottomNav && (
        <nav className="fixed bottom-0 left-0 right-0 bg-white dark:bg-slate-900 border-t border-slate-200 dark:border-white/10 z-40 md:hidden">
          <div className="flex">
            {primaryItems.slice(0, 5).map(({ to, icon: Icon, label, labelKey, end }, index) => (
              <NavLink
                key={`${label || labelKey}-${to}-${index}`}
                to={to}
                end={end}
                className={({ isActive }) =>
                  `flex-1 flex flex-col items-center py-2 text-[11px] font-medium transition-colors ${isActive ? 'text-indigo-600' : 'text-slate-400 dark:text-slate-500'}`
                }
              >
                <Icon size={19} />
                <span className="mt-0.5">{(label || labelKey).split(' ')[0]}</span>
              </NavLink>
            ))}
          </div>
        </nav>
      )}
    </div>
  );
}
