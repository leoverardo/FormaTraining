import { Target, Activity, ClipboardCheck, Sparkles } from 'lucide-react';

const valueCards = [
  { title: 'Treinos personalizados', description: 'Plano alinhado ao seu objetivo e rotina.', icon: Target },
  { title: 'Acompanhamento de progresso', description: 'Evolucao monitorada para manter consistencia.', icon: Activity },
  { title: 'Check-ins e ajustes', description: 'Revisoes para adaptar carga e estrategia.', icon: ClipboardCheck },
  { title: 'Conteudos exclusivos', description: 'Dicas praticas para acelerar resultados.', icon: Sparkles },
];

export function PublicAuthoritySection() {
  return (
    <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-[0_14px_35px_rgba(15,23,42,0.08)] sm:p-8">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-cyan-700">Metodo e acompanhamento</p>
      <h2 className="mt-2 text-2xl font-bold text-slate-900 sm:text-3xl">Consultoria online personalizada para evoluir com metodo</h2>
      <p className="mt-3 max-w-3xl text-sm text-slate-600 sm:text-base">Treinos, acompanhamento e conteudos organizados para voce treinar com clareza, consistencia e evolucao real.</p>
      <div className="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        {valueCards.map((card) => (
          <article key={card.title} className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4 transition hover:-translate-y-0.5 hover:border-cyan-200 hover:bg-white hover:shadow-lg">
            <card.icon className="text-cyan-700" size={20} />
            <h3 className="mt-3 text-sm font-semibold text-slate-900">{card.title}</h3>
            <p className="mt-1 text-sm text-slate-600">{card.description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
