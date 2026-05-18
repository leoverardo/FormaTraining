import { useEffect, useMemo, useState } from 'react';
import { ClipboardCheck, ShieldAlert, Sparkles, CheckCircle2 } from 'lucide-react';
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

const mainGoalOptions = ['Emagrecimento', 'Hipertrofia', 'Ganho de forca', 'Condicionamento fisico', 'Saude e qualidade de vida', 'Outro'];
const trainingExperienceOptions = ['Nunca treinei', 'Iniciante', 'Intermediario', 'Avancado'];
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
      toast('Informacoes salvas com sucesso!');
      setHasExisting(true);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar', 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="mx-auto w-full max-w-7xl space-y-6 pb-20 sm:pb-0">
      <header className="rounded-3xl border border-slate-200 bg-gradient-to-br from-white via-slate-50 to-indigo-50 p-6 shadow-sm sm:p-7">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-start gap-4">
            <div className="mt-0.5 rounded-2xl bg-indigo-600 p-2.5 text-white shadow-lg shadow-indigo-200">
              <ClipboardCheck size={22} />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Meu Perfil de Treino</h1>
              <p className="mt-2 max-w-2xl text-sm leading-relaxed text-slate-600 sm:text-base">
                Essas informacoes ajudam seu personal a montar treinos mais seguros, personalizados e alinhados aos seus objetivos.
              </p>
              {hasExisting ? (
                <p className="mt-3 inline-flex items-center gap-1.5 rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-700">
                  <CheckCircle2 size={14} /> Informacoes carregadas com sucesso
                </p>
              ) : (
                <p className="mt-3 inline-flex items-center gap-1.5 rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-medium text-amber-700">
                  <Sparkles size={14} /> Perfil pronto para ser atualizado
                </p>
              )}
            </div>
          </div>
        </div>
      </header>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <form onSubmit={handleSave} className="space-y-5 xl:col-span-2">
          <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
            <div className="mb-5">
              <h2 className="text-lg font-semibold text-slate-900">Objetivos do treino</h2>
              <p className="mt-1 text-sm text-slate-500">Conte um pouco sobre seus objetivos e sua rotina para que os treinos facam mais sentido para voce.</p>
            </div>

            <div className="space-y-5">
              <div className="rounded-2xl border border-slate-200 bg-slate-50/60 p-4 sm:p-5">
                <h3 className="text-sm font-semibold text-slate-900">Objetivo e experiencia</h3>
                <div className="mt-4 space-y-4">
                  <SelectField
                    label="Objetivo principal"
                    value={mainGoalSelectValue}
                    onChange={(e) => {
                      const v = e.target.value;
                      if (!v) return setForm((p) => ({ ...p, mainGoal: '' }));
                      if (v === 'Outro') return setForm((p) => ({ ...p, mainGoal: customMainGoal || '' }));
                      setForm((p) => ({ ...p, mainGoal: v }));
                    }}
                    options={mainGoalOptions}
                    placeholder="Selecione seu principal objetivo"
                  />
                  {mainGoalSelectValue === 'Outro' && (
                    <Input
                      label="Qual e seu objetivo?"
                      placeholder="Descreva seu objetivo"
                      value={customMainGoal}
                      onChange={(e) => {
                        const v = e.target.value;
                        setCustomMainGoal(v);
                        setForm((p) => ({ ...p, mainGoal: v }));
                      }}
                    />
                  )}

                  <SelectField
                    label="Experiencia com treino"
                    value={experienceSelectValue}
                    onChange={(e) => {
                      const v = e.target.value;
                      if (!v) return setForm((p) => ({ ...p, trainingExperience: '' }));
                      if (v === 'Outro') return setForm((p) => ({ ...p, trainingExperience: customTrainingExperience || '' }));
                      setForm((p) => ({ ...p, trainingExperience: v }));
                    }}
                    options={[...trainingExperienceOptions, ...(experienceSelectValue === 'Outro' ? ['Outro'] : [])]}
                    placeholder="Selecione sua experiencia"
                  />
                  {experienceSelectValue === 'Outro' && (
                    <Input
                      label="Descreva sua experiencia"
                      placeholder="Conte um pouco do seu historico"
                      value={customTrainingExperience}
                      onChange={(e) => {
                        const v = e.target.value;
                        setCustomTrainingExperience(v);
                        setForm((p) => ({ ...p, trainingExperience: v }));
                      }}
                    />
                  )}
                </div>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-slate-50/60 p-4 sm:p-5">
                <h3 className="text-sm font-semibold text-slate-900">Disponibilidade e estrutura</h3>
                <div className="mt-4 space-y-4">
                  <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                    <SelectField
                      label="Dias disponiveis por semana"
                      value={form.availableDaysPerWeek}
                      onChange={(e) => setForm((p) => ({ ...p, availableDaysPerWeek: e.target.value }))}
                      options={['1', '2', '3', '4', '5', '6', '7']}
                      placeholder="Selecione"
                    />

                    <SelectField
                      label="Local de treino"
                      value={locationSelectValue}
                      onChange={(e) => {
                        const v = e.target.value;
                        if (!v) return setForm((p) => ({ ...p, trainingLocation: '' }));
                        if (v === 'Outro') return setForm((p) => ({ ...p, trainingLocation: customTrainingLocation || '' }));
                        setForm((p) => ({ ...p, trainingLocation: v }));
                      }}
                      options={trainingLocationOptions}
                      placeholder="Selecione"
                    />
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
                    label="Equipamentos disponiveis"
                    placeholder="Ex.: academia completa, halteres, elasticos, banco, barra..."
                    hint="Se nao tiver equipamentos, pode escrever 'nenhum'."
                    value={form.availableEquipment}
                    onChange={(e) => setForm((p) => ({ ...p, availableEquipment: e.target.value }))}
                  />
                </div>
              </div>
            </div>
          </section>

          <section className="rounded-3xl border border-amber-200 bg-gradient-to-br from-amber-50/80 via-white to-white p-5 shadow-sm sm:p-6">
            <div className="mb-5 flex items-start gap-3">
              <div className="rounded-xl bg-amber-100 p-2 text-amber-700">
                <ShieldAlert size={18} />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-slate-900">Saude e cuidados</h2>
                <p className="mt-1 text-sm text-slate-600">Essas informacoes ajudam seu personal a respeitar seus limites e adaptar os exercicios quando necessario.</p>
              </div>
            </div>

            <div className="space-y-4">
              <HealthQuestionCard
                title="Possui alguma lesao anterior ou atual?"
                active={hasInjuries}
                onNo={() => setForm((p) => ({ ...p, injuries: '' }))}
                onYes={() => setForm((p) => ({ ...p, injuries: p.injuries || '' }))}
                placeholder="Conte quais lesoes ja teve ou ainda possui e se algo incomoda durante o treino."
                value={form.injuries}
                onChange={(v) => setForm((p) => ({ ...p, injuries: v }))}
              />

              <HealthQuestionCard
                title="Possui alguma restricao de saude relevante para o treino?"
                active={hasHealthRestrictions}
                onNo={() => setForm((p) => ({ ...p, healthRestrictions: '' }))}
                onYes={() => setForm((p) => ({ ...p, healthRestrictions: p.healthRestrictions || '' }))}
                placeholder="Ex.: pressao alta, limitacao medica, cirurgia recente ou outra informacao importante."
                value={form.healthRestrictions}
                onChange={(v) => setForm((p) => ({ ...p, healthRestrictions: v }))}
              />
            </div>
          </section>

          <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
            <h2 className="text-lg font-semibold text-slate-900">Rotina e bem-estar</h2>

            <div className="mt-5 grid grid-cols-1 gap-4 md:grid-cols-2">
              <ScaleInput
                label="Qualidade do sono"
                value={form.sleepQuality}
                labels={sleepLabels}
                onChange={(v) => setForm((p) => ({ ...p, sleepQuality: String(v) }))}
              />
              <ScaleInput
                label="Nivel de estresse"
                value={form.stressLevel}
                labels={stressLabels}
                onChange={(v) => setForm((p) => ({ ...p, stressLevel: String(v) }))}
              />
            </div>

            <div className="mt-4 space-y-4">
              <TextAreaField
                label="Rotina alimentar"
                placeholder="Conte brevemente como costuma ser sua alimentacao no dia a dia."
                value={form.foodRoutineNotes}
                onChange={(e) => setForm((p) => ({ ...p, foodRoutineNotes: e.target.value }))}
              />

              <TextAreaField
                label="Observacoes adicionais"
                placeholder="Compartilhe qualquer informacao que possa ajudar seu personal."
                value={form.additionalNotes}
                onChange={(e) => setForm((p) => ({ ...p, additionalNotes: e.target.value }))}
              />
            </div>
          </section>

          <section className="rounded-3xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <p className="text-sm text-slate-500">Revise os dados e salve para manter seu perfil sempre atualizado.</p>
              <Button type="submit" loading={saving} className="w-full sm:w-auto">
                Salvar alteracoes
              </Button>
            </div>
          </section>
        </form>

        <aside className="xl:col-span-1">
          <div className="space-y-4 xl:sticky xl:top-24">
            <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm">
              <h3 className="text-sm font-semibold text-slate-900">Como essas informacoes ajudam seu personal</h3>
              <ul className="mt-3 space-y-2 text-sm text-slate-600">
                <li className="rounded-xl bg-slate-50 px-3 py-2">Treino mais alinhado ao seu objetivo.</li>
                <li className="rounded-xl bg-slate-50 px-3 py-2">Adaptacoes seguras para restricoes e lesoes.</li>
                <li className="rounded-xl bg-slate-50 px-3 py-2">Melhor escolha de intensidade e progressao.</li>
              </ul>
            </section>

            <section className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm">
              <h3 className="text-sm font-semibold text-slate-900">Status do perfil</h3>
              <p className="mt-2 text-sm text-slate-600">
                {hasExisting ? 'Perfil com informacoes carregadas e pronto para ajustes.' : 'Preencha seus dados para receber treinos mais personalizados.'}
              </p>
            </section>
          </div>
        </aside>
      </div>
    </div>
  );
}

