import { useEffect, useMemo, useRef, useState } from 'react';
import { publicPageService } from '../../services/publicPageService';
import { trainerService } from '../../services/trainerService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { LoadingState } from '../../components/ui/LoadingState';
import { Globe, Copy, ExternalLink, Link2, MessageCircle, Eye, EyeOff, LayoutTemplate, Megaphone } from 'lucide-react';
import { FormPage } from '../../components/forms/FormPage';
import { FormHeader } from '../../components/forms/FormHeader';
import { FormSection } from '../../components/forms/FormSection';
import { FormGrid } from '../../components/forms/FormGrid';
import { FormField } from '../../components/forms/FormField';
import { Textarea } from '../../components/forms/Textarea';
import { Checkbox } from '../../components/forms/Checkbox';
import { Switch } from '../../components/forms/Switch';
import { PreviewCard } from '../../components/forms/PreviewCard';
import { SaveBar } from '../../components/forms/SaveBar';

const initialForm = {
  publicSlug: '',
  publicPageEnabled: false,
  publicHeadline: '',
  publicDescription: '',
  whatsappNumber: '',
  showInstagram: true,
  showTestimonials: true,
  bannerUrl: '',
  welcomeMessage: '',
};

function formatWhatsapp(number) {
  return String(number || '').replace(/\D/g, '');
}

