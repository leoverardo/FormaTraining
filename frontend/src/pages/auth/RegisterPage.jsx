import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { onboardingService } from '../../services/onboardingService';
import { platformPlanService } from '../../services/platformPlanService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { CheckCircle, CreditCard, MapPin, User, Briefcase, Zap } from 'lucide-react';

const STEPS = [
  { id: 1, label: 'Dados pessoais', icon: User },
  { id: 2, label: 'Profissional', icon: Briefcase },
  { id: 3, label: 'Endereco', icon: MapPin },
  { id: 4, label: 'Plano', icon: CreditCard },
  { id: 5, label: 'Pagamento', icon: Zap }
];

const cycleLabels = { Monthly: 'Mensal', Quarterly: 'Trimestral', Semiannual: 'Semestral', Yearly: 'Anual' };
const cycleValues = { Monthly: 1, Quarterly: 2, Semiannual: 3, Yearly: 4 };
const cycleBadges = { Monthly: '', Quarterly: '10% OFF', Semiannual: '15% OFF', Yearly: '20% OFF - Maior economia' };

const formatMoney = (cents) => `R$ ${(cents / 100).toFixed(2).replace('.', ',')}`;

export function RegisterPage() {
  const { toast } = useToast();
  const [step, setStep] = useState(1);
  const [onboardingId, setOnboardingId] = useState(null);
  const [plans, setPlans] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [selectedPrice, setSelectedPrice] = useState(null);
  const [billingCycle, setBillingCycle] = useState('Monthly');
  const [loading, setLoading] = useState(false);
  const [couponCode, setCouponCode] = useState('');
  const [appliedCouponCode, setAppliedCouponCode] = useState(null);
  const [pricing, setPricing] = useState(null);

  const [personal, setPersonal] = useState({ fullName: '', email: '', phone: '', cpf: '', birthDate: '', acceptPrivacyPolicy: false, acceptTermsOfUse: false });
  const [professional, setProfessional] = useState({ brandName: '', cref: '', bio: '', specialties: '', instagram: '', profilePhotoUrl: '', logoUrl: '', primaryColor: '#6366f1', secondaryColor: '#a855f7' });
  const [address, setAddress] = useState({ zipCode: '', street: '', addressNumber: '', complement: '', neighborhood: '', city: '', state: '' });

  useEffect(() => {
    platformPlanService.getAll().then(r => setPlans(r.data.data?.filter(p => p.active && p.isPublic) || []));
  }, []);

  useEffect(() => {
    const plan = plans.find(p => p.id === selectedPlan?.id) || null;
    const price = plan?.prices?.find(p => p.billingCycle === billingCycle) || null;
    setSelectedPlan(plan);
    setSelectedPrice(price);
    setPricing(null);
    setAppliedCouponCode(null);
    setCouponCode('');
  }, [billingCycle]); // eslint-disable-line react-hooks/exhaustive-deps

  const baseBreakdown = useMemo(() => {
    if (!selectedPrice) return null;
    const totalCents = Math.round(selectedPrice.price * 100);
    const discountPct = billingCycle === 'Quarterly' ? 10 : billingCycle === 'Semiannual' ? 15 : billingCycle === 'Yearly' ? 20 : 0;
    const months = billingCycle === 'Quarterly' ? 3 : billingCycle === 'Semiannual' ? 6 : billingCycle === 'Yearly' ? 12 : 1;
    const monthlyBase = months > 1 ? Math.round((totalCents / (1 - discountPct / 100)) / months) : totalCents;
    const cycleBase = monthlyBase * months;
    const cycleDiscount = cycleBase - totalCents;
    return {
      cycleBaseAmountInCents: cycleBase,
      cycleDiscountAmountInCents: cycleDiscount,
      subtotalAfterCycleDiscountInCents: totalCents,
      couponDiscountAmountInCents: 0,
      finalAmountInCents: totalCents
    };
  }, [selectedPrice, billingCycle]);

  const effectivePricing = pricing || baseBreakdown;

  const handlePersonal = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await onboardingService.start({ fullName: personal.fullName, email: personal.email, phone: personal.phone, cpf: personal.cpf, birthDate: personal.birthDate || null, acceptPrivacyPolicy: personal.acceptPrivacyPolicy, acceptTermsOfUse: personal.acceptTermsOfUse });
      setOnboardingId(res.data.data.id);
      setStep(2);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao iniciar cadastro', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleProfessional = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await onboardingService.updateProfessional(onboardingId, professional);
      setStep(3);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar dados profissionais', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleAddress = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await onboardingService.updateAddress(onboardingId, address);
      setStep(4);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar endereco', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectPlan = async () => {
    if (!selectedPlan || !selectedPrice || !selectedPlan.isAvailableForPurchase) {
      toast('Selecione um plano disponivel para contratacao.', 'error');
      return;
    }
    setLoading(true);
    try {
      await onboardingService.selectPlan(onboardingId, {
        platformPlanId: selectedPlan.id,
        platformPlanPriceId: selectedPrice.id,
        billingCycle: cycleValues[billingCycle]
      });
      setStep(5);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao selecionar plano', 'error');
    } finally {
      setLoading(false);
    }
  };

  const applyCoupon = async () => {
    if (!couponCode.trim()) {
      toast('Informe um cupom para aplicar.', 'error');
      return;
    }
    setLoading(true);
    try {
      const res = await onboardingService.validateCoupon(onboardingId, { couponCode: couponCode.trim() });
      const data = res.data?.data;
      if (data?.isValid) {
        setPricing(data);
        setAppliedCouponCode(data.couponCode || couponCode.trim().toUpperCase());
        toast('Cupom aplicado com sucesso.');
      } else {
        setPricing(baseBreakdown);
        setAppliedCouponCode(null);
        toast(data?.message || 'Cupom invalido.', 'error');
      }
    } catch (err) {
      const msg = err.response?.data?.message || 'Cupom invalido.';
      setAppliedCouponCode(null);
      setPricing(baseBreakdown);
      toast(msg, 'error');
    } finally {
      setLoading(false);
    }
  };

  const removeCoupon = () => {
    setCouponCode('');
    setAppliedCouponCode(null);
    setPricing(baseBreakdown);
    toast('Cupom removido.');
  };

  const handlePayment = async () => {
    setLoading(true);
    try {
      const res = await onboardingService.createCheckout(onboardingId, { couponCode: appliedCouponCode || null });
      localStorage.setItem('onboarding_payment_pending_id', onboardingId);
      localStorage.setItem('onboarding_payment_checkout_url', res.data?.data?.checkoutUrl || '');
      const url = res.data?.data?.checkoutUrl;
      if (!url) throw new Error('Checkout sem URL.');
      toast('Redirecionando para pagamento seguro...');
      window.location.href = url;
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao gerar checkout', 'error');
    } finally {
      setLoading(false);
    }
  };

  const StepIndicator = () => (
    <div className="flex items-center justify-center gap-2 mb-8 flex-wrap">
      {STEPS.map((s, i) => {
        const Icon = s.icon;
        const active = s.id === step;
        const done = s.id < step;
        return (
          <div key={s.id} className="flex items-center gap-2">
            <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium transition-all ${active ? 'bg-indigo-600 text-white' : done ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-400'}`}>
              {done ? <CheckCircle size={12} /> : <Icon size={12} />}
              <span className="hidden sm:inline">{s.label}</span>
            </div>
            {i < STEPS.length - 1 && <div className={`w-4 h-0.5 ${done ? 'bg-emerald-400' : 'bg-gray-200'}`} />}
          </div>
        );
      })}
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 flex items-center justify-center p-4">
      <div className="w-full max-w-lg">
        <div className="text-center mb-6">
          <h1 className="text-xl font-bold text-gray-900">FitPlatform</h1>
          <p className="text-gray-500 text-sm">Cadastro de personal trainer</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-200 shadow-sm p-6 sm:p-8">
          <StepIndicator />

          {step === 1 && (
            <form onSubmit={handlePersonal} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Dados pessoais</h2>
              <Input label="Nome completo" value={personal.fullName} onChange={e => setPersonal(p => ({ ...p, fullName: e.target.value }))} required />
              <Input label="E-mail" type="email" value={personal.email} onChange={e => setPersonal(p => ({ ...p, email: e.target.value }))} required />
              <div className="grid grid-cols-2 gap-4">
                <Input label="Telefone" value={personal.phone} onChange={e => setPersonal(p => ({ ...p, phone: e.target.value }))} />
                <Input label="CPF" value={personal.cpf} onChange={e => setPersonal(p => ({ ...p, cpf: e.target.value }))} />
              </div>
              <Input label="Data de nascimento (opcional)" type="date" value={personal.birthDate} onChange={e => setPersonal(p => ({ ...p, birthDate: e.target.value }))} />
              <label className="block text-xs"><input type="checkbox" checked={personal.acceptTermsOfUse} onChange={e => setPersonal(p => ({ ...p, acceptTermsOfUse: e.target.checked }))} /> Li e concordo com os <Link to="/terms-of-use" className="text-indigo-600">Termos de Uso</Link>.</label>
              <label className="block text-xs"><input type="checkbox" checked={personal.acceptPrivacyPolicy} onChange={e => setPersonal(p => ({ ...p, acceptPrivacyPolicy: e.target.checked }))} /> Li e concordo com a <Link to="/privacy-policy" className="text-indigo-600">Politica de Privacidade</Link>.</label>
              <Button type="submit" loading={loading} className="w-full">Continuar</Button>
              <p className="text-center text-sm text-gray-500">Ja tem conta? <Link to="/login" className="text-indigo-600 font-medium">Entrar</Link></p>
            </form>
          )}

          {step === 2 && (
            <form onSubmit={handleProfessional} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Dados profissionais</h2>
              <Input label="Nome da marca / consultoria" value={professional.brandName} onChange={e => setProfessional(p => ({ ...p, brandName: e.target.value }))} required />
              <div className="grid grid-cols-2 gap-4">
                <Input label="CREF (opcional)" value={professional.cref} onChange={e => setProfessional(p => ({ ...p, cref: e.target.value }))} />
                <Input label="Instagram (opcional)" value={professional.instagram} onChange={e => setProfessional(p => ({ ...p, instagram: e.target.value }))} />
              </div>
              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(1)} className="flex-1">Voltar</Button>
                <Button type="submit" loading={loading} className="flex-1">Continuar</Button>
              </div>
            </form>
          )}

          {step === 3 && (
            <form onSubmit={handleAddress} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Endereco (opcional)</h2>
              <div className="grid grid-cols-2 gap-4">
                <Input label="CEP" value={address.zipCode} onChange={e => setAddress(p => ({ ...p, zipCode: e.target.value }))} />
                <Input label="Estado" value={address.state} onChange={e => setAddress(p => ({ ...p, state: e.target.value }))} maxLength={2} />
              </div>
              <Input label="Cidade" value={address.city} onChange={e => setAddress(p => ({ ...p, city: e.target.value }))} />
              <Input label="Rua / Logradouro" value={address.street} onChange={e => setAddress(p => ({ ...p, street: e.target.value }))} />
              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(2)} className="flex-1">Voltar</Button>
                <Button type="submit" loading={loading} className="flex-1">Continuar</Button>
              </div>
            </form>
          )}

          {step === 4 && (
            <div className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-2">Escolha seu plano</h2>
              <div className="grid grid-cols-2 gap-2">
                {Object.keys(cycleLabels).map(c => (
                  <button key={c} onClick={() => setBillingCycle(c)} className={`py-2 text-xs font-medium rounded-lg border ${billingCycle === c ? 'bg-indigo-600 text-white border-indigo-600' : 'text-gray-600 border-gray-300'}`}>
                    {cycleLabels[c]} {cycleBadges[c] ? `- ${cycleBadges[c]}` : ''}
                  </button>
                ))}
              </div>

              <div className="space-y-3">
                {plans.map(plan => {
                  const price = plan?.prices?.find(p => p.billingCycle === billingCycle);
                  const isSelected = selectedPlan?.id === plan.id;
                  const months = billingCycle === 'Quarterly' ? 3 : billingCycle === 'Semiannual' ? 6 : billingCycle === 'Yearly' ? 12 : 1;
                  const monthlyEquivalent = price ? (price.price / months) : null;

                  return (
                    <button
                      key={plan.id}
                      type="button"
                      disabled={!plan.isAvailableForPurchase}
                      onClick={() => { setSelectedPlan(plan); setSelectedPrice(price || null); setPricing(null); setAppliedCouponCode(null); setCouponCode(''); }}
                      className={`w-full text-left border-2 rounded-2xl p-4 transition-all ${isSelected ? 'border-indigo-600 bg-indigo-50' : 'border-gray-200 hover:border-indigo-300'} ${!plan.isAvailableForPurchase ? 'opacity-90 cursor-not-allowed' : ''}`}
                    >
                      <div className="flex items-center justify-between">
                        <div>
                          <div className="flex items-center gap-2">
                            <p className="font-bold text-gray-900">{plan.name}</p>
                            {plan.code === 'BASIC' ? <span className="text-[10px] font-semibold text-indigo-700 bg-indigo-100 px-2 py-0.5 rounded-full">Mais indicado para comecar</span> : null}
                            {plan.isComingSoon ? <span className="text-[10px] font-semibold text-amber-700 bg-amber-100 px-2 py-0.5 rounded-full">Em breve</span> : null}
                          </div>
                          <p className="text-xs text-gray-500 mt-0.5">{plan.code === 'BASIC' ? 'O personal gerencia' : plan.code === 'PRO' ? 'O personal automatiza' : 'O personal cresce'}</p>
                          <p className="text-xs text-gray-500 mt-1">{plan.hasUnlimitedStudents ? 'Alunos ilimitados' : `Ate ${plan.maxActiveStudents} alunos ativos`}</p>
                        </div>
                        <div className="text-right">
                          {price ? (
                            <>
                              <p className="text-xl font-bold text-indigo-600">R$ {price.price.toFixed(2).replace('.', ',')}</p>
                              <p className="text-xs text-gray-400">/{cycleLabels[billingCycle].toLowerCase()}</p>
                              {billingCycle !== 'Monthly' && <p className="text-xs text-emerald-600">equivale a R$ {monthlyEquivalent?.toFixed(2).replace('.', ',')}/mes</p>}
                            </>
                          ) : <p className="text-sm text-gray-400">Indisponivel</p>}
                        </div>
                      </div>
                    </button>
                  );
                })}
              </div>

              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(3)} className="flex-1">Voltar</Button>
                <Button onClick={handleSelectPlan} loading={loading} disabled={!selectedPlan || !selectedPrice || !selectedPlan?.isAvailableForPurchase} className="flex-1">Continuar</Button>
              </div>
            </div>
          )}

          {step === 5 && (
            <div className="space-y-5">
              <h2 className="text-lg font-semibold text-gray-900">Finalizar contratacao</h2>

              <div className="bg-indigo-50 border border-indigo-200 rounded-2xl p-4 text-sm space-y-2">
                <div className="flex justify-between"><span>Preco cheio do ciclo</span><span>{formatMoney(effectivePricing?.cycleBaseAmountInCents || 0)}</span></div>
                <div className="flex justify-between text-emerald-700"><span>Desconto do ciclo</span><span>- {formatMoney(effectivePricing?.cycleDiscountAmountInCents || 0)}</span></div>
                <div className="flex justify-between"><span>Subtotal</span><span>{formatMoney(effectivePricing?.subtotalAfterCycleDiscountInCents || 0)}</span></div>
                <div className="flex justify-between text-emerald-700"><span>Desconto do cupom</span><span>- {formatMoney(effectivePricing?.couponDiscountAmountInCents || 0)}</span></div>
                <div className="flex justify-between font-semibold text-base"><span>Total final</span><span>{formatMoney(effectivePricing?.finalAmountInCents || 0)}</span></div>
              </div>

              <div className="space-y-2">
                <Input label="Cupom de desconto (opcional)" value={couponCode} onChange={e => setCouponCode(e.target.value)} />
                <div className="flex gap-2">
                  <Button type="button" size="sm" onClick={applyCoupon} loading={loading}>Aplicar</Button>
                  {appliedCouponCode && <Button type="button" size="sm" variant="secondary" onClick={removeCoupon}>Remover</Button>}
                </div>
                {appliedCouponCode && <p className="text-xs text-emerald-700">Cupom aplicado com sucesso.</p>}
              </div>

              <p className="text-xs text-gray-500">Voce sera redirecionado para pagamento seguro. A conta sera liberada apos confirmacao do pagamento.</p>

              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(4)} className="flex-1">Voltar</Button>
                <Button onClick={handlePayment} loading={loading} className="flex-1">Continuar para pagamento</Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
