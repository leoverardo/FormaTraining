import { useEffect, useMemo, useRef, useState } from 'react';
import { trainerService } from '../../services/trainerService';
import { useToast } from '../../components/ui/Toast';
import { Input } from '../../components/ui/Input';
import { LoadingState } from '../../components/ui/LoadingState';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { MediaCategory } from '../../services/uploadService';
import { User, Briefcase, MapPin, Palette, Sparkles } from 'lucide-react';
import { FormPage } from '../../components/forms/FormPage';
import { FormHeader } from '../../components/forms/FormHeader';
import { FormSection } from '../../components/forms/FormSection';
import { FormGrid } from '../../components/forms/FormGrid';
import { FormField } from '../../components/forms/FormField';
import { Textarea } from '../../components/forms/Textarea';
import { ColorPickerField } from '../../components/forms/ColorPickerField';
import { PreviewCard } from '../../components/forms/PreviewCard';
import { SaveBar } from '../../components/forms/SaveBar';
import { UploadField } from '../../components/forms/UploadField';
import { useI18n } from '../../i18n';

const initialForm = {
  name: '', brandName: '', phone: '', cpf: '', birthDate: '',
  cref: '', bio: '', specialties: '', instagram: '', profilePhotoUrl: '', logoUrl: '',
  primaryColor: '#0f766e', secondaryColor: '#0891b2',
  zipCode: '', street: '', addressNumber: '', complement: '', neighborhood: '', city: '', state: '',
};

