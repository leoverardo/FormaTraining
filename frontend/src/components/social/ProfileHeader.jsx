import { Avatar } from '../ui/Avatar';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { SocialStats } from './SocialStats';

export function ProfileHeader({ profile, onPrimaryClick, onSecondaryClick }) {
  return (
    <section className="rounded-3xl overflow-hidden border border-slate-200 bg-white shadow-[0_12px_36px_rgba(15,23,42,0.09)]">
      <div className="h-40 sm:h-56 bg-gradient-to-r from-indigo-500 via-cyan-500 to-emerald-400 relative">
        {profile.bannerUrl && <img src={profile.bannerUrl} alt="Banner" className="absolute inset-0 w-full h-full object-cover opacity-55" />}
      </div>
      <div className="px-5 sm:px-8 pb-6">
        <div className="-mt-12 sm:-mt-14 mb-3"><Avatar src={profile.avatarUrl} name={profile.name} className="h-24 w-24 sm:h-28 sm:w-28 border-4 border-white shadow-md" /></div>
        <h1 className="text-2xl font-bold text-slate-900">{profile.name}</h1>
        <p className="text-sm text-slate-500 mt-1">{profile.headline}</p>
        {profile.bio && <p className="text-sm text-slate-600 mt-3 leading-relaxed">{profile.bio}</p>}
        <div className="mt-3 flex flex-wrap gap-2">{(profile.specialties || []).map((tag) => <Badge key={tag} variant="gray">{tag}</Badge>)}</div>
        <div className="mt-4 flex flex-wrap gap-2">
          {onPrimaryClick && <Button onClick={onPrimaryClick}>Falar no WhatsApp</Button>}
          {onSecondaryClick && <Button variant="outline" onClick={onSecondaryClick}>Ver conteúdos</Button>}
        </div>
        {!!profile.stats?.length && <div className="mt-5"><SocialStats items={profile.stats} /></div>}
      </div>
    </section>
  );
}


