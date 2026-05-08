import { useEffect, useState } from 'react';
import { anamnesisService } from '../../services/anamnesisService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { LoadingState } from '../../components/ui/LoadingState';
import { ClipboardCheck } from 'lucide-react';

const empty = { mainGoal: '', trainingExperience: '', injuries: '', healthRestrictions: '', availableDaysPerWeek: '', trainingLocation: '', availableEquipment: '', sleepQuality: '', stressLevel: '', foodRoutineNotes: '', additionalNotes: '' };

export function StudentAnamnesisPage() {
  const { toast } = useToast();
  const [form, setForm] = useState(empty);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [hasExisting, setHasExisting] = useState(false);

  useEffect(() => {
    anamnesisService.getOwn()
      .then(r => {
        const d = r.data.data;
        if (d) {
          setHasExisting(true);
          setForm({ mainGoal: d.mainGoal || '', trainingExperience: d.trainingExperience || '', injuries: d.injuries || '', healthRestrictions: d.healthRestrictions || '', availableDaysPerWeek: d.availableDaysPerWeek || '', trainingLocation: d.trainingLocation || '', availableEquipment: d.availableEquipment || '', sleepQuality: d.sleepQuality || '', stressLevel: d.stressLevel || '', foodRoutineNotes: d.foodRoutineNotes || '', additionalNotes: d.additionalNotes || '' });
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = { ...form, availableDaysPerWeek: form.availableDaysPerWeek ? parseInt(form.availableDaysPerWeek) : null, sleepQuality: form.sleepQuality ? parseInt(form.sleepQuality) : null, stressLevel: form.stressLevel ? parseInt(form.stressLevel) : null };
      await anamnesisService.save(payload);
      toast('Anamnese salva com sucesso!');
      setHasExisting(true);
    } catch (err) { toast(err.response?.data?.message || 'Erro ao salvar', 'error'); }
    finally { setSaving(false); }
  };

  const f = (field) => ({ value: form[field], onChange: e => setForm(p => ({ ...p, [field]: e.target.value })) });

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4 pb-20 sm:pb-0 max-w-lg">
      <div className="flex items-center gap-3">
        <ClipboardCheck size={22} className="text-indigo-600" />
        <div>
          <h1 className="text-xl font-bold text-gray-900">Anamnese</h1>
          <p className="text-gray-400 text-xs mt-0.5">{hasExisting ? 'Atualiza quando precisar' : 'Preencha para seu personal te conhecer melhor'}</p>
        </div>
      </div>

      <form onSubmit={handleSave} className="space-y-4">
        <div className="bg-white rounded-2xl border border-gray-200 p-5 space-y-4">
          <h3 className="font-semibold text-gray-900 text-sm">Objetivos e experiência</h3>
          <Input label="Objetivo principal" placeholder="Ex: Perda de peso, hipertrofia..." {...f('mainGoal')} />
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Experiência com treino</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} placeholder="Há quanto tempo treina? Qual modalidade?" value={form.trainingExperience} onChange={e => setForm(p => ({ ...p, trainingExperience: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Dias disponíveis/semana" type="number" min="1" max="7" {...f('availableDaysPerWeek')} />
            <Input label="Local de treino" placeholder="Academia, casa..." {...f('trainingLocation')} />
          </div>
          <Input label="Equipamentos disponíveis" placeholder="Halteres, barra, elásticos..." {...f('availableEquipment')} />
        </div>

        <div className="bg-white rounded-2xl border border-gray-200 p-5 space-y-4">
          <h3 className="font-semibold text-gray-900 text-sm">Saúde e restrições</h3>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Lesões anteriores ou atuais</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} value={form.injuries} onChange={e => setForm(p => ({ ...p, injuries: e.target.value }))} />
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Restrições de saúde</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} placeholder="Hipertensão, diabetes, problemas cardíacos..." value={form.healthRestrictions} onChange={e => setForm(p => ({ ...p, healthRestrictions: e.target.value }))} />
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-gray-200 p-5 space-y-4">
          <h3 className="font-semibold text-gray-900 text-sm">Hábitos de vida</h3>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">Qualidade do sono (1-5)</label>
              <input type="number" min="1" max="5" className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('sleepQuality')} />
            </div>
            <div className="space-y-1">
              <label className="block text-sm font-medium text-gray-700">Nível de estresse (1-5)</label>
              <input type="number" min="1" max="5" className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" {...f('stressLevel')} />
            </div>
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Rotina alimentar</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} value={form.foodRoutineNotes} onChange={e => setForm(p => ({ ...p, foodRoutineNotes: e.target.value }))} />
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Observações adicionais</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} value={form.additionalNotes} onChange={e => setForm(p => ({ ...p, additionalNotes: e.target.value }))} />
          </div>
        </div>

        <Button type="submit" loading={saving} className="w-full">Salvar anamnese</Button>
      </form>
    </div>
  );
}


