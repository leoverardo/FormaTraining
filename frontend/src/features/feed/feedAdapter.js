/**
 * Adapter layer for social feed.
 * Replace local interaction state with API endpoints when available.
 */
export function mapPostToFeedItem(post, authorFallback = {}) {
  const media = [];
  if (post.imageUrl) media.push({ type: 'image', url: post.imageUrl, alt: post.title || 'Imagem do post' });
  if (post.videoUrl) media.push({ type: 'video', url: post.videoUrl, alt: post.title || 'Vídeo do post' });

  const resolvedId =
    post.id
    ?? post.postId
    ?? post.notificationId
    ?? post.relatedEntityId
    ?? `${post.type || post.relatedEntityType || 'item'}-${post.createdAt || Date.now()}-${post.title || ''}`;

  return {
    id: String(resolvedId),
    authorName: authorFallback.name || 'Personal Trainer',
    authorAvatarUrl: authorFallback.avatarUrl || '',
    authorRole: authorFallback.role || 'Trainer',
    createdAt: post.createdAt || new Date().toISOString(),
    title: post.title || '',
    text: post.description || '',
    media,
    tags: post.tags || [],
    likesCount: post.likesCount || 0,
    commentsCount: post.commentsCount || 0,
    isLiked: false,
    isSaved: false,
    visibility: post.status || 'Published',
  };
}

export const feedTabs = [
  { value: 'all', label: 'Todos' },
  { value: 'workout', label: 'Treinos' },
  { value: 'tips', label: 'Dicas' },
  { value: 'progress', label: 'Evolução' },
  { value: 'class', label: 'Aulas' },
  { value: 'notice', label: 'Avisos' },
];

export const mockFeedFallback = [
  {
    id: 'mock-1',
    authorName: 'Forma Training Coach',
    authorAvatarUrl: '',
    authorRole: 'Trainer',
    createdAt: new Date().toISOString(),
    title: 'Bem-vindo ao seu novo feed',
    text: 'Aqui você verá treinos, dicas e avisos publicados pelo seu personal.',
    media: [],
    tags: ['dicas'],
    likesCount: 9,
    commentsCount: 2,
    isLiked: false,
    isSaved: false,
    visibility: 'Published',
  },
];



