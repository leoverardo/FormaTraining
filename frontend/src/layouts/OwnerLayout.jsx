import { useNavigate } from 'react-router-dom';
import { LayoutDashboard, ShieldCheck, Tag } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';
import { useI18n } from '../i18n';

export function OwnerLayout({ children }) {
  const { user, logout } = useAuth();
  const { t } = useI18n();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };
  const groups = [{
    label: t('nav.administration'),
    items: [
      { to: '/owner', icon: LayoutDashboard, label: t('common.dashboard'), end: true },
      { to: '/owner/plans', icon: Tag, label: t('nav.plans') },
      { to: '/owner/privacy', icon: ShieldCheck, label: t('nav.privacyLgpd') },
    ],
  }];

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel={t('roles.Owner')}
    >
      {children}
    </AppShell>
  );
}

