import { useMemo, useState } from 'react';
import { Avatar } from '../ui/Avatar';
import { Heart, MessageCircle, Bookmark, Share2, Image as ImageIcon } from 'lucide-react';

export function PublicFeedCard({ item, fallbackName, fallbackAvatar }) {
  const [liked, setLiked] = useState(Boolean(item.isLiked));
  const [saved, setSaved] = useState(Boolean(item.isSaved));
  const [likesCount, setLikesCount] = useState(Number(item.likesCount || 0));
  const createdLabel = useMemo(() => new Date(item.createdAt).toLocaleDateString('pt-BR'), [item.createdAt]);

  const media = item.media?.[0];
  const tags = (item.tags || []).filter(Boolean);

  const toggleLike = () => {
    setLikesCount((prev) => prev + (liked ? -1 : 1));
    setLiked((prev) => !prev);
  };

  return (
    <article className="rounded-3xl border border-slate-200 bg-white p-5 shadow-[0_10px_30px_rgba(15,23,42,0.07)] transition hover:-translate-y-0.5 hover:shadow-[0_18px_40px_rgba(15,23,42,0.12)]">
      <header className="flex items-center gap-3">
        <Avatar src={item.authorAvatarUrl || fallbackAvatar} name={item.authorName || fallbackName} className="h-11 w-11" />
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-slate-900">{item.authorName || fallbackName}</p>
          <p className="text-xs text-slate-500">{createdLabel}</p>
        </div>
      </header>

      {item.title ? <h3 className="mt-4 text-lg font-semibold text-slate-900">{item.title}</h3> : null}
      {item.text ? <p className="mt-2 text-sm leading-relaxed text-slate-600">{item.text}</p> : null}

      <div className="mt-4 overflow-hidden rounded-2xl border border-slate-200 bg-slate-100">
        {media?.type === 'video' ? (
          <video controls className="h-60 w-full object-cover" src={media.url} />
        ) : media?.url ? (
          <img src={media.url} alt={media.alt || 'Midia do post'} className="h-60 w-full object-cover" />
        ) : (
          <div className="flex h-52 items-center justify-center bg-[linear-gradient(120deg,_#0f172a,_#155e75,_#14532d)] text-cyan-50">
            <div className="text-center">
              <ImageIcon className="mx-auto" size={28} />
              <p className="mt-2 text-sm">Conteudo em destaque</p>
            </div>
          </div>
        )}
      </div>

      {!!tags.length && (
        <div className="mt-4 flex flex-wrap gap-2">
          {tags.map((tag) => (
            <span key={tag} className="rounded-full bg-cyan-50 px-2.5 py-1 text-xs font-semibold text-cyan-700">#{tag}</span>
          ))}
        </div>
      )}

      <div className="mt-4 flex items-center gap-2 text-sm text-slate-600">
        <button onClick={toggleLike} className="inline-flex items-center gap-1 rounded-lg px-3 py-1.5 transition hover:bg-slate-100">
          <Heart size={16} className={liked ? 'fill-rose-500 text-rose-500' : ''} />
          {likesCount}
        </button>
        <button className="inline-flex items-center gap-1 rounded-lg px-3 py-1.5 transition hover:bg-slate-100">
          <MessageCircle size={16} />
          {item.commentsCount || 0}
        </button>
        <button onClick={() => setSaved((prev) => !prev)} className="inline-flex items-center gap-1 rounded-lg px-3 py-1.5 transition hover:bg-slate-100">
          <Bookmark size={16} className={saved ? 'fill-slate-700 text-slate-700' : ''} />
          Salvar
        </button>
        <button className="inline-flex items-center gap-1 rounded-lg px-3 py-1.5 transition hover:bg-slate-100">
          <Share2 size={16} />
          Compartilhar
        </button>
      </div>
    </article>
  );
}
