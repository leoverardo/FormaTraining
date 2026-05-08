import { useNavigate } from 'react-router-dom';
import { LayoutDashboard, ClipboardList, FileText, Shield, TrendingUp, Camera, CalendarCheck, ClipboardCheck } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';

const groups = [{
  items: [
    { to: '/student', icon: LayoutDashboard, label: 'Dashboard', end: true },
    { to: '/student/workouts', icon: ClipboardList, label: 'Meus Treinos' },
    { to: '/student/check-in', icon: CalendarCheck, label: 'Check-in' },
    { to: '/student/anamnesis', icon: ClipboardCheck, label: 'Anamnese' },
    { to: '/student/posts', icon: FileText, label: 'Conteudos' },
    { to: '/student/progress', icon: TrendingUp, label: 'Progresso' },
    { to: '/student/photos', icon: Camera, label: 'Fotos' },
    { to: '/student/access', icon: Shield, label: 'Meu Acesso' },
  ],
}];

export function StudentLayout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel="Aluno"
      useBottomNav
    >
      {children}
    </AppShell>
  );
}

