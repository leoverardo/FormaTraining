import { useEffect, useMemo, useState } from 'react';
import { progressService } from '../../services/progressService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Modal } from '../../components/ui/Modal';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { TrendingUp, Plus, Trash2 } from 'lucide-react';
import { useI18n } from '../../i18n';

const emptyForm = {
  weight: '', height: '', chest: '', waist: '', abdomen: '', hip: '',
  rightArm: '', leftArm: '', rightThigh: '', leftThigh: '', bodyFatPercentage: '',
  notes: '', progressDate: new Date().toISOString().split('T')[0],
};

const metricLabels = [
  ['weight', 'Peso', 'kg'],
  ['height', 'Altura', 'cm'],
  ['chest', 'Peito', 'cm'],
  ['waist', 'Cintura', 'cm'],
  ['abdomen', 'Abdômen', 'cm'],
  ['hip', 'Quadril', 'cm'],
  ['rightArm', 'Braço D', 'cm'],
  ['leftArm', 'Braço E', 'cm'],
  ['rightThigh', 'Coxa D', 'cm'],
  ['leftThigh', 'Coxa E', 'cm'],
  ['bodyFatPercentage', '% Gordura', '%'],
];

function ProgressSkeleton() {
  return (
    <PageContainer className="space-y-4">
      <Skeleton className="h-24 w-full" />
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
      </div>
      <Skeleton className="h-48 w-full" />
      <Skeleton className="h-48 w-full" />
    </PageContainer>
  );
}

function valueDiff(current, previous) {
  if (current == null || previous == null) return null;
  const diff = Number(current) - Number(previous);
  if (Number.isNaN(diff)) return null;
  return diff;
}

export function StudentProgressPage() {
  const { toast } = useToast();
  const { t, language } = useI18n();
  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const load = () => progressService.getOwn().then((r) => setRecords(r.data.data || [])).finally(() => setLoading(false));
  useEffect(() => { load(); }, []);

  const latest = records[0];
  const previous = records[1];

  const highlights = useMemo(() => {
    if (!latest) return [];
    const selected = [
      ['Peso', latest.weight, previous?.weight, 'kg'],
      ['Cintura', latest.waist, previous?.waist, 'cm'],
      ['% Gordura', latest.bodyFatPercentage, previous?.bodyFatPercentage, '%'],
      ['Abdômen', latest.abdomen, previous?.abdomen, 'cm'],
    ];
    return selected
      .filter(([, current]) => current != null)
      .map(([label, current, prev, unit]) => ({ label, current, diff: valueDiff(current, prev), unit }));
  }, [latest, previous]);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = Object.fromEntries(
        Object.entries(form).map(([k, v]) => [k, k === 'progressDate' || k === 'notes' ? (v || null) : (v === '' ? null : parseFloat(v) || null)]),
      );
      await progressService.createOwn(payload);
      toast(t('student.progress.savedSuccess'));
      setModalOpen(false);
      setForm(emptyForm);
      load();
    } catch (err) {
      toast(err.response?.data?.message || t('common.error'), 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    try {
      await progressService.deleteOwn(deleteTarget);
      toast(t('student.progress.deletedSuccess'));
      setDeleteTarget(null);
      load();
    } catch {
      toast(t('common.error'), 'error');
    }
  };

  const f = (field) => ({ value: form[field], onChange: (e) => setForm((p) => ({ ...p, [field]: e.target.value })) });

  if (loading) return <ProgressSkeleton />;

  return (
    <PageContainer className="space-y-5 pb-20 sm:pb-0">
      <section className="rounded-3xl bg-gradient-to-r from-indigo-600 via-violet-600 to-cyan-600 p-5 sm:p-6 text-white shadow-[0_16px_38px_rgba(79,70,229,0.32)]">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="text-indigo-100 text-sm">{t('student.progress.trackEvolution')}</p>
            <h1 className="text-2xl sm:text-3xl font-bold mt-1">{t('nav.progress')}</h1>
            <p className="text-indigo-100 text-sm mt-2">{t('student.progress.lastUpdate')}: {latest ? new Date(latest.progressDate).toLocaleDateString(language) : '—'}</p>
          </div>
          <Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />{t('student.progress.register')}</Button>
        </div>
      </section>

      {records.length === 0 ? (
        <EmptyState
          icon={TrendingUp}
          title={t('student.progress.emptyTitle')}
          description={t('student.progress.emptyDescription')}
          action={<Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />{t('student.progress.registerProgress')}</Button>}
        />
      ) : (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            {highlights.map((item) => (
              <SectionCard key={item.label} className="p-4" title={item.label}>
                <p className="text-2xl font-bold text-slate-900">{item.current}{item.unit}</p>
                {item.diff != null ? (
                  <p className={`text-xs mt-1 ${item.diff <= 0 ? 'text-emerald-600' : 'text-amber-600'}`}>
                    {item.diff > 0 ? '+' : ''}{item.diff.toFixed(1)}{item.unit} desde o último registro
                  </p>
                ) : <p className="text-xs mt-1 text-slate-400">Sem comparação anterior</p>}
              </SectionCard>
            ))}
          </div>

          <SectionCard title={t('student.progress.timelineTitle')} description={t('student.progress.timelineDescription')}>
            <div className="space-y-4">
              {records.map((record, idx) => (
                <article key={record.id} className="relative rounded-2xl border border-slate-200 p-4">
                  {idx !== records.length - 1 && <span className="absolute left-5 top-12 bottom-[-18px] w-px bg-slate-200" />}
                  <div className="flex items-start justify-between gap-3 mb-3">
                    <div className="flex items-center gap-3">
                      <span className="h-3 w-3 rounded-full bg-indigo-500 mt-1" />
                      <div>
                        <p className="font-semibold text-slate-900">{new Date(record.progressDate).toLocaleDateString(language)}</p>
                        <p className="text-xs text-slate-500">Registro #{records.length - idx}</p>
                      </div>
                    </div>
                    <button onClick={() => setDeleteTarget(record.id)} className="p-1.5 rounded-lg hover:bg-red-50 text-red-400"><Trash2 size={14} /></button>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-2">
                    {metricLabels.filter(([field]) => record[field] != null).map(([field, label, unit]) => (
                      <div key={field} className="rounded-xl bg-slate-50 px-3 py-2 text-center border border-slate-100">
                        <p className="text-xs text-slate-500">{label}</p>
                        <p className="font-bold text-slate-800 text-sm">{record[field]}{unit}</p>
                      </div>
                    ))}
                  </div>
                  {record.notes && <p className="text-xs text-slate-600 mt-3 bg-amber-50 border border-amber-100 rounded-lg px-3 py-2">{record.notes}</p>}
                </article>
              ))}
            </div>
          </SectionCard>
        </>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={t('student.progress.registerProgress')} size="lg">
        <form onSubmit={handleSave} className="space-y-4">
          <Input label="Data" type="date" {...f('progressDate')} />
          <div className="grid grid-cols-2 gap-3">
            {metricLabels.map(([field, label]) => <Input key={field} label={`${label}${field === 'bodyFatPercentage' ? '' : ' (cm/kg conforme campo)'}`} type="number" step="0.01" {...f(field)} />)}
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">{t('student.progress.notes')}</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={3} value={form.notes} onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))} />
          </div>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">{t('common.cancel')}</Button>
            <Button type="submit" loading={saving} className="flex-1">{t('common.save')}</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} title={t('student.progress.removeRecord')} description={t('student.progress.removeRecordConfirm')} />
    </PageContainer>
  );
}
