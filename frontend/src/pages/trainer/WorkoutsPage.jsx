import { useEffect, useState } from 'react';
import { workoutService } from '../../services/workoutService';
import { exerciseService } from '../../services/exerciseService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Select } from '../../components/ui/Select';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { ClipboardList, Plus, Pencil, Trash2, ChevronDown, ChevronUp, GripVertical } from 'lucide-react';

const emptyWorkout = { name: '', goal: '', level: 1, description: '', status: 1 };

export function WorkoutsPage() {
  const { toast } = useToast();
  const [workouts, setWorkouts] = useState([]);
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editWorkout, setEditWorkout] = useState(null);
  const [form, setForm] = useState(emptyWorkout);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [expandedId, setExpandedId] = useState(null);
  const [addExModal, setAddExModal] = useState(null);
  const [exForm, setExForm] = useState({ exerciseId: '', sets: 3, reps: '12', suggestedLoad: '', restSeconds: 60, notes: '', orderIndex: 1 });

  const load = () => {
    setLoading(true);
    Promise.all([workoutService.getAll(), exerciseService.getAll()])
      .then(([w, e]) => { setWorkouts(w.data.data || []); setExercises(e.data.data || []); })
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleSaveWorkout = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editWorkout) { await workoutService.update(editWorkout.id, form); toast('Treino atualizado!'); }
      else { await workoutService.create(form); toast('Treino criado!'); }
      setModalOpen(false); load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSaving(false); }
  };

  const handleAddExercise = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await workoutService.addExercise(addExModal.id, { ...exForm, exerciseId: exForm.exerciseId, sets: +exForm.sets, restSeconds: +exForm.restSeconds, orderIndex: +exForm.orderIndex });
      toast('Exercício adicionado!');
      setAddExModal(null);
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSaving(false); }
  };

  const removeExercise = async (workoutId, weId) => {
    try { await workoutService.removeExercise(workoutId, weId); toast('Exercício removido.'); load(); }
    catch { toast('Erro ao remover', 'error'); }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Treinos</h1>
          <p className="text-gray-500 text-sm mt-1">{workouts.length} treinos criados</p>
        </div>
        <Button onClick={() => { setEditWorkout(null); setForm(emptyWorkout); setModalOpen(true); }}>
          <Plus size={16} />Novo treino
        </Button>
      </div>

      {workouts.length === 0 ? (
        <EmptyState icon={ClipboardList} title="Nenhum treino criado" description="Crie seu primeiro treino e adicione exercícios." action={<Button onClick={() => { setEditWorkout(null); setForm(emptyWorkout); setModalOpen(true); }}><Plus size={16} />Novo treino</Button>} />
      ) : (
        <div className="space-y-3">
          {workouts.map(w => (
            <div key={w.id} className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
              <div className="px-6 py-4 flex items-center justify-between">
                <div className="flex items-center gap-3 min-w-0">
                  <button onClick={() => setExpandedId(expandedId === w.id ? null : w.id)} className="text-gray-400 hover:text-gray-600">
                    {expandedId === w.id ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
                  </button>
                  <div className="min-w-0">
                    <p className="font-semibold text-gray-900 text-sm">{w.name}</p>
                    <p className="text-gray-400 text-xs">{w.exercises?.length || 0} exercícios · {w.level}</p>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <Badge variant={w.status === 'Active' ? 'success' : 'gray'}>{w.status === 'Active' ? 'Ativo' : 'Inativo'}</Badge>
                  <button onClick={() => { setAddExModal(w); setExForm({ exerciseId: exercises[0]?.id || '', sets: 3, reps: '12', suggestedLoad: '', restSeconds: 60, notes: '', orderIndex: (w.exercises?.length || 0) + 1 }); }} className="p-1.5 rounded-lg hover:bg-green-50 text-green-600 transition-colors" title="Adicionar exercício"><Plus size={16} /></button>
                  <button onClick={() => { setEditWorkout(w); const lm = { Beginner: 1, Intermediate: 2, Advanced: 3 }; setForm({ name: w.name, goal: w.goal || '', level: lm[w.level] || 1, description: w.description || '', status: w.status === 'Active' ? 1 : 2 }); setModalOpen(true); }} className="p-1.5 rounded-lg hover:bg-blue-50 text-blue-500 transition-colors"><Pencil size={16} /></button>
                  <button onClick={() => setDeleteTarget(w)} className="p-1.5 rounded-lg hover:bg-red-50 text-red-500 transition-colors"><Trash2 size={16} /></button>
                </div>
              </div>
              {expandedId === w.id && (
                <div className="border-t border-gray-100 px-6 py-4 bg-gray-50">
                  {w.exercises?.length === 0 ? (
                    <p className="text-sm text-gray-400 text-center py-4">Nenhum exercício adicionado.</p>
                  ) : (
                    <div className="space-y-2">
                      {w.exercises?.sort((a, b) => a.orderIndex - b.orderIndex).map(we => (
                        <div key={we.id} className="flex items-center gap-3 bg-white rounded-xl px-4 py-3 border border-gray-200">
                          <GripVertical size={16} className="text-gray-300" />
                          <div className="flex-1 min-w-0">
                            <p className="font-medium text-sm text-gray-900">{we.exercise?.name}</p>
                            <p className="text-xs text-gray-400">{we.sets}x{we.reps} · {we.suggestedLoad || 'Carga livre'} · Descanso: {we.restSeconds}s</p>
                          </div>
                          <button onClick={() => removeExercise(w.id, we.id)} className="p-1 rounded-lg hover:bg-red-50 text-red-400 transition-colors"><Trash2 size={14} /></button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editWorkout ? 'Editar treino' : 'Novo treino'}>
        <form onSubmit={handleSaveWorkout} className="space-y-4">
          <Input label="Nome do treino" value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} required />
          <Input label="Objetivo" value={form.goal} onChange={e => setForm(p => ({ ...p, goal: e.target.value }))} />
          <div className="grid grid-cols-2 gap-4">
            <Select label="Nível" value={form.level} onChange={e => setForm(p => ({ ...p, level: parseInt(e.target.value) }))}>
              <option value={1}>Iniciante</option><option value={2}>Intermediário</option><option value={3}>Avançado</option>
            </Select>
            <Select label="Status" value={form.status} onChange={e => setForm(p => ({ ...p, status: parseInt(e.target.value) }))}>
              <option value={1}>Ativo</option><option value={2}>Inativo</option>
            </Select>
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Descrição</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} />
          </div>
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <Modal open={!!addExModal} onClose={() => setAddExModal(null)} title={`Adicionar exercício ? ${addExModal?.name}`}>
        <form onSubmit={handleAddExercise} className="space-y-4">
          <Select label="Exercício" value={exForm.exerciseId} onChange={e => setExForm(p => ({ ...p, exerciseId: e.target.value }))} required>
            <option value="">Selecione...</option>
            {exercises.map(ex => <option key={ex.id} value={ex.id}>{ex.name}</option>)}
          </Select>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Séries" type="number" value={exForm.sets} onChange={e => setExForm(p => ({ ...p, sets: e.target.value }))} min={1} />
            <Input label="Repetições" value={exForm.reps} onChange={e => setExForm(p => ({ ...p, reps: e.target.value }))} placeholder="12 ou 8-12" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Carga sugerida" value={exForm.suggestedLoad} onChange={e => setExForm(p => ({ ...p, suggestedLoad: e.target.value }))} placeholder="ex: 20kg" />
            <Input label="Descanso (s)" type="number" value={exForm.restSeconds} onChange={e => setExForm(p => ({ ...p, restSeconds: e.target.value }))} />
          </div>
          <Input label="Ordem" type="number" value={exForm.orderIndex} onChange={e => setExForm(p => ({ ...p, orderIndex: e.target.value }))} min={1} />
          <Input label="Observação" value={exForm.notes} onChange={e => setExForm(p => ({ ...p, notes: e.target.value }))} />
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setAddExModal(null)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Adicionar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={async () => { try { await workoutService.delete(deleteTarget.id); toast('Treino removido.'); setDeleteTarget(null); load(); } catch { toast('Erro', 'error'); } }} title="Excluir treino" description={`Deseja excluir "${deleteTarget?.name}"?`} />
    </div>
  );
}

