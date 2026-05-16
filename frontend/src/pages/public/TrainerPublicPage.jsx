import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { AlertCircle } from 'lucide-react';
import { publicPageService } from '../../services/publicPageService';
import { serviceSalesService } from '../../services/serviceSalesService';
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
  const [buyingOfferId, setBuyingOfferId] = useState(null);
  const [checkoutOpen, setCheckoutOpen] = useState(false);
  const [selectedOffer, setSelectedOffer] = useState(null);
  const [checkoutForm, setCheckoutForm] = useState({ buyerName: '', buyerEmail: '', buyerPhone: '' });
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

  const buyOffer = async (event) => {
    event.preventDefault();
    if (!selectedOffer) return;
    setBuyingOfferId(selectedOffer.id);
    try {
      const response = await serviceSalesService.createPublicOrder(slug, selectedOffer.id, {
        buyerName: checkoutForm.buyerName,
        buyerEmail: checkoutForm.buyerEmail,
        buyerPhone: checkoutForm.buyerPhone || null,
      });
      const checkoutUrl = response?.data?.data?.checkoutUrl;
      if (!checkoutUrl) throw new Error('Checkout indisponível.');
      window.location.href = checkoutUrl;
    } catch (error) {
      toast(error?.response?.data?.message || 'Não foi possível iniciar a contratação.', 'error');
    } finally {
      setBuyingOfferId(null);
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

        {Array.isArray(data.serviceOffers) && data.serviceOffers.length > 0 && (
          <section className="rounded-3xl border border-slate-200 bg-white/90 p-5 shadow-sm backdrop-blur">
            <div className="flex items-center justify-between gap-2">
              <div>
                <h2 className="text-xl font-semibold text-slate-900">Serviços</h2>
                <p className="text-sm text-slate-500">Contrate diretamente pelo checkout seguro.</p>
              </div>
            </div>
            <div className="mt-4 grid gap-3 md:grid-cols-2">
              {data.serviceOffers.map((offer) => (
                <div key={offer.id} className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                  <p className="font-semibold text-slate-900">{offer.title}</p>
                  <p className="mt-1 text-sm text-slate-600">{offer.description || 'Serviço personalizado.'}</p>
                  <p className="mt-2 text-lg font-bold text-slate-900">R$ {Number(offer.price || 0).toFixed(2)}</p>
                  <Button
                    className="mt-3 w-full"
                    onClick={() => {
                      setSelectedOffer(offer);
                      setCheckoutOpen(true);
                    }}
                  >
                    Contratar
                  </Button>
                </div>
              ))}
            </div>
          </section>
        )}

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

      <Modal open={checkoutOpen} onClose={() => setCheckoutOpen(false)} title={`Contratar ${selectedOffer?.title || 'serviço'}`} size="md">
        <form onSubmit={buyOffer} className="space-y-3">
          <Input label="Nome" value={checkoutForm.buyerName} onChange={(e) => setCheckoutForm((p) => ({ ...p, buyerName: e.target.value }))} required />
          <Input label="E-mail" type="email" value={checkoutForm.buyerEmail} onChange={(e) => setCheckoutForm((p) => ({ ...p, buyerEmail: e.target.value }))} required />
          <Input label="Telefone" value={checkoutForm.buyerPhone} onChange={(e) => setCheckoutForm((p) => ({ ...p, buyerPhone: e.target.value }))} />
          <Button type="submit" className="w-full" loading={buyingOfferId === selectedOffer?.id}>Ir para pagamento</Button>
        </form>
      </Modal>
    </div>
  );
}
