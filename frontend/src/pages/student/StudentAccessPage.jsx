import { useEffect, useState } from 'react';
import { studentAreaService } from '../../services/studentAreaService';
import { LoadingState } from '../../components/ui/LoadingState';
import { CheckCircle, AlertCircle, XCircle } from 'lucide-react';

export function StudentAccessPage() {
  const [access, setAccess] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentAreaService.getAccessStatus().then(r => setAccess(r.data.data)).finally(() => setLoading(false));
  }, []);

  if (loading) return <LoadingState />;

  const isActive = access?.allowed;
  const Icon = isActive ? CheckCircle : access?.reason === 'Inactive' ? XCircle : AlertCircle;
  const color = isActive ? 'text-emerald-500' : 'text-red-500';
  const bg = isActive ? 'bg-emerald-50' : 'bg-red-50';
  const border = isActive ? 'border-emerald-200' : 'border-red-200';

  return (
    <div className="pb-20 sm:pb-0">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Meu Acesso</h1>
      <div className={`rounded-2xl border ${border} ${bg} p-8 text-center`}>
        <Icon size={48} className={`${color} mx-auto mb-4`} />
        <h2 className={`text-lg font-bold ${color} mb-2`}>{isActive ? 'Acesso ativo' : 'Acesso bloqueado'}</h2>
        <p className="text-gray-600 text-sm max-w-sm mx-auto">{access?.message}</p>
      </div>
    </div>
  );
}


