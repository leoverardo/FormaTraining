import { useEffect, useState } from 'react';
import { studentService } from '../../services/studentService';
import { scheduleService } from '../../services/scheduleService';
import { workoutService } from '../../services/workoutService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { Input } from '../../components/ui/Input';
import { CalendarDays, Plus, Trash2 } from 'lucide-react';
import { useI18n } from '../../i18n';

const days = ['Domingo', 'Segunda-feira', 'Terça-feira', 'Quarta-feira', 'Quinta-feira', 'Sexta-feira', 'Sábado'];

export function SchedulePage() {
  const { toast } = useToast();
  const { t } = useI18n();
  const [students, setStudents] = useState([]);
  const [workouts, setWorkouts] = useState([]);
  const [selectedStudent, setSelectedStudent] = useState('');
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingSchedule, setLoadingSchedule] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState({ workoutId: '', dayOfWeek: 1, notes: '' });
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    Promise.all([studentService.getAll(), workoutService.getAll()])
      .then(([s, w]) => { setStudents(s.data.data || []); setWorkouts(w.data.data || []); })
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!selectedStudent) { setSchedules([]); return; }
    setLoadingSchedule(true);
    scheduleService.getByStudent(selectedStudent).then(r => setSchedules(r.data.data || [])).finally(() => setLoadingSchedule(false));
  }, [selectedStudent]);

  const handleAdd = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await scheduleService.create(selectedStudent, { workoutId: form.workoutId, dayOfWeek: parseInt(form.dayOfWeek), notes: form.notes });
      toast('Treino agendado!');
      setModalOpen(false);
      scheduleService.getByStudent(selectedStudent).then(r => setSchedules(r.data.data || []));
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSaving(false); }
  };

  const handleDelete = async () => {
    try {
      await scheduleService.delete(deleteTarget.id);
      toast('Agendamento removido.');
      setDeleteTarget(null);
      scheduleService.getByStudent(selectedStudent).then(r => setSchedules(r.data.data || []));
    } catch { toast('Erro ao remover', 'error'); }
  };

  if (loading) return <LoadingState />;

  const scheduleByDay = days.map((day, idx) => ({
    day, idx, items: schedules.filter(s => s.dayOfWeek === idx)
  }));

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">Rotina Semanal</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Monte a rotina semanal por aluno</p>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-6 dark:border-white/10 dark:bg-slate-900">
        <div className="flex items-end gap-4 flex-wrap">
          <div className="flex-1 min-w-48">
            <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-200">Selecionar aluno</label>
            <select
              className="w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:border-white/10 dark:bg-slate-950 dark:text-white"
              value={selectedStudent}
              onChange={e => setSelectedStudent(e.target.value)}
            >
              <option value="">? Selecione um aluno ?</option>
              {students.filter(s => s.status === 'Active').map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          {selectedStudent && (
            <Button onClick={() => { setForm({ workoutId: workouts[0]?.id || '', dayOfWeek: 1, notes: '' }); setModalOpen(true); }}>
              <Plus size={16} />Adicionar treino
            </Button>
          )}
        </div>
      </div>

      {!selectedStudent ? (
        <EmptyState icon={CalendarDays} title="Selecione um aluno" description="Escolha um aluno acima para ver ou montar a rotina semanal." />
      ) : loadingSchedule ? <LoadingState /> : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {scheduleByDay.map(({ day, idx, items }) => (
            <div key={idx} className="overflow-hidden rounded-2xl border border-slate-200 bg-white dark:border-white/10 dark:bg-slate-900">
              <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 dark:border-white/10 dark:bg-slate-950">
                <p className="text-sm font-semibold text-slate-700 dark:text-slate-200">{day}</p>
              </div>
              <div className="px-4 py-3 min-h-[80px]">
                {items.length === 0 ? (
                  <p className="py-4 text-center text-xs text-slate-300 dark:text-slate-500">Descanso</p>
                ) : items.map(s => (
                  <div key={s.id} className="flex items-center justify-between gap-2 bg-indigo-50 rounded-lg px-3 py-2 mb-2">
                    <p className="text-xs font-medium text-indigo-700 leading-snug">{s.workoutName}</p>
                    <button onClick={() => setDeleteTarget(s)} className="text-indigo-300 hover:text-red-500 transition-colors"><Trash2 size={14} /></button>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={t('trainer.schedule.addWorkoutToRoutine')}>
        <form onSubmit={handleAdd} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-200">Dia da semana</label>
            <select className="w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:border-white/10 dark:bg-slate-950 dark:text-white" value={form.dayOfWeek} onChange={e => setForm(p => ({ ...p, dayOfWeek: parseInt(e.target.value) }))}>
              {days.map((d, i) => <option key={i} value={i}>{d}</option>)}
            </select>
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-200">{t('nav.workouts')}</label>
            <select className="w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:border-white/10 dark:bg-slate-950 dark:text-white" value={form.workoutId} onChange={e => setForm(p => ({ ...p, workoutId: e.target.value }))} required>
              <option value="">{t('trainer.schedule.select')}...</option>
              {workouts.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
            </select>
          </div>
          <Input label="Observações (opcional)" value={form.notes} onChange={e => setForm(p => ({ ...p, notes: e.target.value }))} />
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} title="Remover treino" description="Deseja remover este treino da rotina?" />
    </div>
  );
}




