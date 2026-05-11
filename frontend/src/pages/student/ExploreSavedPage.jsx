import { useEffect, useState } from 'react';
import { Bookmark } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { EmptyState } from '../../components/ui/EmptyState';
import { SectionCard } from '../../components/ui/SectionCard';
import { exploreService } from '../../services/exploreService';

export function ExploreSavedPage() {
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
        setError(err?.response?.status === 404
          ? 'A lista de salvos ainda não está disponível.'
          : 'Não foi possível carregar seus salvos agora.');
      }
    };
    load();
    return () => { mounted = false; };
  }, []);

  return (
    <PageContainer className="space-y-4">
      <SectionCard title="Personais salvos" description="Perfis que você marcou para acompanhar depois.">
        {items.length === 0 ? (
          <EmptyState icon={Bookmark} title="Nenhum personal salvo" description={error || 'Quando você salvar personais, eles aparecerão aqui.'} />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((trainer) => (
              <article key={trainer.trainerId} className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-900">{trainer.brandName || trainer.fullName}</p>
                <p className="text-xs text-slate-500">{trainer.city || '-'}{trainer.state ? `, ${trainer.state}` : ''}</p>
              </article>
            ))}
          </div>
        )}
      </SectionCard>
    </PageContainer>
  );
}
