import { useEffect, useState } from 'react';
import { appointmentService } from '../../services/appointmentService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { CalendarDays, ExternalLink } from 'lucide-react';

export function StudentAppointmentsPage() {
  const { toast } = useToast();
  const [loading, setLoading] = useState(true);
  const [items, setItems] = useState([]);

  const load = async () => {
    setLoading(true);
    try {
      const response = await appointmentService.studentList();
      setItems(response.data.data || []);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao carregar compromissos', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); /* eslint-disable-next-line */ }, []);

  const confirm = async (id) => {
    try {
      await appointmentService.studentConfirm(id);
      await load();
      toast('Presença confirmada!');
    } catch (err) {
      toast(err.response?.data?.message || 'Não foi possível confirmar', 'error');
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4 pb-20">
      <div>
        <h1 className="text-xl font-bold text-gray-900">Compromissos</h1>
        <p className="text-sm text-gray-500">Seus próximos atendimentos com o personal</p>
      </div>
      {items.length === 0 ? (
        <EmptyState icon={CalendarDays} title="Nenhum compromisso" description="Quando seu personal agendar, aparecerá aqui." />
      ) : (
        <div className="space-y-2">
          {items.sort((a, b) => new Date(a.startAt) - new Date(b.startAt)).map((item) => (
            <div key={item.id} className="bg-white rounded-xl border border-gray-200 p-4">
              <div className="flex items-start justify-between gap-2">
                <div>
                  <p className="font-semibold text-gray-900">{item.title}</p>
                  <p className="text-xs text-gray-500">{item.type} • {new Date(item.startAt).toLocaleString('pt-BR')}</p>
                  {item.location && <p className="text-xs text-gray-500 mt-1">Local: {item.location}</p>}
                </div>
                <span className="text-xs px-2 py-1 rounded-lg bg-slate-100 text-slate-700">{item.status}</span>
              </div>
              <div className="mt-3 flex flex-wrap gap-2">
                {item.status === 'Scheduled' && <Button size="sm" onClick={() => confirm(item.id)}>Confirmar presença</Button>}
                {item.onlineMeetingUrl && (
                  <a href={item.onlineMeetingUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-xs rounded-lg px-2 py-1 bg-indigo-50 text-indigo-700">
                    <ExternalLink size={12} />Abrir link
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
