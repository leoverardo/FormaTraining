import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { LoadingState } from '../../components/ui/LoadingState';
import { PageContainer } from '../../components/ui/PageContainer';
import { ContentGrid } from '../../components/ui/ContentGrid';
import { SectionCard } from '../../components/ui/SectionCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { FeedCard } from '../../components/social/FeedCard';
import { WorkoutCard, CheckInCard, WeeklyScheduleCard, ProgressMetricCard } from '../../components/fitness/Cards';
import { mapPostToFeedItem, mockFeedFallback } from '../../features/feed/feedAdapter';
import { AlertCircle, FileText } from 'lucide-react';

const weekDays = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];

export function StudentDashboard() {
  const [data, setData] = useState(null);
  const [access, setAccess] = useState(null);
  const [loading, setLoading] = useState(true);
  const [posts, setPosts] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    studentAreaService.getAccessStatus()
      .then((r) => {
        const acc = r.data.data;
        setAccess(acc);
        if (!acc.allowed) return null;
        return Promise.all([
          studentAreaService.getDashboard().then((d) => setData(d.data.data)),
          studentAreaService.getPosts().then((p) => setPosts(p.data.data || [])),
        ]);
      })
      .finally(() => setLoading(false));
  }, []);

  const feedItems = useMemo(() => {
    const mapped = posts.map((post) => mapPostToFeedItem(post, { name: data?.trainerBrand || 'Seu personal', role: 'Trainer' }));
    return mapped.length ? mapped : mockFeedFallback;
  }, [posts, data?.trainerBrand]);

  if (loading) return <LoadingState />;

  if (!access?.allowed) {
    return (
      <PageContainer>
        <EmptyState
          icon={AlertCircle}
          title="Acesso indisponível"
          description={access?.message || 'Seu acesso está temporariamente indisponível.'}
        />
      </PageContainer>
    );
  }

  const today = new Date().getDay();
  const weekSchedule = weekDays.map((day, idx) => {
    const schedule = data?.weekSchedule?.find((item) => item.dayOfWeek === idx);
    return {
      day,
      active: !!schedule,
      label: schedule ? schedule.workoutName : 'Descanso',
      workoutId: schedule?.workoutId,
    };
  });

  return (
    <PageContainer className="space-y-5">
      <section className="rounded-3xl border border-slate-200 bg-gradient-to-r from-indigo-600 to-violet-600 p-5 sm:p-6 text-white shadow-[0_16px_36px_rgba(79,70,229,0.32)]">
        <p className="text-indigo-100 text-sm">Olá, {data?.studentName?.split(' ')[0]}</p>
        <h1 className="text-2xl sm:text-3xl font-bold mt-1">Seu plano de hoje está pronto</h1>
        <p className="text-indigo-100 mt-2 text-sm">Acompanhamento de {data?.trainerBrand || 'seu personal'} com treino, progresso e check-ins.</p>
      </section>

      <ContentGrid>
        <div className="lg:col-span-8 space-y-4">
          <WorkoutCard
            workout={data?.todayWorkout ? { name: data.todayWorkout.name, highlights: ['Exercícios principais definidos', 'Objetivo do dia ativo'], statusLabel: 'Treino pendente' } : null}
            onOpen={data?.todayWorkout ? () => navigate(`/student/workouts/${data.todayWorkout.id}`) : undefined}
          />

          <SectionCard title="Feed do personal" description="Dicas, aulas e avisos recentes">
            <div className="space-y-3">
              {feedItems.length ? feedItems.map((item) => <FeedCard key={item.id} item={item} />) : (
                <EmptyState icon={FileText} title="Ainda não há conteúdos por aqui" description="Quando seu personal publicar dicas, aulas ou avisos, eles aparecerão neste feed." />
              )}
            </div>
          </SectionCard>
        </div>

        <div className="lg:col-span-4 space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <ProgressMetricCard label="Último peso" value={data?.lastWeight ? `${data.lastWeight}kg` : '--'} hint="Registro mais recente" />
            <ProgressMetricCard label="Última medida" value={data?.lastWaist ? `${data.lastWaist}cm` : '--'} hint="Cintura" />
          </div>

          <CheckInCard
            status={data?.lastCheckInAt ? 'Check-in recente registrado' : 'Check-in pendente'}
            summary={data?.lastCheckInAt ? `Último envio em ${new Date(data.lastCheckInAt).toLocaleDateString('pt-BR')}` : 'Atualize seu feedback semanal'}
            onOpen={() => navigate('/student/check-in')}
          />

          <WeeklyScheduleCard
            items={weekSchedule.map((item, idx) => ({
              ...item,
              day: idx === today ? `${item.day} • Hoje` : item.day,
            }))}
          />
        </div>
      </ContentGrid>
    </PageContainer>
  );
}


