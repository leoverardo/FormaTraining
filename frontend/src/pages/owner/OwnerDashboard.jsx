import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { PageContainer } from '../../components/ui/PageContainer';
import { ownerService } from '../../services/ownerService';
import {
  AlertTriangle,
  ArrowUpRight,
  BadgeDollarSign,
  CheckCircle2,
  CreditCard,
  Globe,
  LayoutDashboard,
  ShieldAlert,
  ShoppingBag,
  TrendingUp,
  Users,
  UserCheck,
  Zap,
} from 'lucide-react';

// ─── Utilitários ──────────────────────────────────────────────────────────────

function money(v) {
  return Number(v || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function pct(v) {
  return `${Number(v || 0).toFixed(1)}%`;
}

function dt(v) {
  if (!v) return '-';
  return new Date(v).toLocaleDateString('pt-BR');
}

function num(v) {
  return Number(v || 0).toLocaleString('pt-BR');
}

const PERIOD_OPTIONS = [
  { value: 7, label: '7 dias' },
  { value: 30, label: '30 dias' },
  { value: 90, label: '90 dias' },
];

// ─── Componentes base ─────────────────────────────────────────────────────────

function SectionHeader({ icon: Icon, title, description, accent = 'indigo' }) {
  const colors = {
    indigo: 'text-indigo-600 bg-indigo-50',
    emerald: 'text-emerald-600 bg-emerald-50',
    amber: 'text-amber-600 bg-amber-50',
    sky: 'text-sky-600 bg-sky-50',
    violet: 'text-violet-600 bg-violet-50',
    rose: 'text-rose-600 bg-rose-50',
    slate: 'text-slate-600 bg-slate-100',
  };
  return (
    <div className="flex items-start gap-3 mb-4">
      <div className={`rounded-xl p-2 ${colors[accent] || colors.indigo}`}>
        <Icon size={18} />
      </div>
      <div>
        <h2 className="text-base font-bold text-slate-900 leading-tight">{title}</h2>
        {description && <p className="text-xs text-slate-500 mt-0.5">{description}</p>}
      </div>
    </div>
  );
}

function Section({ children, className = '' }) {
  return (
    <section className={`rounded-2xl border border-slate-200 bg-white p-5 shadow-sm ${className}`}>
      {children}
    </section>
  );
}

function KpiCard({ title, value, subtitle, icon: Icon, tone = 'indigo', badge }) {
  const tones = {
    indigo: { bg: 'bg-indigo-50', text: 'text-indigo-600', border: 'border-indigo-100' },
    emerald: { bg: 'bg-emerald-50', text: 'text-emerald-600', border: 'border-emerald-100' },
    amber: { bg: 'bg-amber-50', text: 'text-amber-600', border: 'border-amber-100' },
    rose: { bg: 'bg-rose-50', text: 'text-rose-600', border: 'border-rose-100' },
    sky: { bg: 'bg-sky-50', text: 'text-sky-600', border: 'border-sky-100' },
    violet: { bg: 'bg-violet-50', text: 'text-violet-600', border: 'border-violet-100' },
    slate: { bg: 'bg-slate-100', text: 'text-slate-600', border: 'border-slate-200' },
  };
  const t = tones[tone] || tones.indigo;
  return (
    <article className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0 flex-1">
          <p className="text-[11px] font-semibold uppercase tracking-widest text-slate-400">{title}</p>
          <p className="mt-1 text-2xl font-bold text-slate-900 leading-tight truncate">{value}</p>
          {subtitle && <p className="mt-1 text-xs text-slate-500">{subtitle}</p>}
          {badge && <p className="mt-1 text-xs font-medium text-indigo-600">{badge}</p>}
        </div>
        <div className={`shrink-0 rounded-xl border p-2 ${t.bg} ${t.text} ${t.border}`}>
          <Icon size={16} />
        </div>
      </div>
    </article>
  );
}

function StatusBadge({ status }) {
  const map = {
    Active: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    Pending: 'bg-amber-50 text-amber-700 border-amber-200',
    Expired: 'bg-rose-50 text-rose-700 border-rose-200',
    Canceled: 'bg-slate-100 text-slate-600 border-slate-200',
    Approved: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    Rejected: 'bg-rose-50 text-rose-700 border-rose-200',
    None: 'bg-slate-100 text-slate-500 border-slate-200',
  };
  return (
    <span className={`rounded-full border px-2 py-0.5 text-[11px] font-semibold ${map[status] || 'bg-slate-100 text-slate-600 border-slate-200'}`}>
      {status}
    </span>
  );
}

function BarRow({ label, value, max, color = 'bg-indigo-500', suffix = '', extra }) {
  const width = max > 0 ? Math.min(100, (value / max) * 100) : 0;
  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between text-sm">
        <span className="font-medium text-slate-800 truncate pr-2">{label}</span>
        <span className="shrink-0 text-slate-600 font-semibold">{suffix ? suffix : num(value)}</span>
      </div>
      {extra && <p className="text-xs text-slate-400">{extra}</p>}
      <div className="h-1.5 rounded-full bg-slate-100">
        <div className={`h-1.5 rounded-full ${color} transition-all`} style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}

function FunnelStep({ label, value, isLast }) {
  return (
    <div className="flex items-center gap-1">
      <div className="flex-1 rounded-xl bg-slate-50 border border-slate-200 p-3 text-center">
        <p className="text-lg font-bold text-slate-900">{num(value)}</p>
        <p className="text-[11px] text-slate-500 mt-0.5 leading-tight">{label}</p>
      </div>
      {!isLast && <span className="text-slate-300 text-lg font-light shrink-0">›</span>}
    </div>
  );
}

function AlertCard({ item }) {
  const sev = {
    High: 'border-l-4 border-l-rose-500 bg-rose-50/40',
    Medium: 'border-l-4 border-l-amber-400 bg-amber-50/40',
    Low: 'border-l-4 border-l-slate-300 bg-slate-50',
  };
  const badge = {
    High: 'bg-rose-100 text-rose-700',
    Medium: 'bg-amber-100 text-amber-700',
    Low: 'bg-slate-200 text-slate-600',
  };
  return (
    <article className={`rounded-xl p-3 ${sev[item.severity] || sev.Low}`}>
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-semibold text-slate-900 leading-tight">{item.title}</p>
        <span className={`shrink-0 rounded-full px-2 py-0.5 text-[11px] font-bold ${badge[item.severity] || badge.Low}`}>
          {item.severity}
        </span>
      </div>
      <p className="mt-1 text-xs text-slate-600">{item.description}</p>
      <p className="mt-1.5 text-[10px] uppercase tracking-wide text-slate-400">{item.type} · {dt(item.createdAt)}</p>
    </article>
  );
}

function EmptyState({ message = 'Sem dados no período' }) {
  return (
    <div className="flex flex-col items-center justify-center py-8 text-slate-400">
      <LayoutDashboard size={32} strokeWidth={1} />
      <p className="mt-2 text-sm">{message}</p>
    </div>
  );
}

// ─── Dashboard principal ──────────────────────────────────────────────────────

export function OwnerDashboard() {
  const navigate = useNavigate();
  const [range, setRange] = useState(30);
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  const load = useCallback((r) => {
    setLoading(true);
    setError(false);
    ownerService.getDashboard(r)
      .then((res) => setData(res.data.data))
      .catch(() => setError(true))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(range); }, [load, range]);

  const d = useMemo(() => {
    if (!data) return null;
    return {
      range: data.range ?? 30,
      summary: data.summary ?? {},
      revenue: data.revenue ?? {},
      subscriptions: data.subscriptions ?? {},
      planDistribution: data.planDistribution ?? [],
      billingCycleDistribution: data.billingCycleDistribution ?? [],
      upcomingExpirations: data.upcomingExpirations ?? [],
      recentPayments: data.recentPayments ?? [],
      recentTrainers: data.recentTrainers ?? [],
      newTrainersInPeriod: data.newTrainersInPeriod ?? 0,
      onboarding: data.onboarding ?? {},
      studentMetrics: data.studentMetrics ?? {},
      topTrainersByStudents: data.topTrainersByStudents ?? [],
      leadFunnel: data.leadFunnel ?? {},
      topTrainersByLeads: data.topTrainersByLeads ?? [],
      serviceSales: data.serviceSales ?? {},
      topTrainersByB2C: data.topTrainersByB2C ?? [],
      topServicesByOrders: data.topServicesByOrders ?? [],
      attentionItems: data.attentionItems ?? [],
    };
  }, [data]);

  const periodLabel = PERIOD_OPTIONS.find((x) => x.value === range)?.label ?? '30 dias';

  if (loading) {
    return (
      <PageContainer>
        <div className="flex flex-col items-center justify-center py-24 text-slate-400">
          <div className="h-8 w-8 rounded-full border-2 border-indigo-300 border-t-indigo-600 animate-spin" />
          <p className="mt-4 text-sm">Carregando painel...</p>
        </div>
      </PageContainer>
    );
  }

  if (error || !d) {
    return (
      <PageContainer>
        <div className="flex flex-col items-center justify-center py-24 text-slate-500">
          <ShieldAlert size={40} strokeWidth={1} className="text-rose-400" />
          <p className="mt-3 text-sm font-medium">Erro ao carregar o painel</p>
          <button onClick={() => load(range)} className="mt-3 text-xs text-indigo-600 underline">Tentar novamente</button>
        </div>
      </PageContainer>
    );
  }

  const { summary, revenue, subscriptions, planDistribution, billingCycleDistribution,
    upcomingExpirations, recentPayments, recentTrainers, newTrainersInPeriod,
    onboarding, studentMetrics, topTrainersByStudents, leadFunnel, topTrainersByLeads,
    serviceSales, topTrainersByB2C, topServicesByOrders, attentionItems } = d;

  const highAlerts = attentionItems.filter((x) => x.severity === 'High').length;

  return (
    <PageContainer className="space-y-5">

      {/* ── Cabeçalho ──────────────────────────────────────────────────────── */}
      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-[0_8px_24px_rgba(15,23,42,0.07)]">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">Painel Owner</h1>
              {highAlerts > 0 && (
                <span className="rounded-full bg-rose-100 text-rose-700 text-xs font-bold px-2 py-0.5">
                  {highAlerts} alerta{highAlerts > 1 ? 's' : ''}
                </span>
              )}
            </div>
            <p className="text-sm text-slate-500 mt-1">Visão executiva de negócio, operação e crescimento.</p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Button variant="outline" size="sm" onClick={() => navigate('/owner/plans')}>Gerenciar planos</Button>
            <div className="flex rounded-xl border border-slate-300 bg-slate-50 overflow-hidden">
              {PERIOD_OPTIONS.map((opt) => (
                <button
                  key={opt.value}
                  onClick={() => setRange(opt.value)}
                  className={`px-3 py-1.5 text-xs font-semibold transition-colors ${
                    range === opt.value
                      ? 'bg-indigo-600 text-white'
                      : 'text-slate-600 hover:bg-slate-100'
                  }`}
                >
                  {opt.label}
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* ── BLOCO 1: Quick KPIs ────────────────────────────────────────────── */}
      <div className="grid grid-cols-2 gap-3 xl:grid-cols-4">
        <KpiCard
          title="MRR estimado"
          value={money(revenue.monthlyRecurringRevenue)}
          subtitle="Equivalente mensal ativo"
          icon={BadgeDollarSign}
          tone="indigo"
        />
        <KpiCard
          title={`Receita (${periodLabel})`}
          value={money(revenue.revenueInPeriod)}
          subtitle={`Crescimento mês: ${pct(revenue.growthPercentageComparedToLastMonth)}`}
          badge={revenue.growthPercentageComparedToLastMonth > 0 ? `↑ ${pct(revenue.growthPercentageComparedToLastMonth)} vs mês anterior` : undefined}
          icon={ArrowUpRight}
          tone="emerald"
        />
        <KpiCard
          title="Trainers pagantes"
          value={num(summary.activeTrainers)}
          subtitle={`${num(summary.totalTrainers)} cadastrados · ${num(newTrainersInPeriod)} novos`}
          icon={UserCheck}
          tone="violet"
        />
        <KpiCard
          title="Alunos ativos"
          value={num(summary.activeStudents)}
          subtitle={`${num(summary.totalStudents)} total · ${num(studentMetrics.newStudentsInPeriod ?? 0)} novos`}
          icon={Users}
          tone="sky"
        />
      </div>

      {/* ── BLOCO 2: Receita e Assinaturas SaaS ───────────────────────────── */}
      <Section>
        <SectionHeader icon={CreditCard} title="Receita e Assinaturas SaaS" description="Saúde financeira da plataforma" accent="indigo" />

        <div className="grid grid-cols-2 gap-3 mb-5 xl:grid-cols-4">
          <KpiCard title="Assinaturas ativas" value={num(subscriptions.active)} subtitle={`${num(subscriptions.expiringIn7Days)} vencem em 7 dias`} icon={CheckCircle2} tone="emerald" />
          <KpiCard title={`Novas (${periodLabel})`} value={num(subscriptions.newInPeriod)} subtitle="Novas assinaturas no período" icon={TrendingUp} tone="indigo" />
          <KpiCard title="Inadimplência" value={money(revenue.overdueAmount)} subtitle="Pendente + rejeitado + expirado" icon={ShieldAlert} tone="rose" />
          <KpiCard title="Ticket médio/personal" value={money(revenue.averageRevenuePerTrainer)} subtitle="Receita mensal por trainer ativo" icon={BadgeDollarSign} tone="amber" />
        </div>

        <div className="grid grid-cols-1 gap-4 xl:grid-cols-3 mb-4">
          {/* Status das assinaturas */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Status das assinaturas</p>
            <div className="space-y-2.5">
              <BarRow label="Ativas" value={subscriptions.active} max={summary.totalSubscriptions || 1} color="bg-emerald-500" />
              <BarRow label="Pendentes" value={subscriptions.pending} max={summary.totalSubscriptions || 1} color="bg-amber-400" />
              <BarRow label="Vencidas" value={subscriptions.expired} max={summary.totalSubscriptions || 1} color="bg-rose-400" />
              <BarRow label="Canceladas" value={subscriptions.canceled} max={summary.totalSubscriptions || 1} color="bg-slate-300" />
            </div>
          </div>

          {/* Distribuição por plano */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Receita por plano (MRR)</p>
            {planDistribution.length === 0 ? <EmptyState message="Nenhuma assinatura ativa" /> : (
              <div className="space-y-3">
                {planDistribution.map((item) => (
                  <BarRow
                    key={item.planId}
                    label={item.planName}
                    value={item.revenue}
                    max={revenue.monthlyRecurringRevenue || 1}
                    color="bg-indigo-500"
                    suffix={`${money(item.revenue)} · ${item.activeSubscriptions} assin.`}
                    extra={`${pct(item.percentage)} do MRR`}
                  />
                ))}
              </div>
            )}
          </div>

          {/* Distribuição por ciclo */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Distribuição por ciclo</p>
            {billingCycleDistribution.length === 0 ? <EmptyState message="Nenhuma assinatura ativa" /> : (
              <div className="space-y-3">
                {billingCycleDistribution.map((item) => (
                  <BarRow
                    key={item.billingCycle}
                    label={item.billingCycle}
                    value={item.count}
                    max={subscriptions.active || 1}
                    color="bg-cyan-500"
                    suffix={`${item.count} · ${money(item.revenue)}/mês eq.`}
                    extra={`${pct(item.percentage)} do MRR`}
                  />
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
          {/* Próximos vencimentos */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Próximos vencimentos (30 dias)</p>
            {upcomingExpirations.length === 0 ? <EmptyState message="Nenhum vencimento próximo" /> : (
              <div className="overflow-x-auto">
                <table className="min-w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-400">
                      <th className="pb-2 pr-3">Personal</th>
                      <th className="pb-2 pr-3">Plano</th>
                      <th className="pb-2 pr-3">Valor</th>
                      <th className="pb-2 pr-3">Vence</th>
                    </tr>
                  </thead>
                  <tbody>
                    {upcomingExpirations.slice(0, 6).map((item) => (
                      <tr key={item.subscriptionId} className="border-b border-slate-100">
                        <td className="py-2 pr-3">
                          <p className="font-medium text-slate-800 truncate max-w-[120px]">{item.brandName}</p>
                        </td>
                        <td className="py-2 pr-3 text-xs text-slate-600">{item.planName}</td>
                        <td className="py-2 pr-3 text-xs font-semibold">{money(item.amount)}</td>
                        <td className="py-2 pr-3 text-xs text-slate-500">{dt(item.endDate)} <span className="text-amber-600">({item.daysRemaining}d)</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Pagamentos recentes */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Pagamentos recentes</p>
            {recentPayments.length === 0 ? <EmptyState message="Nenhum pagamento registrado" /> : (
              <div className="overflow-x-auto">
                <table className="min-w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-400">
                      <th className="pb-2 pr-3">Personal</th>
                      <th className="pb-2 pr-3">Valor</th>
                      <th className="pb-2 pr-3">Status</th>
                      <th className="pb-2 pr-3">Data</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentPayments.slice(0, 6).map((item) => (
                      <tr key={item.paymentId} className="border-b border-slate-100">
                        <td className="py-2 pr-3">
                          <p className="font-medium text-slate-800 truncate max-w-[120px]">{item.brandName}</p>
                          <p className="text-[11px] text-slate-400">{item.planName}</p>
                        </td>
                        <td className="py-2 pr-3 text-xs font-semibold">{money(item.amount)}</td>
                        <td className="py-2 pr-3"><StatusBadge status={item.status} /></td>
                        <td className="py-2 pr-3 text-xs text-slate-500">{dt(item.paidAt || item.createdAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </Section>

      {/* ── BLOCO 3: Aquisição e ativação de trainers ──────────────────────── */}
      <Section>
        <SectionHeader icon={TrendingUp} title="Aquisição e Ativação de Trainers" description={`Crescimento e funil de onboarding nos últimos ${periodLabel}`} accent="violet" />

        <div className="grid grid-cols-2 gap-3 mb-5 xl:grid-cols-4">
          <KpiCard title={`Novos trainers (${periodLabel})`} value={num(newTrainersInPeriod)} subtitle={`${num(summary.totalTrainers)} total cadastrados`} icon={Users} tone="violet" />
          <KpiCard title="Onboardings no período" value={num(onboarding.inPeriod)} subtitle={`${num(onboarding.totalAllTime)} total histórico`} icon={ArrowUpRight} tone="indigo" />
          <KpiCard title="Concluídos" value={num(onboarding.completed)} subtitle="Onboardings concluídos" icon={CheckCircle2} tone="emerald" />
          <KpiCard title="Taxa de conclusão" value={pct(onboarding.completionRate)} subtitle="Completados / iniciados" icon={Zap} tone="amber" />
        </div>

        {/* Funil de onboarding */}
        <div className="rounded-xl border border-slate-100 bg-slate-50 p-4 mb-4">
          <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Funil de onboarding (total histórico)</p>
          <div className="flex flex-wrap items-center gap-1">
            <FunnelStep label="Iniciado" value={onboarding.totalAllTime - onboarding.canceled} />
            <FunnelStep label="Aguard. pagto." value={onboarding.waitingPayment} />
            <FunnelStep label="Pagto. aprovado" value={onboarding.paymentApproved} />
            <FunnelStep label="Conta criada" value={onboarding.accountCreated} />
            <FunnelStep label="Concluído" value={onboarding.completed} isLast />
          </div>
          {onboarding.canceled > 0 && (
            <p className="mt-2 text-xs text-slate-400">{num(onboarding.canceled)} cancelados fora do funil</p>
          )}
        </div>

        {/* Trainers recentes */}
        {recentTrainers.length > 0 && (
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Trainers mais recentes</p>
            <div className="overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-400">
                    <th className="pb-2 pr-3">Personal</th>
                    <th className="pb-2 pr-3">E-mail</th>
                    <th className="pb-2 pr-3">Plano</th>
                    <th className="pb-2 pr-3">Status</th>
                    <th className="pb-2 pr-3">Cadastro</th>
                  </tr>
                </thead>
                <tbody>
                  {recentTrainers.slice(0, 8).map((t) => (
                    <tr key={t.trainerId} className="border-b border-slate-100">
                      <td className="py-2 pr-3">
                        <p className="font-medium text-slate-800">{t.brandName}</p>
                        <p className="text-[11px] text-slate-400">{t.name}</p>
                      </td>
                      <td className="py-2 pr-3 text-xs text-slate-500 truncate max-w-[160px]">{t.email}</td>
                      <td className="py-2 pr-3 text-xs text-slate-700">{t.currentPlan}</td>
                      <td className="py-2 pr-3"><StatusBadge status={t.subscriptionStatus} /></td>
                      <td className="py-2 pr-3 text-xs text-slate-500">{dt(t.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Section>

      {/* ── BLOCO 4: Base de alunos ────────────────────────────────────────── */}
      <Section>
        <SectionHeader icon={Users} title="Base de Alunos e Uso da Plataforma" description="Ocupação, crescimento e engajamento dos alunos" accent="sky" />

        <div className="grid grid-cols-2 gap-3 mb-5 xl:grid-cols-4">
          <KpiCard title="Total de alunos" value={num(studentMetrics.totalStudents)} subtitle={`${num(studentMetrics.newStudentsInPeriod ?? 0)} novos nos últimos ${periodLabel}`} icon={Users} tone="sky" />
          <KpiCard title="Alunos ativos" value={num(studentMetrics.activeStudents)} subtitle={`${num(studentMetrics.inactiveStudents)} inativos`} icon={UserCheck} tone="emerald" />
          <KpiCard title="Média por trainer ativo" value={studentMetrics.averagePerActiveTrainer ?? '0'} subtitle="Alunos ativos / trainers ativos" icon={TrendingUp} tone="indigo" />
          <KpiCard title="Trainers no limite" value={num(studentMetrics.trainersAtCapacity)} subtitle={`${num(studentMetrics.trainersNearCapacity)} próximos do limite (≥80%)`} icon={ShieldAlert} tone={studentMetrics.trainersAtCapacity > 0 ? 'rose' : 'slate'} />
        </div>

        {topTrainersByStudents.length > 0 && (
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Top trainers por alunos ativos</p>
            <div className="space-y-2.5">
              {topTrainersByStudents.slice(0, 8).map((t) => (
                <BarRow
                  key={t.trainerId}
                  label={t.brandName}
                  value={t.activeStudents}
                  max={topTrainersByStudents[0]?.activeStudents || 1}
                  color="bg-sky-500"
                  suffix={`${num(t.activeStudents)} alunos${t.maxStudents > 0 ? ` / ${num(t.maxStudents)}` : ''}`}
                  extra={t.maxStudents > 0 ? `${pct(t.occupancyRate)} de ocupação` : undefined}
                />
              ))}
            </div>
          </div>
        )}
      </Section>

      {/* ── BLOCO 5: Funil público ─────────────────────────────────────────── */}
      <Section>
        <SectionHeader icon={Globe} title="Funil Público: Explore, Páginas e Leads" description={`Presença online e geração de leads nos últimos ${periodLabel}`} accent="emerald" />

        <div className="grid grid-cols-2 gap-3 mb-5 xl:grid-cols-4">
          <KpiCard title="Páginas públicas ativas" value={num(leadFunnel.trainersWithPublicPage)} subtitle="Trainers com página pública" icon={Globe} tone="emerald" />
          <KpiCard title="No Explore" value={num(leadFunnel.trainersInExplore)} subtitle="Trainers visíveis na busca" icon={ArrowUpRight} tone="sky" />
          <KpiCard title="Aceitando alunos" value={num(leadFunnel.trainersAcceptingStudents)} subtitle="Trainers com vagas abertas" icon={UserCheck} tone="indigo" />
          <KpiCard title={`Leads (${periodLabel})`} value={num(leadFunnel.inPeriod)} subtitle={`${num(leadFunnel.totalAllTime)} total · ${pct(leadFunnel.conversionRate)} convertidos`} icon={TrendingUp} tone="violet" />
        </div>

        <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
          {/* Leads por status */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Leads por status (total histórico)</p>
            {leadFunnel.totalAllTime === 0 ? <EmptyState message="Nenhum lead registrado ainda" /> : (
              <div className="space-y-2.5">
                <BarRow label="Novos" value={leadFunnel.newLeads} max={leadFunnel.totalAllTime || 1} color="bg-sky-400" />
                <BarRow label="Contatados" value={leadFunnel.contactedLeads} max={leadFunnel.totalAllTime || 1} color="bg-amber-400" />
                <BarRow label="Convertidos" value={leadFunnel.convertedLeads} max={leadFunnel.totalAllTime || 1} color="bg-emerald-500" />
                <BarRow label="Arquivados" value={leadFunnel.archivedLeads} max={leadFunnel.totalAllTime || 1} color="bg-slate-300" />
              </div>
            )}
          </div>

          {/* Top trainers por leads */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Top trainers por leads</p>
            {topTrainersByLeads.length === 0 ? <EmptyState message="Nenhum lead ainda" /> : (
              <div className="space-y-2.5">
                {topTrainersByLeads.slice(0, 6).map((t) => (
                  <BarRow
                    key={t.trainerId}
                    label={t.brandName}
                    value={t.totalLeads}
                    max={topTrainersByLeads[0]?.totalLeads || 1}
                    color="bg-emerald-500"
                    suffix={`${num(t.totalLeads)} leads · ${num(t.convertedLeads)} conv.`}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </Section>

      {/* ── BLOCO 6: Vendas B2C ────────────────────────────────────────────── */}
      <Section>
        <SectionHeader icon={ShoppingBag} title="Vendas B2C dos Trainers" description={`Serviços e pedidos no período de ${periodLabel}`} accent="amber" />

        <div className="grid grid-cols-2 gap-3 mb-5 xl:grid-cols-4">
          <KpiCard
            title={`Volume aprovado (${periodLabel})`}
            value={money(serviceSales.volumeApprovedInPeriod)}
            subtitle={`${money(serviceSales.volumeApprovedAllTime)} aprovado no total`}
            icon={BadgeDollarSign}
            tone="amber"
          />
          <KpiCard
            title={`Pedidos (${periodLabel})`}
            value={num(serviceSales.ordersInPeriod)}
            subtitle={`${num(serviceSales.approvedInPeriod)} aprovados · ${num(serviceSales.pendingInPeriod)} pendentes`}
            icon={ShoppingBag}
            tone="indigo"
          />
          <KpiCard
            title="Taxa de aprovação"
            value={pct(serviceSales.approvalRate)}
            subtitle={`${num(serviceSales.rejectedOrCancelledInPeriod)} rejeitados/cancelados`}
            icon={CheckCircle2}
            tone={serviceSales.approvalRate >= 70 ? 'emerald' : 'rose'}
          />
          <KpiCard
            title="Ticket médio B2C"
            value={money(serviceSales.averageTicket)}
            subtitle={`${num(serviceSales.activePublicOffers)} serviços públicos ativos`}
            icon={TrendingUp}
            tone="sky"
          />
        </div>

        {serviceSales.pendingManualLinking > 0 && (
          <div className="mb-4 rounded-xl border border-amber-200 bg-amber-50 p-3 flex items-center gap-2">
            <AlertTriangle size={16} className="text-amber-600 shrink-0" />
            <p className="text-sm text-amber-800">
              <span className="font-bold">{num(serviceSales.pendingManualLinking)}</span> pedido{serviceSales.pendingManualLinking > 1 ? 's' : ''} aprovado{serviceSales.pendingManualLinking > 1 ? 's' : ''} aguardando vinculação manual de aluno.
            </p>
          </div>
        )}

        <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
          {/* Top trainers por B2C */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Top trainers por volume B2C (total)</p>
            {topTrainersByB2C.length === 0 ? <EmptyState message="Nenhuma venda B2C aprovada ainda" /> : (
              <div className="space-y-2.5">
                {topTrainersByB2C.slice(0, 6).map((t) => (
                  <BarRow
                    key={t.trainerId}
                    label={t.brandName}
                    value={t.volumeApproved}
                    max={topTrainersByB2C[0]?.volumeApproved || 1}
                    color="bg-amber-500"
                    suffix={`${money(t.volumeApproved)} · ${num(t.approvedOrders)} ped.`}
                  />
                ))}
              </div>
            )}
          </div>

          {/* Top serviços */}
          <div className="rounded-xl border border-slate-200 p-4">
            <p className="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Top serviços mais vendidos (total)</p>
            {topServicesByOrders.length === 0 ? <EmptyState message="Nenhum serviço vendido ainda" /> : (
              <div className="space-y-2.5">
                {topServicesByOrders.slice(0, 6).map((s) => (
                  <div key={s.offerId} className="flex items-center justify-between gap-2 rounded-lg border border-slate-100 bg-slate-50 px-3 py-2">
                    <div className="min-w-0">
                      <p className="text-sm font-semibold text-slate-800 truncate">{s.title}</p>
                      <p className="text-[11px] text-slate-400">{s.trainerBrandName}</p>
                    </div>
                    <div className="shrink-0 text-right">
                      <p className="text-sm font-bold text-slate-900">{money(s.totalVolume)}</p>
                      <p className="text-[11px] text-slate-500">{num(s.approvedOrders)} vendas</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Resumo de ofertas */}
        <div className="mt-4 grid grid-cols-3 gap-3">
          <div className="rounded-xl border border-slate-100 bg-slate-50 p-3 text-center">
            <p className="text-xl font-bold text-slate-900">{num(serviceSales.totalOffers)}</p>
            <p className="text-xs text-slate-500 mt-0.5">Serviços cadastrados</p>
          </div>
          <div className="rounded-xl border border-slate-100 bg-slate-50 p-3 text-center">
            <p className="text-xl font-bold text-emerald-700">{num(serviceSales.activePublicOffers)}</p>
            <p className="text-xs text-slate-500 mt-0.5">Serviços públicos ativos</p>
          </div>
          <div className="rounded-xl border border-slate-100 bg-slate-50 p-3 text-center">
            <p className="text-xl font-bold text-indigo-700">{num(serviceSales.trainersWithActiveOffers)}</p>
            <p className="text-xs text-slate-500 mt-0.5">Trainers com ofertas ativas</p>
          </div>
        </div>
      </Section>

      {/* ── BLOCO 7: Alertas operacionais ─────────────────────────────────── */}
      <Section>
        <SectionHeader icon={AlertTriangle} title="Alertas Operacionais" description="Pontos de atenção que exigem acompanhamento" accent="rose" />
        {attentionItems.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-8 text-emerald-600">
            <CheckCircle2 size={36} strokeWidth={1.5} />
            <p className="mt-2 text-sm font-medium">Nenhum alerta operacional no momento.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-2 lg:grid-cols-2">
            {attentionItems.slice(0, 12).map((item, i) => (
              <AlertCard key={`${item.type}-${item.trainerId ?? i}`} item={item} />
            ))}
          </div>
        )}
      </Section>

    </PageContainer>
  );
}