export function PublicPageSettingsPage() {
  const { toast } = useToast();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const formRef = useRef(null);
  const [tab, setTab] = useState('edit');
  const [form, setForm] = useState(initialForm);

  useEffect(() => {
    trainerService.getProfile().then((response) => {
      const profile = response.data.data;
      setForm({
        publicSlug: profile.publicSlug || '',
        publicPageEnabled: profile.publicPageEnabled || false,
        publicHeadline: profile.publicHeadline || '',
        publicDescription: profile.publicDescription || '',
        whatsappNumber: profile.whatsappNumber || '',
        showInstagram: profile.showInstagram ?? true,
        showTestimonials: profile.showTestimonials ?? true,
        bannerUrl: profile.bannerUrl || '',
        welcomeMessage: profile.welcomeMessage || '',
      });
    }).finally(() => setLoading(false));
  }, []);

  const pageUrl = form.publicSlug ? `${window.location.origin}/p/${form.publicSlug}` : '';

  const handleSave = async (event) => {
    event.preventDefault();
    setSaving(true);
    try {
      await publicPageService.updatePage(form);
      toast('Pagina publica atualizada!');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar pagina publica', 'error');
    } finally {
      setSaving(false);
    }
  };

  const previewName = useMemo(() => {
    if (!form.publicSlug) return 'Seu perfil';
    return form.publicSlug.replace(/[-_]/g, ' ');
  }, [form.publicSlug]);

  if (loading) return <LoadingState />;

  return (
    <FormPage>
      <FormHeader title="Editor da Pagina Publica" description="Configure seu link publico, CTA e conteudos visiveis para captacao." />

      <div className="grid gap-4 lg:hidden">
        <div className="inline-flex rounded-xl border border-slate-200 bg-white p-1">
          <button onClick={() => setTab('edit')} className={`flex-1 rounded-lg px-3 py-2 text-sm font-medium ${tab === 'edit' ? 'bg-slate-900 text-white' : 'text-slate-600'}`}>Editar</button>
          <button onClick={() => setTab('preview')} className={`flex-1 rounded-lg px-3 py-2 text-sm font-medium ${tab === 'preview' ? 'bg-slate-900 text-white' : 'text-slate-600'}`}>Preview</button>
        </div>
      </div>

      <form ref={formRef} onSubmit={handleSave} className="space-y-5">
        <div className="grid gap-5 lg:grid-cols-[1.1fr_0.9fr]">
          <div className={`space-y-5 ${tab === 'preview' ? 'hidden lg:block' : ''}`}>
            <FormSection icon={Globe} title="Status da pagina" description="Ative sua vitrine publica quando quiser divulgar.">
              <Switch checked={form.publicPageEnabled} onChange={(value) => setForm((p) => ({ ...p, publicPageEnabled: value }))} label={form.publicPageEnabled ? 'Pagina ativa' : 'Pagina inativa'} />
            </FormSection>

            <FormSection icon={Link2} title="URL publica" description="Defina seu slug e compartilhe com alunos e leads.">
              <FormGrid>
                <Input label="Slug" placeholder="carlos-trainer" value={form.publicSlug} onChange={(e) => setForm((p) => ({ ...p, publicSlug: e.target.value }))} />
                <FormField label="URL final">
                  <div className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-700">{pageUrl || `${window.location.origin}/p/seu-slug`}</div>
                </FormField>
              </FormGrid>
              <div className="mt-3 flex flex-wrap gap-2">
                <Button type="button" variant="outline" disabled={!pageUrl} onClick={() => { navigator.clipboard.writeText(pageUrl); toast('Link copiado!'); }}>
                  <Copy size={15} />Copiar link
                </Button>
                <Button type="button" variant="outline" disabled={!form.publicSlug} onClick={() => window.open(`/p/${form.publicSlug}`, '_blank', 'noopener,noreferrer')}>
                  <ExternalLink size={15} />Abrir pagina
                </Button>
              </div>
            </FormSection>

            <FormSection icon={Megaphone} title="Headline e descricao" description="Texto principal que vende sua consultoria.">
              <Input label="Headline" placeholder="Consultoria online para evoluir com metodo" value={form.publicHeadline} onChange={(e) => setForm((p) => ({ ...p, publicHeadline: e.target.value }))} />
              <FormField label="Descricao publica" helper={`${form.publicDescription.length}/220 caracteres`}>
                <Textarea rows={4} maxLength={220} value={form.publicDescription} onChange={(e) => setForm((p) => ({ ...p, publicDescription: e.target.value }))} />
              </FormField>
              <Input label="Mensagem de boas-vindas" value={form.welcomeMessage} onChange={(e) => setForm((p) => ({ ...p, welcomeMessage: e.target.value }))} />
            </FormSection>

            <FormSection icon={MessageCircle} title="Contato e CTA" description="Configure o WhatsApp para conversao direta.">
              <Input label="WhatsApp" placeholder="(11) 99999-9999" value={form.whatsappNumber} onChange={(e) => setForm((p) => ({ ...p, whatsappNumber: e.target.value }))} />
              <Input label="URL do banner" placeholder="https://..." value={form.bannerUrl} onChange={(e) => setForm((p) => ({ ...p, bannerUrl: e.target.value }))} hint="Compativel com o backend atual. Upload pode ser adicionado depois." />
            </FormSection>

            <FormSection icon={LayoutTemplate} title="Conteudos visiveis" description="Escolha o que aparece para visitantes.">
              <div className="flex flex-col gap-2">
                <Checkbox checked={form.showInstagram} onChange={(value) => setForm((p) => ({ ...p, showInstagram: value }))} label="Mostrar Instagram" />
                <Checkbox checked={form.showTestimonials} onChange={(value) => setForm((p) => ({ ...p, showTestimonials: value }))} label="Mostrar depoimentos" />
              </div>
            </FormSection>
          </div>

          <div className={`space-y-4 lg:sticky lg:top-6 lg:self-start ${tab === 'edit' ? 'hidden lg:block' : ''}`}>
            <PreviewCard title="Preview da pagina" subtitle="Visual resumido da sua pagina publica.">
              <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
                <div className="relative h-36 bg-[linear-gradient(120deg,_#0f172a,_#0e7490,_#14532d)]">
                  {form.bannerUrl ? <img src={form.bannerUrl} alt="Banner" className="absolute inset-0 h-full w-full object-cover" /> : null}
                  <div className="absolute inset-0 bg-black/30" />
                </div>
                <div className="p-4">
                  <p className="text-sm font-semibold text-slate-900">{previewName}</p>
                  <p className="mt-1 text-sm text-slate-600">{form.publicHeadline || 'Consultoria personalizada para sua evolucao.'}</p>
                  <p className="mt-1 text-xs text-slate-500">{form.publicDescription || 'Adicione uma descricao para melhorar conversao.'}</p>
                  <div className="mt-4 flex gap-2">
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${formatWhatsapp(form.whatsappNumber) ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
                      {formatWhatsapp(form.whatsappNumber) ? 'CTA WhatsApp ativo' : 'CTA WhatsApp inativo'}
                    </span>
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${form.publicPageEnabled ? 'bg-cyan-100 text-cyan-700' : 'bg-slate-100 text-slate-500'}`}>
                      {form.publicPageEnabled ? <Eye size={12} className="mr-1" /> : <EyeOff size={12} className="mr-1" />}
                      {form.publicPageEnabled ? 'Publica' : 'Oculta'}
                    </span>
                  </div>
                </div>
              </div>
            </PreviewCard>
          </div>
        </div>

        <SaveBar saving={saving} onSave={() => formRef.current?.requestSubmit()} label="Salvar pagina publica" />
      </form>
    </FormPage>
  );
}
