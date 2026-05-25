import { useEffect, useState } from 'react';
import { FileText, MapPin } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { FeedCard } from '../../components/social/FeedCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { exploreService } from '../../services/exploreService';
import { privacyService } from '../../services/privacyService';
import { mapPostToFeedItem } from '../../features/feed/feedAdapter';

export function ExploreFeedPage() {
  const [items, setItems] = useState([]);
  const [recommended, setRecommended] = useState([]);
  const [feedError, setFeedError] = useState('');
  const [recommendedError, setRecommendedError] = useState('');
  const [feedLoadError, setFeedLoadError] = useState(false);
  const [recommendedLoadError, setRecommendedLoadError] = useState(false);
  const [locationLoading, setLocationLoading] = useState(false);
  const [location, setLocation] = useState({ latitude: null, longitude: null });
  const [showGeoPrompt, setShowGeoPrompt] = useState(false);

  useEffect(() => {
    let mounted = true;

    const loadFeed = async () => {
      try {
        setFeedError('');
        const response = await exploreService.getFeed({ page: 1, pageSize: 20 });
        if (!mounted) return;
        setItems(response.data.data || []);
        setFeedLoadError(false);
      } catch (error) {
        if (!mounted) return;
        setItems([]);
        setFeedLoadError(true);
        setFeedError(error?.response?.status === 404 ? 'O feed Explore ainda não está disponível.' : 'Não foi possível carregar o feed agora.');
      }
    };

    const loadRecommended = async (params = {}) => {
      try {
        setRecommendedError('');
        const response = await exploreService.getRecommended(params);
        if (!mounted) return;
        setRecommended(response.data?.data?.items || []);
        setRecommendedLoadError(false);
      } catch (error) {
        if (!mounted) return;
        setRecommended([]);
        setRecommendedLoadError(true);
        setRecommendedError(error?.response?.status === 404 ? 'As recomendações ainda não estão disponíveis.' : 'Não foi possível carregar recomendações agora.');
      }
    };

    loadFeed();
    loadRecommended();
    return () => { mounted = false; };
  }, []);

  const handleUseMyLocation = () => setShowGeoPrompt(true);

  const confirmUseMyLocation = () => {
    if (!navigator.geolocation) {
      setRecommendedError('Seu navegador não suporta localização. Você ainda pode explorar normalmente.');
      setShowGeoPrompt(false);
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
        await privacyService.updateConsent('GEOLOCATION_FOR_EXPLORE', true).catch(() => {});
        try {
          const response = await exploreService.getRecommended(params);
          setRecommended(response.data?.data?.items || []);
          setRecommendedError('');
          setRecommendedLoadError(false);
        } catch {
          setRecommended([]);
          setRecommendedLoadError(true);
          setRecommendedError('Não foi possível carregar recomendações por localização agora.');
        } finally {
          setLocationLoading(false);
          setShowGeoPrompt(false);
        }
      },
      () => {
        setLocationLoading(false);
        setShowGeoPrompt(false);
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
        <h1 className="text-2xl font-bold">Descubra personais e conteúdos para evoluir</h1>
        <p className="mt-1 text-sm text-cyan-100">Feed público com dicas, aulas e publicações abertas.</p>
      </section>

      <SectionCard title="Feed público">
        {feed.length ? (
          <div className="space-y-3">
            {feed.map((item, index) => <FeedCard key={item.id ?? `explore-${index}`} item={item} />)}
          </div>
        ) : (
          <EmptyState icon={FileText} title={feedLoadError ? 'Erro ao carregar feed' : 'Sem conteúdos públicos'} description={feedError || 'Publique conteúdos públicos para aparecer aqui.'} />
        )}
      </SectionCard>

      <SectionCard title="Personais recomendados para você">
        <div className="mb-3">
          <Button variant="outline" onClick={handleUseMyLocation} loading={locationLoading}>
            <MapPin size={14} />Encontrar personais próximos
          </Button>
        </div>
        {showGeoPrompt && (
          <div className="mb-3 rounded-xl border border-indigo-200 bg-indigo-50 p-3 text-sm">
            <p>Usamos sua localização aproximada apenas para sugerir trainers próximos. Você pode continuar sem permitir.</p>
            <div className="mt-2 flex gap-2">
              <Button onClick={confirmUseMyLocation} loading={locationLoading}>Permitir localização</Button>
              <Button variant="outline" onClick={() => { setShowGeoPrompt(false); privacyService.updateConsent('GEOLOCATION_FOR_EXPLORE', false).catch(() => {}); }}>
                Continuar sem localização
              </Button>
            </div>
          </div>
        )}
        {recommended.length ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {recommended.map((trainer) => (
              <div key={trainer.trainerId} className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-900">{trainer.brandName || trainer.fullName}</p>
                <p className="text-xs text-slate-500">
                  {trainer.city || '-'}
                  {trainer.state ? `, ${trainer.state}` : ''}
                </p>
                {trainer.distanceKm != null && <p className="mt-1 text-xs text-indigo-600">A {trainer.distanceKm} km de voce</p>}
                {!trainer.distanceKm && location.latitude && <p className="mt-1 text-xs text-slate-400">Distancia indisponivel</p>}
              </div>
            ))}
          </div>
        ) : (
          <EmptyState icon={FileText} title={recommendedLoadError ? 'Erro ao carregar recomendações' : 'Sem recomendações por enquanto'} description={recommendedError || 'As recomendações aparecerão quando houver personais públicos compatíveis.'} />
        )}
      </SectionCard>
    </PageContainer>
  );
}
