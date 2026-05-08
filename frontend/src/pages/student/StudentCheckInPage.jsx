import { useEffect, useState } from 'react';
import { checkInService } from '../../services/checkInService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { Modal } from '../../components/ui/Modal';
import { CalendarCheck, Plus, CheckCircle } from 'lucide-react';

const RatingInput = ({ label, value, onChange }) => (
  <div className="space-y-1">
    <label className="block text-sm font-medium text-gray-700">{label}</label>
    <div className="flex gap-2">
      {[1, 2, 3, 4, 5].map(v => (
        <button key={v} type="button" onClick={() => onChange(v)}
          className={`w-10 h-10 rounded-xl text-sm font-bold transition-all ${value === v ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'}`}>
          {v}
        </button>
      ))}
    </div>
  </div>
);

const empty = { weight: '', moodLevel: 3, energyLevel: 3, sleepQuality: 3, dietAdherence: 3, trainingAdherence: 3, completedWorkoutsCount: '', hasPain: false, painDescription: '', notes: '', photoUrl: '' };

export function StudentCheckInPage() {
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
        completedWorkoutsCount: form.completedWorkoutsCount ? parseInt(form.completedWorkoutsCount) : null,
      };
      if (currentWeek) { await checkInService.update(currentWeek.id, payload); toast('Check-in atualizado!'); }
      else { await checkInService.create(payload); toast('Check-in enviado!'); }
      setModalOpen(false);
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro ao salvar', 'error'); }
    finally { setSaving(false); }
  };

  const openModal = () => {
    if (currentWeek) {
      setForm({ weight: currentWeek.weight || '', moodLevel: currentWeek.moodLevel || 3, energyLevel: currentWeek.energyLevel || 3, sleepQuality: currentWeek.sleepQuality || 3, dietAdherence: currentWeek.dietAdherence || 3, trainingAdherence: currentWeek.trainingAdherence || 3, completedWorkoutsCount: currentWeek.completedWorkoutsCount || '', hasPain: currentWeek.hasPain || false, painDescription: currentWeek.painDescription || '', notes: currentWeek.notes || '', photoUrl: currentWeek.photoUrl || '' });
    } else {
      setForm(empty);
    }
    setModalOpen(true);
  };

  const f = (field) => ({ value: form[field], onChange: e => setForm(p => ({ ...p, [field]: e.target.value })) });
  const r = (field) => ({ value: form[field], onChange: (v) => setForm(p => ({ ...p, [field]: v })) });

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4 pb-20 sm:pb-0">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">Check-in Semanal</h1>
        <Button size="sm" onClick={openModal}>
          {currentWeek ? <CheckCircle size={14} /> : <Plus size={14} />}
          {currentWeek ? 'Atualizar' : 'Fazer check-in'}
        </Button>
      </div>

      {currentWeek ? (
        <div className="bg-gradient-to-r from-emerald-500 to-teal-500 rounded-2xl p-5 text-white">
          <p className="text-emerald-100 text-xs font-medium uppercase tracking-wide mb-2">Esta semana ✓</p>
          <div className="grid grid-cols-3 gap-3">
            {[['Humor', currentWeek.moodLevel], ['Energia', currentWeek.energyLevel], ['Dieta', currentWeek.dietAdherence]].map(([label, value]) =>
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
        <div className="bg-amber-50 border border-amber-200 rounded-2xl p-4 flex items-start gap-3">
          <CalendarCheck size={20} className="text-amber-500 shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold text-amber-800 text-sm">Check-in pendente esta semana</p>
            <p className="text-amber-600 text-xs mt-0.5">Informe como foi sua semana para seu personal acompanhar seu progresso.</p>
          </div>
        </div>
      )}

      {checkIns.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-3">Histórico</h3>
          <div className="space-y-3">
            {checkIns.map(c => (
              <div key={c.id} className="bg-white rounded-2xl border border-gray-200 p-4">
                <div className="flex justify-between items-start mb-3">
                  <p className="font-semibold text-gray-900 text-sm">Semana de {new Date(c.weekStartDate).toLocaleDateString('pt-BR')}</p>
                  {c.weight && <p className="text-sm text-gray-500">{c.weight} kg</p>}
                </div>
                <div className="grid grid-cols-3 gap-2">
                  {[['Humor', c.moodLevel], ['Energia', c.energyLevel], ['Sono', c.sleepQuality], ['Dieta', c.dietAdherence], ['Treino', c.trainingAdherence]].filter(([, v]) => v != null).map(([label, value]) => (
                    <div key={label} className="bg-gray-50 rounded-xl p-2 text-center">
                      <p className="text-xs text-gray-400 leading-none">{label}</p>
                      <p className="font-bold text-gray-800 mt-0.5">{value}/5</p>
                    </div>
                  ))}
                </div>
                {c.notes && <p className="text-xs text-gray-500 mt-2">{c.notes}</p>}
                {c.comments?.length > 0 && (
                  <div className="mt-3 pt-3 border-t border-gray-100 space-y-2">
                    {c.comments.map(cm => (
                      <div key={cm.id} className="bg-indigo-50 rounded-xl px-3 py-2">
                        <p className="text-xs font-medium text-indigo-700">{cm.authorName}</p>
                        <p className="text-xs text-indigo-600 mt-0.5">{cm.comment}</p>
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
        <EmptyState icon={CalendarCheck} title="Nenhum check-in ainda" description="Faça seu primeiro check-in semanal." action={<Button size="sm" onClick={openModal}><Plus size={14} />Fazer check-in</Button>} />
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={currentWeek ? 'Atualizar check-in' : 'Check-in desta semana'} size="lg">
        <form onSubmit={handleSave} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">Peso (kg)</label>
              <input type="number" step="0.1" className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('weight')} />
            </div>
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">Treinos concluídos</label>
              <input type="number" min="0" className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('completedWorkoutsCount')} />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <RatingInput label="Humor geral" {...r('moodLevel')} />
            <RatingInput label="Nível de energia" {...r('energyLevel')} />
            <RatingInput label="Qualidade do sono" {...r('sleepQuality')} />
            <RatingInput label="Aderência à dieta" {...r('dietAdherence')} />
            <RatingInput label="Aderência aos treinos" {...r('trainingAdherence')} />
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="hasPain" checked={form.hasPain} onChange={e => setForm(p => ({ ...p, hasPain: e.target.checked }))} className="rounded" />
            <label htmlFor="hasPain" className="text-sm text-gray-700">Sinto dores ou desconforto</label>
          </div>
          {form.hasPain && (
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">Descreva a dor/desconforto</label>
              <input className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('painDescription')} />
            </div>
          )}
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Observações gerais</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={3} value={form.notes} onChange={e => setForm(p => ({ ...p, notes: e.target.value }))} />
          </div>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Enviar check-in</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}


