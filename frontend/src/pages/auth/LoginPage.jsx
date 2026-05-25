import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { BrandLogo } from '../../components/brand/BrandLogo';
import { Zap, Users, BarChart2, ChevronRight } from 'lucide-react';

const features = [
  { icon: Users, text: 'Gerencie alunos, treinos e metas em um só lugar' },
  { icon: Zap, text: 'Acompanhe o progresso e check-ins semanais' },
  { icon: BarChart2, text: 'Relatórios de engajamento e evolução física' },
];

export function LoginPage() {
  const { login } = useAuth();
  const { toast } = useToast();
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const user = await login(form.email, form.password);
      if (user.role === 'Owner') navigate('/owner');
      else if (user.role === 'Trainer') navigate('/trainer/dashboard');
      else navigate(user.hasActiveTrainerLink ? '/student/dashboard' : '/explore');
    } catch (err) {
      toast(err.response?.data?.message || 'E-mail ou senha incorretos.', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      <div className="hidden lg:flex lg:flex-col lg:justify-between lg:w-1/2 bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 p-12 text-white">
        <BrandLogo showText size="md" textClassName="text-xl font-bold tracking-tight text-white" />

        <div className="space-y-8">
          <div>
            <h1 className="text-4xl font-bold leading-tight text-white">
              Gerencie sua consultoria fitness em um só lugar
            </h1>
            <p className="text-slate-400 mt-4 text-lg leading-relaxed">
              Treinos, alunos, progresso e conteúdos organizados em uma plataforma profissional e fácil de usar.
            </p>
          </div>

          <div className="space-y-4">
            {features.map(({ icon: Icon, text }, i) => (
              <div key={i} className="flex items-center gap-3">
                <div className="w-8 h-8 bg-indigo-500/20 border border-indigo-500/30 rounded-lg flex items-center justify-center shrink-0">
                  <Icon size={16} className="text-indigo-400" />
                </div>
                <p className="text-slate-300 text-sm">{text}</p>
              </div>
            ))}
          </div>
        </div>

        <p className="text-slate-600 text-sm">
          © {new Date().getFullYear()} Forma Training. Todos os direitos reservados.
        </p>
      </div>

      <div className="flex-1 flex items-center justify-center p-6 bg-slate-50">
        <div className="w-full max-w-md">
          <BrandLogo className="mb-8 lg:hidden" size="sm" showText textClassName="text-lg font-bold text-slate-900 max-w-[170px]" />

          <div className="bg-white rounded-3xl shadow-sm border border-slate-200 p-8">
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-slate-900">Bem-vindo de volta</h2>
              <p className="text-slate-500 text-sm mt-1">Entre com suas credenciais para continuar.</p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              <Input label="E-mail" type="email" placeholder="seu@email.com"
                value={form.email} onChange={e => setForm(p => ({ ...p, email: e.target.value }))} required />
              <Input label="Senha" type="password" placeholder="••••••••"
                value={form.password} onChange={e => setForm(p => ({ ...p, password: e.target.value }))} required />
              <Button type="submit" loading={loading} className="w-full mt-2">
                Entrar <ChevronRight size={16} />
              </Button>
            </form>

            <p className="text-center text-sm text-slate-500 mt-5">
              Personal trainer?{' '}
              <Link to="/register" className="text-indigo-600 font-semibold hover:text-indigo-700">Criar conta</Link>
            </p>
            <p className="text-center text-sm text-slate-500 mt-2">
              Aluno explorador?{' '}
              <Link to="/student/register" className="text-indigo-600 font-semibold hover:text-indigo-700">Criar conta</Link>
            </p>
          </div>

          <div className="mt-4 bg-slate-100 rounded-2xl p-4">
            <p className="text-xs text-slate-500 font-semibold mb-2 uppercase tracking-wide">Credenciais de teste</p>
            <div className="space-y-1">
              {[['Owner', 'admin@test.com'], ['Trainer', 'trainer@test.com'], ['Aluno', 'aluno@test.com']].map(([role, email]) => (
                <button key={email} type="button" onClick={() => setForm({ email, password: '123456' })}
                  className="w-full text-left flex items-center justify-between px-3 py-1.5 rounded-lg hover:bg-slate-200 transition-colors">
                  <span className="text-xs text-slate-600 font-medium">{role}</span>
                  <span className="text-xs text-slate-400">{email}</span>
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
