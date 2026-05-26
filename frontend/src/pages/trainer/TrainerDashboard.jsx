import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { trainerService } from '../../services/trainerService';
import { LoadingState } from '../../components/ui/LoadingState';
import { Button } from '../../components/ui/Button';
import { PageContainer } from '../../components/ui/PageContainer';
import { ContentGrid } from '../../components/ui/ContentGrid';
import { SectionCard } from '../../components/ui/SectionCard';
import { StatCard } from '../../components/ui/StatCard';
import { FeedCard } from '../../components/social/FeedCard';
import { PostComposer } from '../../components/social/PostComposer';
import { mapPostToFeedItem, mockFeedFallback } from '../../features/feed/feedAdapter';
import { useAuth } from '../../contexts/AuthContext';
import { useI18n } from '../../i18n';
import { Plus, Users, ClipboardList, FileText, AlertTriangle, Globe } from 'lucide-react';

export function TrainerDashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { user } = useAuth();
  const { t } = useI18n();

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const response = await trainerService.getDashboard();
        setData(response.data.data);
      } catch (error) {
        console.error('Erro ao carregar dashboard do treinador', {
          status: error?.response?.status,
          data: error?.response?.data,
          message: error?.message,
        });
        setData(null);
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

  if (loading) return <LoadingState />;
  if (!data) return null;

  const feed = (data.recentActivities || []).map((post) => mapPostToFeedItem(post, { name: user?.name || 'Personal', role: 'Trainer' }));

  return (
    <PageContainer className="space-y-5">
      <section className="rounded-3xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 p-6 shadow-[0_12px_32px_rgba(15,23,42,0.08)] dark:shadow-[0_16px_36px_rgba(2,6,23,0.45)]">
        <div className="flex flex-wrap gap-3 items-start justify-between">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white">{t('trainer.dashboard.title')}</h1>
            <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">{t('trainer.dashboard.subtitle')}</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button size="sm" onClick={() => navigate('/trainer/students')}><Plus size={14} />{t('trainer.dashboard.newStudent')}</Button>
            <Button size="sm" variant="outline" onClick={() => navigate('/trainer/workouts')}>{t('trainer.dashboard.newWorkout')}</Button>
            <Button size="sm" variant="outline" onClick={() => navigate('/trainer/public-page')}><Globe size={14} />{t('trainer.dashboard.publicPage')}</Button>
          </div>
        </div>
      </section>

      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-4">
        <StatCard icon={Users} title={t('trainer.dashboard.stats.activeStudents')} value={data.activeStudents} />
        <StatCard icon={ClipboardList} title={t('trainer.dashboard.stats.createdWorkouts')} value={data.totalWorkouts} />
        <StatCard icon={FileText} title={t('trainer.dashboard.stats.contents')} value={data.totalPublishedPosts || 0} />
        <StatCard icon={AlertTriangle} title={t('trainer.dashboard.stats.pendingCheckins')} value={data.missingCheckInsCount || 0} />
        <StatCard icon={Users} title={t('trainer.dashboard.stats.todayAppointments')} value={data.appointmentsTodayCount || 0} />
      </div>

      <ContentGrid>
        <div className="lg:col-span-8 space-y-4">
          <PostComposer onCreate={() => navigate('/trainer/posts')} />
          <SectionCard title={t('trainer.dashboard.recentActivityTitle')} description={t('trainer.dashboard.recentActivityDescription')}>
            <div className="space-y-3">
              {(feed.length ? feed : mockFeedFallback).map((item, index) => (
                <FeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} />
              ))}
            </div>
          </SectionCard>
        </div>
        <div className="lg:col-span-4 space-y-4">
          <SectionCard title={t('trainer.dashboard.quickActionsTitle')}>
            <div className="grid grid-cols-1 gap-2">
              <Button variant="outline" onClick={() => navigate('/trainer/students')}>{t('trainer.dashboard.manageStudents')}</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/workouts')}>{t('trainer.dashboard.manageWorkouts')}</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/posts')}>{t('trainer.dashboard.manageContents')}</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/public-page')}>{t('trainer.dashboard.editPublicPage')}</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/appointments')}>{t('trainer.dashboard.openAppointments')}</Button>
            </div>
          </SectionCard>
        </div>
      </ContentGrid>
    </PageContainer>
  );
}
