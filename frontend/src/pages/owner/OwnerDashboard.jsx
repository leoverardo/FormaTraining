import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { LoadingState } from '../../components/ui/LoadingState';
import { ownerService } from '../../services/ownerService';
import { AlertTriangle, ArrowUpRight, BadgeDollarSign, CalendarClock, CreditCard, ShieldAlert, Users } from 'lucide-react';

const periodOptions = [
  { value: 'today', label: 'Hoje' },
  { value: 'month', label: 'Este mês' },
  { value: 'last30', label: 'Últimos 30 dias' },
  { value: 'year', label: 'Este ano' },
];

function money(value) {
  return Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function date(value) {
  if (!value) return '-';
  return new Date(value).toLocaleDateString('pt-BR');
}

function StatusBadge({ status }) {
  const map = {
    Active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    Pending: 'bg-amber-50 text-amber-700 border-amber-200',
    Expired: 'bg-rose-50 text-rose-700 border-rose-200',
    Canceled: 'bg-slate-100 text-slate-600 border-slate-200',
    Approved: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    Rejected: 'bg-rose-50 text-rose-700 border-rose-200',
    Refunded: 'bg-sky-50 text-sky-700 border-sky-200',
  };
  return <span className={`rounded-full border px-2 py-0.5 text-xs font-medium ${map[status] || 'bg-slate-100 text-slate-700 border-slate-200'}`}>{status}</span>;
}

function MetricCard({ title, value, subtitle, icon: Icon, tone = 'indigo' }) {
  const tones = {
    indigo: 'border-indigo-200 bg-indigo-50/50 text-indigo-700',
    emerald: 'border-emerald-200 bg-emerald-50/50 text-emerald-700',
    amber: 'border-amber-200 bg-amber-50/50 text-amber-700',
    rose: 'border-rose-200 bg-rose-50/50 text-rose-700',
    slate: 'border-slate-200 bg-slate-50/50 text-slate-700',
  };
  return (
    <article className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs uppercase tracking-wide text-slate-500">{title}</p>
          <p className="mt-1 text-2xl font-bold text-slate-900">{value}</p>
          {subtitle ? <p className="mt-1 text-xs text-slate-500">{subtitle}</p> : null}
        </div>
        <div className={`rounded-xl border p-2 ${tones[tone] || tones.indigo}`}>
          <Icon size={16} />
        </div>
      </div>
    </article>
  );
}

export function OwnerDashboard() {
  const navigate = useNavigate();
  const [period, setPeriod] = useState('month');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    ownerService.getDashboard()
      .then((response) => setData(response.data.data))
      .catch(() => setData(null))
      .finally(() => setLoading(false));
  }, []);

  const safe = useMemo(() => {
    const fallback = {
      summary: {},
      revenue: {},
      subscriptions: {},
      planDistribution: [],
      billingCycleDistribution: [],
      upcomingExpirations: [],
      recentPayments: [],
      attentionItems: [],
    };
    if (!data) return fallback;
    return {
      ...fallback,
      ...data,
      summary: { ...fallback.summary, ...(data.summary || {}) },
      revenue: { ...fallback.revenue, ...(data.revenue || {}) },
      subscriptions: { ...fallback.subscriptions, ...(data.subscriptions || {}) },
      planDistribution: Array.isArray(data.planDistribution) ? data.planDistribution : [],
      billingCycleDistribution: Array.isArray(data.billingCycleDistribution) ? data.billingCycleDistribution : [],
      upcomingExpirations: Array.isArray(data.upcomingExpirations) ? data.upcomingExpirations : [],
      recentPayments: Array.isArray(data.recentPayments) ? data.recentPayments : [],
      attentionItems: Array.isArray(data.attentionItems) ? data.attentionItems : [],
    };
  }, [data]);

  const revenuePrimary = useMemo(() => {
    if (period === 'today') return safe.revenue.revenueToday;
    if (period === 'year') return safe.revenue.revenueThisYear;
    if (period === 'last30') return safe.revenue.expectedRevenueNext30Days;
    return safe.revenue.revenueThisMonth;
  }, [safe, period]);

  if (loading) return <LoadingState />;
  if (!data) return null;

  return (
    <PageContainer className="space-y-5">
      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-[0_12px_32px_rgba(15,23,42,0.08)]">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">Painel Owner</h1>
            <p className="text-sm text-slate-500 mt-1">Visão financeira e operacional da plataforma.</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" onClick={() => navigate('/owner/plans')}>Gerenciar planos</Button>
            <Button variant="outline">Ver assinaturas</Button>
            <select value={period} onChange={(e) => setPeriod(e.target.value)} className="rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm">
              {periodOptions.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
            </select>
          </div>
        </div>
      </section>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard title="MRR estimado" value={money(safe.revenue.monthlyRecurringRevenue)} subtitle="Equivalente mensal ativo" icon={BadgeDollarSign} tone="indigo" />
        <MetricCard title="Receita do período" value={money(revenuePrimary)} subtitle={`Filtro: ${periodOptions.find((x) => x.value === period)?.label}`} icon={ArrowUpRight} tone="emerald" />
        <MetricCard title="Receita prevista (30d)" value={money(safe.revenue.expectedRevenueNext30Days)} subtitle="Assinaturas vencendo em até 30 dias" icon={CalendarClock} tone="amber" />
        <MetricCard title="Inadimplência estimada" value={money(safe.revenue.overdueAmount)} subtitle="Pendente + rejeitado + expirado" icon={ShieldAlert} tone="rose" />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard title="Assinaturas ativas" value={safe.subscriptions.active} subtitle={`${safe.subscriptions.expiringIn7Days || 0} vencem em 7 dias`} icon={CreditCard} tone="emerald" />
        <MetricCard title="Pendentes" value={safe.subscriptions.pending} subtitle="Necessitam ação de cobrança" icon={AlertTriangle} tone="amber" />
        <MetricCard title="Vencidas" value={safe.subscriptions.expired} subtitle="Sem renovação ativa" icon={ShieldAlert} tone="rose" />
        <MetricCard title="Canceladas" value={safe.subscriptions.canceled} subtitle="Histórico de churn" icon={Users} tone="slate" />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard title="Personais ativos" value={safe.summary.activeTrainers} subtitle={`${safe.summary.totalTrainers || 0} cadastrados`} icon={Users} tone="indigo" />
        <MetricCard title="Total de alunos" value={safe.summary.totalStudents} subtitle={`${safe.summary.activeStudents || 0} ativos`} icon={Users} tone="emerald" />
        <MetricCard title="Planos ativos" value={safe.summary.activePlans} subtitle={`${safe.summary.totalPlans || 0} no catálogo`} icon={CreditCard} tone="amber" />
        <MetricCard title="Ticket médio/personal" value={money(safe.revenue.averageRevenuePerTrainer)} subtitle={`Crescimento mês: ${safe.revenue.growthPercentageComparedToLastMonth || 0}%`} icon={BadgeDollarSign} tone="indigo" />
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <SectionCard title="Receita por plano" description="Participação no MRR por plano ativo">
          <div className="space-y-3">
            {safe.planDistribution.map((item) => (
              <div key={item.planId} className="rounded-xl border border-slate-200 p-3">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-semibold text-slate-900">{item.planName}</p>
                  <p className="text-sm font-semibold text-slate-700">{money(item.revenue)}</p>
                </div>
                <p className="mt-1 text-xs text-slate-500">{item.activeSubscriptions} assinaturas • {item.percentage}%</p>
                <div className="mt-2 h-2 rounded-full bg-slate-100">
                  <div className="h-2 rounded-full bg-indigo-500" style={{ width: `${Math.min(100, item.percentage)}%` }} />
                </div>
              </div>
            ))}
          </div>
        </SectionCard>

        <SectionCard title="Distribuição por ciclo" description="Uso e receita equivalente mensal por ciclo">
          <div className="space-y-3">
            {safe.billingCycleDistribution.map((item) => (
              <div key={item.billingCycle} className="rounded-xl border border-slate-200 p-3">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-semibold text-slate-900">{item.billingCycle}</p>
                  <p className="text-sm font-semibold text-slate-700">{money(item.revenue)}</p>
                </div>
                <p className="mt-1 text-xs text-slate-500">{item.count} assinaturas • {item.percentage}%</p>
                <div className="mt-2 h-2 rounded-full bg-slate-100">
                  <div className="h-2 rounded-full bg-cyan-500" style={{ width: `${Math.min(100, item.percentage)}%` }} />
                </div>
              </div>
            ))}
          </div>
        </SectionCard>
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <SectionCard title="Próximos vencimentos" description="Assinaturas com vencimento próximo">
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500">
                  <th className="py-2 pr-3">Personal</th>
                  <th className="py-2 pr-3">Plano</th>
                  <th className="py-2 pr-3">Valor</th>
                  <th className="py-2 pr-3">Vencimento</th>
                  <th className="py-2 pr-3">Status</th>
                </tr>
              </thead>
              <tbody>
                {safe.upcomingExpirations.slice(0, 8).map((item) => (
                  <tr key={item.subscriptionId} className="border-b border-slate-100">
                    <td className="py-2 pr-3">
                      <p className="font-medium text-slate-800">{item.brandName}</p>
                      <p className="text-xs text-slate-500">{item.trainerName}</p>
                    </td>
                    <td className="py-2 pr-3">{item.planName} • {item.billingCycle}</td>
                    <td className="py-2 pr-3">{money(item.amount)}</td>
                    <td className="py-2 pr-3">{date(item.endDate)} <span className="text-xs text-slate-500">({item.daysRemaining}d)</span></td>
                    <td className="py-2 pr-3"><StatusBadge status={item.status} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </SectionCard>

        <SectionCard title="Pagamentos recentes" description="Últimas transações registradas">
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-xs uppercase tracking-wide text-slate-500">
                  <th className="py-2 pr-3">Personal</th>
                  <th className="py-2 pr-3">Plano</th>
                  <th className="py-2 pr-3">Valor</th>
                  <th className="py-2 pr-3">Status</th>
                  <th className="py-2 pr-3">Data</th>
                </tr>
              </thead>
              <tbody>
                {safe.recentPayments.slice(0, 8).map((item) => (
                  <tr key={item.paymentId} className="border-b border-slate-100">
                    <td className="py-2 pr-3">
                      <p className="font-medium text-slate-800">{item.brandName}</p>
                      <p className="text-xs text-slate-500">{item.provider}</p>
                    </td>
                    <td className="py-2 pr-3">{item.planName} • {item.billingCycle}</td>
                    <td className="py-2 pr-3">{money(item.amount)}</td>
                    <td className="py-2 pr-3"><StatusBadge status={item.status} /></td>
                    <td className="py-2 pr-3">{date(item.paidAt || item.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </SectionCard>
      </div>

      <SectionCard title="Itens que precisam de atenção" description="Sinais operacionais e financeiros prioritários">
        <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
          {safe.attentionItems.slice(0, 10).map((item, index) => (
            <article key={`${item.type}-${item.subscriptionId || item.trainerId || index}`} className="rounded-2xl border border-slate-200 bg-slate-50/60 p-4">
              <div className="flex items-center justify-between gap-2">
                <p className="text-sm font-semibold text-slate-900">{item.title}</p>
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${item.severity === 'High' ? 'bg-rose-100 text-rose-700' : item.severity === 'Medium' ? 'bg-amber-100 text-amber-700' : 'bg-slate-200 text-slate-700'}`}>{item.severity}</span>
              </div>
              <p className="mt-1 text-xs text-slate-600">{item.description}</p>
              <p className="mt-2 text-[11px] uppercase tracking-wide text-slate-400">{item.type} • {date(item.createdAt)}</p>
            </article>
          ))}
        </div>
      </SectionCard>
    </PageContainer>
  );
}
