import { useNavigate } from 'react-router-dom';
import {
  LayoutDashboard, Users, Dumbbell, ClipboardList, CalendarDays,
  FileText, CreditCard, User, BarChart2, Globe, Library, Inbox, MessageCircle, ShoppingBag
} from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';
import { useI18n } from '../i18n';

export function TrainerLayout({ children }) {
  const { user, logout } = useAuth();
  const { t } = useI18n();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };
  const groups = [
    {
      label: t('nav.overview'),
      items: [
        { to: '/trainer', icon: LayoutDashboard, label: t('common.dashboard'), end: true },
        { to: '/trainer/reports', icon: BarChart2, label: t('nav.reports') },
      ],
    },
    {
      label: t('nav.students'),
      items: [{ to: '/trainer/students', icon: Users, label: t('nav.students') }],
    },
    {
      label: t('nav.content'),
      items: [
        { to: '/trainer/exercises', icon: Dumbbell, label: t('nav.myExercises') },
        { to: '/trainer/workouts', icon: ClipboardList, label: t('nav.workouts') },
        { to: '/trainer/library', icon: Library, label: t('nav.baseLibrary') },
        { to: '/trainer/schedule', icon: CalendarDays, label: t('nav.weeklyRoutine') },
        { to: '/trainer/appointments', icon: CalendarDays, label: t('nav.appointments') },
        { to: '/trainer/posts', icon: FileText, label: t('nav.contents') },
        { to: '/trainer/messages', icon: MessageCircle, label: t('nav.messages') },
      ],
    },
    {
      label: t('nav.account'),
      items: [
        { to: '/trainer/public-page', icon: Globe, label: t('nav.publicPage') },
        { to: '/trainer/leads', icon: Inbox, label: t('nav.leads') },
        { to: '/trainer/sales', icon: ShoppingBag, label: t('nav.servicesAndSales') },
        { to: '/trainer/subscription', icon: CreditCard, label: t('nav.subscription') },
        { to: '/trainer/profile', icon: User, label: t('nav.myProfile') },
        { to: '/trainer/privacy', icon: User, label: t('nav.privacyAndData') },
      ],
    },
  ];

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel={t('roles.Trainer')}
    >
      {children}
    </AppShell>
  );
}