function SelectField({ label, value, onChange, options, placeholder }) {
  return (
    <div className="space-y-1.5">
      <label className="block text-sm font-semibold text-slate-700">{label}</label>
      <select
        className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3.5 text-sm text-slate-800 shadow-sm transition focus:border-indigo-400 focus:outline-none focus:ring-4 focus:ring-indigo-100"
        value={value}
        onChange={onChange}
      >
        <option value="">{placeholder}</option>
        {options.map((opt) => (
          <option key={opt} value={opt}>{opt}</option>
        ))}
      </select>
    </div>
  );
}

function TextAreaField({ label, placeholder, value, onChange }) {
  return (
    <div className="space-y-1.5">
      <label className="block text-sm font-semibold text-slate-700">{label}</label>
      <textarea
        className="w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-slate-800 placeholder:text-slate-400 shadow-sm transition focus:border-indigo-400 focus:outline-none focus:ring-4 focus:ring-indigo-100"
        rows={3}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
      />
    </div>
  );
}

function HealthQuestionCard({ title, active, onNo, onYes, placeholder, value, onChange }) {
  return (
    <div className="rounded-2xl border border-amber-200/70 bg-white p-4">
      <p className="text-sm font-semibold text-slate-800">{title}</p>
      <div className="mt-3 inline-flex rounded-xl border border-slate-200 bg-slate-50 p-1">
        <button
          type="button"
          onClick={onNo}
          className={`min-w-20 rounded-lg px-3 py-2 text-sm font-medium transition ${!active ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-600 hover:text-slate-800'}`}
        >
          Nao
        </button>
        <button
          type="button"
          onClick={onYes}
          className={`min-w-20 rounded-lg px-3 py-2 text-sm font-medium transition ${active ? 'bg-indigo-600 text-white shadow-sm' : 'text-slate-600 hover:text-slate-800'}`}
        >
          Sim
        </button>
      </div>
      {active && (
        <textarea
          className="mt-3 w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-slate-800 placeholder:text-slate-400 shadow-sm transition focus:border-indigo-400 focus:outline-none focus:ring-4 focus:ring-indigo-100"
          rows={3}
          placeholder={placeholder}
          value={value}
          onChange={(e) => onChange(e.target.value)}
        />
      )}
    </div>
  );
}

function ScaleInput({ label, value, labels, onChange }) {
  const selected = Number(value) || 0;
  return (
    <div className="rounded-2xl border border-slate-200 bg-slate-50/60 p-4">
      <label className="block text-sm font-semibold text-slate-700">{label}</label>
      <div className="mt-3 grid grid-cols-5 gap-2">
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            key={n}
            type="button"
            onClick={() => onChange(n)}
            className={`rounded-xl border px-2 py-2 text-sm font-medium transition ${
              selected === n
                ? 'border-indigo-600 bg-indigo-600 text-white'
                : 'border-slate-300 bg-white text-slate-700 hover:border-slate-400'
            }`}
          >
            {n}
          </button>
        ))}
      </div>
      <p className="mt-2 min-h-4 text-xs text-slate-500">{selected ? `${selected} - ${labels[selected]}` : 'Selecione de 1 a 5'}</p>
    </div>
  );
}
