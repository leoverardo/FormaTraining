import { Avatar } from '../ui/Avatar';
import { Button } from '../ui/Button';
import { MessageCircle, PlayCircle } from 'lucide-react';

export function PublicProfileHero({ profile, onPrimaryClick, onSecondaryClick, hasWhatsapp }) {
  return (
    <section className="relative overflow-hidden ds-card">
      <div className="relative h-64 sm:h-80 lg:h-96">
        {profile.bannerUrl ? (
          <img src={profile.bannerUrl} alt="Banner" className="h-full w-full object-cover" />
        ) : (
          <div className="h-full w-full bg-[var(--brand-ink)]" />
        )}
        <div className="absolute inset-0 bg-[var(--brand-ink)]/35" />
      </div>

      <div className="relative -mt-20 px-5 pb-7 sm:px-8 sm:pb-8 lg:px-10">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="flex flex-col gap-4">
            <Avatar
              src={profile.avatarUrl}
              name={profile.name}
              className="h-24 w-24 border-4 border-white/95 text-lg sm:h-28 sm:w-28"
            />
            <div className="ds-card border-slate-200/80 /95 backdrop-blur p-[var(--card-padding)]">
              <p className="text-xs font-semibold uppercase tracking-[0.15em] text-[var(--link)]">Perfil profissional</p>
              <h1 className="mt-1 font-semibold text-slate-950 uc-heading">{profile.name}</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-700 sm:text-base">{profile.headline}</p>
              {profile.bio ? <p className="mt-2 max-w-2xl text-sm text-slate-600">{profile.bio}</p> : null}
            </div>
            {!!profile.specialties?.length && (
              <div className="flex flex-wrap gap-2">
                {profile.specialties.slice(0, 6).map((tag) => (
                  <span key={tag} className="rounded-full border border-[var(--primary-soft)] bg-[var(--primary-soft)] px-2.5 py-1 text-xs font-semibold text-[var(--link)]">{tag}</span>
                ))}
              </div>
            )}
          </div>

          <div className="flex w-full max-w-md flex-col gap-2 sm:flex-row lg:w-auto lg:flex-col">
            <Button onClick={onPrimaryClick} disabled={!hasWhatsapp} size="lg" className="w-full">
              <MessageCircle size={18} />
              {hasWhatsapp ? 'Falar no WhatsApp' : 'WhatsApp indisponivel'}
            </Button>
            <Button onClick={onSecondaryClick} variant="outline" size="lg" className="w-full border-slate-300 bg-[var(--surface)] text-slate-700 hover:bg-[var(--surface-muted)]">
              <PlayCircle size={18} />
              Ver conteudos
            </Button>
          </div>
        </div>

        {!!profile.stats?.length && (
          <div className="mt-6 grid grid-cols-2 gap-3 md:grid-cols-4">
            {profile.stats.map((stat) => (
              <div key={stat.label} className="rounded-2xl border border-slate-200 bg-[var(--surface-muted)] px-4 py-3">
                <p className="text-xl font-semibold text-slate-950">{stat.value}</p>
                <p className="text-xs uppercase tracking-wide text-slate-500">{stat.label}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
