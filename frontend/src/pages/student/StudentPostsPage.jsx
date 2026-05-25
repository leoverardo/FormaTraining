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
import { useI18n } from '../../i18n';
import { FileText, ChevronLeft, Play } from 'lucide-react';

export function StudentPostsPage() {
  const { user } = useAuth();
  const { t } = useI18n();
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
          setError(t('student.posts.noTrainerLinked'));
          return;
        }

        setError(t('student.posts.loadError'));
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [t, user?.hasActiveTrainerLink]);

  const tabs = feedTabs.map((item) => ({ value: item.value, label: t(`trainer.feedFilters.${item.value}`) }));
  const feedItems = useMemo(() => posts.map((post) => mapPostToFeedItem(post, { role: 'Trainer' })), [posts]);
  const isBlockedState = !user?.hasActiveTrainerLink || !!error;

  if (loading) return <LoadingState />;

  return (
    <PageContainer className="space-y-4">
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('nav.contents')}</h1>
        <Tabs tabs={tabs} value={tab} onChange={setTab} />
      </div>

      {feedItems.length === 0 ? (
        <EmptyState
          icon={FileText}
          title={isBlockedState ? t('student.posts.lockedTitle') : t('student.posts.emptyTitle')}
          description={isBlockedState ? (error || t('student.posts.lockedDescription')) : t('student.posts.emptyDescription')}
          action={isBlockedState ? <Button onClick={() => navigate('/explore/trainers')}>{t('student.workouts.exploreTrainers')}</Button> : null}
        />
      ) : (
        <div className="space-y-3">{feedItems.map((item, index) => <FeedCard key={item.id ?? item.postId ?? item.relatedEntityId ?? `feed-${index}`} item={item} />)}</div>
      )}
    </PageContainer>
  );
}

export function StudentPostDetailPage() {
  const { user } = useAuth();
  const { t } = useI18n();
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
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-slate-500 hover:text-slate-700 dark:text-slate-300 dark:hover:text-slate-100">
        <ChevronLeft size={18} />{t('common.back')}
      </button>
      <article className="bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-white/10 overflow-hidden">
        {post.imageUrl && <img src={post.imageUrl} alt={post.title} className="w-full h-56 object-cover" />}
        <div className="p-6">
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{post.title}</h1>
          <p className="text-slate-400 dark:text-slate-500 text-xs mt-1 mb-4">{new Date(post.createdAt).toLocaleDateString('pt-BR')}</p>
          {post.description && <p className="text-slate-600 dark:text-slate-300 text-sm leading-relaxed whitespace-pre-wrap">{post.description}</p>}
          {post.videoUrl && (
            <a href={post.videoUrl} target="_blank" rel="noopener noreferrer" className="mt-5 inline-flex items-center gap-2 rounded-xl border border-red-100 dark:border-red-900/30 bg-red-50 dark:bg-red-950/30 px-3 py-2 text-sm text-red-700 dark:text-red-300 font-medium">
              <Play size={16} />{t('student.posts.watchVideo')}
            </a>
          )}
        </div>
      </article>
    </PageContainer>
  );
}
