import { useEffect, useState } from 'react';
import { trainerService } from '../../services/trainerService';
import { platformPlanService } from '../../services/platformPlanService';
import { paymentService } from '../../services/paymentService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { LoadingState } from '../../components/ui/LoadingState';
import { Modal } from '../../components/ui/Modal';

export function SubscriptionPage() {
  const { toast } = useToast();
  const [sub, setSub] = useState(null);
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [upgradeOpen, setUpgradeOpen] = useState(false);
  const [billingCycle, setBillingCycle] = useState(1);
  const [couponCode, setCouponCode] = useState('');
  const [premiumBlockMessage, setPremiumBlockMessage] = useState('');
  const monthsByCycle = { 1: 1, 2: 3, 3: 6, 4: 12 };
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

  useEffect(() => {
    const message = sessionStorage.getItem('premiumBlockMessage');
    if (!message) return;
    setPremiumBlockMessage(message);
    sessionStorage.removeItem('premiumBlockMessage');
  }, []);

  const handleChoosePlan = async (planId) => {
    try {
      const res = await paymentService.createCheckout({ platformPlanId: planId, billingCycle, couponCode: couponCode || null });
      const url = res.data?.data?.checkoutUrl;
      if (!url) throw new Error('Checkout sem URL.');
      toast('Redirecionando para pagamento...');
      window.location.assign(url);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao gerar checkout', 'error');
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Minha Assinatura</h1>
        <p className="text-gray-500 text-sm mt-1">Aguardando confirmação da assinatura quando aplicável.</p>
        {premiumBlockMessage ? (
          <p className="mt-3 rounded-xl border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
            {premiumBlockMessage}
          </p>
        ) : null}
      </div>

      {!sub ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-8 text-center">
          <p className="font-semibold text-gray-700 mb-2">Nenhuma assinatura encontrada</p>
          <Button onClick={() => setUpgradeOpen(true)}>Escolher plano</Button>
        </div>
      ) : (
        <div className="bg-white rounded-2xl border border-gray-200 p-6 space-y-3">
          <h2 className="text-lg font-semibold text-gray-900">{sub.planName}</h2>
          <p className="text-sm text-gray-500">Status: {sub.status}</p>
          <p className="text-sm text-gray-500">Ciclo: {sub.billingCycle}</p>
          <Button variant="secondary" size="sm" onClick={() => setUpgradeOpen(true)}>Trocar de plano</Button>
        </div>
      )}

      <Modal open={upgradeOpen} onClose={() => setUpgradeOpen(false)} title="Escolher plano" size="lg">
        <div className="mb-4 flex flex-wrap gap-2">
          {[{ label: 'Mensal', value: 1 }, { label: 'Trimestral - 10% OFF', value: 2 }, { label: 'Semestral - 15% OFF', value: 3 }, { label: 'Anual - 20% OFF', value: 4 }].map(c => (
            <button key={c.value} onClick={() => setBillingCycle(c.value)} className={`px-3 py-1 text-xs rounded-lg border ${billingCycle === c.value ? 'bg-indigo-600 text-white border-indigo-600' : 'border-gray-300 text-gray-600'}`}>{c.label}</button>
          ))}
        </div>
        <div className="mb-4">
          <input value={couponCode} onChange={(e) => setCouponCode(e.target.value)} placeholder="Cupom de desconto (opcional)" className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm" />
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {plans.filter(p => p.active && p.isPublic).map(p => (
            <div key={p.id} className={`border rounded-2xl p-5 ${p.code === 'BASIC' ? 'border-indigo-300 bg-indigo-50/40' : 'border-gray-200'}`}>
              <div className="flex items-center justify-between gap-2">
                <h3 className="font-bold text-gray-900">{p.name}</h3>
                {p.isComingSoon ? <span className="text-xs font-semibold text-amber-700 bg-amber-100 px-2 py-0.5 rounded-full">Em breve</span> : null}
              </div>
              <p className="text-sm text-gray-500 mt-1">{p.code === 'BASIC' ? 'Para organizar alunos e treinos' : p.code === 'PRO' ? 'Para automatizar atendimento e retenção' : 'Para captar alunos e crescer o negócio'}</p>
              {(() => {
                const cycleName = billingCycle === 1 ? 'mês' : billingCycle === 2 ? 'trimestre' : billingCycle === 3 ? 'semestre' : 'ano';
                const cyclePrice = p.prices?.find(x => (x.billingCycle === 'Monthly' && billingCycle === 1) || (x.billingCycle === 'Quarterly' && billingCycle === 2) || (x.billingCycle === 'Semiannual' && billingCycle === 3) || (x.billingCycle === 'Yearly' && billingCycle === 4))?.price;
                const monthlyEquivalent = cyclePrice ? cyclePrice / monthsByCycle[billingCycle] : p.monthlyPrice;
                return (
                  <>
                    <p className="text-sm text-gray-500 mt-2">R$ {(cyclePrice ?? p.monthlyPrice)?.toFixed(2).replace('.', ',')}/{cycleName}</p>
                    {billingCycle !== 1 ? <p className="text-xs text-emerald-600 mt-1">equivale a R$ {monthlyEquivalent?.toFixed(2).replace('.', ',')}/mês</p> : null}
                  </>
                );
              })()}
              <p className="text-xs text-gray-500 mt-1">{p.hasUnlimitedStudents ? 'Alunos ilimitados' : `Até ${p.maxActiveStudents} alunos ativos`}</p>
              <Button
                className="w-full mt-4"
                size="sm"
                disabled={!p.isAvailableForPurchase}
                onClick={() => handleChoosePlan(p.id)}
              >
                {p.isAvailableForPurchase ? `Assinar ${p.name}` : 'Em breve'}
              </Button>
            </div>
          ))}
        </div>
      </Modal>
    </div>
  );
}


