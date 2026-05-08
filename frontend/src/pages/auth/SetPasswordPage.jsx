import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import api from '../../services/api';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dumbbell, CheckCircle } from 'lucide-react';

export function SetPasswordPage() {
  const { toast } = useToast();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get('token') || '';

  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password !== confirm) { toast('As senhas não coincidem', 'error'); return; }
    if (password.length < 6) { toast('A senha deve ter pelo menos 6 caracteres', 'error'); return; }

    setLoading(true);
    try {
      await api.post('/auth/set-password', { token, newPassword: password });
      setDone(true);
      toast('Senha definida com sucesso!');
    } catch (err) {
      toast(err.response?.data?.message || 'Link inválido ou expirado', 'error');
    } finally { setLoading(false); }
  };

  if (done) return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl border border-gray-200 p-10 text-center max-w-sm w-full shadow-sm">
        <div className="w-14 h-14 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <CheckCircle size={28} className="text-emerald-600" />
        </div>
        <h2 className="text-xl font-bold text-gray-900 mb-2">Senha definida!</h2>
        <p className="text-gray-500 text-sm mb-6">Agora você pode entrar na plataforma.</p>
        <Button onClick={() => navigate('/login')} className="w-full">Ir para o login</Button>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 flex items-center justify-center p-4">
      <div className="w-full max-w-sm">
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-2xl mb-3">
            <Dumbbell size={22} className="text-white" />
          </div>
          <h1 className="text-xl font-bold text-gray-900">Definir senha</h1>
          <p className="text-gray-500 text-sm mt-1">Escolha uma senha para acessar a plataforma</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-200 shadow-sm p-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input label="Nova senha" type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="Mínimo 6 caracteres" required />
            <Input label="Confirmar senha" type="password" value={confirm} onChange={e => setConfirm(e.target.value)} placeholder="Repita a senha" required />
            <Button type="submit" loading={loading} className="w-full">Salvar senha</Button>
          </form>
        </div>
      </div>
    </div>
  );
}

