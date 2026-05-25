import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { habitService } from '../../services/habitService';
import { gamificationService } from '../../services/gamificationService';
import { LoadingState } from '../../components/ui/LoadingState';
import { PageContainer } from '../../components/ui/PageContainer';
import { ContentGrid } from '../../components/ui/ContentGrid';
import { SectionCard } from '../../components/ui/SectionCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { FeedCard } from '../../components/social/FeedCard';
import { WorkoutCard, CheckInCard, WeeklyScheduleCard, ProgressMetricCard } from '../../components/fitness/Cards';
import { mapPostToFeedItem, mockFeedFallback } from '../../features/feed/feedAdapter';
import { AlertCircle, FileText } from 'lucide-react';
import { useI18n } from '../../i18n';

const weekDays = ['Domingo', 'Segunda', 'Terca', 'Quarta', 'Quinta', 'Sexta', 'Sabado'];

export function StudentDashboard() {
  const { t } = useI18n();
  const [data, setData] = useState(null);
  const [access, setAccess] = useState(null);
  const [loading, setLoading] = useState(true);
  const [posts, setPosts] = useState([]);
  const [habitsToday, setHabitsToday] = useState(null);
  const [nutrition, setNutrition] = useState(null);
  const [updatingHabitId, setUpdatingHabitId] = useState(null);
  const [gamification, setGamification] = useState(null);
  const [achievements, setAchievements] = useState([]);
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
          habitService.getToday().then((h) => setHabitsToday(h.data.data || null)),
          habitService.getStudentGuidance().then((g) => setNutrition(g.data.data || null)),
          gamificationService.getStudentSummary().then((g) => setGamification(g.data.data || null)),
          gamificationService.getStudentAchievements().then((a) => setAchievements((a.data.data || []).filter((x) => x.unlocked).slice(0, 4))),
        ]);
      })
      .finally(() => setLoading(false));
  }, []);

  const toggleHabit = async (item) => {
    setUpdatingHabitId(item.habitId);
    try {
      const response = await habitService.updateToday(item.habitId, {
        isCompleted: !item.isCompleted,
        value: item.value,
        note: item.note,
      });
      const updated = response.data.data;
      setHabitsToday((prev) => {
        if (!prev) return prev;
        const items = prev.items.map((i) => (i.habitId === updated.habitId ? { ...i, ...updated } : i));
        return { ...prev, items, completedHabits: items.filter((i) => i.isCompleted).length };
      });
    } finally {
      setUpdatingHabitId(null);
    }
  };

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
          title={t('student.dashboard.accessUnavailable')}
          description={access?.message || t('student.dashboard.accessTemporarilyUnavailable')}
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
      label: schedule ? schedule.workoutName : t('student.dashboard.rest'),
      workoutId: schedule?.workoutId,
    };
  });

  return (
    <PageContainer className="space-y-5">
      <section className="rounded-3xl border border-slate-200 bg-gradient-to-r from-indigo-600 to-violet-600 p-5 sm:p-6 text-white shadow-[0_16px_36px_rgba(79,70,229,0.32)]">
        <p className="text-indigo-100 text-sm">Ola, {data?.studentName?.split(' ')[0]}</p>
        <h1 className="text-2xl sm:text-3xl font-bold mt-1">Seu plano de hoje esta pronto</h1>
        <p className="text-indigo-100 mt-2 text-sm">Acompanhamento de {data?.trainerBrand || 'seu personal'} com treino, progresso e check-ins.</p>
      </section>

      <ContentGrid>
        <div className="lg:col-span-8 space-y-4">
          <WorkoutCard
            workout={data?.todayWorkout ? { name: data.todayWorkout.name, highlights: ['Exercicios principais definidos', 'Objetivo do dia ativo'], statusLabel: 'Treino pendente' } : null}
            onOpen={data?.todayWorkout ? () => navigate(`/student/workouts/${data.todayWorkout.id}`) : undefined}
          />

          <SectionCard title={t('student.dashboard.feedTitle')} description={t('student.dashboard.feedDescription')}>
            <div className="space-y-3">
              {feedItems.length ? feedItems.map((item, index) => <FeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} />) : (
                <EmptyState icon={FileText} title={t('student.posts.emptyTitle')} description={t('student.posts.emptyDescription')} />
              )}
            </div>
          </SectionCard>
        </div>

        <div className="lg:col-span-4 space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <ProgressMetricCard label="Ultimo peso" value={data?.lastWeight ? `${data.lastWeight}kg` : '--'} hint="Registro mais recente" />
            <ProgressMetricCard label="Ultima medida" value={data?.lastWaist ? `${data.lastWaist}cm` : '--'} hint="Cintura" />
          </div>

          <CheckInCard
            status={data?.lastCheckInAt ? 'Check-in recente registrado' : 'Check-in pendente'}
            summary={data?.lastCheckInAt ? `Ultimo envio em ${new Date(data.lastCheckInAt).toLocaleDateString('pt-BR')}` : 'Atualize seu feedback semanal'}
            onOpen={() => navigate('/student/check-in')}
          />

          <WeeklyScheduleCard
            items={weekSchedule.map((item, idx) => ({
              ...item,
              day: idx === today ? `${item.day} - Hoje` : item.day,
            }))}
          />

          <SectionCard title="Proximo compromisso" description={data?.nextAppointment ? new Date(data.nextAppointment.startAt).toLocaleString('pt-BR') : 'Sem compromissos futuros'}>
            {!data?.nextAppointment ? (
              <p className="text-sm text-slate-500">Seu personal ainda nao agendou um compromisso.</p>
            ) : (
              <div className="space-y-2">
                <p className="text-sm font-semibold text-slate-900">{data.nextAppointment.title}</p>
                <p className="text-xs text-slate-500">{data.nextAppointment.type} • {data.nextAppointment.status}</p>
                {data.nextAppointment.location && <p className="text-xs text-slate-500">Local: {data.nextAppointment.location}</p>}
                <button onClick={() => navigate('/student/appointments')} className="text-xs font-medium text-indigo-600 hover:text-indigo-700">
                  Ver todos os compromissos
                </button>
              </div>
            )}
          </SectionCard>

          <SectionCard title={t('student.dashboard.habitsTodayTitle')} description={habitsToday ? t('student.dashboard.habitsProgress', { completed: habitsToday.completedHabits, total: habitsToday.totalHabits }) : t('student.dashboard.noHabitsForToday')}>
            {!habitsToday || habitsToday.totalHabits === 0 ? (
              <p className="text-sm text-slate-500">Seu personal ainda nao configurou habitos diarios.</p>
            ) : (
              <div className="space-y-2">
                {habitsToday.items.map((item) => (
                  <button
                    key={item.habitId}
                    onClick={() => toggleHabit(item)}
                    disabled={updatingHabitId === item.habitId}
                    className={`w-full rounded-xl border px-3 py-2 text-left transition ${item.isCompleted ? 'border-emerald-200 bg-emerald-50' : 'border-slate-200 bg-white'}`}
                  >
                    <p className="text-sm font-medium text-slate-800">{item.title}</p>
                    <p className="text-xs text-slate-500">{item.targetValue ? `${item.targetValue} ${item.targetUnit || ''}` : item.category}</p>
                  </button>
                ))}
              </div>
            )}
          </SectionCard>

          <SectionCard title="Orientacao alimentar" description={nutrition?.updatedAt ? `Atualizado em ${new Date(nutrition.updatedAt).toLocaleDateString('pt-BR')}` : 'Sem orientacao cadastrada'}>
            {!nutrition?.guidanceText ? (
              <p className="text-sm text-slate-500">Seu personal ainda nao registrou orientacoes.</p>
            ) : (
              <div className="space-y-2">
                <p className="text-sm text-slate-700 whitespace-pre-wrap">{nutrition.guidanceText}</p>
                {nutrition.strategicNotes && <p className="text-xs text-amber-700 bg-amber-50 rounded-lg p-2">{nutrition.strategicNotes}</p>}
              </div>
            )}
          </SectionCard>

          <SectionCard title={t('student.dashboard.consistencyTitle')} description={t('student.dashboard.consistencyDescription')}>
            {!gamification ? (
              <p className="text-sm text-slate-500">Ainda sem dados suficientes.</p>
            ) : (
              <div className="grid grid-cols-3 gap-2">
                {[
                  { label: 'Treino', value: gamification.trainingStreak?.current ?? 0, unit: 'sem' },
                  { label: 'Habitos', value: gamification.habitStreak?.current ?? 0, unit: 'dias' },
                  { label: 'Check-in', value: gamification.checkInStreak?.current ?? 0, unit: 'sem' },
                ].map((item) => (
                  <div key={item.label} className="rounded-xl border border-slate-200 bg-slate-50 px-2 py-3 text-center">
                    <p className="text-[11px] uppercase tracking-wide text-slate-500">{item.label}</p>
                    <p className="text-lg font-bold text-slate-900">{item.value}</p>
                    <p className="text-[11px] text-slate-500">{item.unit}</p>
                  </div>
                ))}
              </div>
            )}
          </SectionCard>

          <SectionCard title={t('student.dashboard.monthGoalsTitle')} description={t('student.dashboard.currentProgress')}>
            {!gamification?.monthlyGoals ? (
              <p className="text-sm text-slate-500">Metas ainda indisponiveis.</p>
            ) : (
              <div className="space-y-3">
                {[
                  { label: 'Treinos', value: gamification.monthlyGoals.workoutsCompleted, target: gamification.monthlyGoals.workoutTarget },
                  { label: 'Dias de habitos', value: gamification.monthlyGoals.habitDaysCompleted, target: gamification.monthlyGoals.habitDaysTarget },
                  { label: 'Check-ins', value: gamification.monthlyGoals.checkInsCompleted, target: gamification.monthlyGoals.checkInTarget },
                ].map((goal) => {
                  const ratio = goal.target > 0 ? Math.min(100, Math.round((goal.value / goal.target) * 100)) : 0;
                  return (
                    <div key={goal.label}>
                      <div className="flex items-center justify-between text-xs text-slate-600 mb-1">
                        <span>{goal.label}</span>
                        <span>{goal.value}/{goal.target}</span>
                      </div>
                      <div className="h-2 rounded-full bg-slate-100 overflow-hidden">
                        <div className="h-2 rounded-full bg-indigo-500" style={{ width: `${ratio}%` }} />
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </SectionCard>

          <SectionCard title={t('student.dashboard.achievementsTitle')} description={t('student.dashboard.latestUnlocked')}>
            {achievements.length === 0 ? (
              <p className="text-sm text-slate-500">Conclua treinos e habitos para desbloquear suas conquistas.</p>
            ) : (
              <div className="space-y-2">
                {achievements.map((item) => (
                  <div key={item.code} className="rounded-xl border border-amber-200 bg-amber-50 px-3 py-2">
                    <p className="text-sm font-semibold text-amber-900">{item.title}</p>
                    <p className="text-xs text-amber-700">{item.description}</p>
                  </div>
                ))}
              </div>
            )}
          </SectionCard>
        </div>
      </ContentGrid>
    </PageContainer>
  );
}
