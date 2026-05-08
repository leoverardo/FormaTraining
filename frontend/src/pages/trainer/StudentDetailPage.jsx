import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentService } from '../../services/studentService';
import { scheduleService } from '../../services/scheduleService';
import { progressService } from '../../services/progressService';
import { workoutService } from '../../services/workoutService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { ChevronLeft, Plus, Trash2, TrendingUp, Camera, CalendarDays, User } from 'lucide-react';

const TABS = ['Dados', 'Rotina', 'Progresso', 'Fotos'];
const days = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];

const emptyProgress = { weight: '', height: '', chest: '', waist: '', abdomen: '', hip: '', rightArm: '', leftArm: '', rightThigh: '', leftThigh: '', bodyFatPercentage: '', notes: '', progressDate: new Date().toISOString().split('T')[0] };

export function StudentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [activeTab, setActiveTab] = useState('Dados');
  const [student, setStudent] = useState(null);
  const [schedules, setSchedules] = useState([]);
  const [workouts, setWorkouts] = useState([]);
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

  useEffect(() => {
    Promise.all([
      studentService.getById(id),
      scheduleService.getByStudent(id),
      workoutService.getAll(),
      progressService.getByStudent(id),
      progressService.getPhotosByStudent(id),
    ]).then(([s, sc, w, pr, ph]) => {
      setStudent(s.data.data);
      setSchedules(sc.data.data || []);
      setWorkouts(w.data.data || []);
      setProgress(pr.data.data || []);
      setPhotos(ph.data.data || []);
    }).finally(() => setLoading(false));
  }, [id]);

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

