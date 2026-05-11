import { useEffect, useState } from 'react';
import { FileText, MapPin } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { FeedCard } from '../../components/social/FeedCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { exploreService } from '../../services/exploreService';
import { mapPostToFeedItem } from '../../features/feed/feedAdapter';

export function ExploreFeedPage() {
  const [items, setItems] = useState([]);
  const [recommended, setRecommended] = useState([]);
  const [feedError, setFeedError] = useState('');
  const [recommendedError, setRecommendedError] = useState('');
  const [locationLoading, setLocationLoading] = useState(false);
  const [location, setLocation] = useState({ latitude: null, longitude: null });

  useEffect(() => {
    let mounted = true;

    const loadFeed = async () => {
      try {
        setFeedError('');
        const response = await exploreService.getFeed({ page: 1, pageSize: 20 });
        if (!mounted) return;
        setItems(response.data.data || []);
      } catch (error) {
        if (!mounted) return;
        setItems([]);
        setFeedError(
          error?.response?.status === 404
            ? 'O feed Explore ainda nao esta disponivel.'
            : 'Nao foi possivel carregar o feed agora.',
        );
      }
    };

    const loadRecommended = async (params = {}) => {
      try {
        setRecommendedError('');
        const response = await exploreService.getRecommended(params);
        if (!mounted) return;
        setRecommended(response.data?.data?.items || []);
      } catch (error) {
        if (!mounted) return;
        setRecommended([]);
        setRecommendedError(
          error?.response?.status === 404
            ? 'As recomendacoes ainda nao estao disponiveis.'
            : 'Nao foi possivel carregar recomendacoes agora.',
        );
      }
    };

    loadFeed();
    loadRecommended();
    return () => { mounted = false; };
  }, []);

  const handleUseMyLocation = () => {
    if (!navigator.geolocation) {
      setRecommendedError('Seu navegador não suporta localização. Você ainda pode explorar normalmente.');
      return;
    }

    setLocationLoading(true);
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const params = {
          latitude: Number(position.coords.latitude.toFixed(6)),
          longitude: Number(position.coords.longitude.toFixed(6)),
        };

        setLocation(params);
        try {
          const response = await exploreService.getRecommended(params);
          setRecommended(response.data?.data?.items || []);
          setRecommendedError('');
        } catch {
          setRecommended([]);
          setRecommendedError('Não foi possível carregar recomendações por localização agora.');
        } finally {
          setLocationLoading(false);
        }
      },
      () => {
        setLocationLoading(false);
        setRecommendedError('Não foi possível acessar sua localização. Você ainda pode buscar por cidade ou estado.');
      },
      {
        enableHighAccuracy: false,
        timeout: 10000,
        maximumAge: 300000,
      },
    );
  };

  const feed = items.map((item) => mapPostToFeedItem(item, {
    name: item.authorName,
    role: 'Trainer',
    avatarUrl: item.authorAvatarUrl,
  }));

  return (
    <PageContainer className="space-y-4">
      <section className="rounded-3xl border border-slate-200 bg-gradient-to-r from-cyan-500 to-indigo-600 p-6 text-white">
        <h1 className="text-2xl font-bold">Descubra personais e conteudos para evoluir</h1>
        <p className="mt-1 text-sm text-cyan-100">Feed publico com dicas, aulas e publicacoes abertas.</p>
      </section>

      <SectionCard title="Feed publico">
        {feed.length ? (
          <div className="space-y-3">
            {feed.map((item, index) => <FeedCard key={item.id ?? `explore-${index}`} item={item} />)}
          </div>
        ) : (
          <EmptyState
            icon={FileText}
            title="Sem conteudos publicos"
            description={feedError || 'Publique conteudos publicos para aparecer aqui.'}
          />
        )}
      </SectionCard>

      <SectionCard title="Personais recomendados para voce">
        <div className="mb-3">
          <Button variant="outline" onClick={handleUseMyLocation} loading={locationLoading}>
            <MapPin size={14} />Encontrar personais próximos
          </Button>
        </div>
        {recommended.length ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {recommended.map((trainer) => (
              <div key={trainer.trainerId} className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-900">{trainer.brandName || trainer.fullName}</p>
                <p className="text-xs text-slate-500">
                  {trainer.city || '-'}
                  {trainer.state ? `, ${trainer.state}` : ''}
                </p>
                {trainer.distanceKm != null && <p className="mt-1 text-xs text-indigo-600">A {trainer.distanceKm} km de você</p>}
                {!trainer.distanceKm && location.latitude && <p className="mt-1 text-xs text-slate-400">Distância indisponível</p>}
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            icon={FileText}
            title="Sem recomendacoes por enquanto"
            description={recommendedError || 'As recomendacoes aparecerao quando houver personais publicos compativeis.'}
          />
        )}
      </SectionCard>
    </PageContainer>
  );
}
