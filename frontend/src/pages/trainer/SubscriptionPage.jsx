import { useEffect, useState } from 'react';
import { trainerService } from '../../services/trainerService';
import { platformPlanService } from '../../services/platformPlanService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { LoadingState } from '../../components/ui/LoadingState';
import { Modal } from '../../components/ui/Modal';
import { CreditCard, CheckCircle, AlertTriangle, Clock, Zap } from 'lucide-react';

const statusConfig = {
  Active: { label: 'Ativa', badge: 'success', icon: CheckCircle },
  Pending: { label: 'Pendente', badge: 'warning', icon: Clock },
  Expired: { label: 'Vencida', badge: 'danger', icon: AlertTriangle },
  Canceled: { label: 'Cancelada', badge: 'danger', icon: AlertTriangle },
};

export function SubscriptionPage() {
  const { toast } = useToast();
  const [sub, setSub] = useState(null);
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [upgradeOpen, setUpgradeOpen] = useState(false);
  const [simulating, setSimulating] = useState(false);

  const load = () => {
    setLoading(true);
    Promise.allSettled([trainerService.getSubscription(), platformPlanService.getAll()])
      .then(([s, p]) => {
        if (s.status === 'fulfilled') setSub(s.value.data.data);
        if (p.status === 'fulfilled') setPlans(p.value.data.data || []);
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const simulateApproved = async () => {
    setSimulating(true);
    try {
      await trainerService.simulateApproved();
      toast('Pagamento simulado! Assinatura ativada.');
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSimulating(false); }
  };

  const simulateExpired = async () => {
    setSimulating(true);
    try {
      await trainerService.simulateExpired();
      toast('Assinatura marcada como vencida.', 'error');
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSimulating(false); }
  };

  const handleChoosePlan = async (planId) => {
    try {
      await trainerService.createSubscription({ platformPlanId: planId });
      toast('Assinatura criada! Simule o pagamento para ativá-la.');
      setUpgradeOpen(false);
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
  };

  if (loading) return <LoadingState />;

  const cfg = sub ? (statusConfig[sub.status] || statusConfig.Pending) : null;

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Minha Assinatura</h1>
        <p className="text-gray-500 text-sm mt-1">Gerencie seu plano na plataforma</p>
      </div>

      {!sub ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-8 text-center">
          <CreditCard size={40} className="text-gray-300 mx-auto mb-4" />
          <p className="font-semibold text-gray-700 mb-2">Nenhuma assinatura encontrada</p>
          <p className="text-gray-400 text-sm mb-6">Assine um plano para começar a usar a plataforma.</p>
          <Button onClick={() => setUpgradeOpen(true)}><Zap size={16} />Escolher plano</Button>
        </div>
      ) : (
        <>
          <div className="bg-white rounded-2xl border border-gray-200 p-6 space-y-4">
            <div className="flex items-start justify-between">
              <div>
                <h2 className="text-lg font-semibold text-gray-900">{sub.planName}</h2>
                <p className="text-gray-400 text-sm">R$ {sub.monthlyPrice?.toFixed(2)}/mês</p>
              </div>
              {cfg && <Badge variant={cfg.badge}>{cfg.label}</Badge>}
            </div>

            <div className="grid grid-cols-2 gap-4 pt-2">
              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-xs text-gray-400">Limite de alunos ativos</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">{sub.maxActiveStudents}</p>
              </div>
              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-xs text-gray-400">Vencimento</p>
                <p className="font-semibold text-gray-900 mt-1 text-sm">{sub.endDate ? new Date(sub.endDate).toLocaleDateString('pt-BR') : '?'}</p>
              </div>
            </div>

            <div className="flex gap-3 pt-2">
              <Button size="sm" onClick={simulateApproved} loading={simulating} variant="success">
                <CheckCircle size={16} />Simular pagamento aprovado
              </Button>
              <Button size="sm" variant="secondary" onClick={simulateExpired} loading={simulating}>
                <AlertTriangle size={16} />Simular vencimento
              </Button>
            </div>

            <Button variant="secondary" size="sm" onClick={() => setUpgradeOpen(true)}>
              <Zap size={16} />Trocar de plano
            </Button>
          </div>

          {sub.payments?.length > 0 && (
            <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
              <div className="px-6 py-4 border-b border-gray-100">
                <h3 className="font-semibold text-gray-900 text-sm">Histórico de pagamentos</h3>
              </div>
              <div className="divide-y divide-gray-100">
                {sub.payments.map(p => (
                  <div key={p.id} className="px-6 py-3 flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-700">R$ {p.amount?.toFixed(2)}</p>
                      <p className="text-xs text-gray-400">{p.provider} · {p.paidAt ? new Date(p.paidAt).toLocaleDateString('pt-BR') : 'Pendente'}</p>
                    </div>
                    <Badge variant={p.status === 'Approved' ? 'success' : p.status === 'Pending' ? 'warning' : 'danger'}>
                      {p.status === 'Approved' ? 'Aprovado' : p.status === 'Pending' ? 'Pendente' : p.status}
                    </Badge>
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}

      <Modal open={upgradeOpen} onClose={() => setUpgradeOpen(false)} title="Escolher plano" size="lg">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {plans.filter(p => p.active).map(p => (
            <div key={p.id} className="border border-gray-200 rounded-2xl p-5 hover:border-indigo-300 hover:shadow-sm transition-all cursor-pointer" onClick={() => handleChoosePlan(p.id)}>
              <h3 className="font-bold text-gray-900">{p.name}</h3>
              <p className="text-2xl font-bold text-indigo-600 mt-2">R$ {p.monthlyPrice?.toFixed(2)}<span className="text-sm text-gray-400 font-normal">/mês</span></p>
              <p className="text-sm text-gray-500 mt-3">Até {p.maxActiveStudents} alunos ativos</p>
              {p.description && <p className="text-xs text-gray-400 mt-1">{p.description}</p>}
              <Button className="w-full mt-4" size="sm">Escolher</Button>
            </div>
          ))}
        </div>
      </Modal>
    </div>
  );
}

