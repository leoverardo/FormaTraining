import { Button } from '../ui/Button';
import { MessageCircle, PlayCircle } from 'lucide-react';

export function PublicCtaSection({ onPrimaryClick, onSecondaryClick, hasWhatsapp, trainerName }) {
  return (
    <section className="rounded-[var(--radius-card)] border border-slate-200 bg-[var(--brand-ink)] p-7 text-white sm:p-9">
      <h2 className="text-2xl font-bold sm:text-3xl">Pronto para comecar sua evolucao?</h2>
      <p className="mt-2 max-w-2xl text-sm text-white/90 sm:text-base">Entre em contato e descubra como a consultoria pode ser ajustada para sua rotina, objetivo e nivel de treino.</p>
      <div className="mt-5 flex flex-col gap-2 sm:flex-row">
        <Button onClick={onPrimaryClick} disabled={!hasWhatsapp} size="lg">
          <MessageCircle size={18} />
          {hasWhatsapp ? `Falar com ${trainerName || 'o personal'} no WhatsApp` : 'WhatsApp indisponivel'}
        </Button>
        <Button onClick={onSecondaryClick} size="lg" variant="outline" className="border-slate-300 bg-white text-slate-700 hover:bg-slate-50">
          <PlayCircle size={18} />
          Ver conteudos
        </Button>
      </div>
    </section>
  );
}
