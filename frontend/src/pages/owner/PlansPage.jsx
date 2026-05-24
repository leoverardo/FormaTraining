import { useEffect, useState } from 'react';
import { platformPlanService } from '../../services/platformPlanService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { Tag, Plus, Pencil, Trash2 } from 'lucide-react';

const empty = {
  code: '',
  name: '',
  description: '',
  monthlyPrice: '',
  maxActiveStudents: '',
  hasUnlimitedStudents: false,
  active: true,
  isPublic: true,
  isComingSoon: false,
  isAvailableForPurchase: true
};

export function PlansPage() {
  const { toast } = useToast();
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editPlan, setEditPlan] = useState(null);
  const [form, setForm] = useState(empty);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const load = () => {
    setLoading(true);
    platformPlanService.getAll().then(r => setPlans(r.data.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openEdit = (p) => {
    setEditPlan(p);
    setForm({
      code: p.code || '',
      name: p.name,
      description: p.description || '',
      monthlyPrice: p.monthlyPrice,
      maxActiveStudents: p.maxActiveStudents,
      hasUnlimitedStudents: !!p.hasUnlimitedStudents,
      active: p.active,
      isPublic: p.isPublic ?? true,
      isComingSoon: p.isComingSoon ?? false,
      isAvailableForPurchase: p.isAvailableForPurchase ?? true
    });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const data = {
        ...form,
        code: (form.code || form.name).toUpperCase(),
        monthlyPrice: parseFloat(form.monthlyPrice),
        maxActiveStudents: form.hasUnlimitedStudents ? 0 : parseInt(form.maxActiveStudents),
      };
      if (editPlan) { await platformPlanService.update(editPlan.id, data); toast('Plano atualizado!'); }
      else { await platformPlanService.create(data); toast('Plano criado!'); }
      setModalOpen(false); load();
    } catch (err) { toast(err.response?.data?.message || 'Erro', 'error'); }
    finally { setSaving(false); }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try { await platformPlanService.delete(deleteTarget.id); toast('Plano removido.'); setDeleteTarget(null); load(); }
    catch { toast('Erro ao remover', 'error'); }
    finally { setDeleting(false); }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Planos da Plataforma</h1>
          <p className="text-gray-500 text-sm mt-1">{plans.length} planos cadastrados</p>
        </div>
        <Button onClick={() => { setEditPlan(null); setForm(empty); setModalOpen(true); }}><Plus size={16} />Novo plano</Button>
      </div>

      {plans.length === 0 ? (
        <EmptyState icon={Tag} title="Nenhum plano" description="Crie planos para os personal trainers contratarem." action={<Button onClick={() => { setEditPlan(null); setForm(empty); setModalOpen(true); }}><Plus size={16} />Novo plano</Button>} />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {plans.map(p => (
            <div key={p.id} className="bg-white rounded-2xl border border-gray-200 p-6">
              <div className="flex items-start justify-between mb-3">
                <h3 className="font-bold text-gray-900 text-lg">{p.name}</h3>
                <Badge variant={p.active ? 'success' : 'gray'}>{p.active ? 'Ativo' : 'Inativo'}</Badge>
              </div>
              {p.isComingSoon ? <p className="text-xs font-semibold text-amber-700 mb-2">Em breve</p> : null}
              <p className="text-3xl font-bold text-purple-600 mb-1">R$ {p.monthlyPrice?.toFixed(2).replace('.', ',')}<span className="text-sm text-gray-400 font-normal">/mes</span></p>
              <p className="text-sm text-gray-500 mb-1">{p.hasUnlimitedStudents ? 'Alunos ilimitados' : <>Ate <span className="font-semibold text-gray-700">{p.maxActiveStudents}</span> alunos ativos</>}</p>
              {p.description && <p className="text-xs text-gray-400 mt-2">{p.description}</p>}
              <div className="flex gap-2 mt-5 pt-4 border-t border-gray-100">
                <button onClick={() => openEdit(p)} className="flex-1 flex items-center justify-center gap-1.5 py-2 text-xs text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"><Pencil size={14} />Editar</button>
                <button onClick={() => setDeleteTarget(p)} className="flex-1 flex items-center justify-center gap-1.5 py-2 text-xs text-red-500 hover:bg-red-50 rounded-lg transition-colors"><Trash2 size={14} />Excluir</button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editPlan ? 'Editar plano' : 'Novo plano'}>
        <form onSubmit={handleSave} className="space-y-4">
          <Input label="Codigo" value={form.code} onChange={e => setForm(p => ({ ...p, code: e.target.value.toUpperCase() }))} required />
          <Input label="Nome do plano" value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} required />
          <Input label="Descricao" value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Preco mensal (R$)" type="number" step="0.01" value={form.monthlyPrice} onChange={e => setForm(p => ({ ...p, monthlyPrice: e.target.value }))} required />
            <Input label="Limite de alunos ativos" type="number" value={form.maxActiveStudents} onChange={e => setForm(p => ({ ...p, maxActiveStudents: e.target.value }))} disabled={form.hasUnlimitedStudents} required={!form.hasUnlimitedStudents} />
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="hasUnlimitedStudents" checked={form.hasUnlimitedStudents} onChange={e => setForm(p => ({ ...p, hasUnlimitedStudents: e.target.checked }))} className="rounded" />
            <label htmlFor="hasUnlimitedStudents" className="text-sm text-gray-700">Alunos ilimitados</label>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="active" checked={form.active} onChange={e => setForm(p => ({ ...p, active: e.target.checked }))} className="rounded" />
            <label htmlFor="active" className="text-sm text-gray-700">Plano ativo</label>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isPublic" checked={form.isPublic} onChange={e => setForm(p => ({ ...p, isPublic: e.target.checked }))} className="rounded" />
            <label htmlFor="isPublic" className="text-sm text-gray-700">Plano publico</label>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isComingSoon" checked={form.isComingSoon} onChange={e => setForm(p => ({ ...p, isComingSoon: e.target.checked }))} className="rounded" />
            <label htmlFor="isComingSoon" className="text-sm text-gray-700">Em breve</label>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isAvailableForPurchase" checked={form.isAvailableForPurchase} onChange={e => setForm(p => ({ ...p, isAvailableForPurchase: e.target.checked }))} className="rounded" />
            <label htmlFor="isAvailableForPurchase" className="text-sm text-gray-700">Disponivel para contratacao</label>
          </div>
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} loading={deleting} title="Excluir plano" description={`Deseja excluir o plano "${deleteTarget?.name}"?`} />
    </div>
  );
}
