import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { LoadingState } from '../../components/ui/LoadingState';
import { Badge } from '../../components/ui/Badge';
import { EmptyState } from '../../components/ui/EmptyState';
import { ClipboardList, ChevronRight, ChevronLeft, Play, Clock, Repeat } from 'lucide-react';

const levelBadge = { Beginner: 'success', Intermediate: 'warning', Advanced: 'danger' };
const levelLabel = { Beginner: 'Iniciante', Intermediate: 'Intermediário', Advanced: 'Avançado' };

export function StudentWorkoutsPage() {
  const [workouts, setWorkouts] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    studentAreaService.getWorkouts().then(r => setWorkouts(r.data.data || [])).catch(() => {}).finally(() => setLoading(false));
  }, []);

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4 pb-20 sm:pb-0">
      <h1 className="text-xl font-bold text-gray-900">Meus Treinos</h1>
      {workouts.length === 0 ? (
        <EmptyState icon={ClipboardList} title="Nenhum treino disponível" description="Seu personal trainer ainda não criou treinos para você." />
      ) : (
        <div className="space-y-3">
          {workouts.map(w => (
            <button key={w.id} onClick={() => navigate(`/student/workouts/${w.id}`)} className="w-full bg-white rounded-2xl border border-gray-200 p-5 text-left hover:shadow-sm hover:border-indigo-200 transition-all">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-semibold text-gray-900">{w.name}</p>
                  <p className="text-gray-400 text-sm mt-0.5">{w.exercises?.length || 0} exercícios · {w.goal || 'Treino geral'}</p>
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
  const { id } = useParams();
  const navigate = useNavigate();
  const [workout, setWorkout] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentAreaService.getWorkoutById(id).then(r => setWorkout(r.data.data)).catch(() => navigate('/student/workouts')).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <LoadingState />;
  if (!workout) return null;

  return (
    <div className="space-y-4 pb-20 sm:pb-0">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
        <ChevronLeft size={18} />Voltar
      </button>
      <div>
        <h1 className="text-xl font-bold text-gray-900">{workout.name}</h1>
        {workout.goal && <p className="text-gray-500 text-sm mt-1">{workout.goal}</p>}
        {workout.description && <p className="text-gray-400 text-sm mt-1">{workout.description}</p>}
      </div>

      <div className="space-y-3">
        {workout.exercises?.sort((a, b) => a.orderIndex - b.orderIndex).map((we, i) => (
          <div key={we.id} className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
            {we.exercise?.imageUrl && (
              <img src={we.exercise.imageUrl} alt={we.exercise.name} className="w-full h-40 object-cover" />
            )}
            <div className="p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <span className="text-xs text-indigo-600 font-semibold">#{i + 1}</span>
                  <h3 className="font-semibold text-gray-900 mt-0.5">{we.exercise?.name}</h3>
                  {we.exercise?.muscleGroup && <p className="text-gray-400 text-xs mt-0.5">{we.exercise.muscleGroup}</p>}
                </div>
                {we.exercise?.videoUrl && (
                  <a href={we.exercise.videoUrl} target="_blank" rel="noopener noreferrer" className="p-2 bg-red-50 rounded-xl text-red-500 hover:bg-red-100 transition-colors">
                    <Play size={16} />
                  </a>
                )}
              </div>

              <div className="grid grid-cols-3 gap-2 mb-3">
                <div className="bg-indigo-50 rounded-xl px-3 py-2 text-center">
                  <p className="text-xs text-indigo-400">Séries</p>
                  <p className="font-bold text-indigo-700 text-lg">{we.sets}</p>
                </div>
                <div className="bg-purple-50 rounded-xl px-3 py-2 text-center">
                  <p className="text-xs text-purple-400">Reps</p>
                  <p className="font-bold text-purple-700 text-lg">{we.reps || '—'}</p>
                </div>
                <div className="bg-gray-50 rounded-xl px-3 py-2 text-center">
                  <p className="text-xs text-gray-400 flex items-center justify-center gap-1"><Clock size={10} />Descanso</p>
                  <p className="font-bold text-gray-700 text-lg">{we.restSeconds || '—'}s</p>
                </div>
              </div>

              {we.suggestedLoad && <p className="text-xs text-gray-500 mb-1"><span className="font-medium">Carga:</span> {we.suggestedLoad}</p>}
              {we.notes && <p className="text-xs text-amber-700 bg-amber-50 rounded-lg px-3 py-2 mt-2">{we.notes}</p>}

              {we.exercise?.instructions && (
                <div className="mt-3 pt-3 border-t border-gray-100">
                  <p className="text-xs text-gray-500 font-medium mb-1">Execução:</p>
                  <p className="text-xs text-gray-500 leading-relaxed">{we.exercise.instructions}</p>
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}


