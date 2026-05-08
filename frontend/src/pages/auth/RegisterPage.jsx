import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { onboardingService } from '../../services/onboardingService';
import { platformPlanService } from '../../services/platformPlanService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dumbbell, CheckCircle, User, Briefcase, MapPin, CreditCard, Zap } from 'lucide-react';

const STEPS = [
  { id: 1, label: 'Dados pessoais', icon: User },
  { id: 2, label: 'Profissional', icon: Briefcase },
  { id: 3, label: 'Endereço', icon: MapPin },
  { id: 4, label: 'Plano', icon: CreditCard },
  { id: 5, label: 'Pagamento', icon: Zap },
];

const cycleLabels = { Monthly: 'Mensal', Quarterly: 'Trimestral', Yearly: 'Anual' };
const cycleValues = { Monthly: 1, Quarterly: 2, Yearly: 3 };

export function RegisterPage() {
  const { toast } = useToast();
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [onboardingId, setOnboardingId] = useState(null);
  const [plans, setPlans] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [selectedPrice, setSelectedPrice] = useState(null);
  const [billingCycle, setBillingCycle] = useState('Monthly');
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  const [personal, setPersonal] = useState({ fullName: '', email: '', phone: '', cpf: '', birthDate: '' });
  const [professional, setProfessional] = useState({ brandName: '', cref: '', bio: '', specialties: '', instagram: '', profilePhotoUrl: '', logoUrl: '', primaryColor: '#6366f1', secondaryColor: '#a855f7' });
  const [address, setAddress] = useState({ zipCode: '', street: '', addressNumber: '', complement: '', neighborhood: '', city: '', state: '' });

  useEffect(() => {
    platformPlanService.getAll().then(r => setPlans(r.data.data?.filter(p => p.active) || []));
  }, []);

  const getPriceForCycle = (plan) => plan?.prices?.find(p => p.billingCycle === billingCycle);

  const handlePersonal = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await onboardingService.start({ fullName: personal.fullName, email: personal.email, phone: personal.phone, cpf: personal.cpf, birthDate: personal.birthDate || null });
      setOnboardingId(res.data.data.id);
      setStep(2);
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setLoading(false); }
  };

  const handleProfessional = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await onboardingService.updateProfessional(onboardingId, professional);
      setStep(3);
    } catch { toast('Erro ao salvar dados profissionais', 'error'); }
    finally { setLoading(false); }
  };

  const handleAddress = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await onboardingService.updateAddress(onboardingId, address);
      setStep(4);
    } catch { toast('Erro ao salvar endereço', 'error'); }
    finally { setLoading(false); }
  };

  const handleSelectPlan = async () => {
    if (!selectedPlan || !selectedPrice) { toast('Selecione um plano e ciclo', 'error'); return; }
    setLoading(true);
    try {
      await onboardingService.selectPlan(onboardingId, { platformPlanId: selectedPlan.id, platformPlanPriceId: selectedPrice.id, billingCycle: cycleValues[billingCycle] });
      setStep(5);
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setLoading(false); }
  };

  const handlePayment = async () => {
    setLoading(true);
    try {
      await onboardingService.simulatePayment(onboardingId);
      setDone(true);
      toast('Conta criada! Verifique o console para o link de definição de senha.');
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setLoading(false); }
  };

  if (done) return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl border border-gray-200 p-10 text-center max-w-md w-full shadow-sm">
        <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <CheckCircle size={32} className="text-emerald-600" />
        </div>
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Conta criada!</h1>
        <p className="text-gray-500 text-sm mb-2">Verifique o console do servidor para o link de definição de senha.</p>
        <p className="text-xs text-gray-400 bg-gray-50 p-3 rounded-xl mb-6">Em produção, o link será enviado por e-mail automaticamente.</p>
        <Button onClick={() => navigate('/login')} className="w-full">Ir para o login</Button>
      </div>
    </div>
  );

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
          <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-2xl mb-3">
            <Dumbbell size={22} className="text-white" />
          </div>
          <h1 className="text-xl font-bold text-gray-900">FitPlatform</h1>
          <p className="text-gray-500 text-sm">Cadastro de personal trainer</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-200 shadow-sm p-6 sm:p-8">
          <StepIndicator />

          {/* Step 1: Personal data */}
          {step === 1 && (
            <form onSubmit={handlePersonal} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Dados pessoais</h2>
              <Input label="Nome completo" value={personal.fullName} onChange={e => setPersonal(p => ({ ...p, fullName: e.target.value }))} required />
              <Input label="E-mail" type="email" value={personal.email} onChange={e => setPersonal(p => ({ ...p, email: e.target.value }))} required />
              <div className="grid grid-cols-2 gap-4">
                <Input label="Telefone" value={personal.phone} onChange={e => setPersonal(p => ({ ...p, phone: e.target.value }))} />
                <Input label="CPF" value={personal.cpf} onChange={e => setPersonal(p => ({ ...p, cpf: e.target.value }))} placeholder="000.000.000-00" />
              </div>
              <Input label="Data de nascimento (opcional)" type="date" value={personal.birthDate} onChange={e => setPersonal(p => ({ ...p, birthDate: e.target.value }))} />
              <Button type="submit" loading={loading} className="w-full">Continuar</Button>
              <p className="text-center text-sm text-gray-500">Já tem conta? <Link to="/login" className="text-indigo-600 font-medium">Entrar</Link></p>
            </form>
          )}

          {/* Step 2: Professional data */}
          {step === 2 && (
            <form onSubmit={handleProfessional} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Dados profissionais</h2>
              <Input label="Nome da marca / consultoria" value={professional.brandName} onChange={e => setProfessional(p => ({ ...p, brandName: e.target.value }))} required />
              <div className="grid grid-cols-2 gap-4">
                <Input label="CREF (opcional)" value={professional.cref} onChange={e => setProfessional(p => ({ ...p, cref: e.target.value }))} placeholder="000000-G/SP" />
                <Input label="Instagram (opcional)" value={professional.instagram} onChange={e => setProfessional(p => ({ ...p, instagram: e.target.value }))} placeholder="@seuperfil" />
              </div>
              <div className="space-y-1">
                <label className="block text-sm font-medium text-gray-700">Bio (opcional)</label>
                <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={3} value={professional.bio} onChange={e => setProfessional(p => ({ ...p, bio: e.target.value }))} />
              </div>
              <Input label="Especialidades (opcional)" value={professional.specialties} onChange={e => setProfessional(p => ({ ...p, specialties: e.target.value }))} placeholder="Hipertrofia, Emagrecimento..." />
              <Input label="URL da foto de perfil (opcional)" value={professional.profilePhotoUrl} onChange={e => setProfessional(p => ({ ...p, profilePhotoUrl: e.target.value }))} placeholder="https://..." />
              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(1)} className="flex-1">Voltar</Button>
                <Button type="submit" loading={loading} className="flex-1">Continuar</Button>
              </div>
            </form>
          )}

          {/* Step 3: Address */}
          {step === 3 && (
            <form onSubmit={handleAddress} className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Endereço (opcional)</h2>
              <div className="grid grid-cols-2 gap-4">
                <Input label="CEP" value={address.zipCode} onChange={e => setAddress(p => ({ ...p, zipCode: e.target.value }))} />
                <Input label="Estado" value={address.state} onChange={e => setAddress(p => ({ ...p, state: e.target.value }))} placeholder="SP" maxLength={2} />
              </div>
              <Input label="Cidade" value={address.city} onChange={e => setAddress(p => ({ ...p, city: e.target.value }))} />
              <Input label="Rua / Logradouro" value={address.street} onChange={e => setAddress(p => ({ ...p, street: e.target.value }))} />
              <div className="grid grid-cols-2 gap-4">
                <Input label="Número" value={address.addressNumber} onChange={e => setAddress(p => ({ ...p, addressNumber: e.target.value }))} />
                <Input label="Bairro" value={address.neighborhood} onChange={e => setAddress(p => ({ ...p, neighborhood: e.target.value }))} />
              </div>
              <Input label="Complemento" value={address.complement} onChange={e => setAddress(p => ({ ...p, complement: e.target.value }))} />
              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(2)} className="flex-1">Voltar</Button>
                <Button type="submit" loading={loading} className="flex-1">Continuar</Button>
              </div>
            </form>
          )}

          {/* Step 4: Plan selection */}
          {step === 4 && (
            <div className="space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 mb-2">Escolha seu plano</h2>

              {/* Billing cycle toggle */}
              <div className="flex bg-gray-100 rounded-xl p-1 gap-1">
                {Object.keys(cycleLabels).map(c => (
                  <button key={c} onClick={() => setBillingCycle(c)}
                    className={`flex-1 py-1.5 text-xs font-medium rounded-lg transition-all ${billingCycle === c ? 'bg-white text-indigo-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
                    {cycleLabels[c]}
                  </button>
                ))}
              </div>

              {/* Plan cards */}
              <div className="space-y-3">
                {plans.map(plan => {
                  const price = getPriceForCycle(plan);
                  const isSelected = selectedPlan?.id === plan.id;
                  return (
                    <button key={plan.id} onClick={() => { setSelectedPlan(plan); setSelectedPrice(price); }}
                      className={`w-full text-left border-2 rounded-2xl p-4 transition-all ${isSelected ? 'border-indigo-600 bg-indigo-50' : 'border-gray-200 hover:border-indigo-300'}`}>
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="font-bold text-gray-900">{plan.name}</p>
                          <p className="text-xs text-gray-500 mt-0.5">Até {plan.maxActiveStudents} alunos ativos</p>
                        </div>
                        <div className="text-right">
                          {price ? (
                            <>
                              <p className="text-2xl font-bold text-indigo-600">R$ {price.price.toFixed(2)}</p>
                              <p className="text-xs text-gray-400">/{cycleLabels[billingCycle].toLowerCase()}</p>
                            </>
                          ) : <p className="text-sm text-gray-400">?</p>}
                        </div>
                      </div>
                      {isSelected && <div className="mt-2 text-xs text-indigo-600 font-medium flex items-center gap-1"><CheckCircle size={12} />Selecionado</div>}
                    </button>
                  );
                })}
              </div>

              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(3)} className="flex-1">Voltar</Button>
                <Button onClick={handleSelectPlan} loading={loading} disabled={!selectedPlan} className="flex-1">Continuar</Button>
              </div>
            </div>
          )}

          {/* Step 5: Payment simulation */}
          {step === 5 && (
            <div className="space-y-6">
              <h2 className="text-lg font-semibold text-gray-900">Confirmar e pagar</h2>

              <div className="bg-indigo-50 border border-indigo-200 rounded-2xl p-4">
                <p className="text-sm font-semibold text-indigo-800 mb-2">Resumo do pedido</p>
                <div className="space-y-1">
                  <div className="flex justify-between text-sm">
                    <span className="text-indigo-700">Plano {selectedPlan?.name}</span>
                    <span className="font-bold text-indigo-900">R$ {selectedPrice?.price.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between text-xs text-indigo-600">
                    <span>Ciclo {cycleLabels[billingCycle]}</span>
                    <span>Até {selectedPlan?.maxActiveStudents} alunos</span>
                  </div>
                </div>
              </div>

              <div className="bg-amber-50 border border-amber-200 rounded-2xl p-4">
                <p className="text-xs text-amber-700 font-medium mb-1">Ambiente de demonstração</p>
                <p className="text-xs text-amber-600">O botão abaixo simula um pagamento aprovado. Em produção, aqui seria o checkout do Mercado Pago.</p>
              </div>

              <div className="flex gap-3">
                <Button variant="secondary" type="button" onClick={() => setStep(4)} className="flex-1">Voltar</Button>
                <Button onClick={handlePayment} loading={loading} className="flex-1">
                  <Zap size={16} />Simular pagamento aprovado
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

