import { Dumbbell, Flame, House, HeartPulse } from 'lucide-react';

const helpCards = [
  { title: 'Hipertrofia', description: 'Estrutura de treino para ganho de massa com progressao inteligente.', icon: Dumbbell },
  { title: 'Emagrecimento', description: 'Treino e rotina focados em reduzir gordura com consistencia.', icon: Flame },
  { title: 'Treino em casa', description: 'Protocolos eficientes para treinar com pouco equipamento.', icon: House },
  { title: 'Condicionamento', description: 'Aumento de resistencia e energia para o dia a dia.', icon: HeartPulse },
];

export function PublicSpecialtiesSection() {
  return (
    <section className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 sm:text-3xl">Como posso te ajudar</h2>
        <p className="mt-1 text-sm text-slate-600">Areas de atuacao para construir seu plano ideal.</p>
      </div>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {helpCards.map((card) => (
          <article key={card.title} className="rounded-2xl border border-slate-200 bg-white p-5 transition hover:-translate-y-0.5 hover:border-cyan-200 hover:shadow-[0_12px_26px_rgba(15,23,42,0.1)]">
            <card.icon size={20} className="text-cyan-700" />
            <h3 className="mt-3 text-base font-semibold text-slate-900">{card.title}</h3>
            <p className="mt-1 text-sm text-slate-600">{card.description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
