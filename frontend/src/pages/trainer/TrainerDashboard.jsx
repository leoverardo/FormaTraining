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
import { Plus, Users, ClipboardList, FileText, AlertTriangle, Globe } from 'lucide-react';

export function TrainerDashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { user } = useAuth();

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
      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-[0_12px_32px_rgba(15,23,42,0.08)]">
        <div className="flex flex-wrap gap-3 items-start justify-between">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">Painel do personal</h1>
            <p className="text-sm text-slate-500 mt-1">Resumo da consultoria e acoes rapidas.</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button size="sm" onClick={() => navigate('/trainer/students')}><Plus size={14} />Novo aluno</Button>
            <Button size="sm" variant="outline" onClick={() => navigate('/trainer/workouts')}>Novo treino</Button>
            <Button size="sm" variant="outline" onClick={() => navigate('/trainer/public-page')}><Globe size={14} />Pagina publica</Button>
          </div>
        </div>
      </section>

      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-4">
        <StatCard icon={Users} title="Alunos ativos" value={data.activeStudents} />
        <StatCard icon={ClipboardList} title="Treinos criados" value={data.totalWorkouts} />
        <StatCard icon={FileText} title="Conteudos" value={data.totalPublishedPosts || 0} />
        <StatCard icon={AlertTriangle} title="Check-ins pendentes" value={data.missingCheckInsCount || 0} />
        <StatCard icon={Users} title="Compromissos hoje" value={data.appointmentsTodayCount || 0} />
      </div>

      <ContentGrid>
        <div className="lg:col-span-8 space-y-4">
          <PostComposer onCreate={() => navigate('/trainer/posts')} />
          <SectionCard title="Atividade recente" description="Publicacoes e interacoes dos alunos">
            <div className="space-y-3">
              {(feed.length ? feed : mockFeedFallback).map((item, index) => (
                <FeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} />
              ))}
            </div>
          </SectionCard>
        </div>
        <div className="lg:col-span-4 space-y-4">
          <SectionCard title="Gestao rapida">
            <div className="grid grid-cols-1 gap-2">
              <Button variant="outline" onClick={() => navigate('/trainer/students')}>Gerenciar alunos</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/workouts')}>Gerenciar treinos</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/posts')}>Gerenciar conteudos</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/public-page')}>Editar pagina publica</Button>
              <Button variant="outline" onClick={() => navigate('/trainer/appointments')}>Abrir compromissos</Button>
            </div>
          </SectionCard>
        </div>
      </ContentGrid>
    </PageContainer>
  );
}
