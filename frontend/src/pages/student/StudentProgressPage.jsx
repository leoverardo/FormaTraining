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
import { Textarea } from '../../components/forms/Textarea';
import { TrendingUp, Plus, Trash2, ArrowDownRight, ArrowUpRight, Minus } from 'lucide-react';

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
  ['rightArm', 'Braço direito', 'cm'],
  ['leftArm', 'Braço esquerdo', 'cm'],
  ['rightThigh', 'Coxa direita', 'cm'],
  ['leftThigh', 'Coxa esquerda', 'cm'],
  ['bodyFatPercentage', 'Gordura corporal', '%'],
];

const numberFormat = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 });
const diffFormat = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1, signDisplay: 'exceptZero' });
// Measurements are calendar dates. Do not shift midnight UTC to the previous day.
const formatDate = value => new Date(`${value.slice(0, 10)}T12:00:00`).toLocaleDateString('pt-BR');

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
      ['Gordura corporal', latest.bodyFatPercentage, previous?.bodyFatPercentage, '%'],
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
        Object.entries(form).map(([k, v]) => [k, k === 'progressDate' || k === 'notes' ? (v || null) : (v === '' ? null : parseFloat(v))]),
      );
      await progressService.createOwn(payload);
      toast('Progresso registrado!');
      setModalOpen(false);
      setForm(emptyForm);
      load();
    } catch (err) {
      toast(err.response?.data?.message || 'Erro', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    try {
      await progressService.deleteOwn(deleteTarget);
      toast('Registro removido.');
      setDeleteTarget(null);
      load();
    } catch {
      toast('Erro', 'error');
    }
  };

  const f = (field) => ({ value: form[field], onChange: (e) => setForm((p) => ({ ...p, [field]: e.target.value })) });

  if (loading) return <ProgressSkeleton />;

  return (
    <PageContainer className="space-y-5 pb-20 sm:pb-0">
      <section className="text-[var(--text)]">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="uc-heading">Meu Progresso</h1>
            <p className="text-[var(--text-secondary)] text-sm mt-2">Seu histórico de peso e medidas, registro a registro.</p>
            {latest && <p className="text-[var(--text-secondary)] text-xs mt-2">Última atualização: {formatDate(latest.progressDate)}</p>}
          </div>
          <Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />Registrar</Button>
        </div>
      </section>

      {records.length === 0 ? (
        <EmptyState
          icon={TrendingUp}
          title="Nenhum registro ainda"
          description="Comece a registrar seu peso e medidas para acompanhar sua evolução."
          action={<Button size="sm" onClick={() => setModalOpen(true)}><Plus size={14} />Registrar progresso</Button>}
        />
      ) : (
        <>
          {highlights.length > 0 && (
            <SectionCard title="Últimas medidas" description={previous ? `Variação em relação a ${formatDate(previous.progressDate)}` : 'Seu primeiro registro é o ponto de partida.'}>
              <dl className="grid grid-cols-2 xl:grid-cols-4 gap-x-6 gap-y-6">
                {highlights.map((item) => {
                  const difference = item.diff != null && Math.abs(item.diff) < 0.05 ? 0 : item.diff;
                  const DifferenceIcon = difference > 0 ? ArrowUpRight : difference < 0 ? ArrowDownRight : Minus;
                  return (
                    <div key={item.label} className="min-w-0">
                      <dt className="text-sm text-[var(--text-secondary)]">{item.label}</dt>
                      <dd className="mt-1 text-2xl sm:text-3xl font-semibold tabular-nums text-[var(--text)]">
                        {numberFormat.format(item.current)} <span className="text-sm font-normal text-[var(--text-secondary)]">{item.unit}</span>
                      </dd>
                      <dd className="mt-2 text-xs text-[var(--text-secondary)]">
                        {difference != null ? (
                          <span className="inline-flex items-center gap-1.5"><DifferenceIcon size={15} aria-hidden="true" />
                            <span>{diffFormat.format(difference)} {item.unit === '%' ? 'p.p.' : item.unit}</span>
                          </span>
                        ) : 'Sem comparação anterior'}
                      </dd>
                    </div>
                  );
                })}
              </dl>
            </SectionCard>
          )}

          <section aria-labelledby="progress-history-title" className="pt-3">
            <div className="mb-5">
              <h2 id="progress-history-title" className="text-xl font-semibold text-[var(--text)]">Linha do tempo</h2>
              <p className="text-sm text-[var(--text-secondary)] mt-1">{records.length} {records.length === 1 ? 'registro' : 'registros'} · Do mais recente ao primeiro</p>
            </div>
            <ol aria-label="Registros de evolução" className="uc-progress-timeline space-y-5">
              {records.map((record, idx) => (
                <li key={record.id} className="relative pl-6 sm:pl-8">
                  <article aria-labelledby={`progress-date-${record.id}`} className="rounded-[var(--radius-md)] bg-[var(--surface)] p-4 sm:p-6">
                    <div className="flex items-start justify-between gap-3 mb-3">
                      <div>
                        <h3 id={`progress-date-${record.id}`} className="text-lg font-semibold tabular-nums text-[var(--text)]"><time dateTime={record.progressDate.slice(0, 10)}>{formatDate(record.progressDate)}</time></h3>
                        <p className="text-xs text-[var(--text-secondary)] mt-1">Registro #{records.length - idx}{idx === 0 ? ' · Mais recente' : ''}</p>
                      </div>
                      <button type="button" aria-label={`Remover registro de ${formatDate(record.progressDate)}`} onClick={() => setDeleteTarget(record.id)} className="uc-icon-button shrink-0 hover:bg-[var(--status-danger-bg)] hover:text-[var(--danger)]"><Trash2 size={18} aria-hidden="true" /></button>
                    </div>
                    <dl className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-x-4 gap-y-5 border-t border-[var(--border)] pt-4">
                      {metricLabels.filter(([field]) => record[field] != null).map(([field, label, unit]) => (
                        <div key={field} className="min-w-0">
                          <dt className="text-xs text-[var(--text-secondary)]">{label}</dt>
                          <dd className="mt-1 text-base sm:text-lg font-semibold tabular-nums text-[var(--text)]">{numberFormat.format(record[field])} <span className="text-xs font-normal text-[var(--text-secondary)]">{unit}</span></dd>
                        </div>
                      ))}
                    </dl>
                    {record.notes && <div className="mt-5 rounded-[var(--radius-sm)] bg-[var(--surface-muted)] p-4">
                      <p className="text-xs font-semibold text-[var(--text-secondary)]">Observações</p>
                      <p className="mt-2 text-sm leading-relaxed text-[var(--text)] whitespace-pre-wrap break-words">{record.notes}</p>
                    </div>}
                  </article>
                </li>
              ))}
            </ol>
          </section>
        </>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="Registrar progresso" size="lg">
        <form onSubmit={handleSave} className="space-y-4">
          <Input label="Data" type="date" {...f('progressDate')} />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {metricLabels.map(([field, label, unit]) => <Input key={field} label={`${label} (${unit})`} type="number" min="0" step="0.01" {...f(field)} />)}
          </div>
          <div className="space-y-1">
            <label htmlFor="progress-notes" className="block text-sm font-semibold text-[var(--text)]">Observações</label>
            <Textarea id="progress-notes" rows={3} value={form.notes} onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))} />
          </div>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} title="Remover registro" description="Deseja remover este registro de progresso?" />
    </PageContainer>
  );
}
