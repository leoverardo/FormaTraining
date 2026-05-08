import { Button } from '../ui/Button';
import { ProgressBar } from '../ui/ProgressBar';
import { SectionCard } from '../ui/SectionCard';

export function WorkoutCard({ workout, onOpen }) {
  return (
    <SectionCard title="Treino do dia" description={workout?.statusLabel || 'Confira seu treino'} action={onOpen ? <Button size="sm" onClick={onOpen}>Ver treino</Button> : null}>
      <p className="text-lg font-semibold text-slate-900">{workout?.name || 'Descanso programado'}</p>
      {workout?.highlights?.length ? <p className="text-sm text-slate-500 mt-1">{workout.highlights.join(' • ')}</p> : <p className="text-sm text-slate-500 mt-1">Use o dia para recuperação.</p>}
    </SectionCard>
  );
}

export function ProgressMetricCard({ label, value, hint }) {
  return <SectionCard className="p-4" title={label} description={hint}><p className="text-2xl font-bold text-slate-900">{value}</p></SectionCard>;
}

export function CheckInCard({ status, summary, onOpen }) {
  return <SectionCard title="Check-in semanal" description={summary} action={onOpen ? <Button size="sm" onClick={onOpen}>Preencher</Button> : null}><p className="text-sm text-slate-600">{status}</p></SectionCard>;
}

export function WeeklyScheduleCard({ items = [] }) {
  return (
    <SectionCard title="Rotina da semana">
      <div className="space-y-2">
        {items.map((item) => (
          <div key={item.day} className="flex items-center justify-between rounded-xl border border-slate-200 p-3">
            <p className="text-sm font-medium text-slate-700">{item.day}</p>
            <p className={`text-xs font-semibold ${item.active ? 'text-indigo-600' : 'text-slate-400'}`}>{item.label}</p>
          </div>
        ))}
      </div>
    </SectionCard>
  );
}

export function BodyProgressCard({ title, value, progress }) {
  return (
    <SectionCard title={title}>
      <p className="text-2xl font-bold text-slate-900 mb-2">{value}</p>
      <ProgressBar value={progress} />
    </SectionCard>
  );
}

export function PhotoComparisonCard({ beforeUrl, afterUrl }) {
  return (
    <SectionCard title="Antes e depois" description="Comparação visual da evolução">
      <div className="grid grid-cols-2 gap-2">
        <img src={beforeUrl} alt="Antes" className="h-40 w-full rounded-xl object-cover border border-slate-100" />
        <img src={afterUrl} alt="Depois" className="h-40 w-full rounded-xl object-cover border border-slate-100" />
      </div>
    </SectionCard>
  );
}


