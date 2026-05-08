import { Heart, MessageCircle, Bookmark, Share2 } from 'lucide-react';

export function PostActions({ state, onToggleLike, onToggleSave, compact = false }) {
  const btnClass = compact ? 'text-xs px-2 py-1' : 'text-sm px-3 py-1.5';
  return (
    <div className="flex flex-wrap items-center gap-2">
      <button onClick={onToggleLike} className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 ${state.isLiked ? 'text-rose-600 bg-rose-50 border-rose-100' : 'text-slate-600 hover:bg-slate-50'}`}><Heart size={14} />{state.likesCount}</button>
      <button className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50`}><MessageCircle size={14} />{state.commentsCount}</button>
      <button onClick={onToggleSave} className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 ${state.isSaved ? 'text-indigo-600 bg-indigo-50 border-indigo-100' : 'text-slate-600 hover:bg-slate-50'}`}><Bookmark size={14} />Salvar</button>
      <button className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50`}><Share2 size={14} />Compartilhar</button>
    </div>
  );
}


