import { useEffect, useState } from 'react';
import { reportsService } from '../../services/reportsService';
import { LoadingState } from '../../components/ui/LoadingState';
import { StatCard } from '../../components/ui/StatCard';
import { Badge } from '../../components/ui/Badge';
import { Users, CalendarCheck, Dumbbell, TrendingUp, Bell, AlertTriangle } from 'lucide-react';

const monitoringBadge = { OnTrack: 'success', ProgressingWell: 'success', MissingCheckIn: 'warning', WorkoutDelayed: 'warning', NeedsAttention: 'danger', Inactive: 'gray' };
const monitoringLabel = { OnTrack: 'Em dia', ProgressingWell: 'Evoluindo bem', MissingCheckIn: 'Sem check-in', WorkoutDelayed: 'Treino atrasado', NeedsAttention: 'Precisa atenção', Inactive: 'Inativo' };

export function ReportsPage() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    reportsService.getOverview().then(r => setData(r.data.data)).finally(() => setLoading(false));
  }, []);

  if (loading) return <LoadingState />;
  if (!data) return null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Relatórios</h1>
        <p className="text-gray-500 text-sm mt-1">Visão geral do engajamento dos seus alunos</p>
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Total de alunos" value={data.totalStudents} icon={Users} color="indigo" />
        <StatCard title="Ativos" value={data.activeStudents} icon={Users} color="emerald" />
        <StatCard title="Check-ins esta semana" value={data.studentsWithCheckInThisWeek} icon={CalendarCheck} color="purple" />
        <StatCard title="Sem check-in" value={data.studentsWithoutCheckInThisWeek} icon={AlertTriangle} color="amber" />
        <StatCard title="Treinos realizados" value={data.workoutsCompletedThisWeek} icon={Dumbbell} color="blue" />
        <StatCard title="Progressos registrados" value={data.progressRecordsThisWeek} icon={TrendingUp} color="emerald" />
        <StatCard title="Notificações não lidas" value={data.unreadNotifications} icon={Bell} color="red" />
      </div>

      {data.studentEngagement?.length > 0 && (
        <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100">
            <h3 className="font-semibold text-gray-900 text-sm">Engajamento dos alunos este mês</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-100">
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase">Aluno</th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase hidden md:table-cell">?ltimo acesso</th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase hidden md:table-cell">?ltimo check-in</th>
                  <th className="text-center px-6 py-3 text-xs font-medium text-gray-500 uppercase">Check-ins</th>
                  <th className="text-center px-6 py-3 text-xs font-medium text-gray-500 uppercase">Treinos</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.studentEngagement.map(s => (
                  <tr key={s.studentId} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-3">
                      <p className="font-medium text-gray-900 text-sm">{s.name}</p>
                    </td>
                    <td className="px-6 py-3">
                      <Badge variant={monitoringBadge[s.monitoringStatus] || 'gray'}>
                        {monitoringLabel[s.monitoringStatus] || s.monitoringStatus}
                      </Badge>
                    </td>
                    <td className="px-6 py-3 hidden md:table-cell text-xs text-gray-500">
                      {s.lastLogin ? new Date(s.lastLogin).toLocaleDateString('pt-BR') : '?'}
                    </td>
                    <td className="px-6 py-3 hidden md:table-cell text-xs text-gray-500">
                      {s.lastCheckIn ? new Date(s.lastCheckIn).toLocaleDateString('pt-BR') : '?'}
                    </td>
                    <td className="px-6 py-3 text-center">
                      <span className="text-sm font-medium text-gray-700">{s.checkInsThisMonth}</span>
                    </td>
                    <td className="px-6 py-3 text-center">
                      <span className="text-sm font-medium text-gray-700">{s.workoutsCompletedThisMonth}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

