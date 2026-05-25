import { useEffect, useMemo, useRef, useState } from 'react';
import { publicPageService } from '../../services/publicPageService';
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
import { useI18n } from '../../i18n';

const initialForm = {
  publicSlug: '',
  publicPageEnabled: false,
  publicSearchEnabled: false,
  acceptingStudents: true,
  publicHeadline: '',
  publicDescription: '',
  whatsappNumber: '',
  showInstagram: true,
  showTestimonials: true,
  publicBannerUrl: '',
  publicBannerMediaId: null,
  welcomeMessage: '',
  primaryColor: '',
  secondaryColor: '',
};

function formatWhatsapp(number) {
  return String(number || '').replace(/\D/g, '');
}

function mapResponseToForm(payload) {
  if (!payload) return initialForm;
  return {
    publicSlug: payload.publicSlug || '',
    publicPageEnabled: payload.publicPageEnabled === true,
    publicSearchEnabled: payload.publicSearchEnabled === true,
    acceptingStudents: payload.acceptingStudents !== false,
    publicHeadline: payload.publicHeadline || '',
    publicDescription: payload.publicDescription || '',
    whatsappNumber: payload.whatsappNumber || '',
    showInstagram: payload.showInstagram ?? true,
    showTestimonials: payload.showTestimonials ?? true,
    publicBannerUrl: payload.publicBannerUrl || payload.bannerUrl || '',
    publicBannerMediaId: payload.publicBannerMediaId ?? null,
    welcomeMessage: payload.welcomeMessage || '',
    primaryColor: payload.primaryColor || '',
    secondaryColor: payload.secondaryColor || '',
  };
}

