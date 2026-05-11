import { PublicFeedCard } from './PublicFeedCard';
import { EmptyState } from '../ui/EmptyState';
import { MessageSquare } from 'lucide-react';

export function PublicFeedSection({ items, fallbackName, fallbackAvatar, feedRef }) {
  return (
    <section ref={feedRef} className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 sm:text-3xl">Conteudos e dicas recentes</h2>
        <p className="mt-1 text-sm text-slate-600 sm:text-base">Acompanhe dicas, treinos e orientacoes publicadas pelo personal.</p>
      </div>

      {items.length ? (
        <div className="space-y-4">
          {items.map((item, index) => (
            <PublicFeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} fallbackName={fallbackName} fallbackAvatar={fallbackAvatar} />
          ))}
        </div>
      ) : (
        <EmptyState
          icon={MessageSquare}
          title="Conteudos em breve"
          description="Novas dicas e orientacoes serao publicadas por aqui." 
        />
      )}
    </section>
  );
}
