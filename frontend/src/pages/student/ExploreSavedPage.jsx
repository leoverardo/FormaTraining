import { useEffect, useState } from 'react';
import { Bookmark } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { EmptyState } from '../../components/ui/EmptyState';
import { SectionCard } from '../../components/ui/SectionCard';
import { exploreService } from '../../services/exploreService';
import { useI18n } from '../../i18n';

export function ExploreSavedPage() {
  const { t } = useI18n();
  const [items, setItems] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      try {
        setError('');
        const response = await exploreService.getSaved();
        if (!mounted) return;
        setItems(response.data.data || []);
      } catch (err) {
        if (!mounted) return;
        setItems([]);
        setError(err?.response?.status === 404 ? t('student.savedPage.notAvailable') : t('student.savedPage.loadError'));
      }
    };
    load();
    return () => { mounted = false; };
  }, [t]);

  return (
    <PageContainer className="space-y-4">
      <SectionCard title={t('student.savedPage.title')} description={t('student.savedPage.description')}>
        {items.length === 0 ? (
          <EmptyState icon={Bookmark} title={t('student.savedPage.emptyTitle')} description={error || t('student.savedPage.emptyDescription')} />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((trainer) => (
              <article key={trainer.trainerId} className="rounded-2xl border border-slate-200 bg-white p-4 dark:bg-slate-900 dark:border-white/10">
                <p className="text-sm font-semibold text-slate-900 dark:text-white">{trainer.brandName || trainer.fullName}</p>
                <p className="text-xs text-slate-500">{trainer.city || '-'}{trainer.state ? `, ${trainer.state}` : ''}</p>
              </article>
            ))}
          </div>
        )}
      </SectionCard>
    </PageContainer>
  );
}
