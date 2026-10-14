import { Heart, MessageCircle, Bookmark, Share2 } from 'lucide-react';

export function PostActions({ state, onToggleLike, onToggleSave, compact = false }) {
  const btnClass = compact ? 'text-xs px-2 py-1' : 'text-sm px-3 py-1.5';
  return (
    <div className="flex flex-wrap items-center gap-2">
      <button onClick={onToggleLike} className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 ${state.isLiked ? 'text-[var(--status-danger-text)] bg-[var(--status-danger-bg)] border-[var(--status-danger-bg)]' : 'text-slate-600 hover:bg-[var(--surface-muted)]'}`}><Heart size={14} />{state.likesCount}</button>
      <button className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-[var(--surface-muted)]`}><MessageCircle size={14} />{state.commentsCount}</button>
      <button onClick={onToggleSave} className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 ${state.isSaved ? 'text-[var(--link)] bg-[var(--primary-soft)] border-[var(--primary-soft)]' : 'text-slate-600 hover:bg-[var(--surface-muted)]'}`}><Bookmark size={14} />Salvar</button>
      <button className={`${btnClass} inline-flex items-center gap-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-[var(--surface-muted)]`}><Share2 size={14} />Compartilhar</button>
    </div>
  );
}


