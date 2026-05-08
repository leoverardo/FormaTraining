import { useMemo, useState } from 'react';
import { Avatar } from '../ui/Avatar';
import { Badge } from '../ui/Badge';
import { Card } from '../ui/Card';
import { PostMediaGrid } from './PostMediaGrid';
import { PostActions } from './PostActions';

export function FeedCard({ item }) {
  const [actionState, setActionState] = useState({
    likesCount: item.likesCount,
    commentsCount: item.commentsCount,
    isLiked: item.isLiked,
    isSaved: item.isSaved,
  });
  const createdLabel = useMemo(() => new Date(item.createdAt).toLocaleDateString('pt-BR'), [item.createdAt]);

  const toggleLike = () => {
    setActionState((prev) => ({ ...prev, isLiked: !prev.isLiked, likesCount: prev.likesCount + (prev.isLiked ? -1 : 1) }));
  };

  const toggleSave = () => {
    setActionState((prev) => ({ ...prev, isSaved: !prev.isSaved }));
  };

  return (
    <Card className="p-4 sm:p-5 space-y-3 transition-all duration-300 hover:shadow-[0_14px_34px_rgba(15,23,42,0.1)]">
      <div className="flex items-start gap-3">
        <Avatar src={item.authorAvatarUrl} name={item.authorName} className="h-11 w-11" />
        <div className="min-w-0 flex-1">
          <p className="font-semibold text-slate-900 text-sm">{item.authorName}</p>
          <p className="text-xs text-slate-500">{item.authorRole} • {createdLabel}</p>
        </div>
      </div>
      {item.title && <h3 className="text-base font-semibold text-slate-900">{item.title}</h3>}
      {item.text && <p className="text-sm leading-relaxed text-slate-600">{item.text}</p>}
      {!!item.media?.length && <PostMediaGrid media={item.media} />}
      {!!item.tags?.length && <div className="flex flex-wrap gap-2">{item.tags.map((tag) => <Badge key={tag} variant="gray">#{tag}</Badge>)}</div>}
      <PostActions state={actionState} onToggleLike={toggleLike} onToggleSave={toggleSave} />
    </Card>
  );
}


