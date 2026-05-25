import { useEffect, useMemo, useState } from 'react';
import { appointmentService } from '../../services/appointmentService';
import { studentService } from '../../services/studentService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { CalendarDays, Plus } from 'lucide-react';
import { useDomainLabels } from '../../i18n/domainLabels';
import { themeClasses } from '../../styles/themeClasses';

const types = ['PhysicalAssessment', 'Consultation', 'FollowUp', 'OnlineSession', 'InPersonSession', 'Other'];
const statuses = ['Scheduled', 'Confirmed', 'Cancelled', 'Completed'];

const emptyForm = {
  studentId: '',
  title: '',
  description: '',
  type: 'Consultation',
  startAt: '',
  endAt: '',
  location: '',
  onlineMeetingUrl: '',
};

export function AppointmentsPage() {
  const { toast } = useToast();
  const { appointmentStatusLabel, appointmentTypeLabel } = useDomainLabels();
  const [loading, setLoading] = useState(true);
  const [items, setItems] = useState([]);
  const [students, setStudents] = useState([]);
  const [filters, setFilters] = useState({ status: '', type: '', studentId: '' });
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const load = async () => {
    setLoading(true);
    try {
      const [appointmentsRes, studentsRes] = await Promise.all([
        appointmentService.trainerList(filters),
        studentService.getAll(),
      ]);
      setItems(appointmentsRes.data.data || []);
      setStudents((studentsRes.data.data || []).filter((s) => s.status === 'Active'));
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao carregar compromissos', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); /* eslint-disable-next-line */ }, [filters.status, filters.type, filters.studentId]);

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setModalOpen(true);
  };

  const openEdit = (item) => {
    setEditingId(item.id);
    setForm({
      studentId: item.studentId || '',
      title: item.title || '',
      description: item.description || '',
      type: item.type || 'Other',
      startAt: item.startAt ? new Date(item.startAt).toISOString().slice(0, 16) : '',
      endAt: item.endAt ? new Date(item.endAt).toISOString().slice(0, 16) : '',
      location: item.location || '',
      onlineMeetingUrl: item.onlineMeetingUrl || '',
    });
    setModalOpen(true);
  };

  const save = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        studentId: form.studentId || null,
        title: form.title,
        description: form.description || null,
        type: form.type,
        startAt: new Date(form.startAt).toISOString(),
        endAt: new Date(form.endAt).toISOString(),
        location: form.location || null,
        onlineMeetingUrl: form.onlineMeetingUrl || null,
      };
      if (editingId) await appointmentService.trainerUpdate(editingId, payload);
      else await appointmentService.trainerCreate(payload);
      setModalOpen(false);
      await load();
      toast('Compromisso salvo.');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar compromisso', 'error');
    } finally {
      setSaving(false);
    }
  };

  const updateStatus = async (item, action) => {
    try {
      if (action === 'complete') await appointmentService.trainerComplete(item.id);
      if (action === 'cancel') await appointmentService.trainerCancel(item.id, { reason: 'Cancelado pelo trainer' });
      await load();
    } catch (err) {
      toast(err.response?.data?.message || 'Operação não permitida', 'error');
    }
  };

  const sorted = useMemo(() => [...items].sort((a, b) => new Date(a.startAt) - new Date(b.startAt)), [items]);
  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={themeClasses.pageTitle}>Compromissos</h1>
          <p className={themeClasses.pageSubtitle}>Agenda comercial do atendimento</p>
        </div>
        <Button onClick={openCreate}><Plus size={16} />Novo compromisso</Button>
      </div>

      <div className={`${themeClasses.card} rounded-2xl p-4 grid gap-3 sm:grid-cols-3`}>
        <select className={`rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={filters.status} onChange={(e) => setFilters((p) => ({ ...p, status: e.target.value }))}>
          <option value="">Todos status</option>{statuses.map((s) => <option key={s} value={s}>{appointmentStatusLabel(s)}</option>)}
        </select>
        <select className={`rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={filters.type} onChange={(e) => setFilters((p) => ({ ...p, type: e.target.value }))}>
          <option value="">Todos tipos</option>{types.map((itemType) => <option key={itemType} value={itemType}>{appointmentTypeLabel(itemType)}</option>)}
        </select>
        <select className={`rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={filters.studentId} onChange={(e) => setFilters((p) => ({ ...p, studentId: e.target.value }))}>
          <option value="">Todos alunos</option>{students.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
      </div>

      {sorted.length === 0 ? (
        <EmptyState icon={CalendarDays} title="Nenhum compromisso" description="Crie compromissos para organizar atendimentos." />
      ) : (
        <div className="space-y-2">
          {sorted.map((item) => (
            <div key={item.id} className={`${themeClasses.card} rounded-xl p-4`}>
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className={themeClasses.cardTitle}>{item.title}</p>
                  <p className={`text-xs ${themeClasses.mutedText}`}>{appointmentTypeLabel(item.type)} • {new Date(item.startAt).toLocaleString('pt-BR')} - {new Date(item.endAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}</p>
                  <p className={`text-xs ${themeClasses.mutedText}`}>{item.studentName || 'Sem aluno vinculado'}</p>
                </div>
                <span className="text-xs px-2 py-1 rounded-lg bg-slate-100 dark:bg-white/10 text-slate-700 dark:text-slate-200">{appointmentStatusLabel(item.status)}</span>
              </div>
              <div className="mt-3 flex flex-wrap gap-2">
                <Button size="sm" variant="secondary" onClick={() => openEdit(item)}>Editar</Button>
                {(item.status === 'Scheduled' || item.status === 'Confirmed') && <Button size="sm" variant="secondary" onClick={() => updateStatus(item, 'cancel')}>Cancelar</Button>}
                {(item.status === 'Scheduled' || item.status === 'Confirmed') && <Button size="sm" onClick={() => updateStatus(item, 'complete')}>Concluir</Button>}
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editingId ? 'Editar compromisso' : 'Novo compromisso'}>
        <form onSubmit={save} className="space-y-3">
          <select className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={form.studentId} onChange={(e) => setForm((p) => ({ ...p, studentId: e.target.value }))}>
            <option value="">Sem aluno vinculado</option>{students.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <input className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} placeholder="Título" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} required />
          <select className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))}>{types.map((itemType) => <option key={itemType} value={itemType}>{appointmentTypeLabel(itemType)}</option>)}</select>
          <textarea className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} rows={2} placeholder="Descrição" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} />
          <div className="grid grid-cols-2 gap-2">
            <input type="datetime-local" className={`rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={form.startAt} onChange={(e) => setForm((p) => ({ ...p, startAt: e.target.value }))} required />
            <input type="datetime-local" className={`rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} value={form.endAt} onChange={(e) => setForm((p) => ({ ...p, endAt: e.target.value }))} required />
          </div>
          <input className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} placeholder="Local (opcional)" value={form.location} onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))} />
          <input className={`w-full rounded-xl border px-3 py-2 text-sm ${themeClasses.input}`} placeholder="Link online (opcional)" value={form.onlineMeetingUrl} onChange={(e) => setForm((p) => ({ ...p, onlineMeetingUrl: e.target.value }))} />
          <div className="flex gap-2">
            <Button type="button" variant="secondary" className="flex-1" onClick={() => setModalOpen(false)}>Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
