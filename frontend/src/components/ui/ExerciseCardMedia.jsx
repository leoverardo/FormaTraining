import { useState } from 'react';
import { Dumbbell } from 'lucide-react';
import { getExerciseExample } from '../../features/exercises/exampleImages';

export function ExerciseCardMedia({ imageUrl, name }) {
  const example = !imageUrl ? getExerciseExample(name) : null;
  const src = imageUrl || example?.src;
  const [failedSrc, setFailedSrc] = useState(null);

  if (src && src !== failedSrc) return (
    <figure>
      <div className={`relative ${example?.kind === 'illustration' ? 'bg-white' : 'bg-[var(--surface-muted)]'}`}>
        <img src={src} alt={example?.alt || name} loading="lazy" decoding="async" onError={() => setFailedSrc(src)} className={`h-40 w-full ${example ? 'object-contain' : 'object-cover'}`} />
        {example && <span className="absolute bottom-2 left-2 rounded-full bg-[var(--surface)] px-3 py-1 text-xs font-medium text-[var(--text)]">{example.kind === 'illustration' ? 'Ilustração de exemplo' : 'Imagem de exemplo'}</span>}
      </div>
      {example && <figcaption className="flex min-w-0 items-center gap-1 bg-[var(--surface-muted)] px-4 py-2 text-xs text-[var(--text-secondary)]">
        <a href={example.authorUrl || example.source} target="_blank" rel="noreferrer" title={example.author} className="min-w-0 truncate underline underline-offset-2">{example.author}</a>
        <span aria-hidden="true">·</span>
        {example.licenseUrl ? <a href={example.licenseUrl} target="_blank" rel="noreferrer" className="shrink-0 underline underline-offset-2">{example.license}</a> : <span className="shrink-0">Domínio público</span>}
        <span className="sr-only">. {example.changes || 'Imagem reduzida e enquadrada na exibição.'}</span>
      </figcaption>}
    </figure>
  );

  return (
    <div className="flex h-40 items-center justify-center bg-[var(--brand-ink)] text-indigo-100">
      <div className="text-center">
        <Dumbbell className="mx-auto" size={24} />
        <p className="mt-2 text-xs">Sem imagem</p>
      </div>
    </div>
  );
}
