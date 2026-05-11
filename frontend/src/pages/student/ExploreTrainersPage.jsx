import { useEffect, useState } from 'react';
import { MapPin, Users } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { EmptyState } from '../../components/ui/EmptyState';
import { exploreService } from '../../services/exploreService';

const defaultQuery = {
  search: '',
  city: '',
  state: '',
  specialty: '',
  serviceMode: '',
  page: 1,
  pageSize: 20,
};

export function ExploreTrainersPage() {
  const [query, setQuery] = useState(defaultQuery);
  const [location, setLocation] = useState({ latitude: null, longitude: null });
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [locationLoading, setLocationLoading] = useState(false);
  const [actionLoadingId, setActionLoadingId] = useState(null);

  const load = async (params = {}) => {
    setLoading(true);
    try {
      setError('');
      const finalParams = { ...query, ...params };
      const response = await exploreService.getTrainers(finalParams);
      const payload = response.data?.data || {};
      setItems(payload.items || []);
      setTotal(payload.total || 0);
    } catch {
      setItems([]);
      setTotal(0);
      setError('Não foi possível carregar a busca de personais agora.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load(defaultQuery).catch(() => {});
  }, []);

  const handleSearch = async (event) => {
    event.preventDefault();
    await load({ ...query, ...location, page: 1 });
  };

  const handleUseMyLocation = () => {
    if (!navigator.geolocation) {
      setError('Seu navegador não suporta localização. Você ainda pode buscar por cidade ou estado.');
      return;
    }

    setLocationLoading(true);
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const lat = Number(position.coords.latitude.toFixed(6));
        const lng = Number(position.coords.longitude.toFixed(6));
        const nextLocation = { latitude: lat, longitude: lng };
        setLocation(nextLocation);
        await load({ ...query, ...nextLocation, radiusKm: 50, page: 1 });
        setLocationLoading(false);
      },
      () => {
        setLocationLoading(false);
        setError('Não foi possível acessar sua localização. Você ainda pode buscar por cidade ou estado.');
      },
      {
        enableHighAccuracy: false,
        timeout: 10000,
        maximumAge: 300000,
      },
    );
  };

  const getErrorMessage = (err, fallback) =>
    err?.response?.data?.message || err?.response?.data?.errors?.[0] || fallback;

  const toggleFollow = async (trainer) => {
    try {
      setActionLoadingId(trainer.trainerId);
      if (trainer.isFollowedByCurrentUser) {
        await exploreService.unfollowTrainer(trainer.trainerId);
        setItems((prev) => prev.map((x) => (x.trainerId === trainer.trainerId ? { ...x, isFollowedByCurrentUser: false } : x)));
        return;
      }

      await exploreService.followTrainer(trainer.trainerId);
      setItems((prev) => prev.map((x) => (x.trainerId === trainer.trainerId ? { ...x, isFollowedByCurrentUser: true } : x)));
    } catch (err) {
      setError(getErrorMessage(err, 'Não foi possível atualizar o status de seguir.'));
    } finally {
      setActionLoadingId(null);
    }
  };

  const toggleSave = async (trainer) => {
    try {
      setActionLoadingId(trainer.trainerId);
      if (trainer.isSavedByCurrentUser) {
        await exploreService.unsaveTrainer(trainer.trainerId);
        setItems((prev) => prev.map((x) => (x.trainerId === trainer.trainerId ? { ...x, isSavedByCurrentUser: false } : x)));
        return;
      }

      await exploreService.saveTrainer(trainer.trainerId);
      setItems((prev) => prev.map((x) => (x.trainerId === trainer.trainerId ? { ...x, isSavedByCurrentUser: true } : x)));
    } catch (err) {
      setError(getErrorMessage(err, 'Não foi possível atualizar o status de salvar.'));
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleOnlyOnline = async () => {
    const next = { ...query, serviceMode: 'Online', page: 1 };
    setQuery(next);
    await load({ ...next, ...location });
  };

  const clearFilters = async () => {
    setQuery(defaultQuery);
    await load({ ...defaultQuery, ...location });
  };

  return (
    <PageContainer className="space-y-4">
      <section className="rounded-3xl border border-slate-200 bg-white p-5">
        <h1 className="text-2xl font-bold text-slate-900">Encontre o personal ideal</h1>
        <p className="mt-1 text-sm text-slate-500">Pesquise por nome, cidade, estado, especialidade e modalidade.</p>
        <form onSubmit={handleSearch} className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3 lg:grid-cols-6">
          <Input label="Busca" value={query.search} onChange={(e) => setQuery((prev) => ({ ...prev, search: e.target.value }))} />
          <Input label="Cidade" value={query.city} onChange={(e) => setQuery((prev) => ({ ...prev, city: e.target.value }))} />
          <Input label="Estado" value={query.state} onChange={(e) => setQuery((prev) => ({ ...prev, state: e.target.value }))} />
          <Input label="Especialidade" value={query.specialty} onChange={(e) => setQuery((prev) => ({ ...prev, specialty: e.target.value }))} />
          <Input label="Modalidade" placeholder="Online, InPerson, Hybrid" value={query.serviceMode} onChange={(e) => setQuery((prev) => ({ ...prev, serviceMode: e.target.value }))} />
          <Button className="self-end" loading={loading}>Buscar</Button>
        </form>
        <div className="mt-3 flex flex-wrap gap-2">
          <Button variant="outline" onClick={handleUseMyLocation} loading={locationLoading}>
            <MapPin size={14} />Usar minha localização
          </Button>
          <Button variant="ghost" onClick={clearFilters}>Limpar filtros</Button>
          <Button variant="ghost" onClick={handleOnlyOnline}>Ver personais online</Button>
        </div>
        <p className="mt-2 text-xs text-slate-500">Total encontrado: {total}</p>
      </section>

      {items.length ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((trainer) => (
            <article key={trainer.trainerId} className="rounded-2xl border border-slate-200 bg-white p-4">
              <p className="text-sm font-semibold text-slate-900">{trainer.brandName || trainer.fullName}</p>
              <p className="mt-1 text-xs text-slate-500">{trainer.headline || trainer.bio || 'Personal trainer'}</p>
              <div className="mt-2 flex flex-wrap gap-1">
                {trainer.serviceMode && <span className="rounded bg-slate-100 px-2 py-0.5 text-[11px] text-slate-700">{trainer.serviceMode}</span>}
                {trainer.acceptingStudents && <span className="rounded bg-emerald-100 px-2 py-0.5 text-[11px] text-emerald-700">Aceitando alunos</span>}
                {trainer.distanceKm != null && <span className="rounded bg-indigo-100 px-2 py-0.5 text-[11px] text-indigo-700">A {trainer.distanceKm} km de você</span>}
              </div>
              <p className="mt-2 text-xs text-slate-400">{trainer.city || '-'}{trainer.state ? `, ${trainer.state}` : ''}</p>
              <div className="mt-3 flex gap-2">
                <a href={`/p/${trainer.slug}`} className="rounded-lg border border-slate-300 px-3 py-1 text-xs text-slate-700">Ver perfil</a>
                <button
                  disabled={actionLoadingId === trainer.trainerId}
                  onClick={() => toggleFollow(trainer)}
                  className="rounded-lg bg-indigo-600 px-3 py-1 text-xs text-white disabled:opacity-60"
                >
                  {trainer.isFollowedByCurrentUser ? 'Seguindo' : 'Seguir'}
                </button>
                <button
                  disabled={actionLoadingId === trainer.trainerId}
                  onClick={() => toggleSave(trainer)}
                  className="rounded-lg border border-indigo-300 px-3 py-1 text-xs text-indigo-700 disabled:opacity-60"
                >
                  {trainer.isSavedByCurrentUser ? 'Salvo' : 'Salvar'}
                </button>
              </div>
            </article>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={Users}
          title="Nenhum personal encontrado"
          description={error || 'Tente ajustar os filtros, buscar por outra cidade ou explorar personais online.'}
          action={
            <div className="flex gap-2">
              <Button variant="outline" onClick={clearFilters}>Limpar filtros</Button>
              <Button onClick={handleOnlyOnline}>Ver personais online</Button>
            </div>
          }
        />
      )}
    </PageContainer>
  );
}
