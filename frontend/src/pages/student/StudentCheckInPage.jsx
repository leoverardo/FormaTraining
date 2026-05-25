import { useEffect, useState } from 'react';
import { checkInService } from '../../services/checkInService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { Modal } from '../../components/ui/Modal';
import { CalendarCheck, Plus, CheckCircle } from 'lucide-react';
import { useI18n } from '../../i18n';

const RatingInput = ({ label, value, onChange }) => (
  <div className="space-y-1">
    <label className="block text-sm font-medium text-gray-700 dark:text-slate-200">{label}</label>
    <div className="flex gap-2">
      {[1, 2, 3, 4, 5].map((v) => (
        <button
          key={v}
          type="button"
          onClick={() => onChange(v)}
          className={`w-10 h-10 rounded-xl text-sm font-bold transition-all ${value === v ? 'bg-indigo-600 text-white' : 'bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-slate-300 hover:bg-gray-200 dark:hover:bg-white/15'}`}
        >
          {v}
        </button>
      ))}
    </div>
  </div>
);

const empty = { weight: '', moodLevel: 3, energyLevel: 3, sleepQuality: 3, dietAdherence: 3, trainingAdherence: 3, completedWorkoutsCount: '', hasPain: false, painDescription: '', notes: '', photoUrl: '' };

