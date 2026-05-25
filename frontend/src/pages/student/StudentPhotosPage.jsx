import { useEffect, useMemo, useState } from 'react';
import { progressService } from '../../services/progressService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Modal } from '../../components/ui/Modal';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { MediaCategory } from '../../services/uploadService';
import { Camera, Plus, Trash2 } from 'lucide-react';
import { useI18n } from '../../i18n';

function PhotosSkeleton() {
  return (
    <PageContainer className="space-y-4">
      <Skeleton className="h-24 w-full" />
      <Skeleton className="h-52 w-full" />
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
        <Skeleton className="aspect-square w-full" />
        <Skeleton className="aspect-square w-full" />
        <Skeleton className="aspect-square w-full" />
      </div>
    </PageContainer>
  );
}

export function StudentPhotosPage() {
  const { toast } = useToast();
  const { t, language } = useI18n();
  const [photos, setPhotos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState({ imageUrl: '', description: '', photoDate: new Date().toISOString().split('T')[0] });
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [compareAId, setCompareAId] = useState('');
  const [compareBId, setCompareBId] = useState('');

  const load = () => progressService.getOwnPhotos().then((r) => setPhotos(r.data.data || [])).finally(() => setLoading(false));
  useEffect(() => { load(); }, []);

  const sortedPhotos = useMemo(() => [...photos].sort((a, b) => new Date(a.photoDate) - new Date(b.photoDate)), [photos]);

  useEffect(() => {
    if (sortedPhotos.length < 2) return;
    if (!compareAId) setCompareAId(String(sortedPhotos[0].id));
    if (!compareBId) setCompareBId(String(sortedPhotos[sortedPhotos.length - 1].id));
  }, [sortedPhotos, compareAId, compareBId]);

  const compareA = sortedPhotos.find((p) => String(p.id) === String(compareAId));
  const compareB = sortedPhotos.find((p) => String(p.id) === String(compareBId));

  const handleSave = async (e) => {
    e.preventDefault();
    if (!form.imageUrl) {
      toast(t('student.photos.selectPhotoError'), 'error');
      return;
    }
    setSaving(true);
    try {
      await progressService.addOwnPhoto(form);
      toast(t('student.photos.addedSuccess'));
      setModalOpen(false);
      setForm({ imageUrl: '', description: '', photoDate: new Date().toISOString().split('T')[0] });
      load();
    } catch {
      toast(t('student.photos.addError'), 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    try {
      await progressService.deleteOwnPhoto(deleteTarget);
      toast(t('student.photos.deletedSuccess'));
      setDeleteTarget(null);
      load();
    } catch {
      toast(t('common.error'), 'error');
    }
  };

  if (loading) return <PhotosSkeleton />;

  return (
    <PageContainer className="space-y-5 pb-20 sm:pb-0">
      <section className="rounded-3xl bg-gradient-to-r from-slate-900 via-indigo-900 to-cyan-800 p-5 sm:p-6 text-white shadow-[0_16px_38px_rgba(15,23,42,0.32)]">
        <div className="flex items-start justify-between gap-3 flex-wrap">
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold">{t('student.photos.title')}</h1>
            <p className="text-slate-200 text-sm mt-2">{t('student.photos.privateDescription')}</p>
          </div>
          <Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />{t('student.photos.add')}</Button>
        </div>
      </section>

      {photos.length >= 2 && (
        <SectionCard title={t('student.photos.beforeAfterTitle')} description={t('student.photos.beforeAfterDescription')}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mb-3">
            <div>
              <p className="text-xs text-slate-500 mb-1">{t('student.photos.before')}</p>
              <select className="w-full text-sm border border-slate-200 rounded-xl px-3 py-2 mb-2 bg-white" value={compareAId} onChange={(e) => setCompareAId(e.target.value)}>
                {sortedPhotos.map((p) => <option key={p.id} value={p.id}>{new Date(p.photoDate).toLocaleDateString(language)}</option>)}
              </select>
              {compareA ? <img src={compareA.imageUrl} alt={t('student.photos.before')} className="w-full h-56 object-cover rounded-2xl border border-slate-100" /> : null}
            </div>
            <div>
              <p className="text-xs text-slate-500 mb-1">{t('student.photos.after')}</p>
              <select className="w-full text-sm border border-slate-200 rounded-xl px-3 py-2 mb-2 bg-white" value={compareBId} onChange={(e) => setCompareBId(e.target.value)}>
                {sortedPhotos.map((p) => <option key={p.id} value={p.id}>{new Date(p.photoDate).toLocaleDateString(language)}</option>)}
              </select>
              {compareB ? <img src={compareB.imageUrl} alt={t('student.photos.after')} className="w-full h-56 object-cover rounded-2xl border border-slate-100" /> : null}
            </div>
          </div>
        </SectionCard>
      )}

      {photos.length === 0 ? (
        <EmptyState
          icon={Camera}
          title={t('student.photos.emptyTitle')}
          description={t('student.photos.emptyDescription')}
          action={<Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />{t('student.photos.addPhoto')}</Button>}
        />
      ) : (
        <SectionCard title={t('student.photos.galleryTitle')} description={t('student.photos.privateRecords', { value: photos.length })}>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
            {photos.map((p) => (
              <div key={p.id} className="relative group rounded-2xl overflow-hidden border border-slate-200 aspect-square">
                <img src={p.imageUrl} alt={p.description || t('student.photos.progressPhoto')} className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105" />
                <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/10 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
                  <div className="absolute bottom-2 left-2 right-2 flex items-end justify-between gap-2">
                    <div>
                      <p className="text-white text-xs font-semibold">{new Date(p.photoDate).toLocaleDateString(language)}</p>
                      {p.description ? <p className="text-white/80 text-xs truncate max-w-[110px]">{p.description}</p> : null}
                    </div>
                    <button onClick={() => setDeleteTarget(p.id)} className="p-1.5 bg-red-500 rounded-lg text-white hover:bg-red-600"><Trash2 size={12} /></button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </SectionCard>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={t('student.photos.addProgressPhoto')}>
        <form onSubmit={handleSave} className="space-y-4">
          <ImageUpload
            label={t('student.photos.progressPhoto')}
            description={t('student.photos.uploadHint')}
            category={MediaCategory.ProgressPhoto}
            currentUrl={form.imageUrl}
            isPublic={false}
            onUploaded={(m) => setForm((p) => ({ ...p, imageUrl: m.url }))}
            onRemoved={() => setForm((p) => ({ ...p, imageUrl: '' }))}
          />
          <p className="text-xs text-slate-500 bg-slate-50 rounded-xl p-3">{t('student.photos.privateHint')}</p>
          <Input label={t('student.photos.photoDate')} type="date" value={form.photoDate} onChange={(e) => setForm((p) => ({ ...p, photoDate: e.target.value }))} />
          <Input label={t('student.photos.descriptionOptional')} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} />
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">{t('common.cancel')}</Button>
            <Button type="submit" loading={saving} className="flex-1">{t('student.photos.add')}</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} title={t('student.photos.removePhoto')} description={t('student.photos.removePhotoConfirm')} />
    </PageContainer>
  );
}
