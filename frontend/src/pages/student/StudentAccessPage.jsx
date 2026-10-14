import { useEffect, useState } from 'react';
import { studentAreaService } from '../../services/studentAreaService';
import { useAuth } from '../../contexts/AuthContext';
import { LoadingState } from '../../components/ui/LoadingState';
import { CheckCircle, AlertCircle, XCircle } from 'lucide-react';

export function StudentAccessPage() {
  const { user } = useAuth();
  const [access, setAccess] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user?.hasActiveTrainerLink) {
      setLoading(false);
      return;
    }

    studentAreaService.getAccessStatus()
      .then(r => setAccess(r.data.data))
      .catch(() => setAccess(null))
      .finally(() => setLoading(false));
  }, [user?.hasActiveTrainerLink]);

  if (loading) return <LoadingState />;

  if (!user?.hasActiveTrainerLink) {
    return (
      <div className="pb-20 sm:pb-0">
        <h1 className="font-semibold text-gray-900 mb-6 uc-heading">Meu Perfil</h1>
        <div className="ds-card p-8 p-[var(--card-padding)]">
          <p className="text-sm text-slate-500 mb-3">Conta</p>
          <p className="text-lg font-semibold text-slate-900">{user?.name}</p>
          <p className="text-sm text-slate-500">{user?.email}</p>
          <div className="mt-5 inline-flex items-center rounded-full bg-[var(--primary-soft)] px-3 py-1 text-xs font-medium text-[var(--link)]">
            Aluno explorador
          </div>
        </div>
      </div>
    );
  }

  const isActive = access?.allowed;
  const Icon = isActive ? CheckCircle : access?.reason === 'Inactive' ? XCircle : AlertCircle;
  const color = isActive ? 'text-[var(--status-success-text)]' : 'text-[var(--status-danger-text)]';
  const bg = isActive ? 'bg-[var(--status-success-bg)]' : 'bg-[var(--status-danger-bg)]';
  const border = isActive ? 'border-[var(--status-success-bg)]' : 'border-[var(--status-danger-bg)]';

  return (
    <div className="pb-20 sm:pb-0">
      <h1 className="font-semibold text-gray-900 mb-6 uc-heading">Meu Acesso</h1>
      <div className={`rounded-2xl border ${border} ${bg} p-8 text-center`}>
        <Icon size={48} className={`${color} mx-auto mb-4`} />
        <h2 className={`text-lg font-semibold ${color} mb-2`}>{isActive ? 'Acesso ativo' : 'Acesso bloqueado'}</h2>
        <p className="text-gray-600 text-sm max-w-sm mx-auto">{access?.message}</p>
      </div>
    </div>
  );
}


