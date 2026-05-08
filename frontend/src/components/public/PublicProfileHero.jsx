import { Avatar } from '../ui/Avatar';
import { Button } from '../ui/Button';
import { MessageCircle, PlayCircle } from 'lucide-react';

export function PublicProfileHero({ profile, onPrimaryClick, onSecondaryClick, hasWhatsapp }) {
  return (
    <section className="relative overflow-hidden rounded-[2rem] border border-slate-200 bg-white shadow-[0_22px_55px_rgba(15,23,42,0.14)]">
      <div className="relative h-64 sm:h-80 lg:h-96">
        {profile.bannerUrl ? (
          <img src={profile.bannerUrl} alt="Banner" className="h-full w-full object-cover" />
        ) : (
          <div className="h-full w-full bg-[radial-gradient(circle_at_top_right,_#bef264,_transparent_38%),radial-gradient(circle_at_bottom_left,_#67e8f9,_transparent_40%),linear-gradient(120deg,_#0f172a,_#1d4ed8_48%,_#0f766e)]" />
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950/70 via-slate-900/35 to-slate-900/15" />
      </div>

      <div className="relative -mt-20 px-5 pb-7 sm:px-8 sm:pb-8 lg:px-10">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="flex flex-col gap-4">
            <Avatar
              src={profile.avatarUrl}
              name={profile.name}
              className="h-24 w-24 border-4 border-white/95 text-lg shadow-xl sm:h-28 sm:w-28"
            />
            <div>
              <p className="text-sm font-medium uppercase tracking-[0.15em] text-cyan-100">Perfil profissional</p>
              <h1 className="mt-1 text-2xl font-bold text-white sm:text-4xl">{profile.name}</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-100 sm:text-base">{profile.headline}</p>
              {profile.bio ? <p className="mt-2 max-w-2xl text-sm text-slate-200/90">{profile.bio}</p> : null}
            </div>
            {!!profile.specialties?.length && (
              <div className="flex flex-wrap gap-2">
                {profile.specialties.slice(0, 6).map((tag) => (
                  <span key={tag} className="rounded-full border border-white/30 bg-white/15 px-2.5 py-1 text-xs font-semibold text-cyan-50 backdrop-blur">{tag}</span>
                ))}
              </div>
            )}
          </div>

          <div className="flex w-full max-w-md flex-col gap-2 sm:flex-row lg:w-auto lg:flex-col">
            <Button onClick={onPrimaryClick} disabled={!hasWhatsapp} size="lg" className="w-full bg-emerald-500 text-slate-950 hover:bg-emerald-400 focus:ring-emerald-300">
              <MessageCircle size={18} />
              {hasWhatsapp ? 'Falar no WhatsApp' : 'WhatsApp indisponivel'}
            </Button>
            <Button onClick={onSecondaryClick} variant="outline" size="lg" className="w-full border-white/60 bg-white/10 text-white backdrop-blur hover:bg-white/20">
              <PlayCircle size={18} />
              Ver conteudos
            </Button>
          </div>
        </div>

        {!!profile.stats?.length && (
          <div className="mt-6 grid grid-cols-2 gap-3 md:grid-cols-4">
            {profile.stats.map((stat) => (
              <div key={stat.label} className="rounded-2xl border border-white/25 bg-white/10 px-4 py-3 backdrop-blur">
                <p className="text-xl font-bold text-white">{stat.value}</p>
                <p className="text-xs uppercase tracking-wide text-slate-200">{stat.label}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
