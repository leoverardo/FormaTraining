import { Button } from '../ui/Button';
import { MessageCircle, PlayCircle } from 'lucide-react';

export function PublicCtaSection({ onPrimaryClick, onSecondaryClick, hasWhatsapp, trainerName }) {
  return (
    <section className="rounded-[2rem] border border-slate-200 bg-[radial-gradient(circle_at_top,_#bbf7d0,_transparent_40%),linear-gradient(135deg,_#0f172a,_#164e63_55%,_#14532d)] p-7 text-white shadow-[0_16px_34px_rgba(15,23,42,0.25)] sm:p-9">
      <h2 className="text-2xl font-bold sm:text-3xl">Pronto para comecar sua evolucao?</h2>
      <p className="mt-2 max-w-2xl text-sm text-slate-100 sm:text-base">Entre em contato e descubra como a consultoria pode ser ajustada para sua rotina, objetivo e nivel de treino.</p>
      <div className="mt-5 flex flex-col gap-2 sm:flex-row">
        <Button onClick={onPrimaryClick} disabled={!hasWhatsapp} size="lg" className="bg-emerald-500 text-slate-950 hover:bg-emerald-400 focus:ring-emerald-300">
          <MessageCircle size={18} />
          {hasWhatsapp ? `Falar com ${trainerName || 'o personal'} no WhatsApp` : 'WhatsApp indisponivel'}
        </Button>
        <Button onClick={onSecondaryClick} size="lg" variant="outline" className="border-white/60 bg-white/10 text-white hover:bg-white/20">
          <PlayCircle size={18} />
          Ver conteudos
        </Button>
      </div>
    </section>
  );
}
