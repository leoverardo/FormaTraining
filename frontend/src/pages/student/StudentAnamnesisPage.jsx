import { useEffect, useMemo, useState } from 'react';
import { ClipboardCheck } from 'lucide-react';
import { anamnesisService } from '../../services/anamnesisService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { LoadingState } from '../../components/ui/LoadingState';

const empty = {
  mainGoal: '',
  trainingExperience: '',
  injuries: '',
  healthRestrictions: '',
  availableDaysPerWeek: '',
  trainingLocation: '',
  availableEquipment: '',
  sleepQuality: '',
  stressLevel: '',
  foodRoutineNotes: '',
  additionalNotes: '',
};

const mainGoalOptions = ['Emagrecimento', 'Hipertrofia', 'Ganho de força', 'Condicionamento físico', 'Saúde e qualidade de vida', 'Outro'];
const trainingExperienceOptions = ['Nunca treinei', 'Iniciante', 'Intermediário', 'Avançado'];
const trainingLocationOptions = ['Academia', 'Casa', 'Ao ar livre', 'Academia e casa', 'Outro'];

const sleepLabels = {
  1: 'Muito ruim',
  2: 'Ruim',
  3: 'Regular',
  4: 'Boa',
  5: 'Excelente',
};

const stressLabels = {
  1: 'Muito baixo',
  2: 'Baixo',
  3: 'Moderado',
  4: 'Alto',
  5: 'Muito alto',
};

const normalizeNum = (v) => (v === null || v === undefined || v === '' ? '' : String(v));

