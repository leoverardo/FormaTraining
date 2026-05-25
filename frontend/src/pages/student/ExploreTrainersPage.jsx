import { useEffect, useState } from 'react';
import { MapPin, Users } from 'lucide-react';
import { PageContainer } from '../../components/ui/PageContainer';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { EmptyState } from '../../components/ui/EmptyState';
import { exploreService } from '../../services/exploreService';
import { privacyService } from '../../services/privacyService';
import { useI18n } from '../../i18n';
import { useDomainLabels } from '../../i18n/domainLabels';

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
  const { t } = useI18n();
  const { serviceModeLabel } = useDomainLabels();
  const [query, setQuery] = useState(defaultQuery);
  const [location, setLocation] = useState({ latitude: null, longitude: null });
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [error, setError] = useState('');
  const [hasLoadError, setHasLoadError] = useState(false);
  const [loading, setLoading] = useState(false);
  const [locationLoading, setLocationLoading] = useState(false);
  const [actionLoadingId, setActionLoadingId] = useState(null);
  const [showGeoPrompt, setShowGeoPrompt] = useState(false);

  const load = async (params = {}) => {
    setLoading(true);
    try {
      setError('');
      const finalParams = { ...query, ...params };
      const response = await exploreService.getTrainers(finalParams);
      const payload = response.data?.data || {};
      setItems(payload.items || []);
      setTotal(payload.total || 0);
      setHasLoadError(false);
    } catch {
      setItems([]);
      setTotal(0);
      setHasLoadError(true);
      setError(t('student.exploreLoadError'));
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

  const handleUseMyLocation = () => setShowGeoPrompt(true);

  const confirmUseMyLocation = () => {
    if (!navigator.geolocation) {
      setError(t('student.geolocationUnsupported'));
      setShowGeoPrompt(false);
      return;
    }

    setLocationLoading(true);
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const lat = Number(position.coords.latitude.toFixed(6));
        const lng = Number(position.coords.longitude.toFixed(6));
        const nextLocation = { latitude: lat, longitude: lng };
        setLocation(nextLocation);
        await privacyService.updateConsent('GEOLOCATION_FOR_EXPLORE', true).catch(() => {});
        await load({ ...query, ...nextLocation, radiusKm: 50, page: 1 });
        setLocationLoading(false);
        setShowGeoPrompt(false);
      },
      () => {
        setLocationLoading(false);
        setShowGeoPrompt(false);
        setError(t('student.geolocationAccessError'));
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
      setError(getErrorMessage(err, t('student.followUpdateError')));
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
      setError(getErrorMessage(err, t('student.saveUpdateError')));
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
      <section className="rounded-3xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 p-5">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('student.findTrainer')}</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('student.findTrainerDescription')}</p>
        <form onSubmit={handleSearch} className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3 lg:grid-cols-6">
          <Input label={t('common.search')} value={query.search} onChange={(e) => setQuery((prev) => ({ ...prev, search: e.target.value }))} />
          <Input label={t('student.city')} value={query.city} onChange={(e) => setQuery((prev) => ({ ...prev, city: e.target.value }))} />
          <Input label={t('student.state')} value={query.state} onChange={(e) => setQuery((prev) => ({ ...prev, state: e.target.value }))} />
          <Input label={t('student.specialty')} value={query.specialty} onChange={(e) => setQuery((prev) => ({ ...prev, specialty: e.target.value }))} />
          <Input label={t('student.serviceMode')} placeholder="Online, InPerson, Hybrid" value={query.serviceMode} onChange={(e) => setQuery((prev) => ({ ...prev, serviceMode: e.target.value }))} />
          <Button className="self-end" loading={loading}>{t('common.search')}</Button>
        </form>
        <div className="mt-3 flex flex-wrap gap-2">
          <Button variant="outline" onClick={handleUseMyLocation} loading={locationLoading}>
            <MapPin size={14} />{t('student.useMyLocation')}
          </Button>
          <Button variant="ghost" onClick={clearFilters}>{t('student.clearFilters')}</Button>
          <Button variant="ghost" onClick={handleOnlyOnline}>{t('student.viewOnlineTrainers')}</Button>
        </div>
        {showGeoPrompt && (
          <div className="mt-3 rounded-xl border border-indigo-200 bg-indigo-50 p-3 text-sm">
            <p>{t('student.locationPrompt')}</p>
            <div className="mt-2 flex gap-2">
              <Button onClick={confirmUseMyLocation} loading={locationLoading}>{t('student.allowLocation')}</Button>
              <Button variant="outline" onClick={() => { setShowGeoPrompt(false); privacyService.updateConsent('GEOLOCATION_FOR_EXPLORE', false).catch(() => {}); }}>
                {t('student.continueWithoutLocation')}
              </Button>
            </div>
          </div>
        )}
        <p className="mt-2 text-xs text-slate-500 dark:text-slate-400">{t('student.totalFound')}: {total}</p>
      </section>

      {items.length ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((trainer) => (
            <article key={trainer.trainerId} className="rounded-2xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 p-4">
              <p className="text-sm font-semibold text-slate-900 dark:text-white">{trainer.brandName || trainer.fullName}</p>
              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{trainer.headline || trainer.bio || t('student.personalTrainer')}</p>
              <div className="mt-2 flex flex-wrap gap-1">
                {trainer.serviceMode && <span className="rounded bg-slate-100 dark:bg-white/10 px-2 py-0.5 text-[11px] text-slate-700 dark:text-slate-200">{serviceModeLabel(trainer.serviceMode)}</span>}
                {trainer.acceptingStudents && <span className="rounded bg-emerald-100 dark:bg-emerald-500/20 px-2 py-0.5 text-[11px] text-emerald-700 dark:text-emerald-300">{t('student.acceptingStudents')}</span>}
                {trainer.distanceKm != null && <span className="rounded bg-indigo-100 dark:bg-indigo-500/20 px-2 py-0.5 text-[11px] text-indigo-700 dark:text-indigo-300">{t('student.distanceKm', { value: trainer.distanceKm })}</span>}
              </div>
              <p className="mt-2 text-xs text-slate-400">{trainer.city || '-'}{trainer.state ? `, ${trainer.state}` : ''}</p>
              <div className="mt-3 flex gap-2">
                <a href={`/p/${trainer.slug}`} className="rounded-lg border border-slate-300 dark:border-white/10 px-3 py-1 text-xs text-slate-700 dark:text-slate-200">{t('student.viewProfile')}</a>
                <button
                  disabled={actionLoadingId === trainer.trainerId}
                  onClick={() => toggleFollow(trainer)}
                  className="rounded-lg bg-indigo-600 px-3 py-1 text-xs text-white disabled:opacity-60"
                >
                  {trainer.isFollowedByCurrentUser ? t('student.following') : t('student.follow')}
                </button>
                <button
                  disabled={actionLoadingId === trainer.trainerId}
                  onClick={() => toggleSave(trainer)}
                  className="rounded-lg border border-indigo-300 px-3 py-1 text-xs text-indigo-700 disabled:opacity-60"
                >
                  {trainer.isSavedByCurrentUser ? t('student.saved') : t('student.save')}
                </button>
              </div>
            </article>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={Users}
          title={hasLoadError ? t('student.exploreLoadErrorTitle') : t('student.noTrainersFound')}
          description={hasLoadError ? error : t('student.noTrainersFoundDescription')}
          action={
            <div className="flex gap-2">
              <Button variant="outline" onClick={clearFilters}>{t('student.clearFilters')}</Button>
              <Button onClick={handleOnlyOnline}>{t('student.viewOnlineTrainers')}</Button>
            </div>
          }
        />
      )}
    </PageContainer>
  );
}
