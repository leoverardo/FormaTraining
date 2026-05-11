import { useEffect, useState } from 'react';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { useToast } from '../../components/ui/Toast';
import { trainerService } from '../../services/trainerService';
import { Users } from 'lucide-react';

const statusOptions = ['New', 'Contacted', 'Archived', 'Converted'];

export function TrainerLeadsPage() {
  const [leads, setLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const { toast } = useToast();

  const load = async () => {
    setLoading(true);
    try {
      const response = await trainerService.getLeads();
      setLeads(response.data.data || []);
    } catch {
      setLeads([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const updateStatus = async (id, status) => {
    try {
      await trainerService.updateLeadStatus(id, statusOptions.indexOf(status) + 1);
      toast('Status atualizado.');
      load();
    } catch {
      toast('Falha ao atualizar status.', 'error');
    }
  };

  const convert = async (id) => {
    try {
      await trainerService.convertLeadToStudent(id);
      toast('Lead convertido em aluno.');
      load();
    } catch (error) {
      toast(error?.response?.data?.message || 'Falha ao converter lead.', 'error');
    }
  };

  if (loading) return <PageContainer><SectionCard title="Leads"><p className="text-sm text-slate-500">Carregando...</p></SectionCard></PageContainer>;

  return (
    <PageContainer className="space-y-4">
      <SectionCard title="Leads de interesse" description="Contatos recebidos pela sua página e exploração pública.">
        {leads.length === 0 ? (
          <EmptyState icon={Users} title="Sem leads por enquanto" description="Quando alguém demonstrar interesse, aparecerá aqui." />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-xs uppercase tracking-wide text-slate-500">
                  <th className="py-2 pr-3">Nome</th>
                  <th className="py-2 pr-3">Contato</th>
                  <th className="py-2 pr-3">Objetivo</th>
                  <th className="py-2 pr-3">Status</th>
                  <th className="py-2 pr-3">Ações</th>
                </tr>
              </thead>
              <tbody>
                {leads.map((lead) => (
                  <tr key={lead.id} className="border-b border-slate-100">
                    <td className="py-2 pr-3">
                      <p className="font-medium text-slate-800">{lead.name}</p>
                      <p className="text-xs text-slate-500">{new Date(lead.createdAt).toLocaleDateString('pt-BR')}</p>
                    </td>
                    <td className="py-2 pr-3">
                      <p>{lead.email}</p>
                      <p className="text-xs text-slate-500">{lead.phone || '-'}</p>
                    </td>
                    <td className="py-2 pr-3">{lead.goal || '-'}</td>
                    <td className="py-2 pr-3">
                      <select
                        value={lead.status}
                        onChange={(e) => updateStatus(lead.id, e.target.value)}
                        className="rounded-lg border border-slate-300 px-2 py-1 text-xs"
                      >
                        {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                      </select>
                    </td>
                    <td className="py-2 pr-3">
                      <Button size="sm" onClick={() => convert(lead.id)}>Converter em aluno</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </SectionCard>
    </PageContainer>
  );
}
