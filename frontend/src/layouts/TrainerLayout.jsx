import { useNavigate } from 'react-router-dom';
import {
  LayoutDashboard, Users, Dumbbell, ClipboardList, CalendarDays,
  FileText, CreditCard, User, BarChart2, Globe, Library
} from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';

const groups = [
  {
    label: 'Visao geral',
    items: [
      { to: '/trainer', icon: LayoutDashboard, label: 'Dashboard', end: true },
      { to: '/trainer/reports', icon: BarChart2, label: 'Relatorios' },
    ],
  },
  {
    label: 'Alunos',
    items: [{ to: '/trainer/students', icon: Users, label: 'Alunos' }],
  },
  {
    label: 'Conteudo',
    items: [
      { to: '/trainer/exercises', icon: Dumbbell, label: 'Meus Exercicios' },
      { to: '/trainer/workouts', icon: ClipboardList, label: 'Treinos' },
      { to: '/trainer/library', icon: Library, label: 'Biblioteca Base' },
      { to: '/trainer/schedule', icon: CalendarDays, label: 'Rotina Semanal' },
      { to: '/trainer/posts', icon: FileText, label: 'Conteudos' },
    ],
  },
  {
    label: 'Conta',
    items: [
      { to: '/trainer/public-page', icon: Globe, label: 'Pagina Publica' },
      { to: '/trainer/subscription', icon: CreditCard, label: 'Assinatura' },
      { to: '/trainer/profile', icon: User, label: 'Meu Perfil' },
    ],
  },
];

export function TrainerLayout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel="Trainer"
    >
      {children}
    </AppShell>
  );
}

