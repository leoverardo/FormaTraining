import { ThemeToggle } from '../../components/ui/ThemeToggle';
import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { ChevronRight } from 'lucide-react';
import { BrandLogo } from '../../components/brand/BrandLogo';
import { BRAND_ASSETS } from '../../components/brand/brandAssets';

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
    <div className="auth-screen min-h-screen flex">
      <ThemeToggle className="fixed top-4 right-4 z-40 border border-[var(--border)] bg-[var(--surface)]" />
      {/* Brand imagery; product information lives on the landing. */}
      <div className="hidden lg:flex lg:flex-col lg:w-1/2 bg-[var(--surface-muted)] p-12 text-[var(--text)]">
        <BrandLogo size="md" />

        <div className="flex flex-1 items-center py-10">
          <div className="w-full overflow-hidden rounded-2xl border border-white/10">
            <img src={BRAND_ASSETS.cover} width={1672} height={941}
              alt="UpCoach — Mais alunos, melhores resultados. Capa oficial em uma academia."
              className="block h-auto w-full" fetchPriority="high" />
            <img src={BRAND_ASSETS.subCover} width={1916} height={821}
              alt="Treine, gerencie, conquiste. A plataforma completa para o personal trainer."
              className="block h-auto w-full" />
          </div>
        </div>
      </div>

      {/* Right column ? login form */}
      <div className="flex-1 flex items-center justify-center p-6 bg-[var(--surface)]">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="flex items-center gap-2 mb-8 lg:hidden">
            <BrandLogo size="sm" />
          </div>

          <div className="ds-card p-[var(--card-padding)]">
            <div className="mb-6">
              <h2 className="text-2xl font-semibold text-slate-900">Bem-vindo de volta</h2>
              <p className="text-slate-500 text-sm mt-1">Entre com suas credenciais para continuar.</p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              <Input label="E-mail" type="email" placeholder="seu@email.com"
                value={form.email} onChange={e => setForm(p => ({ ...p, email: e.target.value }))} required />
              <Input label="Senha" type="password" placeholder="⬢⬢⬢⬢⬢⬢⬢⬢"
                value={form.password} onChange={e => setForm(p => ({ ...p, password: e.target.value }))} required />
              <Button type="submit" loading={loading} className="w-full mt-2">
                Entrar <ChevronRight size={16} />
              </Button>
            </form>

            <p className="text-center text-sm text-slate-500 mt-5">
              Personal trainer?{' '}
              <Link to="/register" className="text-[var(--link)] font-semibold hover:text-[var(--link)]">Criar conta</Link>
            </p>
            <p className="text-center text-sm text-slate-500 mt-2">
              Aluno explorador?{' '}
              <Link to="/student/register" className="text-[var(--link)] font-semibold hover:text-[var(--link)]">Criar conta</Link>
            </p>
          </div>

          {/* Dev credentials */}
          <div className="mt-4 bg-[var(--surface-subtle)] rounded-2xl p-4">
            <p className="text-xs text-slate-500 font-semibold mb-2 uppercase tracking-wide">Credenciais de teste</p>
            <div className="space-y-1">
              {[['Owner', 'admin@test.com'], ['Trainer', 'trainer@test.com'], ['Aluno', 'aluno@test.com']].map(([role, email]) => (
                <button key={email} type="button" onClick={() => setForm({ email, password: '123456' })}
                  className="w-full text-left flex items-center justify-between px-3 py-1.5 rounded-lg hover:bg-[var(--surface-control)] transition-colors">
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

