import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { workoutSessionService } from '../../services/workoutSessionService';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../components/ui/Toast';
import { LoadingState } from '../../components/ui/LoadingState';
import { Badge } from '../../components/ui/Badge';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { ClipboardList, ChevronRight, ChevronLeft, Clock, CheckCircle2, TimerReset } from 'lucide-react';

const levelBadge = { Beginner: 'success', Intermediate: 'warning', Advanced: 'danger' };
const levelLabel = { Beginner: 'Iniciante', Intermediate: 'Intermediario', Advanced: 'Avancado' };

const formatDuration = (seconds) => {
  if (!seconds || seconds < 0) return '00:00';
  const mm = Math.floor(seconds / 60).toString().padStart(2, '0');
  const ss = Math.floor(seconds % 60).toString().padStart(2, '0');
  return `${mm}:${ss}`;
};

const formatDate = (value) => {
  if (!value) return '';
  return new Date(value).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
};

export function StudentWorkoutsPage() {
  const { user } = useAuth();
  const [workouts, setWorkouts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (!user?.hasActiveTrainerLink) {
      setWorkouts([]);
      setLoading(false);
      return;
    }

    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const response = await studentAreaService.getWorkouts();
        setWorkouts(response.data.data || []);
      } catch (err) {
        if (err.response?.status === 403) {
          setError('Voce ainda nao possui um personal vinculado.');
          return;
        }
        setError('Nao foi possivel carregar seus treinos agora.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [user?.hasActiveTrainerLink]);

  if (loading) return <LoadingState />;

  if (!user?.hasActiveTrainerLink || error) {
    return (
      <EmptyState
        icon={ClipboardList}
        title="Essa area sera liberada quando voce estiver vinculado a um personal."
        description={error || 'Encontre um personal para liberar seus treinos.'}
        action={<Button onClick={() => navigate('/explore/trainers')}>Explorar personais</Button>}
      />
    );
  }

  return (
    <div className="space-y-4 pb-20 sm:pb-0">
      <h1 className="text-xl font-bold text-gray-900">Meus Treinos</h1>
      {workouts.length === 0 ? (
        <EmptyState icon={ClipboardList} title="Nenhum treino disponivel" description="Seu personal trainer ainda nao criou treinos para voce." />
      ) : (
        <div className="space-y-3">
          {workouts.map((w) => (
            <button key={w.id} onClick={() => navigate(`/student/workouts/${w.id}`)} className="w-full bg-white rounded-2xl border border-gray-200 p-5 text-left hover:shadow-sm hover:border-indigo-200 transition-all">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-semibold text-gray-900">{w.name}</p>
                  <p className="text-gray-400 text-sm mt-0.5">{w.exercises?.length || 0} exercicios · {w.goal || 'Treino geral'}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={levelBadge[w.level] || 'gray'}>{levelLabel[w.level] || w.level}</Badge>
                  <ChevronRight size={18} className="text-gray-400" />
                </div>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

export function StudentWorkoutDetailPage() {
  const { user } = useAuth();
  const { toast } = useToast();
  const { id } = useParams();
  const navigate = useNavigate();
  const [workout, setWorkout] = useState(null);
  const [sessions, setSessions] = useState([]);
  const [sessionLoading, setSessionLoading] = useState(false);
  const [loading, setLoading] = useState(true);
  const [execution, setExecution] = useState(null);
  const [savingSetId, setSavingSetId] = useState(null);
  const [completingExerciseId, setCompletingExerciseId] = useState(null);
  const [completingSession, setCompletingSession] = useState(false);
  const [skipping, setSkipping] = useState(false);
  const [restTargetSeconds, setRestTargetSeconds] = useState(0);
  const [restRemainingSeconds, setRestRemainingSeconds] = useState(0);
  const autosaveTimersRef = useRef({});

  const activeSession = useMemo(() => sessions.find((s) => s.workoutId === id && s.status === 'Started') || null, [sessions, id]);

  useEffect(() => {
    if (!restTargetSeconds || restRemainingSeconds <= 0) return undefined;
    const timer = setInterval(() => {
      setRestRemainingSeconds((prev) => Math.max(0, prev - 1));
    }, 1000);
    return () => clearInterval(timer);
  }, [restTargetSeconds, restRemainingSeconds]);

  useEffect(() => () => {
    Object.values(autosaveTimersRef.current).forEach((timer) => clearTimeout(timer));
  }, []);

  const loadData = async () => {
    if (!user?.hasActiveTrainerLink) {
      navigate('/explore', { replace: true });
      return;
    }

    setLoading(true);
    try {
      const [workoutResponse, sessionsResponse] = await Promise.all([
        studentAreaService.getWorkoutById(id),
        workoutSessionService.getOwn(),
      ]);
      setWorkout(workoutResponse.data.data);
      setSessions(sessionsResponse.data.data || []);
    } catch {
      navigate('/student/workouts');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, navigate, user?.hasActiveTrainerLink]);

  const loadExecution = async (sessionId) => {
    setSessionLoading(true);
    try {
      const response = await workoutSessionService.getExecution(sessionId);
      setExecution(response.data.data);
    } catch (err) {
      toast(err.response?.data?.message || 'Nao foi possivel carregar a execucao.', 'error');
    } finally {
      setSessionLoading(false);
    }
  };

  useEffect(() => {
    if (activeSession) loadExecution(activeSession.id);
    else setExecution(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeSession?.id]);

  const handleStart = async () => {
    if (!workout || sessionLoading) return;
    setSessionLoading(true);
    try {
      await workoutSessionService.start({ workoutId: workout.id });
      const sessionsResponse = await workoutSessionService.getOwn();
      setSessions(sessionsResponse.data.data || []);
      toast('Sessao iniciada.');
    } catch (err) {
      toast(err.response?.data?.message || 'Nao foi possivel iniciar o treino.', 'error');
    } finally {
      setSessionLoading(false);
    }
  };

  const handleSetUpdate = async (sessionId, setId, patch, restSeconds) => {
    setSavingSetId(setId);
    try {
      const response = await workoutSessionService.updateSet(sessionId, setId, patch);
      const updated = response.data.data;
      setExecution((prev) => {
        if (!prev) return prev;
        const exercises = prev.exercises.map((ex) => {
          if (!ex.sets.some((set) => set.id === setId)) return ex;
          const sets = ex.sets.map((set) => (set.id === setId ? { ...set, ...updated } : set));
          const completed = sets.filter((x) => x.isCompleted).length;
          return {
            ...ex,
            sets,
            isCompleted: completed === sets.length,
            completedAt: completed === sets.length ? new Date().toISOString() : null,
          };
        });
        const totalSets = exercises.reduce((acc, ex) => acc + ex.sets.length, 0);
        const completedSets = exercises.reduce((acc, ex) => acc + ex.sets.filter((s) => s.isCompleted).length, 0);
        const completedExercises = exercises.filter((ex) => ex.isCompleted).length;
        return { ...prev, exercises, totalSets, completedSets, completedExercises };
      });

      if (patch.isCompleted && restSeconds) {
        setRestTargetSeconds(restSeconds);
        setRestRemainingSeconds(restSeconds);
      }
    } catch (err) {
      toast(err.response?.data?.message || 'Falha ao salvar serie.', 'error');
    } finally {
      setSavingSetId(null);
    }
  };

  const scheduleSetAutosave = (sessionId, setId, patch) => {
    const key = `${sessionId}:${setId}`;
    if (autosaveTimersRef.current[key]) clearTimeout(autosaveTimersRef.current[key]);
    autosaveTimersRef.current[key] = setTimeout(() => {
      handleSetUpdate(sessionId, setId, patch);
      delete autosaveTimersRef.current[key];
    }, 300);
  };

  const handleCompleteExercise = async (sessionId, exerciseSessionId, isCompleted) => {
    setCompletingExerciseId(exerciseSessionId);
    try {
      await workoutSessionService.completeExercise(sessionId, exerciseSessionId, { isCompleted });
      await loadExecution(sessionId);
    } catch (err) {
      toast(err.response?.data?.message || 'Falha ao atualizar exercicio.', 'error');
    } finally {
      setCompletingExerciseId(null);
    }
  };

  const handleCompleteSession = async () => {
    if (!activeSession) return;
    setCompletingSession(true);
    try {
      await workoutSessionService.complete(activeSession.id, { notes: null, exercises: [] });
      toast('Treino concluido!');
      await loadData();
    } catch (err) {
      toast(err.response?.data?.message || 'Nao foi possivel concluir o treino.', 'error');
    } finally {
      setCompletingSession(false);
    }
  };

  const handleSkipSession = async () => {
    if (!activeSession) return;
    setSkipping(true);
    try {
      await workoutSessionService.skip(activeSession.id);
      toast('Sessao marcada como pulada.');
      await loadData();
    } catch (err) {
      toast(err.response?.data?.message || 'Nao foi possivel pular a sessao.', 'error');
    } finally {
      setSkipping(false);
    }
  };

  if (loading) return <LoadingState />;
  if (!workout) return null;

  return (
    <div className="space-y-4 pb-24 sm:pb-8">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
        <ChevronLeft size={18} />Voltar
      </button>

      <div className="rounded-2xl border border-slate-200 bg-white p-4">
        <h1 className="text-xl font-bold text-gray-900">{workout.name}</h1>
        {workout.goal && <p className="text-gray-500 text-sm mt-1">{workout.goal}</p>}
        {workout.description && <p className="text-gray-400 text-sm mt-1">{workout.description}</p>}
      </div>

      {!activeSession ? (
        <div className="rounded-2xl border border-slate-200 bg-white p-4 space-y-3">
          <p className="text-sm text-slate-600">Nenhuma sessao ativa para este treino.</p>
          <Button onClick={handleStart} loading={sessionLoading}>
            Iniciar treino agora
          </Button>
        </div>
      ) : sessionLoading || !execution ? (
        <LoadingState />
      ) : (
        <>
          <div className="sticky top-0 z-20 rounded-2xl border border-indigo-200 bg-indigo-600 p-4 text-white shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs text-indigo-100">Sessao em andamento</p>
                <p className="font-semibold">{execution.workoutName}</p>
              </div>
              <Badge variant="gray">{execution.completedExercises}/{execution.totalExercises} exercicios</Badge>
            </div>
            <div className="mt-3">
              <div className="h-2 rounded-full bg-indigo-500/50 overflow-hidden">
                <div className="h-full bg-emerald-300" style={{ width: `${execution.totalSets ? (execution.completedSets / execution.totalSets) * 100 : 0}%` }} />
              </div>
              <div className="mt-2 flex items-center justify-between text-xs text-indigo-100">
                <span>{execution.completedSets}/{execution.totalSets} series</span>
                <span>{formatDuration(execution.durationSeconds)}</span>
              </div>
            </div>
          </div>

          {restRemainingSeconds > 0 && (
            <div className="rounded-2xl border border-emerald-200 bg-emerald-50 p-3 flex items-center justify-between">
              <div className="flex items-center gap-2 text-emerald-700">
                <TimerReset size={16} />
                <span className="text-sm font-medium">Descanso: {formatDuration(restRemainingSeconds)}</span>
              </div>
              <button className="text-xs text-emerald-700 underline" onClick={() => setRestRemainingSeconds(0)}>Encerrar</button>
            </div>
          )}

          <div className="space-y-3">
            {execution.exercises.map((exercise, index) => (
              <div key={exercise.workoutSessionExerciseId} className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
                {exercise.exerciseImageUrl && <img src={exercise.exerciseImageUrl} alt={exercise.exerciseName} className="w-full h-36 object-cover" />}
                <div className="p-4 space-y-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="text-xs font-semibold text-indigo-600">Exercicio {index + 1}</p>
                      <h3 className="font-semibold text-slate-900">{exercise.exerciseName}</h3>
                      {exercise.exerciseInstructions && <p className="text-xs text-slate-500 mt-1">{exercise.exerciseInstructions}</p>}
                    </div>
                    <button
                      disabled={completingExerciseId === exercise.workoutSessionExerciseId}
                      onClick={() => handleCompleteExercise(activeSession.id, exercise.workoutSessionExerciseId, !exercise.isCompleted)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-semibold border ${exercise.isCompleted ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-50 text-slate-600 border-slate-200'}`}
                    >
                      {exercise.isCompleted ? 'Concluido' : 'Marcar concluido'}
                    </button>
                  </div>

                  {exercise.lastExecutionSummary && (
                    <p className="text-xs text-amber-700 bg-amber-50 rounded-lg px-3 py-2">
                      Ultima vez ({formatDate(exercise.lastExecutionDate)}): {exercise.lastExecutionSummary}
                    </p>
                  )}

                  {exercise.prescribedNotes && <p className="text-xs text-amber-700 bg-amber-50 rounded-lg px-3 py-2">Orientacao do trainer: {exercise.prescribedNotes}</p>}
                  {exercise.executionNotes && <p className="text-xs text-slate-500">Observacao registrada: {exercise.executionNotes}</p>}

                  <div className="space-y-2">
                    {exercise.sets.map((set) => (
                      <div key={set.id} className={`rounded-xl border p-3 ${set.isCompleted ? 'border-emerald-200 bg-emerald-50/60' : 'border-slate-200 bg-slate-50/70'}`}>
                        <div className="flex items-center justify-between mb-2">
                          <p className="text-sm font-semibold text-slate-700">Serie {set.setNumber}</p>
                          <button
                            className={`inline-flex items-center gap-1 text-xs px-2 py-1 rounded-lg ${set.isCompleted ? 'bg-emerald-600 text-white' : 'bg-white text-slate-600 border border-slate-200'}`}
                            disabled={savingSetId === set.id}
                            onClick={() => handleSetUpdate(activeSession.id, set.id, { isCompleted: !set.isCompleted }, set.prescribedRestSeconds)}
                          >
                            <CheckCircle2 size={13} />{set.isCompleted ? 'Feita' : 'Marcar'}
                          </button>
                        </div>
                        <div className="grid grid-cols-2 gap-2">
                          <input
                            value={set.actualLoad || ''}
                            onChange={(e) => {
                              const actualLoad = e.target.value;
                              setExecution((prev) => {
                                if (!prev) return prev;
                                const exercises = prev.exercises.map((x) => ({ ...x, sets: x.sets.map((s) => (s.id === set.id ? { ...s, actualLoad } : s)) }));
                                return { ...prev, exercises };
                              });
                            }}
                            onBlur={(e) => scheduleSetAutosave(activeSession.id, set.id, { actualLoad: e.target.value })}
                            placeholder={`Carga (${set.prescribedLoad || '-'})`}
                            className="w-full rounded-lg border border-slate-300 px-2 py-2 text-sm"
                          />
                          <input
                            value={set.actualReps || ''}
                            onChange={(e) => {
                              const actualReps = e.target.value;
                              setExecution((prev) => {
                                if (!prev) return prev;
                                const exercises = prev.exercises.map((x) => ({ ...x, sets: x.sets.map((s) => (s.id === set.id ? { ...s, actualReps } : s)) }));
                                return { ...prev, exercises };
                              });
                            }}
                            onBlur={(e) => scheduleSetAutosave(activeSession.id, set.id, { actualReps: e.target.value })}
                            placeholder={`Reps (${set.prescribedReps || '-'})`}
                            className="w-full rounded-lg border border-slate-300 px-2 py-2 text-sm"
                          />
                        </div>
                        <div className="mt-2 flex items-center justify-between text-xs text-slate-500">
                          <span className="inline-flex items-center gap-1"><Clock size={12} />Descanso: {set.prescribedRestSeconds || '-'}s</span>
                          {savingSetId === set.id && <span>Salvando...</span>}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-4 space-y-3">
            <div className="flex items-center justify-between text-sm text-slate-600">
              <span>Exercicios concluidos</span>
              <span className="font-semibold text-slate-900">{execution.completedExercises}/{execution.totalExercises}</span>
            </div>
            <div className="flex items-center justify-between text-sm text-slate-600">
              <span>Series concluidas</span>
              <span className="font-semibold text-slate-900">{execution.completedSets}/{execution.totalSets}</span>
            </div>
            <div className="flex items-center justify-between text-sm text-slate-600">
              <span>Duracao</span>
              <span className="font-semibold text-slate-900">{formatDuration(execution.durationSeconds)}</span>
            </div>
            <div className="flex gap-2 pt-2">
              <Button variant="secondary" className="flex-1" onClick={handleSkipSession} loading={skipping}>Pular sessao</Button>
              <Button className="flex-1" onClick={handleCompleteSession} loading={completingSession}>Concluir treino</Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}




