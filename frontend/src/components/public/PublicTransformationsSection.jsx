import { EmptyState } from '../ui/EmptyState';
import { Images } from 'lucide-react';

function PlaceholderImage({ label }) {
  return (
    <div className="flex h-52 items-center justify-center bg-[linear-gradient(140deg,_#0f172a,_#334155,_#155e75)] text-slate-100 sm:h-60">
      <div className="rounded-xl border border-white/20 bg-white/10 px-3 py-1 text-xs font-semibold uppercase tracking-wide">{label}</div>
    </div>
  );
}

export function PublicTransformationsSection({ transformations = [] }) {
  if (!transformations.length) {
    return <EmptyState icon={Images} title="Transformacoes em breve" description="Evolucoes autorizadas serao exibidas aqui com antes e depois." />;
  }

  return (
    <section className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 sm:text-3xl">Transformacoes autorizadas</h2>
        <p className="mt-1 text-sm text-slate-600">Evolucoes compartilhadas com consentimento dos alunos.</p>
      </div>

      <div className="grid gap-4">
        {transformations.map((item, idx) => {
          const duration = item.durationLabel || item.duration || item.period || item.timeToResult;
          const result = item.resultLabel || item.result || item.outcome;
          return (
            <article key={idx} className="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-[0_10px_28px_rgba(15,23,42,0.08)]">
              <div className="grid gap-0 md:grid-cols-2">
                <div className="relative border-b border-slate-200 md:border-b-0 md:border-r">
                  <span className="absolute left-3 top-3 z-10 rounded-full bg-slate-900/80 px-2.5 py-1 text-xs font-semibold text-white">Antes</span>
                  {item.beforePhotoUrl ? <img src={item.beforePhotoUrl} alt="Antes" className="h-52 w-full object-cover sm:h-60" /> : <PlaceholderImage label="Antes" />}
                </div>
                <div className="relative">
                  <span className="absolute left-3 top-3 z-10 rounded-full bg-emerald-600/90 px-2.5 py-1 text-xs font-semibold text-white">Depois</span>
                  {item.afterPhotoUrl ? <img src={item.afterPhotoUrl} alt="Depois" className="h-52 w-full object-cover sm:h-60" /> : <PlaceholderImage label="Depois" />}
                </div>
              </div>

              <div className="flex flex-wrap items-center gap-2 px-4 py-3">
                {duration ? <span className="rounded-full bg-cyan-50 px-2.5 py-1 text-xs font-semibold text-cyan-700">{duration}</span> : null}
                {result ? <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700">{result}</span> : null}
                <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">Autorizado para exibicao</span>
              </div>

              {item.description ? <p className="px-4 pb-4 text-sm text-slate-600">{item.description}</p> : null}
            </article>
          );
        })}
      </div>
    </section>
  );
}
