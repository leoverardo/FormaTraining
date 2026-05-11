import { useEffect, useState } from 'react';
import { HeartHandshake } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { EmptyState } from '../../components/ui/EmptyState';
import { SectionCard } from '../../components/ui/SectionCard';
import { exploreService } from '../../services/exploreService';

export function ExploreFollowingPage() {
  const [items, setItems] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      try {
        setError('');
        const response = await exploreService.getFollowing();
        if (!mounted) return;
        setItems(response.data.data || []);
      } catch (err) {
        if (!mounted) return;
        setItems([]);
        setError(err?.response?.status === 404
          ? 'A lista de personais seguidos ainda não está disponível.'
          : 'Não foi possível carregar seus seguidos agora.');
      }
    };
    load();
    return () => { mounted = false; };
  }, []);

  return (
    <PageContainer className="space-y-4">
      <SectionCard title="Personais que sigo" description="Conteúdos públicos dos personais que você acompanha.">
        {items.length === 0 ? (
          <EmptyState icon={HeartHandshake} title="Você ainda não segue personais" description={error || 'Quando seguir personais, eles aparecerão aqui.'} />
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
