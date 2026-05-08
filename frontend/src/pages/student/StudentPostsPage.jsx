import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentAreaService } from '../../services/studentAreaService';
import { LoadingState } from '../../components/ui/LoadingState';
import { EmptyState } from '../../components/ui/EmptyState';
import { PageContainer } from '../../components/ui/PageContainer';
import { Tabs } from '../../components/ui/Tabs';
import { FeedCard } from '../../components/social/FeedCard';
import { mapPostToFeedItem, feedTabs } from '../../features/feed/feedAdapter';
import { FileText, ChevronLeft, Play } from 'lucide-react';

export function StudentPostsPage() {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState('all');

  useEffect(() => {
    studentAreaService.getPosts().then((r) => setPosts(r.data.data || [])).finally(() => setLoading(false));
  }, []);

  const feedItems = useMemo(() => posts.map((post) => mapPostToFeedItem(post, { role: 'Trainer' })), [posts]);

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
          title="Ainda não há conteúdos por aqui"
          description="Quando seu personal publicar dicas, aulas ou avisos, eles aparecerão neste feed."
        />
      ) : (
        <div className="space-y-3">{feedItems.map((item) => <FeedCard key={item.id} item={item} />)}</div>
      )}
    </PageContainer>
  );
}

export function StudentPostDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [post, setPost] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    studentAreaService.getPostById(id).then((r) => setPost(r.data.data)).catch(() => navigate('/student/posts')).finally(() => setLoading(false));
  }, [id, navigate]);

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


