import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentService } from '../../services/studentService';
import { scheduleService } from '../../services/scheduleService';
import { progressService } from '../../services/progressService';
import { habitService } from '../../services/habitService';
import { gamificationService } from '../../services/gamificationService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { ChevronLeft, Plus, Trash2, TrendingUp, Camera } from 'lucide-react';

const TABS = ['Dados', 'Rotina', 'Progresso', 'Fotos', 'Habitos'];
const days = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];

const emptyProgress = { weight: '', height: '', chest: '', waist: '', abdomen: '', hip: '', rightArm: '', leftArm: '', rightThigh: '', leftThigh: '', bodyFatPercentage: '', notes: '', progressDate: new Date().toISOString().split('T')[0] };

export function StudentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [activeTab, setActiveTab] = useState('Dados');
  const [student, setStudent] = useState(null);
  const [schedules, setSchedules] = useState([]);
  const [progress, setProgress] = useState([]);
  const [photos, setPhotos] = useState([]);
  const [loading, setLoading] = useState(true);

  const [progressModal, setProgressModal] = useState(false);
  const [progressForm, setProgressForm] = useState(emptyProgress);
  const [photoModal, setPhotoModal] = useState(false);
  const [photoForm, setPhotoForm] = useState({ imageUrl: '', description: '', photoDate: new Date().toISOString().split('T')[0] });
  const [saving, setSaving] = useState(false);
  const [compareA, setCompareA] = useState(null);
  const [compareB, setCompareB] = useState(null);
  const [habits, setHabits] = useState([]);
  const [adherenceDays, setAdherenceDays] = useState(7);
  const [adherence, setAdherence] = useState(null);
  const [guidance, setGuidance] = useState({ guidanceText: '', strategicNotes: '' });
  const [habitForm, setHabitForm] = useState({ title: '', description: '', category: 'Custom', targetValue: '', targetUnit: '' });
  const [editingHabitId, setEditingHabitId] = useState(null);
  const [habitModal, setHabitModal] = useState(false);
  const [gamification, setGamification] = useState(null);
  const [goalForm, setGoalForm] = useState({ workoutTarget: 8, habitDaysTarget: 20, checkInTarget: 4 });

  useEffect(() => {
    Promise.all([
      studentService.getById(id),
      scheduleService.getByStudent(id),
      progressService.getByStudent(id),
      progressService.getPhotosByStudent(id),
    ]).then(([s, sc, pr, ph]) => {
      setStudent(s.data.data);
      setSchedules(sc.data.data || []);
      setProgress(pr.data.data || []);
      setPhotos(ph.data.data || []);
      habitService.getTrainerHabits(id).then((r) => setHabits(r.data.data || []));
      habitService.getAdherence(id, adherenceDays).then((r) => setAdherence(r.data.data || null));
      habitService.getTrainerGuidance(id).then((r) => {
        const g = r.data.data;
        if (g) setGuidance({ guidanceText: g.guidanceText || '', strategicNotes: g.strategicNotes || '' });
      });
      gamificationService.getTrainerSummary(id).then((r) => setGamification(r.data.data || null));
      gamificationService.getTrainerMonthlyGoals(id).then((r) => {
        const g = r.data.data;
        if (!g) return;
        setGoalForm({
          workoutTarget: g.workoutTarget ?? 8,
          habitDaysTarget: g.habitDaysTarget ?? 20,
          checkInTarget: g.checkInTarget ?? 4,
        });
      });
    }).finally(() => setLoading(false));
  }, [id, adherenceDays]);

  const reloadHabits = () => {
    habitService.getTrainerHabits(id).then((r) => setHabits(r.data.data || []));
    habitService.getAdherence(id, adherenceDays).then((r) => setAdherence(r.data.data || null));
  };

  const handleAddProgress = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = Object.fromEntries(Object.entries(progressForm).map(([k, v]) => [k, v === '' ? null : k === 'progressDate' ? v : parseFloat(v) || null]));
      payload.progressDate = progressForm.progressDate || null;
      payload.notes = progressForm.notes || null;
      await progressService.createForStudent(id, payload);
      toast('Progresso registrado!');
      setProgressModal(false);
      setProgressForm(emptyProgress);
      progressService.getByStudent(id).then(r => setProgress(r.data.data || []));
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSaving(false); }
  };

  const handleAddPhoto = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await progressService.addPhotoForStudent(id, photoForm);
      toast('Foto adicionada!');
      setPhotoModal(false);
      setPhotoForm({ imageUrl: '', description: '', photoDate: new Date().toISOString().split('T')[0] });
      progressService.getPhotosByStudent(id).then(r => setPhotos(r.data.data || []));
    } catch { toast('Erro ao adicionar foto', 'error'); }
    finally { setSaving(false); }
  };

  const deleteProgress = async (pid) => {
    try { await progressService.deleteForStudent(id, pid); toast('Removido.'); setProgress(prev => prev.filter(p => p.id !== pid)); }
    catch { toast('Erro', 'error'); }
  };

  const deletePhoto = async (pid) => {
    try { await progressService.deletePhotoForStudent(id, pid); toast('Foto removida.'); setPhotos(prev => prev.filter(p => p.id !== pid)); }
    catch { toast('Erro', 'error'); }
  };

  const openCreateHabit = () => {
    setEditingHabitId(null);
    setHabitForm({ title: '', description: '', category: 'Custom', targetValue: '', targetUnit: '' });
    setHabitModal(true);
  };

  const openEditHabit = (habit) => {
    setEditingHabitId(habit.id);
    setHabitForm({
      title: habit.title || '',
      description: habit.description || '',
      category: habit.category || 'Custom',
      targetValue: habit.targetValue ?? '',
      targetUnit: habit.targetUnit || '',
    });
    setHabitModal(true);
  };

  const saveHabit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        title: habitForm.title,
        description: habitForm.description || null,
        category: habitForm.category || 'Custom',
        targetValue: habitForm.targetValue === '' ? null : Number(habitForm.targetValue),
        targetUnit: habitForm.targetUnit || null,
      };
      if (editingHabitId) await habitService.updateHabit(id, editingHabitId, payload);
      else await habitService.createHabit(id, payload);
      toast('Habito salvo com sucesso!');
      setHabitModal(false);
      reloadHabits();
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar habito', 'error');
    } finally {
      setSaving(false);
    }
  };

  const toggleHabitStatus = async (habit) => {
    try {
      await habitService.updateHabitStatus(id, habit.id, { isActive: !habit.isActive });
      reloadHabits();
    } catch {
      toast('Erro ao atualizar status do habito', 'error');
    }
  };

  const removeHabit = async (habitId) => {
    try {
      await habitService.deleteHabit(id, habitId);
      toast('Habito arquivado.');
      reloadHabits();
    } catch {
      toast('Erro ao remover habito', 'error');
    }
  };

  const saveGuidance = async () => {
    setSaving(true);
    try {
      await habitService.upsertTrainerGuidance(id, guidance);
      toast('Orientacao alimentar salva!');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar orientacao', 'error');
    } finally {
      setSaving(false);
    }
  };

  const saveGoals = async () => {
    setSaving(true);
    try {
      const now = new Date();
      await gamificationService.updateTrainerMonthlyGoals(id, now.getFullYear(), now.getMonth() + 1, goalForm);
      const summary = await gamificationService.getTrainerSummary(id);
      setGamification(summary.data.data || null);
      toast('Metas mensais atualizadas!');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar metas', 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingState />;
  if (!student) return null;

  const pf = (field) => ({ value: progressForm[field], onChange: e => setProgressForm(p => ({ ...p, [field]: e.target.value })) });

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/trainer/students')} className="p-2 rounded-lg hover:bg-gray-100 text-gray-500"><ChevronLeft size={20} /></button>
        <div>
          <h1 className="text-xl font-bold text-gray-900">{student.name}</h1>
          <Badge variant={student.status === 'Active' ? 'success' : 'gray'}>{student.status === 'Active' ? 'Ativo' : 'Inativo'}</Badge>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 bg-gray-100 rounded-xl p-1">
        {TABS.map(tab => (
          <button key={tab} onClick={() => setActiveTab(tab)}
            className={`flex-1 py-2 text-sm font-medium rounded-lg transition-all ${activeTab === tab ? 'bg-white text-indigo-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
            {tab}
          </button>
        ))}
      </div>

      {/* Tab: Dados */}
      {activeTab === 'Dados' && (
        <div className="bg-white rounded-2xl border border-gray-200 p-6 space-y-3">
          <Row label="E-mail" value={student.email} />
          <Row label="Telefone" value={student.phone} />
          <Row label="Objetivo" value={student.goal} />
          <Row label="Data de nasc." value={student.birthDate ? new Date(student.birthDate).toLocaleDateString('pt-BR') : null} />
          <Row label="Observações" value={student.notes} />
          <Row label="Cadastrado em" value={new Date(student.createdAt).toLocaleDateString('pt-BR')} />
          <div className="pt-4 border-t border-gray-100 flex gap-2">
            <Button size="sm" variant="secondary" onClick={() => studentService.resendAccessEmail(id).then(() => toast('E-mail reenviado!')).catch(() => toast('Erro', 'error'))}>
              Reenviar acesso
            </Button>
          </div>
        </div>
      )}

      {/* Tab: Rotina */}
      {activeTab === 'Rotina' && (
        <div className="space-y-2">
          {days.map((day, idx) => {
            const items = schedules.filter(s => s.dayOfWeek === idx);
            return (
              <div key={idx} className="bg-white rounded-xl border border-gray-200 px-4 py-3 flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700 w-28">{day}</span>
                <div className="flex flex-wrap gap-2 flex-1">
                  {items.length === 0 ? <span className="text-xs text-gray-300">Descanso</span> : items.map(s => (
                    <span key={s.id} className="bg-indigo-50 text-indigo-700 text-xs font-medium px-2.5 py-1 rounded-lg">{s.workoutName}</span>
                  ))}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Tab: Progresso */}
      {activeTab === 'Progresso' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <p className="text-sm text-gray-500">{progress.length} registros</p>
            <Button size="sm" onClick={() => setProgressModal(true)}><Plus size={14} />Registrar</Button>
          </div>

          {progress.length === 0 ? (
            <EmptyState icon={TrendingUp} title="Nenhum registro de progresso" description="Registre o progresso físico deste aluno." action={<Button size="sm" onClick={() => setProgressModal(true)}><Plus size={14} />Registrar</Button>} />
          ) : (
            <div className="space-y-3">
              {progress.map(p => (
                <div key={p.id} className="bg-white rounded-2xl border border-gray-200 p-4">
                  <div className="flex items-start justify-between mb-3">
                    <div>
                      <p className="font-semibold text-gray-900 text-sm">{new Date(p.progressDate).toLocaleDateString('pt-BR')}</p>
                      <p className="text-xs text-gray-400">Por: {p.createdByRole}</p>
                    </div>
                    <button onClick={() => deleteProgress(p.id)} className="p-1 rounded-lg hover:bg-red-50 text-red-400 transition-colors"><Trash2 size={14} /></button>
                  </div>
                  <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
                    {[['Peso', p.weight, 'kg'], ['Altura', p.height, 'cm'], ['Peito', p.chest, 'cm'], ['Cintura', p.waist, 'cm'], ['Abdômen', p.abdomen, 'cm'], ['Quadril', p.hip, 'cm'], ['Braço D', p.rightArm, 'cm'], ['Braço E', p.leftArm, 'cm'], ['Coxa D', p.rightThigh, 'cm'], ['% Gordura', p.bodyFatPercentage, '%']].filter(([, v]) => v != null).map(([label, value, unit]) => (
                      <div key={label} className="bg-gray-50 rounded-xl px-3 py-2 text-center">
                        <p className="text-xs text-gray-400">{label}</p>
                        <p className="font-bold text-gray-800 text-sm">{value}{unit}</p>
                      </div>
                    ))}
                  </div>
                  {p.notes && <p className="text-xs text-gray-500 mt-2 bg-amber-50 rounded-lg px-3 py-1.5">{p.notes}</p>}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Tab: Fotos */}
      {activeTab === 'Fotos' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <p className="text-sm text-gray-400">{photos.length} fotos · Conteúdo privado</p>
            <Button size="sm" onClick={() => setPhotoModal(true)}><Plus size={14} />Adicionar</Button>
          </div>

          {photos.length >= 2 && (
            <div className="bg-white rounded-2xl border border-gray-200 p-4">
              <p className="text-sm font-semibold text-gray-900 mb-3">Comparação</p>
              <div className="grid grid-cols-2 gap-4">
                {[['Foto inicial', compareA, setCompareA], ['Foto atual', compareB, setCompareB]].map(([label, val, setter]) => (
                  <div key={label}>
                    <p className="text-xs text-gray-400 mb-1">{label}</p>
                    <select className="w-full text-xs border border-gray-200 rounded-lg px-2 py-1.5 mb-2 bg-white" value={val?.id || ''} onChange={e => setter(photos.find(p => p.id === e.target.value) || null)}>
                      <option value="">Selecione...</option>
                      {photos.map(p => <option key={p.id} value={p.id}>{new Date(p.photoDate).toLocaleDateString('pt-BR')}</option>)}
                    </select>
                    {val && <img src={val.imageUrl} className="w-full h-36 object-cover rounded-xl" alt="compare" />}
                  </div>
                ))}
              </div>
            </div>
          )}

          {photos.length === 0 ? (
            <EmptyState icon={Camera} title="Nenhuma foto" description="Adicione fotos de progresso do aluno." action={<Button size="sm" onClick={() => setPhotoModal(true)}><Plus size={14} />Adicionar</Button>} />
          ) : (
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
              {photos.map(p => (
                <div key={p.id} className="relative group rounded-2xl overflow-hidden border border-gray-200">
                  <img src={p.imageUrl} alt={p.description} className="w-full h-36 object-cover" />
                  <div className="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-all flex items-end">
                    <div className="p-2 w-full opacity-0 group-hover:opacity-100 transition-all flex justify-between items-end">
                      <span className="text-white text-xs">{new Date(p.photoDate).toLocaleDateString('pt-BR')}</span>
                      <button onClick={() => deletePhoto(p.id)} className="p-1 bg-red-500 rounded-lg text-white"><Trash2 size={12} /></button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'Habitos' && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <p className="text-sm text-gray-500">{habits.length} habitos cadastrados</p>
            <Button size="sm" onClick={openCreateHabit}><Plus size={14} />Novo habito</Button>
          </div>

          <div className="grid sm:grid-cols-2 gap-3">
            <div className="bg-white rounded-2xl border border-gray-200 p-4">
              <p className="text-xs text-gray-400">Aderencia ({adherenceDays} dias)</p>
              <p className="text-2xl font-bold text-slate-900 mt-1">{adherence?.completionRate ?? 0}%</p>
              <p className="text-xs text-gray-500 mt-1">{adherence?.totalCompleted ?? 0} de {adherence?.totalExpected ?? 0} marcacoes concluidas</p>
              <div className="mt-3 flex gap-2">
                <button onClick={() => setAdherenceDays(7)} className={`px-2 py-1 text-xs rounded-lg ${adherenceDays === 7 ? 'bg-indigo-100 text-indigo-700' : 'bg-gray-100 text-gray-600'}`}>7 dias</button>
                <button onClick={() => setAdherenceDays(30)} className={`px-2 py-1 text-xs rounded-lg ${adherenceDays === 30 ? 'bg-indigo-100 text-indigo-700' : 'bg-gray-100 text-gray-600'}`}>30 dias</button>
              </div>
              {adherence?.lowestHabitTitle && (
                <p className="text-xs text-amber-700 bg-amber-50 rounded-lg px-2 py-1 mt-3">
                  Menor taxa: {adherence.lowestHabitTitle} ({adherence.lowestHabitRate}%)
                </p>
              )}
            </div>
            <div className="bg-white rounded-2xl border border-gray-200 p-4 space-y-2">
              <p className="text-sm font-semibold text-gray-900">Orientacao alimentar</p>
              <textarea
                value={guidance.guidanceText}
                onChange={(e) => setGuidance((p) => ({ ...p, guidanceText: e.target.value }))}
                rows={4}
                className="w-full rounded-xl border border-gray-300 px-3 py-2 text-sm"
                placeholder="Ex.: priorizar alimentos in natura, manter hidratacao..."
              />
              <textarea
                value={guidance.strategicNotes}
                onChange={(e) => setGuidance((p) => ({ ...p, strategicNotes: e.target.value }))}
                rows={2}
                className="w-full rounded-xl border border-gray-300 px-3 py-2 text-sm"
                placeholder="Observacoes estrategicas opcionais"
              />
              <Button size="sm" onClick={saveGuidance} loading={saving}>Salvar orientacao</Button>
            </div>
          </div>

          <div className="grid sm:grid-cols-2 gap-3">
            <div className="bg-white rounded-2xl border border-gray-200 p-4">
              <p className="text-sm font-semibold text-gray-900">Resumo de gamificacao</p>
              {!gamification ? (
                <p className="text-xs text-gray-500 mt-2">Sem dados suficientes.</p>
              ) : (
                <div className="grid grid-cols-3 gap-2 mt-3">
                  {[
                    { label: 'Treino', value: gamification.trainingStreak?.current ?? 0 },
                    { label: 'Habitos', value: gamification.habitStreak?.current ?? 0 },
                    { label: 'Check-ins', value: gamification.checkInStreak?.current ?? 0 },
                  ].map((item) => (
                    <div key={item.label} className="rounded-lg bg-gray-50 border border-gray-200 px-2 py-2 text-center">
                      <p className="text-[11px] text-gray-500">{item.label}</p>
                      <p className="text-base font-bold text-gray-900">{item.value}</p>
                    </div>
                  ))}
                </div>
              )}
            </div>
            <div className="bg-white rounded-2xl border border-gray-200 p-4 space-y-2">
              <p className="text-sm font-semibold text-gray-900">Metas do mes</p>
              <div className="grid grid-cols-3 gap-2">
                <Input label="Treinos" type="number" min="1" value={goalForm.workoutTarget} onChange={(e) => setGoalForm((p) => ({ ...p, workoutTarget: Number(e.target.value || 1) }))} />
                <Input label="Habitos" type="number" min="1" value={goalForm.habitDaysTarget} onChange={(e) => setGoalForm((p) => ({ ...p, habitDaysTarget: Number(e.target.value || 1) }))} />
                <Input label="Check-ins" type="number" min="1" value={goalForm.checkInTarget} onChange={(e) => setGoalForm((p) => ({ ...p, checkInTarget: Number(e.target.value || 1) }))} />
              </div>
              <Button size="sm" onClick={saveGoals} loading={saving}>Salvar metas</Button>
            </div>
          </div>

          {habits.length === 0 ? (
            <EmptyState title="Nenhum habito configurado" description="Crie habitos diarios para acompanhar disciplina e rotina do aluno." />
          ) : (
            <div className="space-y-2">
              {habits.map((habit) => (
                <div key={habit.id} className="bg-white rounded-xl border border-gray-200 p-4 flex items-center justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold text-gray-900">{habit.title}</p>
                    <p className="text-xs text-gray-500">{habit.category} {habit.targetValue ? `• ${habit.targetValue} ${habit.targetUnit || ''}` : ''}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={habit.isActive ? 'success' : 'gray'}>{habit.isActive ? 'Ativo' : 'Inativo'}</Badge>
                    <Button size="sm" variant="secondary" onClick={() => openEditHabit(habit)}>Editar</Button>
                    <Button size="sm" variant="secondary" onClick={() => toggleHabitStatus(habit)}>{habit.isActive ? 'Inativar' : 'Ativar'}</Button>
                    <button title="Arquivar habito" onClick={() => removeHabit(habit.id)} className="p-2 rounded-lg hover:bg-red-50 text-red-500"><Trash2 size={14} /></button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Progress Modal */}
      <Modal open={progressModal} onClose={() => setProgressModal(false)} title="Registrar progresso" size="lg">
        <form onSubmit={handleAddProgress} className="space-y-4">
          <Input label="Data" type="date" {...pf('progressDate')} />
          <div className="grid grid-cols-2 gap-3">
            {[['weight', 'Peso (kg)'], ['height', 'Altura (cm)'], ['chest', 'Peito (cm)'], ['waist', 'Cintura (cm)'], ['abdomen', 'Abdômen (cm)'], ['hip', 'Quadril (cm)'], ['rightArm', 'Braço Direito (cm)'], ['leftArm', 'Braço Esquerdo (cm)'], ['rightThigh', 'Coxa Direita (cm)'], ['leftThigh', 'Coxa Esquerda (cm)'], ['bodyFatPercentage', '% Gordura']].map(([field, label]) => (
              <Input key={field} label={label} type="number" step="0.01" {...pf(field)} />
            ))}
          </div>
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Observações</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={2} {...pf('notes')} />
          </div>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setProgressModal(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      {/* Photo Modal */}
      <Modal open={photoModal} onClose={() => setPhotoModal(false)} title="Adicionar foto de progresso">
        <form onSubmit={handleAddPhoto} className="space-y-4">
          <Input label="URL da imagem" value={photoForm.imageUrl} onChange={e => setPhotoForm(p => ({ ...p, imageUrl: e.target.value }))} placeholder="https://..." required />
          {photoForm.imageUrl && <img src={photoForm.imageUrl} className="w-full h-40 object-cover rounded-xl" alt="preview" />}
          <Input label="Data da foto" type="date" value={photoForm.photoDate} onChange={e => setPhotoForm(p => ({ ...p, photoDate: e.target.value }))} />
          <Input label="Descrição (opcional)" value={photoForm.description} onChange={e => setPhotoForm(p => ({ ...p, description: e.target.value }))} />
          <p className="text-xs text-gray-400 bg-gray-50 rounded-xl p-3">Fotos são privadas e visíveis apenas para o aluno e seu personal trainer.</p>
          <div className="flex gap-3">
            <Button variant="secondary" type="button" onClick={() => setPhotoModal(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Adicionar</Button>
          </div>
        </form>
      </Modal>

      <Modal open={habitModal} onClose={() => setHabitModal(false)} title={editingHabitId ? 'Editar habito' : 'Novo habito'}>
        <form onSubmit={saveHabit} className="space-y-3">
          <Input label="Titulo" value={habitForm.title} onChange={(e) => setHabitForm((p) => ({ ...p, title: e.target.value }))} required />
          <Input label="Descricao (opcional)" value={habitForm.description} onChange={(e) => setHabitForm((p) => ({ ...p, description: e.target.value }))} />
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Categoria</label>
              <select className="w-full rounded-xl border border-gray-300 px-3 py-2 text-sm" value={habitForm.category} onChange={(e) => setHabitForm((p) => ({ ...p, category: e.target.value }))}>
                {['Custom', 'Water', 'Sleep', 'Nutrition', 'Cardio', 'Steps'].map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </div>
            <Input label="Meta numerica (opcional)" type="number" step="0.01" value={habitForm.targetValue} onChange={(e) => setHabitForm((p) => ({ ...p, targetValue: e.target.value }))} />
          </div>
          <Input label="Unidade da meta (opcional)" value={habitForm.targetUnit} onChange={(e) => setHabitForm((p) => ({ ...p, targetUnit: e.target.value }))} placeholder="litros, horas, passos..." />
          <div className="flex gap-3">
            <Button type="button" variant="secondary" className="flex-1" onClick={() => setHabitModal(false)}>Cancelar</Button>
            <Button type="submit" className="flex-1" loading={saving}>Salvar</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function Row({ label, value }) {
  if (!value) return null;
  return (
    <div className="flex justify-between gap-4">
      <span className="text-sm text-gray-400">{label}</span>
      <span className="text-sm text-gray-900 text-right">{value}</span>
    </div>
  );
}