export function StudentAnamnesisPage() {
  const { toast } = useToast();
  const [form, setForm] = useState(empty);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [hasExisting, setHasExisting] = useState(false);
  const [customMainGoal, setCustomMainGoal] = useState('');
  const [customTrainingExperience, setCustomTrainingExperience] = useState('');
  const [customTrainingLocation, setCustomTrainingLocation] = useState('');

  const hasInjuries = useMemo(() => !!(form.injuries || '').trim(), [form.injuries]);
  const hasHealthRestrictions = useMemo(() => !!(form.healthRestrictions || '').trim(), [form.healthRestrictions]);

  const mainGoalSelectValue = useMemo(
    () => (mainGoalOptions.includes(form.mainGoal) ? form.mainGoal : form.mainGoal ? 'Outro' : ''),
    [form.mainGoal],
  );
  const experienceSelectValue = useMemo(
    () => (trainingExperienceOptions.includes(form.trainingExperience) ? form.trainingExperience : form.trainingExperience ? 'Outro' : ''),
    [form.trainingExperience],
  );
  const locationSelectValue = useMemo(
    () => (trainingLocationOptions.includes(form.trainingLocation) ? form.trainingLocation : form.trainingLocation ? 'Outro' : ''),
    [form.trainingLocation],
  );

  useEffect(() => {
    anamnesisService
      .getOwn()
      .then((r) => {
        const d = r.data.data;
        if (!d) return;

        const loaded = {
          mainGoal: d.mainGoal || '',
          trainingExperience: d.trainingExperience || '',
          injuries: d.injuries || '',
          healthRestrictions: d.healthRestrictions || '',
          availableDaysPerWeek: normalizeNum(d.availableDaysPerWeek),
          trainingLocation: d.trainingLocation || '',
          availableEquipment: d.availableEquipment || '',
          sleepQuality: normalizeNum(d.sleepQuality),
          stressLevel: normalizeNum(d.stressLevel),
          foodRoutineNotes: d.foodRoutineNotes || '',
          additionalNotes: d.additionalNotes || '',
        };

        setHasExisting(true);
        setForm(loaded);
        if (loaded.mainGoal && !mainGoalOptions.includes(loaded.mainGoal)) setCustomMainGoal(loaded.mainGoal);
        if (loaded.trainingExperience && !trainingExperienceOptions.includes(loaded.trainingExperience)) setCustomTrainingExperience(loaded.trainingExperience);
        if (loaded.trainingLocation && !trainingLocationOptions.includes(loaded.trainingLocation)) setCustomTrainingLocation(loaded.trainingLocation);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        ...form,
        availableDaysPerWeek: form.availableDaysPerWeek ? parseInt(form.availableDaysPerWeek, 10) : null,
        sleepQuality: form.sleepQuality ? parseInt(form.sleepQuality, 10) : null,
        stressLevel: form.stressLevel ? parseInt(form.stressLevel, 10) : null,
      };
      await anamnesisService.save(payload);
      toast('Informações salvas com sucesso!');
      setHasExisting(true);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar', 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-5 pb-20 sm:pb-0 max-w-4xl">
      <div className="flex items-start gap-3">
        <ClipboardCheck size={22} className="text-indigo-600 mt-0.5" />
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Meu Perfil de Treino</h1>
          <p className="text-slate-500 text-sm mt-1">
            Essas informações ajudam seu personal a montar treinos mais adequados para você. Atualize sempre que algo mudar.
          </p>
          {hasExisting && <p className="text-xs text-slate-400 mt-1">Dados salvos anteriormente carregados com sucesso.</p>}
        </div>
      </div>

      <form onSubmit={handleSave} className="space-y-4">
        <section className="bg-white rounded-2xl border border-slate-200 p-5 space-y-4">
          <h2 className="font-semibold text-slate-900">Objetivos do treino</h2>

          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700">Objetivo principal</label>
            <select
              className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
              value={mainGoalSelectValue}
              onChange={(e) => {
                const v = e.target.value;
                if (!v) return setForm((p) => ({ ...p, mainGoal: '' }));
                if (v === 'Outro') return setForm((p) => ({ ...p, mainGoal: customMainGoal || '' }));
                setForm((p) => ({ ...p, mainGoal: v }));
              }}
            >
              <option value="">Selecione seu principal objetivo</option>
              {mainGoalOptions.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
            </select>
          </div>
          {mainGoalSelectValue === 'Outro' && (
            <Input
              label="Qual é seu objetivo?"
              placeholder="Descreva seu objetivo"
              value={customMainGoal}
              onChange={(e) => {
                const v = e.target.value;
                setCustomMainGoal(v);
                setForm((p) => ({ ...p, mainGoal: v }));
              }}
            />
          )}

          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700">Experiência com treino</label>
            <select
              className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
              value={experienceSelectValue}
              onChange={(e) => {
                const v = e.target.value;
                if (!v) return setForm((p) => ({ ...p, trainingExperience: '' }));
                if (v === 'Outro') return setForm((p) => ({ ...p, trainingExperience: customTrainingExperience || '' }));
                setForm((p) => ({ ...p, trainingExperience: v }));
              }}
            >
              <option value="">Selecione sua experiência</option>
              {trainingExperienceOptions.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
              {experienceSelectValue === 'Outro' && <option value="Outro">Outro</option>}
            </select>
          </div>
          {experienceSelectValue === 'Outro' && (
            <Input
              label="Descreva sua experiência"
              placeholder="Conte um pouco do seu histórico"
              value={customTrainingExperience}
              onChange={(e) => {
                const v = e.target.value;
                setCustomTrainingExperience(v);
                setForm((p) => ({ ...p, trainingExperience: v }));
              }}
            />
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className="block text-sm font-semibold text-slate-700">Dias disponíveis por semana</label>
              <select
                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
                value={form.availableDaysPerWeek}
                onChange={(e) => setForm((p) => ({ ...p, availableDaysPerWeek: e.target.value }))}
              >
                <option value="">Selecione</option>
                {[1, 2, 3, 4, 5, 6, 7].map((n) => <option key={n} value={String(n)}>{n}</option>)}
              </select>
            </div>

            <div className="space-y-1.5">
              <label className="block text-sm font-semibold text-slate-700">Local de treino</label>
              <select
                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 bg-white focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
                value={locationSelectValue}
                onChange={(e) => {
                  const v = e.target.value;
                  if (!v) return setForm((p) => ({ ...p, trainingLocation: '' }));
                  if (v === 'Outro') return setForm((p) => ({ ...p, trainingLocation: customTrainingLocation || '' }));
                  setForm((p) => ({ ...p, trainingLocation: v }));
                }}
              >
                <option value="">Selecione</option>
                {trainingLocationOptions.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
              </select>
            </div>
          </div>
          {locationSelectValue === 'Outro' && (
            <Input
              label="Qual local?"
              placeholder="Descreva seu local de treino"
              value={customTrainingLocation}
              onChange={(e) => {
                const v = e.target.value;
                setCustomTrainingLocation(v);
                setForm((p) => ({ ...p, trainingLocation: v }));
              }}
            />
          )}

          <Input
            label="Equipamentos disponíveis"
            placeholder="Ex.: academia completa, halteres, elásticos, banco, barra..."
            hint="Se não tiver equipamentos, pode escrever “nenhum”."
            value={form.availableEquipment}
            onChange={(e) => setForm((p) => ({ ...p, availableEquipment: e.target.value }))}
          />
        </section>

        <section className="bg-white rounded-2xl border border-slate-200 p-5 space-y-4">
          <h2 className="font-semibold text-slate-900">Saúde e cuidados</h2>

          <div className="space-y-2">
            <label className="block text-sm font-semibold text-slate-700">Possui alguma lesão anterior ou atual?</label>
            <div className="flex gap-2">
              <button type="button" onClick={() => setForm((p) => ({ ...p, injuries: '' }))} className={`px-3 py-2 rounded-xl text-sm border ${!hasInjuries ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-slate-300 text-slate-700'}`}>Não</button>
              <button type="button" onClick={() => setForm((p) => ({ ...p, injuries: p.injuries || '' }))} className={`px-3 py-2 rounded-xl text-sm border ${hasInjuries ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-slate-300 text-slate-700'}`}>Sim</button>
            </div>
            {hasInjuries && (
              <textarea
                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
                rows={3}
                placeholder="Conte quais lesões já teve ou ainda possui e se algo incomoda durante o treino."
                value={form.injuries}
                onChange={(e) => setForm((p) => ({ ...p, injuries: e.target.value }))}
              />
            )}
          </div>

          <div className="space-y-2">
            <label className="block text-sm font-semibold text-slate-700">Possui alguma restrição de saúde relevante para o treino?</label>
            <div className="flex gap-2">
              <button type="button" onClick={() => setForm((p) => ({ ...p, healthRestrictions: '' }))} className={`px-3 py-2 rounded-xl text-sm border ${!hasHealthRestrictions ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-slate-300 text-slate-700'}`}>Não</button>
              <button type="button" onClick={() => setForm((p) => ({ ...p, healthRestrictions: p.healthRestrictions || '' }))} className={`px-3 py-2 rounded-xl text-sm border ${hasHealthRestrictions ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-slate-300 text-slate-700'}`}>Sim</button>
            </div>
            {hasHealthRestrictions && (
              <textarea
                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
                rows={3}
                placeholder="Ex.: pressão alta, limitação médica, cirurgia recente ou outra informação importante."
                value={form.healthRestrictions}
                onChange={(e) => setForm((p) => ({ ...p, healthRestrictions: e.target.value }))}
              />
            )}
          </div>
        </section>

        <section className="bg-white rounded-2xl border border-slate-200 p-5 space-y-4">
          <h2 className="font-semibold text-slate-900">Rotina e bem-estar</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <ScaleInput
              label="Qualidade do sono"
              value={form.sleepQuality}
              labels={sleepLabels}
              onChange={(v) => setForm((p) => ({ ...p, sleepQuality: String(v) }))}
            />
            <ScaleInput
              label="Nível de estresse"
              value={form.stressLevel}
              labels={stressLabels}
              onChange={(v) => setForm((p) => ({ ...p, stressLevel: String(v) }))}
            />
          </div>

          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700">Rotina alimentar</label>
            <textarea
              className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
              rows={3}
              placeholder="Conte brevemente como costuma ser sua alimentação no dia a dia."
              value={form.foodRoutineNotes}
              onChange={(e) => setForm((p) => ({ ...p, foodRoutineNotes: e.target.value }))}
            />
          </div>

          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700">Observações adicionais</label>
            <textarea
              className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl text-sm text-slate-800 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-300"
              rows={3}
              placeholder="Compartilhe qualquer informação que possa ajudar seu personal."
              value={form.additionalNotes}
              onChange={(e) => setForm((p) => ({ ...p, additionalNotes: e.target.value }))}
            />
          </div>
        </section>

        <Button type="submit" loading={saving} className="w-full md:w-auto">
          Salvar informações
        </Button>
      </form>
    </div>
  );
}

function ScaleInput({ label, value, labels, onChange }) {
  const selected = Number(value) || 0;
  return (
    <div className="space-y-2">
      <label className="block text-sm font-semibold text-slate-700">{label}</label>
      <div className="grid grid-cols-5 gap-2">
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            key={n}
            type="button"
            onClick={() => onChange(n)}
            className={`px-2 py-2 rounded-xl text-sm border transition ${
              selected === n ? 'bg-indigo-600 text-white border-indigo-600' : 'bg-white text-slate-700 border-slate-300 hover:border-slate-400'
            }`}
          >
            {n}
          </button>
        ))}
      </div>
      <p className="text-xs text-slate-500 min-h-4">{selected ? `${selected} - ${labels[selected]}` : 'Selecione de 1 a 5'}</p>
    </div>
  );
}
