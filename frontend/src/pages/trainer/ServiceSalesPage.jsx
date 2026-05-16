import { useEffect, useState } from 'react';
import { serviceSalesService } from '../../services/serviceSalesService';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/ui/Toast';

const initialForm = { title: '', description: '', price: '', billingType: 'OneTime', durationDays: '', isActive: true, isPublic: true, displayOrder: 0 };

export function ServiceSalesPage() {
  const { toast } = useToast();
  const [loading, setLoading] = useState(true);
  const [offers, setOffers] = useState([]);
  const [orders, setOrders] = useState([]);
  const [summary, setSummary] = useState(null);
  const [tab, setTab] = useState('offers');
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(initialForm);

  const load = async () => {
    setLoading(true);
    try {
      const [o, or, s] = await Promise.all([
        serviceSalesService.getOffers(),
        serviceSalesService.getOrders(),
        serviceSalesService.getOrdersSummary(),
      ]);
      setOffers(o.data.data || []);
      setOrders(or.data.data || []);
      setSummary(s.data.data || null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setEditing(null);
    setForm(initialForm);
    setOpen(true);
  };

  const openEdit = (item) => {
    setEditing(item);
    setForm({
      title: item.title || '',
      description: item.description || '',
      price: item.price || '',
      billingType: item.billingType || 'OneTime',
      durationDays: item.durationDays || '',
      isActive: item.isActive,
      isPublic: item.isPublic,
      displayOrder: item.displayOrder ?? 0,
    });
    setOpen(true);
  };

  const saveOffer = async (e) => {
    e.preventDefault();
    const payload = {
      ...form,
      price: Number(form.price),
      durationDays: form.durationDays ? Number(form.durationDays) : null,
      displayOrder: Number(form.displayOrder || 0),
    };
    try {
      if (editing) await serviceSalesService.updateOffer(editing.id, payload);
      else await serviceSalesService.createOffer(payload);
      toast('Oferta salva.');
      setOpen(false);
      await load();
    } catch (err) {
      toast(err?.response?.data?.message || 'Erro ao salvar oferta', 'error');
    }
  };

  if (loading) return <LoadingState />;

  return (
    <div className="space-y-4">
      <div className="rounded-2xl border border-slate-200 bg-white p-4">
        <h1 className="text-xl font-bold text-slate-900">Comercial</h1>
        <p className="text-sm text-slate-500">Gerencie serviços e acompanhe contratações.</p>
        {summary && (
          <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-5">
            {[['Pedidos', summary.totalOrders], ['Aprovados', summary.approvedOrders], ['Pendentes', summary.pendingOrders], ['Rejeitados', summary.rejectedOrders], ['Volume', `R$ ${Number(summary.approvedVolume || 0).toFixed(2)}`]].map(([k, v]) => (
              <div key={k} className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2">
                <p className="text-xs text-slate-500">{k}</p>
                <p className="text-sm font-semibold text-slate-900">{v}</p>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="flex gap-2">
        <Button variant={tab === 'offers' ? 'primary' : 'outline'} onClick={() => setTab('offers')}>Serviços</Button>
        <Button variant={tab === 'orders' ? 'primary' : 'outline'} onClick={() => setTab('orders')}>Vendas</Button>
      </div>

      {tab === 'offers' && (
        <div className="space-y-3">
          <div className="flex justify-end"><Button onClick={openCreate}>Nova oferta</Button></div>
          {offers.length === 0 ? <EmptyState title="Nenhuma oferta cadastrada" description="Crie o primeiro serviço para começar a vender." /> : offers.map((item) => (
            <div key={item.id} className="rounded-2xl border border-slate-200 bg-white p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-semibold text-slate-900">{item.title}</p>
                  <p className="text-sm text-slate-500">{item.description || 'Sem descrição'}</p>
                  <p className="text-sm text-slate-700 mt-1">R$ {Number(item.price).toFixed(2)}</p>
                </div>
                <div className="flex gap-2">
                  <Button size="sm" variant="outline" onClick={() => openEdit(item)}>Editar</Button>
                  <Button size="sm" variant="outline" onClick={() => serviceSalesService.updateOfferStatus(item.id, !item.isActive).then(load)}>{item.isActive ? 'Inativar' : 'Ativar'}</Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {tab === 'orders' && (
        <div className="space-y-2">
          {orders.length === 0 ? <EmptyState title="Nenhuma venda registrada" description="As contratações aparecerão aqui." /> : orders.map((item) => (
            <div key={item.id} className="rounded-2xl border border-slate-200 bg-white p-4">
              <p className="font-semibold text-slate-900">{item.serviceTitle}</p>
              <p className="text-sm text-slate-600">{item.buyerName} • {item.buyerEmail}</p>
              <p className="text-sm text-slate-600">R$ {Number(item.amount).toFixed(2)} • {item.status}</p>
            </div>
          ))}
        </div>
      )}

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Editar oferta' : 'Nova oferta'}>
        <form onSubmit={saveOffer} className="space-y-3">
          <Input label="Título" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} required />
          <Input label="Descrição" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} />
          <div className="grid grid-cols-2 gap-2">
            <Input label="Preço (R$)" type="number" min="1" step="0.01" value={form.price} onChange={(e) => setForm((p) => ({ ...p, price: e.target.value }))} required />
            <Input label="Ordem" type="number" min="0" value={form.displayOrder} onChange={(e) => setForm((p) => ({ ...p, displayOrder: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-2">
            <label className="text-sm text-slate-600">Ativo <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} className="ml-2" /></label>
            <label className="text-sm text-slate-600">Público <input type="checkbox" checked={form.isPublic} onChange={(e) => setForm((p) => ({ ...p, isPublic: e.target.checked }))} className="ml-2" /></label>
          </div>
          <Button type="submit" className="w-full">Salvar oferta</Button>
        </form>
      </Modal>
    </div>
  );
}
