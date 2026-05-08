import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { StatCard } from '../../components/ui/StatCard';
import { Tag, Shield, BarChart2, CreditCard } from 'lucide-react';

export function OwnerDashboard() {
  const navigate = useNavigate();

  return (
    <PageContainer className="space-y-5">
      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-[0_12px_32px_rgba(15,23,42,0.08)]">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">Painel owner</h1>
            <p className="text-sm text-slate-500 mt-1">Resumo administrativo da plataforma.</p>
          </div>
          <Button onClick={() => navigate('/owner/plans')}><Tag size={16} />Gerenciar planos</Button>
        </div>
      </section>

      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <StatCard icon={Tag} title="Planos ativos" value="2" color="indigo" />
        <StatCard icon={CreditCard} title="Ciclos" value="3" color="amber" />
        <StatCard icon={BarChart2} title="Operação" value="100%" color="blue" />
        <StatCard icon={Shield} title="Acesso" value="Owner" color="emerald" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <SectionCard title="Gestão de planos" description="Configure preços, ciclos e limites de alunos." action={<Button onClick={() => navigate('/owner/plans')}>Abrir planos</Button>} />
        <SectionCard title="Acesso administrativo" description="Permissão total para operações da plataforma." />
      </div>
    </PageContainer>
  );
}

