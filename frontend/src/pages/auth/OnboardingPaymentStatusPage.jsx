import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { onboardingService } from '../../services/onboardingService';
import { Button } from '../../components/ui/Button';

export function OnboardingPaymentStatusPage() {
  const [params] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState(null);
  const onboardingId = useMemo(() => params.get('onboardingId') || localStorage.getItem('onboarding_payment_pending_id'), [params]);

  const load = async () => {
    if (!onboardingId) return;
    setLoading(true);
    try {
      const res = await onboardingService.paymentStatus(onboardingId);
      setStatus(res.data?.data || null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    const timer = setInterval(load, 5000);
    return () => clearInterval(timer);
  }, [onboardingId]); // eslint-disable-line react-hooks/exhaustive-deps

  if (!onboardingId) {
    return <div className="min-h-screen p-6 flex items-center justify-center text-sm text-gray-600">Não foi possível localizar sua contratação. Volte ao cadastro.</div>;
  }

  const confirmed = status?.isPaymentConfirmed;

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-white border border-gray-200 rounded-2xl p-6 space-y-4">
        <h1 className="text-xl font-semibold text-gray-900">{confirmed ? 'Pagamento confirmado' : 'Aguardando confirmação do pagamento'}</h1>
        <p className="text-sm text-gray-600">
          {confirmed
            ? 'Sua assinatura foi ativada com sucesso. Confira seu e-mail para concluir o acesso.'
            : 'Seu pagamento está em processamento. Esta tela atualiza automaticamente após o webhook.'}
        </p>
        <div className="text-xs text-gray-500 space-y-1">
          <p>Onboarding: {status?.onboardingStatus || '-'}</p>
          <p>Assinatura: {status?.subscriptionStatus || '-'}</p>
          <p>Pagamento: {status?.paymentStatus || '-'}</p>
        </div>
        <div className="flex gap-2">
          <Button onClick={load} loading={loading} className="flex-1">Atualizar status</Button>
          {status?.checkoutUrl && !confirmed && <a href={status.checkoutUrl} className="flex-1 text-center px-3 py-2 rounded-lg border border-gray-300 text-sm">Voltar ao pagamento</a>}
        </div>
        {confirmed && <Link to="/login" className="block text-center text-sm text-indigo-600">Ir para login</Link>}
      </div>
    </div>
  );
}
