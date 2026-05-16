import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { useToast } from '../../components/ui/Toast';
import { authService } from '../../services/authService';
import { useAuth } from '../../contexts/AuthContext';

const empty = {
  fullName: '',
  email: '',
  password: '',
  phone: '',
  city: '',
  state: '',
  goal: '',
  interests: '',
  trainingLevel: '',
  preferredTrainingMode: 'online',
  acceptPrivacyPolicy: false,
  acceptTermsOfUse: false,
  marketingEmail: false,
  marketingWhatsapp: false,
  healthRelatedDataProcessingAcknowledged: false,
};

export function StudentRegisterPage() {
  const [form, setForm] = useState(empty);
  const [loading, setLoading] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    try {
      await authService.registerStudent(form);
      await login(form.email, form.password);
      navigate('/explore', { replace: true });
    } catch (error) {
      toast(error?.response?.data?.message || 'Falha ao criar conta.', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 p-4 sm:p-8">
      <div className="mx-auto max-w-2xl rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
        <h1 className="text-2xl font-bold text-slate-900">Criar conta de aluno</h1>
        <p className="mt-1 text-sm text-slate-500">Entre como explorador para descobrir personais e conteúdos públicos.</p>

        <form onSubmit={handleSubmit} className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <Input label="Nome completo" value={form.fullName} onChange={(e) => setForm((p) => ({ ...p, fullName: e.target.value }))} required />
          </div>
          <Input label="E-mail" type="email" value={form.email} onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))} required />
          <Input label="Senha" type="password" value={form.password} onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))} required />
          <Input label="Telefone" value={form.phone} onChange={(e) => setForm((p) => ({ ...p, phone: e.target.value }))} />
          <Input label="Cidade" value={form.city} onChange={(e) => setForm((p) => ({ ...p, city: e.target.value }))} />
          <Input label="Estado" value={form.state} onChange={(e) => setForm((p) => ({ ...p, state: e.target.value }))} />
          <Input label="Objetivo" value={form.goal} onChange={(e) => setForm((p) => ({ ...p, goal: e.target.value }))} />
          <Input label="Interesses" value={form.interests} onChange={(e) => setForm((p) => ({ ...p, interests: e.target.value }))} />
          <Input label="Nível de treino" value={form.trainingLevel} onChange={(e) => setForm((p) => ({ ...p, trainingLevel: e.target.value }))} />
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Modalidade preferida</label>
            <select className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm" value={form.preferredTrainingMode} onChange={(e) => setForm((p) => ({ ...p, preferredTrainingMode: e.target.value }))}>
              <option value="online">Online</option>
              <option value="presencial">Presencial</option>
              <option value="hibrido">Híbrido</option>
            </select>
          </div>
          <div className="sm:col-span-2 mt-2">
            <label className="text-xs block"><input type="checkbox" checked={form.acceptTermsOfUse} onChange={(e) => setForm((p) => ({ ...p, acceptTermsOfUse: e.target.checked }))} /> Li e concordo com os <Link to="/terms-of-use" className="text-indigo-600">Termos de Uso</Link>.</label>
            <label className="text-xs block"><input type="checkbox" checked={form.acceptPrivacyPolicy} onChange={(e) => setForm((p) => ({ ...p, acceptPrivacyPolicy: e.target.checked }))} /> Li e concordo com a <Link to="/privacy-policy" className="text-indigo-600">Politica de Privacidade</Link>.</label>
            <label className="text-xs block"><input type="checkbox" checked={form.healthRelatedDataProcessingAcknowledged} onChange={(e) => setForm((p) => ({ ...p, healthRelatedDataProcessingAcknowledged: e.target.checked }))} /> Aviso sobre tratamento de dados de treino, habitos e evolucao (texto sujeito a revisao juridica).</label>
            <label className="text-xs block"><input type="checkbox" checked={form.marketingEmail} onChange={(e) => setForm((p) => ({ ...p, marketingEmail: e.target.checked }))} /> Aceito marketing por e-mail (opcional).</label>
            <label className="text-xs block"><input type="checkbox" checked={form.marketingWhatsapp} onChange={(e) => setForm((p) => ({ ...p, marketingWhatsapp: e.target.checked }))} /> Aceito marketing por WhatsApp (opcional).</label>
            <Button type="submit" className="w-full" loading={loading}>Criar conta</Button>
          </div>
        </form>
        <p className="mt-4 text-center text-sm text-slate-500">Já tem conta? <Link to="/login" className="font-semibold text-indigo-600">Entrar</Link></p>
      </div>
    </div>
  );
}
