import { useEffect, useState } from 'react';
import { studentAreaService } from '../../services/studentAreaService';
import { useAuth } from '../../contexts/AuthContext';
import { LoadingState } from '../../components/ui/LoadingState';
import { CheckCircle, AlertCircle, XCircle } from 'lucide-react';
import { useI18n } from '../../i18n';

export function StudentAccessPage() {
  const { user } = useAuth();
  const { t } = useI18n();
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
        <h1 className="text-xl font-bold text-gray-900 dark:text-white mb-6">{t('student.myProfile')}</h1>
        <div className="rounded-2xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 p-8">
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-3">{t('student.account')}</p>
          <p className="text-lg font-semibold text-slate-900 dark:text-white">{user?.name}</p>
          <p className="text-sm text-slate-500 dark:text-slate-400">{user?.email}</p>
          <div className="mt-5 inline-flex items-center rounded-full bg-indigo-50 dark:bg-indigo-500/20 px-3 py-1 text-xs font-medium text-indigo-700 dark:text-indigo-200">
            {t('student.explorerStudent')}
          </div>
        </div>
      </div>
    );
  }

  const isActive = access?.allowed;
  const Icon = isActive ? CheckCircle : access?.reason === 'Inactive' ? XCircle : AlertCircle;
  const color = isActive ? 'text-emerald-500' : 'text-red-500';
  const bg = isActive ? 'bg-emerald-50' : 'bg-red-50';
  const border = isActive ? 'border-emerald-200' : 'border-red-200';

  return (
    <div className="pb-20 sm:pb-0">
      <h1 className="text-xl font-bold text-gray-900 dark:text-white mb-6">{t('student.myAccess')}</h1>
      <div className={`rounded-2xl border ${border} ${bg} p-8 text-center`}>
        <Icon size={48} className={`${color} mx-auto mb-4`} />
        <h2 className={`text-lg font-bold ${color} mb-2`}>{isActive ? t('student.accessActive') : t('student.accessBlocked')}</h2>
        <p className="text-gray-600 dark:text-slate-300 text-sm max-w-sm mx-auto">{access?.message}</p>
      </div>
    </div>
  );
}


