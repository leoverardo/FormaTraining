import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { AlertCircle } from 'lucide-react';
import { publicPageService } from '../../services/publicPageService';
import { useToast } from '../../components/ui/Toast';
import { Modal } from '../../components/ui/Modal';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { EmptyState } from '../../components/ui/EmptyState';
import { mapPostToFeedItem } from '../../features/feed/feedAdapter';
import { PublicProfileHero } from '../../components/public/PublicProfileHero';
import { PublicAuthoritySection } from '../../components/public/PublicAuthoritySection';
import { PublicFeedSection } from '../../components/public/PublicFeedSection';
import { PublicSpecialtiesSection } from '../../components/public/PublicSpecialtiesSection';
import { PublicTestimonialsSection } from '../../components/public/PublicTestimonialsSection';
import { PublicTransformationsSection } from '../../components/public/PublicTransformationsSection';
import { PublicCtaSection } from '../../components/public/PublicCtaSection';
import { PublicFooter } from '../../components/public/PublicFooter';

function PublicPageSkeleton() {
  return (
    <div className="mx-auto max-w-6xl space-y-4 px-4 py-6 sm:py-8">
      <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
      <div className="grid gap-4 md:grid-cols-2">
        <div className="h-40 animate-pulse rounded-3xl bg-slate-200" />
        <div className="h-40 animate-pulse rounded-3xl bg-slate-200" />
      </div>
      <div className="h-72 animate-pulse rounded-3xl bg-slate-200" />
    </div>
  );
}

const defaultSpecialties = ['Hipertrofia', 'Emagrecimento', 'Condicionamento'];

function digitsOnly(value) {
  return String(value || '').replace(/\D/g, '');
}

export function TrainerPublicPage() {
  const { slug } = useParams();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);
  const [leadOpen, setLeadOpen] = useState(false);
  const [sendingLead, setSendingLead] = useState(false);
  const [leadForm, setLeadForm] = useState({ name: '', email: '', phone: '', goal: '', message: '' });
  const { toast } = useToast();
  const feedRef = useRef(null);

  useEffect(() => {
    setLoading(true);
    setNotFound(false);
    publicPageService.getBySlug(slug)
      .then((response) => setData(response.data.data))
      .catch(() => setNotFound(true))
      .finally(() => setLoading(false));
  }, [slug]);

  const profile = useMemo(() => {
    if (!data) return null;

    const specialties = data.specialties
      ? String(data.specialties).split(',').map((value) => value.trim()).filter(Boolean)
      : defaultSpecialties;

    return {
      name: data.brandName || 'Personal Trainer',
      headline: data.publicHeadline || 'Consultoria online personalizada para performance e saude.',
      bio: data.bio || data.publicDescription || '',
      avatarUrl: data.profilePhotoUrl || data.logoUrl || '',
      bannerUrl: data.bannerUrl || '',
      specialties,
      stats: [
        { label: 'Alunos', value: data.publicStudentCount || 120 },
        { label: 'Experiencia', value: `${data.yearsExperience || 8} anos` },
        { label: 'Especialidades', value: specialties.length || defaultSpecialties.length },
        { label: 'Avaliacoes', value: data.publicReviews || '4.9' },
      ],
    };
  }, [data]);

  const whatsapp = digitsOnly(data?.whatsappNumber);
  const hasWhatsapp = Boolean(whatsapp);

  const feedItems = useMemo(() => {
    if (!data) return [];
    return (data.posts || []).map((post) => mapPostToFeedItem(post, {
      name: data.brandName,
      avatarUrl: data.profilePhotoUrl || data.logoUrl,
      role: 'Personal Trainer',
    }));
  }, [data]);

  const openWhatsapp = () => {
    if (!hasWhatsapp) return;
    window.open(`https://wa.me/${whatsapp}`, '_blank', 'noopener,noreferrer');
  };

  const sendLead = async (event) => {
    event.preventDefault();
    if (!slug) return;
    setSendingLead(true);
    try {
      await publicPageService.createLeadBySlug(slug, leadForm);
      toast('Interesse enviado com sucesso!');
      setLeadOpen(false);
      setLeadForm({ name: '', email: '', phone: '', goal: '', message: '' });
    } catch (error) {
      toast(error?.response?.data?.message || 'Falha ao enviar interesse.', 'error');
    } finally {
      setSendingLead(false);
    }
  };

  if (loading) return <PublicPageSkeleton />;

  if (notFound || !data || !profile) {
    return (
      <div className="flex min-h-screen items-center justify-center p-4">
        <EmptyState
          icon={AlertCircle}
          title="Pagina nao encontrada"
          description="Este personal trainer nao possui pagina publica ativa."
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top_right,_#cffafe,_transparent_40%),linear-gradient(180deg,_#f8fafc,_#eef2ff_50%,_#f8fafc)]">
      <main className="mx-auto max-w-6xl space-y-5 px-4 py-5 sm:space-y-6 sm:py-8">
        <PublicProfileHero
          profile={profile}
          onPrimaryClick={openWhatsapp}
          onSecondaryClick={() => setLeadOpen(true)}
          hasWhatsapp={hasWhatsapp}
        />

        <PublicAuthoritySection />

        <PublicFeedSection
          items={feedItems}
          fallbackName={profile.name}
          fallbackAvatar={profile.avatarUrl}
          feedRef={feedRef}
        />

        <PublicSpecialtiesSection />

        {data.showTestimonials ? <PublicTestimonialsSection testimonials={data.testimonials || []} /> : null}

        <PublicTransformationsSection transformations={data.transformations || []} />

        <PublicCtaSection
          onPrimaryClick={openWhatsapp}
          onSecondaryClick={() => setLeadOpen(true)}
          hasWhatsapp={hasWhatsapp}
          trainerName={profile.name}
        />

        <PublicFooter />
      </main>

      <Modal open={leadOpen} onClose={() => setLeadOpen(false)} title="Tenho interesse" size="md">
        <form onSubmit={sendLead} className="space-y-3">
          <Input label="Nome" value={leadForm.name} onChange={(e) => setLeadForm((p) => ({ ...p, name: e.target.value }))} required />
          <Input label="E-mail" type="email" value={leadForm.email} onChange={(e) => setLeadForm((p) => ({ ...p, email: e.target.value }))} required />
          <Input label="Telefone" value={leadForm.phone} onChange={(e) => setLeadForm((p) => ({ ...p, phone: e.target.value }))} />
          <Input label="Objetivo" value={leadForm.goal} onChange={(e) => setLeadForm((p) => ({ ...p, goal: e.target.value }))} />
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Mensagem</label>
            <textarea className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm" rows={4} value={leadForm.message} onChange={(e) => setLeadForm((p) => ({ ...p, message: e.target.value }))} />
          </div>
          <Button type="submit" className="w-full" loading={sendingLead}>Enviar interesse</Button>
        </form>
      </Modal>
    </div>
  );
}
