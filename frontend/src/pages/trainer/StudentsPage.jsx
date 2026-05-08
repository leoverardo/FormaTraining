import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { studentService } from '../../services/studentService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Select } from '../../components/ui/Select';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { Users, Plus, Pencil, Trash2, UserCheck, UserX, ChevronRight } from 'lucide-react';

const empty = { name: '', email: '', password: '', phone: '', goal: '', notes: '', activeOnCreate: true };

export function StudentsPage() {
  const { toast } = useToast();
  const navigate = useNavigate();
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editStudent, setEditStudent] = useState(null);
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const load = () => {
    setLoading(true);
    studentService.getAll().then(r => setStudents(r.data.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => { setEditStudent(null); setForm(empty); setModalOpen(true); };
  const openEdit = (s) => {
    setEditStudent(s);
    setForm({ name: s.name, email: s.email, password: '', phone: s.phone || '', goal: s.goal || '', notes: s.notes || '', activeOnCreate: s.status === 'Active' });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editStudent) {
        await studentService.update(editStudent.id, { name: form.name, phone: form.phone, goal: form.goal, notes: form.notes });
        toast('Aluno atualizado com sucesso!');
      } else {
        await studentService.create(form);
        toast('Aluno cadastrado com sucesso!');
      }
      setModalOpen(false);
      load();
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar aluno', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await studentService.delete(deleteTarget.id);
      toast('Aluno removido.');
      setDeleteTarget(null);
      load();
    } catch { toast('Erro ao remover aluno', 'error'); }
    finally { setDeleting(false); }
  };

  const toggleStatus = async (s) => {
    try {
      if (s.status === 'Active') { await studentService.deactivate(s.id); toast('Aluno desativado.'); }
      else { await studentService.activate(s.id); toast('Aluno ativado!'); }
      load();
    } catch (err) { toast(err.response?.data?.message || 'Erro ao atualizar status', 'error'); }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Alunos</h1>
          <p className="text-gray-500 text-sm mt-1">{students.length} alunos cadastrados</p>
        </div>
        <Button onClick={openCreate}><Plus size={16} />Novo aluno</Button>
      </div>

      {students.length === 0 ? (
        <EmptyState icon={Users} title="Nenhum aluno cadastrado" description="Adicione seu primeiro aluno para começar." action={<Button onClick={openCreate}><Plus size={16} />Novo aluno</Button>} />
      ) : (
        <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50">
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase">Aluno</th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase hidden md:table-cell">Objetivo</th>
                  <th className="text-left px-6 py-3 text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="px-6 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {students.map(s => (
                  <tr key={s.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4">
                      <div>
                        <p className="font-medium text-gray-900 text-sm">{s.name}</p>
                        <p className="text-gray-400 text-xs">{s.email}</p>
                      </div>
                    </td>
                    <td className="px-6 py-4 hidden md:table-cell">
                      <p className="text-gray-600 text-sm">{s.goal || '?'}</p>
                    </td>
                    <td className="px-6 py-4">
                      <Badge variant={s.status === 'Active' ? 'success' : 'gray'}>
                        {s.status === 'Active' ? 'Ativo' : 'Inativo'}
                      </Badge>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2 justify-end">
                        <button onClick={() => toggleStatus(s)} className={`p-1.5 rounded-lg transition-colors ${s.status === 'Active' ? 'hover:bg-amber-50 text-amber-500' : 'hover:bg-emerald-50 text-emerald-500'}`} title={s.status === 'Active' ? 'Desativar' : 'Ativar'}>
                          {s.status === 'Active' ? <UserX size={16} /> : <UserCheck size={16} />}
                        </button>
                        <button onClick={() => openEdit(s)} className="p-1.5 rounded-lg hover:bg-blue-50 text-blue-500 transition-colors"><Pencil size={16} /></button>
                        <button onClick={() => setDeleteTarget(s)} className="p-1.5 rounded-lg hover:bg-red-50 text-red-500 transition-colors"><Trash2 size={16} /></button>
                        <button onClick={() => navigate(`/trainer/students/${s.id}`)} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors" title="Ver detalhes"><ChevronRight size={16} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editStudent ? 'Editar aluno' : 'Novo aluno'}>
        <form onSubmit={handleSave} className="space-y-4">
          <Input label="Nome" value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} required />
          {!editStudent && <>
            <Input label="E-mail" type="email" value={form.email} onChange={e => setForm(p => ({ ...p, email: e.target.value }))} required />
            <Input label="Senha inicial" type="password" value={form.password} onChange={e => setForm(p => ({ ...p, password: e.target.value }))} required />
          </>}
          <Input label="Telefone" value={form.phone} onChange={e => setForm(p => ({ ...p, phone: e.target.value }))} />
          <Input label="Objetivo" value={form.goal} onChange={e => setForm(p => ({ ...p, goal: e.target.value }))} />
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Observações</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={3} value={form.notes} onChange={e => setForm(p => ({ ...p, notes: e.target.value }))} />
          </div>
          {!editStudent && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="active" checked={form.activeOnCreate} onChange={e => setForm(p => ({ ...p, activeOnCreate: e.target.checked }))} className="rounded" />
              <label htmlFor="active" className="text-sm text-gray-700">Cadastrar como ativo</label>
            </div>
          )}
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        loading={deleting}
        title="Remover aluno"
        description={`Deseja remover o aluno "${deleteTarget?.name}"? Esta ação não pode ser desfeita.`}
      />
    </div>
  );
}

