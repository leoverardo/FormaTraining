import { useNavigate } from 'react-router-dom';
import { LayoutDashboard, Tag } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';

const groups = [{
  label: 'Administracao',
  items: [
    { to: '/owner', icon: LayoutDashboard, label: 'Dashboard', end: true },
    { to: '/owner/plans', icon: Tag, label: 'Planos' },
  ],
}];

export function OwnerLayout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel="Owner"
    >
      {children}
    </AppShell>
  );
}

