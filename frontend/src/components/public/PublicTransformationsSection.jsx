import { EmptyState } from '../ui/EmptyState';
import { Images } from 'lucide-react';

function PlaceholderImage({ label }) {
  return (
    <div className="flex h-52 items-center justify-center bg-[var(--brand-ink)] text-slate-100 sm:h-60">
      <div className="rounded-xl border border-white/20 bg-[var(--surface)]/10 px-3 py-1 text-xs font-semibold uppercase tracking-wide">{label}</div>
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
        <h2 className="font-semibold text-slate-900  uc-heading">Transformacoes autorizadas</h2>
        <p className="mt-1 text-sm text-slate-600">Evolucoes compartilhadas com consentimento dos alunos.</p>
      </div>

      <div className="grid gap-4">
        {transformations.map((item, idx) => {
          const duration = item.durationLabel || item.duration || item.period || item.timeToResult;
          const result = item.resultLabel || item.result || item.outcome;
          return (
            <article key={idx} className="overflow-hidden ds-card">
              <div className="grid gap-0 md:grid-cols-2">
                <div className="relative border-b border-slate-200 md:border-b-0 md:border-r">
                  <span className="absolute left-3 top-3 z-10 rounded-full bg-[var(--brand-ink)]/80 px-2.5 py-1 text-xs font-semibold text-white">Antes</span>
                  {item.beforePhotoUrl ? <img src={item.beforePhotoUrl} alt="Antes" className="h-52 w-full object-cover sm:h-60" /> : <PlaceholderImage label="Antes" />}
                </div>
                <div className="relative">
                  <span className="absolute left-3 top-3 z-10 rounded-full bg-[var(--success-fill)]/90 px-2.5 py-1 text-xs font-semibold text-white">Depois</span>
                  {item.afterPhotoUrl ? <img src={item.afterPhotoUrl} alt="Depois" className="h-52 w-full object-cover sm:h-60" /> : <PlaceholderImage label="Depois" />}
                </div>
              </div>

              <div className="flex flex-wrap items-center gap-2 px-4 py-3">
                {duration ? <span className="rounded-full bg-[var(--status-info-bg)] px-2.5 py-1 text-xs font-semibold text-[var(--status-info-text)]">{duration}</span> : null}
                {result ? <span className="rounded-full bg-[var(--status-success-bg)] px-2.5 py-1 text-xs font-semibold text-[var(--status-success-text)]">{result}</span> : null}
                <span className="rounded-full bg-[var(--surface-subtle)] px-2.5 py-1 text-xs font-semibold text-slate-600">Autorizado para exibicao</span>
              </div>

              {item.description ? <p className="px-4 pb-4 text-sm text-slate-600">{item.description}</p> : null}
            </article>
          );
        })}
      </div>
    </section>
  );
}
