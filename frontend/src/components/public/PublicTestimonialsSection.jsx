import { Avatar } from '../ui/Avatar';
import { EmptyState } from '../ui/EmptyState';
import { Star, Quote, MessageCircleHeart } from 'lucide-react';

function ratingValue(raw) {
  const value = Number(raw);
  if (!Number.isFinite(value) || value <= 0) return 5;
  return Math.max(1, Math.min(5, Math.round(value)));
}

export function PublicTestimonialsSection({ testimonials = [] }) {
  const hasItems = testimonials.length > 0;

  return (
    <section className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 sm:text-3xl">Resultados de quem ja treinou</h2>
      </div>

      {hasItems ? (
        <div className="grid gap-3 md:grid-cols-2">
          {testimonials.map((item, idx) => {
            const resultLabel = item.result || item.resultLabel || item.outcome || '';
            const name = item.studentName || 'Aluno';
            const rating = ratingValue(item.rating);
            return (
              <article key={`${name}-${idx}`} className="relative overflow-hidden rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_10px_28px_rgba(15,23,42,0.08)]">
                <Quote className="absolute right-4 top-4 text-slate-200" size={24} />
                <div className="flex items-center gap-3">
                  <Avatar name={name} className="h-10 w-10" />
                  <div>
                    <p className="font-semibold text-slate-900">{name}</p>
                    <div className="mt-1 flex gap-0.5">
                      {Array.from({ length: 5 }, (_, i) => (
                        <Star key={i} size={14} className={i < rating ? 'fill-amber-400 text-amber-400' : 'text-slate-300'} />
                      ))}
                    </div>
                  </div>
                </div>
                <p className="mt-3 text-sm leading-relaxed text-slate-600">"{item.text || 'Depoimento sem texto.'}"</p>
                {resultLabel ? (
                  <p className="mt-3 inline-flex rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">Resultado: {resultLabel}</p>
                ) : null}
              </article>
            );
          })}
        </div>
      ) : (
        <EmptyState icon={MessageCircleHeart} title="Avaliacoes em breve" description="Depoimentos autorizados de alunos aparecerao nesta secao." />
      )}
    </section>
  );
}