export function PublicPageSettingsPage() {
  const { toast } = useToast();
  const { t } = useI18n();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [isDirty, setIsDirty] = useState(false);
  const formRef = useRef(null);
  const [tab, setTab] = useState('edit');
  const [form, setForm] = useState(initialForm);
  const [initialPublicEnabled, setInitialPublicEnabled] = useState(false);

  const loadSettings = async () => {
    setLoading(true);
    try {
      const response = await publicPageService.getTrainerPageSettings();
      const mapped = mapResponseToForm(response.data?.data);
      setForm(mapped);
      setInitialPublicEnabled(mapped.publicPageEnabled);
      setIsDirty(false);
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao carregar página pública', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSettings().catch(() => {});
  }, []);

  const patchForm = (next) => {
    setForm((prev) => ({ ...prev, ...next }));
    setIsDirty(true);
  };

  const pageUrl = form.publicSlug ? `${window.location.origin}/p/${form.publicSlug}` : '';

  const handleSave = async (event) => {
    event.preventDefault();
    if (loading) return;

    setSaving(true);
    try {
      if (!initialPublicEnabled && form.publicPageEnabled) {
        const confirmed = window.confirm('Ao ativar, seu perfil podera ser exibido publicamente e listado na busca de trainers, conforme as informacoes selecionadas. Deseja continuar?');
        if (!confirmed) {
          setSaving(false);
          return;
        }
      }

      const payload = {
        publicPageEnabled: form.publicPageEnabled,
        publicSearchEnabled: form.publicSearchEnabled,
        acceptingStudents: form.acceptingStudents,
        publicSlug: form.publicSlug,
        publicHeadline: form.publicHeadline,
        publicDescription: form.publicDescription,
        welcomeMessage: form.welcomeMessage,
        whatsappNumber: form.whatsappNumber,
        showInstagram: form.showInstagram,
        showTestimonials: form.showTestimonials,
        publicBannerUrl: form.publicBannerUrl,
        publicBannerMediaId: form.publicBannerMediaId,
        primaryColor: form.primaryColor,
        secondaryColor: form.secondaryColor,
      };

      const response = await publicPageService.updatePage(payload);
      const mapped = mapResponseToForm(response.data?.data);
      setForm(mapped);
      setInitialPublicEnabled(mapped.publicPageEnabled);
      setIsDirty(false);
      toast('Página pública atualizada!');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar página pública', 'error');
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
      <FormHeader title={t('trainer.publicPage.editorTitle')} description={t('trainer.publicPage.editorDescription')} />

      <div className="grid gap-4 lg:hidden">
        <div className="inline-flex rounded-xl border border-slate-200 bg-white p-1">
          <button onClick={() => setTab('edit')} className={`flex-1 rounded-lg px-3 py-2 text-sm font-medium ${tab === 'edit' ? 'bg-slate-900 text-white' : 'text-slate-600'}`}>Editar</button>
          <button onClick={() => setTab('preview')} className={`flex-1 rounded-lg px-3 py-2 text-sm font-medium ${tab === 'preview' ? 'bg-slate-900 text-white' : 'text-slate-600'}`}>Preview</button>
        </div>
      </div>

      <form ref={formRef} onSubmit={handleSave} className="space-y-5">
        <div className="grid gap-5 lg:grid-cols-[1.1fr_0.9fr]">
          <div className={`space-y-5 ${tab === 'preview' ? 'hidden lg:block' : ''}`}>
            <FormSection icon={Globe} title={t('trainer.publicPage.statusTitle')} description={t('trainer.publicPage.statusDescription')}>
              <Switch checked={form.publicPageEnabled} onChange={(value) => patchForm({ publicPageEnabled: value })} label={form.publicPageEnabled ? 'Página ativa' : 'Página inativa'} />
              <div className="mt-3 flex flex-col gap-2">
                <Checkbox checked={form.publicSearchEnabled} onChange={(value) => patchForm({ publicSearchEnabled: value })} label="Aparecer no Explore" />
                <Checkbox checked={form.acceptingStudents} onChange={(value) => patchForm({ acceptingStudents: value })} label="Aceitando novos alunos" />
              </div>
            </FormSection>

            <FormSection icon={Link2} title={t('trainer.publicPage.urlTitle')} description={t('trainer.publicPage.urlDescription')}>
              <FormGrid>
                <Input label="Slug" placeholder="carlos-trainer" value={form.publicSlug} onChange={(e) => patchForm({ publicSlug: e.target.value })} />
                <FormField label={t('trainer.publicPage.finalUrl')}>
                  <div className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-700">{pageUrl || `${window.location.origin}/p/seu-slug`}</div>
                </FormField>
              </FormGrid>
              <div className="mt-3 flex flex-wrap gap-2">
                <Button type="button" variant="outline" disabled={!pageUrl || !form.publicPageEnabled} onClick={() => { navigator.clipboard.writeText(pageUrl); toast('Link copiado!'); }}>
                  <Copy size={15} />Copiar link
                </Button>
                <Button type="button" variant="outline" disabled={!pageUrl || !form.publicPageEnabled} onClick={() => window.open(`/p/${form.publicSlug}`, '_blank', 'noopener,noreferrer')}>
                  <ExternalLink size={15} />Abrir página
                </Button>
              </div>
            </FormSection>

            <FormSection icon={Megaphone} title="Headline e descrição" description="Texto principal que vende sua consultoria.">
              <Input label="Headline" placeholder="Consultoria online para evoluir com método" value={form.publicHeadline} onChange={(e) => patchForm({ publicHeadline: e.target.value })} />
              <FormField label="Descrição pública" helper={`${form.publicDescription.length}/220 caracteres`}>
                <Textarea rows={4} maxLength={220} value={form.publicDescription} onChange={(e) => patchForm({ publicDescription: e.target.value })} />
              </FormField>
              <Input label="Mensagem de boas-vindas" value={form.welcomeMessage} onChange={(e) => patchForm({ welcomeMessage: e.target.value })} />
            </FormSection>

            <FormSection icon={MessageCircle} title="Contato e CTA" description="Configure o WhatsApp para conversão direta.">
              <Input label="WhatsApp" placeholder="(11) 99999-9999" value={form.whatsappNumber} onChange={(e) => patchForm({ whatsappNumber: e.target.value })} />
              <Input label="URL do banner público" placeholder="https://..." value={form.publicBannerUrl} onChange={(e) => patchForm({ publicBannerUrl: e.target.value })} />
            </FormSection>

            <FormSection icon={LayoutTemplate} title={t('trainer.publicPage.visibleContentTitle')} description={t('trainer.publicPage.visibleContentDescription')}>
              <div className="flex flex-col gap-2">
                <Checkbox checked={form.showInstagram} onChange={(value) => patchForm({ showInstagram: value })} label="Mostrar Instagram" />
                <Checkbox checked={form.showTestimonials} onChange={(value) => patchForm({ showTestimonials: value })} label="Mostrar depoimentos" />
              </div>
            </FormSection>
          </div>

          <div className={`space-y-4 lg:sticky lg:top-6 lg:self-start ${tab === 'edit' ? 'hidden lg:block' : ''}`}>
            <PreviewCard title={t('trainer.publicPage.previewTitle')} subtitle={t('trainer.publicPage.previewSubtitle')}>
              <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
                <div className="relative h-36 bg-[linear-gradient(120deg,_#0f172a,_#0e7490,_#14532d)]">
                  {form.publicBannerUrl ? <img src={form.publicBannerUrl} alt="Banner" className="absolute inset-0 h-full w-full object-cover" /> : null}
                  <div className="absolute inset-0 bg-black/30" />
                </div>
                <div className="p-4">
                  <p className="text-sm font-semibold text-slate-900">{previewName}</p>
                  <p className="mt-1 text-sm text-slate-600">{form.publicHeadline || 'Consultoria personalizada para sua evolução.'}</p>
                  <p className="mt-1 text-xs text-slate-500">{form.publicDescription || 'Adicione uma descrição para melhorar conversão.'}</p>
                  <div className="mt-4 flex gap-2">
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${formatWhatsapp(form.whatsappNumber) ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
                      {formatWhatsapp(form.whatsappNumber) ? 'CTA WhatsApp ativo' : 'CTA WhatsApp inativo'}
                    </span>
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${form.publicPageEnabled ? 'bg-cyan-100 text-cyan-700' : 'bg-slate-100 text-slate-500'}`}>
                      {form.publicPageEnabled ? <Eye size={12} className="mr-1" /> : <EyeOff size={12} className="mr-1" />}
                      {form.publicPageEnabled ? 'Pública' : 'Oculta'}
                    </span>
                  </div>
                </div>
              </div>
            </PreviewCard>
          </div>
        </div>

        <SaveBar saving={saving} onSave={() => formRef.current?.requestSubmit()} label={isDirty ? 'Salvar página pública' : 'Sem alterações'} />
      </form>
    </FormPage>
  );
}


