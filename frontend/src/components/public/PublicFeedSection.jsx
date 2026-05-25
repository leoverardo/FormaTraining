import { PublicFeedCard } from './PublicFeedCard';
import { EmptyState } from '../ui/EmptyState';
import { MessageSquare } from 'lucide-react';
import { useI18n } from '../../i18n';

export function PublicFeedSection({ items, fallbackName, fallbackAvatar, feedRef }) {
  const { t } = useI18n();
  return (
    <section ref={feedRef} className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 sm:text-3xl">{t('public.feedSectionTitle')}</h2>
        <p className="mt-1 text-sm text-slate-600 sm:text-base">{t('public.feedSectionDescription')}</p>
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
          title={t('public.feedSectionEmptyTitle')}
          description={t('public.feedSectionEmptyDescription')}
        />
      )}
    </section>
  );
}
