import { useNavigate } from 'react-router-dom';
import { LayoutDashboard, ClipboardList, FileText, Shield, TrendingUp, Camera, CalendarCheck, ClipboardCheck, MessageCircle } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { AppShell } from './AppShell';
import { useI18n } from '../i18n';

export function StudentLayout({ children }) {
  const { user, logout } = useAuth();
  const { t } = useI18n();
  const navigate = useNavigate();
  const handleLogout = () => { logout(); navigate('/login'); };
  const linkedGroups = [{
    items: [
      { to: '/student/dashboard', icon: LayoutDashboard, label: t('common.dashboard'), end: true },
      { to: '/student/workouts', icon: ClipboardList, label: t('nav.myWorkouts') },
      { to: '/student/appointments', icon: CalendarCheck, label: t('nav.appointments') },
      { to: '/student/check-in', icon: CalendarCheck, label: t('nav.checkin') },
      { to: '/student/anamnesis', icon: ClipboardCheck, label: t('nav.trainingProfile') },
      { to: '/student/posts', icon: FileText, label: t('nav.contents') },
      { to: '/student/messages', icon: MessageCircle, label: t('nav.messages') },
      { to: '/student/progress', icon: TrendingUp, label: t('nav.progress') },
      { to: '/student/photos', icon: Camera, label: t('nav.photos') },
      { to: '/student/access', icon: Shield, label: t('nav.myAccess') },
      { to: '/student/privacy', icon: Shield, label: t('nav.privacy') },
      { to: '/explore', icon: LayoutDashboard, label: t('nav.explore') },
      { to: '/explore/trainers', icon: ClipboardList, label: t('nav.trainers') },
    ],
  }];
  const explorerGroups = [{
    items: [
      { to: '/explore', icon: LayoutDashboard, label: t('nav.explore'), end: true },
      { to: '/explore/trainers', icon: ClipboardList, label: t('nav.trainers') },
      { to: '/explore/saved', icon: TrendingUp, label: t('nav.saved') },
      { to: '/explore/following', icon: FileText, label: t('nav.following') },
      { to: '/student/profile', icon: Shield, label: t('nav.myProfile') },
      { to: '/student/privacy', icon: Shield, label: t('nav.privacy') },
    ],
  }];
  const groups = user?.hasActiveTrainerLink ? linkedGroups : explorerGroups;

  return (
    <AppShell
      user={user}
      groups={groups}
      onLogout={handleLogout}
      roleLabel={t('roles.Student')}
      useBottomNav
    >
      {children}
    </AppShell>
  );
}

