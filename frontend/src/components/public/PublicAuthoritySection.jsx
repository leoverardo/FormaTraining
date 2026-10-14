import { Target, Activity, ClipboardCheck, Sparkles } from 'lucide-react';

const valueCards = [
  { title: 'Treinos personalizados', description: 'Plano alinhado ao seu objetivo e rotina.', icon: Target },
  { title: 'Acompanhamento de progresso', description: 'Evolucao monitorada para manter consistencia.', icon: Activity },
  { title: 'Check-ins e ajustes', description: 'Revisoes para adaptar carga e estrategia.', icon: ClipboardCheck },
  { title: 'Conteudos exclusivos', description: 'Dicas praticas para acelerar resultados.', icon: Sparkles },
];

export function PublicAuthoritySection() {
  return (
    <section className="ds-card sm:p-8 p-[var(--card-padding)]">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--status-info-text)]">Metodo e acompanhamento</p>
      <h2 className="mt-3 uc-heading">Consultoria online personalizada para evoluir com metodo</h2>
      <p className="mt-3 max-w-3xl text-sm text-slate-600 sm:text-base">Treinos, acompanhamento e conteudos organizados para voce treinar com clareza, consistencia e evolucao real.</p>
      <div className="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        {valueCards.map((card) => (
          <article key={card.title} className="ds-card bg-[var(--surface-muted)] transition-colors hover:bg-[var(--surface-subtle)] p-[var(--card-padding)]">
            <card.icon className="text-[var(--status-info-text)]" size={20} />
            <h3 className="mt-3 text-sm font-semibold text-slate-900">{card.title}</h3>
            <p className="mt-1 text-sm text-slate-600">{card.description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