export function StudentCheckInPage() {
  const { t, language } = useI18n();
  const { toast } = useToast();
  const [checkIns, setCheckIns] = useState([]);
  const [currentWeek, setCurrentWeek] = useState(null);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);

  const load = async () => {
    setLoading(true);
    const [all, week] = await Promise.allSettled([checkInService.getOwn(), checkInService.getCurrentWeek()]);
    setCheckIns(all.status === 'fulfilled' ? all.value.data.data || [] : []);
    setCurrentWeek(week.status === 'fulfilled' ? week.value.data.data : null);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        ...form,
        weight: form.weight ? parseFloat(form.weight) : null,
        completedWorkoutsCount: form.completedWorkoutsCount ? parseInt(form.completedWorkoutsCount, 10) : null,
      };
      if (currentWeek) {
        await checkInService.update(currentWeek.id, payload);
        toast(t('student.checkIn.updated'));
      } else {
        await checkInService.create(payload);
        toast(t('student.checkIn.created'));
      }
      setModalOpen(false);
      load();
    } catch (err) {
      toast(err.response?.data?.message || t('messages.saveError'), 'error');
    } finally { setSaving(false); }
  };

  const openModal = () => {
    if (currentWeek) {
      setForm({ weight: currentWeek.weight || '', moodLevel: currentWeek.moodLevel || 3, energyLevel: currentWeek.energyLevel || 3, sleepQuality: currentWeek.sleepQuality || 3, dietAdherence: currentWeek.dietAdherence || 3, trainingAdherence: currentWeek.trainingAdherence || 3, completedWorkoutsCount: currentWeek.completedWorkoutsCount || '', hasPain: currentWeek.hasPain || false, painDescription: currentWeek.painDescription || '', notes: currentWeek.notes || '', photoUrl: currentWeek.photoUrl || '' });
    } else {
      setForm(empty);
    }
    setModalOpen(true);
  };

  const f = (field) => ({ value: form[field], onChange: (e) => setForm((p) => ({ ...p, [field]: e.target.value })) });
  const r = (field) => ({ value: form[field], onChange: (v) => setForm((p) => ({ ...p, [field]: v })) });

  if (loading) return <LoadingState text={t('student.checkIn.loading')} />;

  return (
    <div className="space-y-4 pb-20 sm:pb-0">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">{t('student.checkIn.title')}</h1>
        <Button size="sm" onClick={openModal}>
          {currentWeek ? <CheckCircle size={14} /> : <Plus size={14} />}
          {currentWeek ? t('common.edit') : t('student.checkIn.new')}
        </Button>
      </div>

      {currentWeek ? (
        <div className="bg-gradient-to-r from-emerald-500 to-teal-500 rounded-2xl p-5 text-white">
          <p className="text-emerald-100 text-xs font-medium uppercase tracking-wide mb-2">{t('student.checkIn.thisWeek')}</p>
          <div className="grid grid-cols-3 gap-3">
            {[[t('student.checkIn.mood'), currentWeek.moodLevel], [t('student.checkIn.energy'), currentWeek.energyLevel], [t('student.checkIn.diet'), currentWeek.dietAdherence]].map(([label, value]) =>
              value != null && (
                <div key={label} className="bg-white/10 rounded-xl p-2 text-center">
                  <p className="text-emerald-100 text-xs">{label}</p>
                  <p className="font-bold text-xl">{value}/5</p>
                </div>
              )
            )}
          </div>
          {currentWeek.notes && <p className="text-emerald-100 text-xs mt-3 opacity-90">{currentWeek.notes}</p>}
        </div>
      ) : (
        <div className="bg-amber-50 dark:bg-amber-500/10 border border-amber-200 dark:border-amber-400/20 rounded-2xl p-4 flex items-start gap-3">
          <CalendarCheck size={20} className="text-amber-500 shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold text-amber-800 dark:text-amber-300 text-sm">{t('student.checkIn.pendingWeek')}</p>
            <p className="text-amber-600 dark:text-amber-200 text-xs mt-0.5">{t('student.checkIn.pendingWeekDescription')}</p>
          </div>
        </div>
      )}

      {checkIns.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 dark:text-slate-200 mb-3">{t('student.checkIn.history')}</h3>
          <div className="space-y-3">
            {checkIns.map((c) => (
              <div key={c.id} className="bg-white dark:bg-slate-900 rounded-2xl border border-gray-200 dark:border-white/10 p-4">
                <div className="flex justify-between items-start mb-3">
                  <p className="font-semibold text-gray-900 dark:text-white text-sm">{t('student.checkIn.weekOf')} {new Date(c.weekStartDate).toLocaleDateString(language === 'pt-BR' ? 'pt-BR' : 'en-US')}</p>
                  {c.weight && <p className="text-sm text-gray-500 dark:text-slate-400">{c.weight} kg</p>}
                </div>
                <div className="grid grid-cols-3 gap-2">
                  {[[t('student.checkIn.mood'), c.moodLevel], [t('student.checkIn.energy'), c.energyLevel], [t('student.checkIn.sleep'), c.sleepQuality], [t('student.checkIn.diet'), c.dietAdherence], [t('student.checkIn.training'), c.trainingAdherence]].filter(([, v]) => v != null).map(([label, value]) => (
                    <div key={label} className="bg-gray-50 dark:bg-white/5 rounded-xl p-2 text-center">
                      <p className="text-xs text-gray-400 dark:text-slate-500 leading-none">{label}</p>
                      <p className="font-bold text-gray-800 dark:text-slate-100 mt-0.5">{value}/5</p>
                    </div>
                  ))}
                </div>
                {c.notes && <p className="text-xs text-gray-500 dark:text-slate-400 mt-2">{c.notes}</p>}
                {c.comments?.length > 0 && (
                  <div className="mt-3 pt-3 border-t border-gray-100 dark:border-white/10 space-y-2">
                    {c.comments.map((cm) => (
                      <div key={cm.id} className="bg-indigo-50 dark:bg-indigo-500/10 rounded-xl px-3 py-2">
                        <p className="text-xs font-medium text-indigo-700 dark:text-indigo-300">{cm.authorName}</p>
                        <p className="text-xs text-indigo-600 dark:text-indigo-200 mt-0.5">{cm.comment}</p>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {checkIns.length === 0 && !currentWeek && (
        <EmptyState icon={CalendarCheck} title={t('student.checkIn.emptyTitle')} description={t('student.checkIn.emptyDescription')} action={<Button size="sm" onClick={openModal}><Plus size={14} />{t('student.checkIn.new')}</Button>} />
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={currentWeek ? t('student.checkIn.updateTitle') : t('student.checkIn.newTitle')} size="lg">
        <form onSubmit={handleSave} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-200">{t('student.checkIn.weight')}</label>
              <input type="number" step="0.1" className="w-full px-3 py-2 border border-gray-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('weight')} />
            </div>
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-200">{t('student.checkIn.completedWorkouts')}</label>
              <input type="number" min="0" className="w-full px-3 py-2 border border-gray-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('completedWorkoutsCount')} />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <RatingInput label={t('student.checkIn.generalMood')} {...r('moodLevel')} />
            <RatingInput label={t('student.checkIn.energyLevel')} {...r('energyLevel')} />
            <RatingInput label={t('student.checkIn.sleepQuality')} {...r('sleepQuality')} />
            <RatingInput label={t('student.checkIn.dietAdherence')} {...r('dietAdherence')} />
            <RatingInput label={t('student.checkIn.trainingAdherence')} {...r('trainingAdherence')} />
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="hasPain" checked={form.hasPain} onChange={(e) => setForm((p) => ({ ...p, hasPain: e.target.checked }))} className="rounded" />
            <label htmlFor="hasPain" className="text-sm text-gray-700 dark:text-slate-200">{t('student.checkIn.hasPain')}</label>
          </div>
          {form.hasPain && (
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-200">{t('student.checkIn.painDescription')}</label>
              <input className="w-full px-3 py-2 border border-gray-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('painDescription')} />
            </div>
          )}
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700 dark:text-slate-200">{t('student.checkIn.notes')}</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={3} value={form.notes} onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))} />
          </div>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">{t('common.cancel')}</Button>
            <Button type="submit" loading={saving} className="flex-1">{t('student.checkIn.save')}</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

