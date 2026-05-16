import { useNavigate } from 'react-router-dom';
import { LayoutDashboard, ClipboardList, FileText, Shield, TrendingUp, Camera, CalendarCheck, ClipboardCheck, MessageCircle } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';

const linkedGroups = [{
  items: [
    { to: '/student/dashboard', icon: LayoutDashboard, label: 'Dashboard', end: true },
    { to: '/student/workouts', icon: ClipboardList, label: 'Meus Treinos' },
    { to: '/student/appointments', icon: CalendarCheck, label: 'Compromissos' },
    { to: '/student/check-in', icon: CalendarCheck, label: 'Check-in' },
    { to: '/student/anamnesis', icon: ClipboardCheck, label: 'Perfil de Treino' },
    { to: '/student/posts', icon: FileText, label: 'Conteudos' },
    { to: '/student/messages', icon: MessageCircle, label: 'Mensagens' },
    { to: '/student/progress', icon: TrendingUp, label: 'Progresso' },
    { to: '/student/photos', icon: Camera, label: 'Fotos' },
    { to: '/student/access', icon: Shield, label: 'Meu Acesso' },
    { to: '/student/privacy', icon: Shield, label: 'Privacidade' },
    { to: '/explore', icon: LayoutDashboard, label: 'Explorar' },
    { to: '/explore/trainers', icon: ClipboardList, label: 'Personais' },
  ],
}];

const explorerGroups = [{
  items: [
    { to: '/explore', icon: LayoutDashboard, label: 'Explorar', end: true },
    { to: '/explore/trainers', icon: ClipboardList, label: 'Personais' },
    { to: '/explore/saved', icon: TrendingUp, label: 'Salvos' },
    { to: '/explore/following', icon: FileText, label: 'Seguindo' },
    { to: '/student/profile', icon: Shield, label: 'Meu Perfil' },
    { to: '/student/privacy', icon: Shield, label: 'Privacidade' },
  ],
}];

export function StudentLayout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };
  const groups = user?.hasActiveTrainerLink ? linkedGroups : explorerGroups;

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