function getInitials(name) {
  return String(name || '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((chunk) => chunk[0]?.toUpperCase())
    .join('') || 'FP';
}

export function ProfilePage() {
  const { toast } = useToast();
  const { t } = useI18n();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(initialForm);
  const formRef = useRef(null);
  const [profilePhotoMediaId, setProfilePhotoMediaId] = useState(null);
  const [logoMediaId, setLogoMediaId] = useState(null);

  useEffect(() => {
    trainerService.getProfile().then((response) => {
      const profile = response.data.data;
      setForm({
        name: profile.name || '',
        brandName: profile.brandName || '',
        phone: profile.phone || '',
        cpf: profile.cpf || '',
        birthDate: profile.birthDate ? profile.birthDate.split('T')[0] : '',
        cref: profile.cref || '',
        bio: profile.bio || '',
        specialties: profile.specialties || '',
        instagram: profile.instagram || '',
        profilePhotoUrl: profile.profilePhotoUrl || '',
        logoUrl: profile.logoUrl || '',
        primaryColor: profile.primaryColor || '#0f766e',
        secondaryColor: profile.secondaryColor || '#0891b2',
        zipCode: profile.zipCode || '',
        street: profile.street || '',
        addressNumber: profile.addressNumber || '',
        complement: profile.complement || '',
        neighborhood: profile.neighborhood || '',
        city: profile.city || '',
        state: profile.state || '',
      });
      setProfilePhotoMediaId(profile.profilePhotoMediaId || null);
      setLogoMediaId(profile.logoMediaId || null);
    }).finally(() => setLoading(false));
  }, []);

  const f = (field) => ({ value: form[field], onChange: (e) => setForm((prev) => ({ ...prev, [field]: e.target.value })) });

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSaving(true);
    try {
      await trainerService.updateProfile({
        ...form,
        birthDate: form.birthDate || null,
        profilePhotoMediaId,
        logoMediaId,
      });
      toast('Perfil atualizado com sucesso!');
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar perfil', 'error');
    } finally {
      setSaving(false);
    }
  };

  const previewGradient = useMemo(
    () => `linear-gradient(130deg, ${form.primaryColor || '#0f766e'}, ${form.secondaryColor || '#0891b2'})`,
    [form.primaryColor, form.secondaryColor],
  );

  if (loading) return <LoadingState />;

  return (
    <FormPage>
      <FormHeader title={t('nav.myProfile')} description={t('trainer.profile.description')} />

      <form ref={formRef} onSubmit={handleSubmit} className="space-y-5">
        <div className="grid gap-5 xl:grid-cols-[1.25fr_0.8fr]">
          <div className="space-y-5">
            <FormSection icon={User} title={t('trainer.profile.personalDataTitle')} description={t('trainer.profile.personalDataDescription')}>
              <FormGrid>
                <div className="md:col-span-2"><Input label="Nome completo" required {...f('name')} /></div>
                <Input label="Telefone" placeholder="(11) 99999-9999" {...f('phone')} />
                <Input label="CPF" placeholder="000.000.000-00" {...f('cpf')} />
                <div className="md:col-span-2"><Input type="date" label="Data de nascimento" {...f('birthDate')} /></div>
              </FormGrid>
            </FormSection>

            <FormSection icon={Briefcase} title={t('trainer.profile.professionalDataTitle')} description={t('trainer.profile.professionalDataDescription')}>
              <FormGrid>
                <div className="md:col-span-2"><Input label="Nome da marca" required {...f('brandName')} /></div>
                <Input label="CREF" placeholder="000000-G/SP" {...f('cref')} />
                <FormField label={t('trainer.profile.instagram')} helper={t('trainer.profile.instagramHelper')}>
                  <div className="flex items-center rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm focus-within:ring-2 focus-within:ring-cyan-200">
                    <span className="text-slate-400">@</span>
                    <input value={form.instagram} onChange={(e) => setForm((p) => ({ ...p, instagram: e.target.value }))} className="ml-1 w-full border-0 p-0 text-slate-800 focus:outline-none" />
                  </div>
                </FormField>
                <div className="md:col-span-2"><Input label="Especialidades" placeholder="Hipertrofia, emagrecimento, condicionamento" {...f('specialties')} /></div>
                <div className="md:col-span-2">
                  <FormField label="Bio" helper={`${form.bio.length}/280 caracteres`}>
                    <Textarea rows={4} maxLength={280} value={form.bio} onChange={(e) => setForm((p) => ({ ...p, bio: e.target.value }))} />
                  </FormField>
                </div>
              </FormGrid>
            </FormSection>

            <FormSection icon={MapPin} title="Endereco" description="Informacoes administrativas do seu cadastro.">
              <FormGrid>
                <Input label="CEP" {...f('zipCode')} />
                <Input label="Estado" maxLength={2} placeholder="SP" {...f('state')} />
                <div className="md:col-span-2"><Input label="Cidade" {...f('city')} /></div>
                <div className="md:col-span-2"><Input label="Rua" {...f('street')} /></div>
                <Input label="Numero" {...f('addressNumber')} />
                <Input label="Bairro" {...f('neighborhood')} />
                <div className="md:col-span-2"><Input label="Complemento" {...f('complement')} /></div>
              </FormGrid>
            </FormSection>

            <FormSection icon={Palette} title="Identidade visual" description="Cores usadas na sua comunicacao e pagina publica.">
              <FormGrid>
                <ColorPickerField label="Cor primaria" value={form.primaryColor} onChange={(value) => setForm((p) => ({ ...p, primaryColor: value }))} />
                <ColorPickerField label="Cor secundaria" value={form.secondaryColor} onChange={(value) => setForm((p) => ({ ...p, secondaryColor: value }))} />
              </FormGrid>
            </FormSection>

            <FormSection icon={Sparkles} title={t('trainer.profile.brandMediaTitle')} description={t('trainer.profile.brandMediaDescription')}>
              <FormGrid>
                <UploadField label={t('trainer.profile.profilePhoto')} helper={t('trainer.profile.uploadHelper')}>
                  <ImageUpload
                    category={MediaCategory.TrainerProfile}
                    currentUrl={form.profilePhotoUrl}
                    currentMediaId={profilePhotoMediaId}
                    isPublic={true}
                    onUploaded={(media) => { setProfilePhotoMediaId(media.id); setForm((p) => ({ ...p, profilePhotoUrl: media.url })); }}
                    onRemoved={() => { setProfilePhotoMediaId(null); setForm((p) => ({ ...p, profilePhotoUrl: '' })); }}
                  />
                </UploadField>
                <UploadField label={t('trainer.profile.brandLogo')} helper={t('trainer.profile.uploadHelper')}>
                  <ImageUpload
                    category={MediaCategory.TrainerLogo}
                    currentUrl={form.logoUrl}
                    currentMediaId={logoMediaId}
                    isPublic={true}
                    onUploaded={(media) => { setLogoMediaId(media.id); setForm((p) => ({ ...p, logoUrl: media.url })); }}
                    onRemoved={() => { setLogoMediaId(null); setForm((p) => ({ ...p, logoUrl: '' })); }}
                  />
                </UploadField>
              </FormGrid>
            </FormSection>
          </div>

          <div className="space-y-4 xl:sticky xl:top-6 xl:self-start">
            <PreviewCard title={t('trainer.profile.brandPreviewTitle')} subtitle={t('trainer.profile.brandPreviewSubtitle')}>
              <div className="overflow-hidden rounded-2xl border border-slate-200 bg-slate-50">
                <div className="h-28 p-4" style={{ background: previewGradient }}>
                  <div className="inline-flex rounded-full bg-white/20 px-2 py-1 text-xs font-semibold text-white">Consultoria</div>
                </div>
                <div className="-mt-8 px-4 pb-4">
                  {form.profilePhotoUrl ? (
                    <img src={form.profilePhotoUrl} alt="Perfil" className="h-16 w-16 rounded-full border-4 border-white object-cover" />
                  ) : (
                    <div className="flex h-16 w-16 items-center justify-center rounded-full border-4 border-white bg-cyan-100 text-sm font-semibold text-cyan-800">{getInitials(form.name || form.brandName)}</div>
                  )}
                  <p className="mt-2 font-semibold text-slate-900">{form.brandName || 'Sua marca'}</p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">{form.specialties || 'Especialidades em fitness e performance'}</p>
                </div>
              </div>
            </PreviewCard>

            <PreviewCard title={t('trainer.profile.logoTitle')} subtitle={t('trainer.profile.logoSubtitle')}>
              {form.logoUrl ? <img src={form.logoUrl} alt="Logo" className="h-20 w-auto rounded-lg border border-slate-200 bg-white p-2" /> : <p className="text-sm text-slate-500 dark:text-slate-400">Adicione sua logo para fortalecer sua identidade.</p>}
            </PreviewCard>
          </div>
        </div>

        <SaveBar saving={saving} onSave={() => formRef.current?.requestSubmit()} />
      </form>
    </FormPage>
  );
}


