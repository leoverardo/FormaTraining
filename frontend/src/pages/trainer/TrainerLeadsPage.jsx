import { useEffect, useState } from 'react';
import { PageContainer } from '../../components/ui/PageContainer';
import { SectionCard } from '../../components/ui/SectionCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { Button } from '../../components/ui/Button';
import { useToast } from '../../components/ui/Toast';
import { trainerService } from '../../services/trainerService';
import { Users } from 'lucide-react';
import { useI18n } from '../../i18n';

const statusOptions = ['New', 'Contacted', 'Archived', 'Converted'];

export function TrainerLeadsPage() {
  const [leads, setLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const { toast } = useToast();
  const { t } = useI18n();

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
      toast(t('trainer.leads.statusUpdated'));
      load();
    } catch {
      toast(t('trainer.leads.statusUpdateError'), 'error');
    }
  };

  const convert = async (id) => {
    try {
      await trainerService.convertLeadToStudent(id);
      toast(t('trainer.leads.convertedSuccess'));
      load();
    } catch (error) {
      toast(error?.response?.data?.message || t('trainer.leads.convertError'), 'error');
    }
  };

  if (loading) return <PageContainer><SectionCard title={t('nav.leads')}><p className="text-sm text-slate-500">{t('common.loading')}</p></SectionCard></PageContainer>;

  return (
    <PageContainer className="space-y-4">
      <SectionCard title={t('trainer.leads.title')} description={t('trainer.leads.description')}>
        {leads.length === 0 ? (
          <EmptyState icon={Users} title={t('trainer.leads.emptyTitle')} description={t('trainer.leads.emptyDescription')} />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-xs uppercase tracking-wide text-slate-500">
                  <th className="py-2 pr-3">{t('public.name')}</th>
                  <th className="py-2 pr-3">{t('trainer.leads.contact')}</th>
                  <th className="py-2 pr-3">{t('public.goal')}</th>
                  <th className="py-2 pr-3">{t('trainer.posts.statusField')}</th>
                  <th className="py-2 pr-3">{t('trainer.leads.actions')}</th>
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
                      <Button size="sm" onClick={() => convert(lead.id)}>{t('trainer.leads.convert')}</Button>
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
