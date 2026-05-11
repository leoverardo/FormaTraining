import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { useAuth } from '../../contexts/AuthContext';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { PageContainer } from '../../components/ui/PageContainer';
import { Tabs } from '../../components/ui/Tabs';
import { Button } from '../../components/ui/Button';
import { FeedCard } from '../../components/social/FeedCard';
import { mapPostToFeedItem, feedTabs } from '../../features/feed/feedAdapter';
import { FileText, ChevronLeft, Play } from 'lucide-react';

export function StudentPostsPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [tab, setTab] = useState('all');

  useEffect(() => {
    if (!user?.hasActiveTrainerLink) {
      setPosts([]);
      setLoading(false);
      return;
    }

    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const response = await studentAreaService.getPosts();
        setPosts(response.data.data || []);
      } catch (err) {
        if (err.response?.status === 403) {
          setError('Você ainda não possui um personal vinculado.');
          return;
        }

        setError('Não foi possível carregar os conteúdos agora.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [user?.hasActiveTrainerLink]);

  const feedItems = useMemo(() => posts.map((post) => mapPostToFeedItem(post, { role: 'Trainer' })), [posts]);
  const isBlockedState = !user?.hasActiveTrainerLink || !!error;

  if (loading) return <LoadingState />;

  return (
    <PageContainer className="space-y-4">
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <h1 className="text-2xl font-bold text-slate-900">Conteúdos</h1>
        <Tabs tabs={feedTabs} value={tab} onChange={setTab} />
      </div>

      {feedItems.length === 0 ? (
        <EmptyState
          icon={FileText}
          title={isBlockedState
            ? 'Essa área será liberada quando você estiver vinculado a um personal.'
            : 'Ainda não há conteúdos por aqui'}
          description={isBlockedState
            ? (error || 'Encontre um personal para liberar seus conteúdos privados.')
            : 'Quando seu personal publicar dicas, aulas ou avisos, eles aparecerão neste feed.'}
          action={isBlockedState
            ? <Button onClick={() => navigate('/explore/trainers')}>Explorar personais</Button>
            : null}
        />
      ) : (
        <div className="space-y-3">{feedItems.map((item, index) => <FeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} />)}</div>
      )}
    </PageContainer>
  );
}

export function StudentPostDetailPage() {
  const { user } = useAuth();
  const { id } = useParams();
  const navigate = useNavigate();
  const [post, setPost] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user?.hasActiveTrainerLink) {
      navigate('/explore', { replace: true });
      return;
    }

    const load = async () => {
      try {
        const response = await studentAreaService.getPostById(id);
        setPost(response.data.data);
      } catch {
        navigate('/student/posts');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id, navigate, user?.hasActiveTrainerLink]);

  if (loading) return <LoadingState />;
  if (!post) return null;

  return (
    <PageContainer className="space-y-4" size="narrow">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-slate-500 hover:text-slate-700">
        <ChevronLeft size={18} />Voltar
      </button>
      <article className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
        {post.imageUrl && <img src={post.imageUrl} alt={post.title} className="w-full h-56 object-cover" />}
        <div className="p-6">
          <h1 className="text-2xl font-bold text-slate-900">{post.title}</h1>
          <p className="text-slate-400 text-xs mt-1 mb-4">{new Date(post.createdAt).toLocaleDateString('pt-BR')}</p>
          {post.description && <p className="text-slate-600 text-sm leading-relaxed whitespace-pre-wrap">{post.description}</p>}
          {post.videoUrl && (
            <a href={post.videoUrl} target="_blank" rel="noopener noreferrer" className="mt-5 inline-flex items-center gap-2 rounded-xl border border-red-100 bg-red-50 px-3 py-2 text-sm text-red-700 font-medium">
              <Play size={16} />Assistir vídeo
            </a>
          )}
        </div>
      </article>
    </PageContainer>
  );
}
