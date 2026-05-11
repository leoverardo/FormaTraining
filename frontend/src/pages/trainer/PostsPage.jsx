import { useEffect, useMemo, useState } from 'react';
import { postService } from '../../services/postService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Select } from '../../components/ui/Select';
import { Badge } from '../../components/ui/Badge';
import { Modal } from '../../components/ui/Modal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { ImageUpload } from '../../components/ui/ImageUpload';
import { VideoUpload } from '../../components/ui/VideoUpload';
import { PageContainer } from '../../components/ui/PageContainer';
import { FeedCard } from '../../components/social/FeedCard';
import { Tabs } from '../../components/ui/Tabs';
import { MediaCategory } from '../../services/uploadService';
import { mapPostToFeedItem, feedTabs } from '../../features/feed/feedAdapter';
import { FileText, Plus, Pencil, Trash2 } from 'lucide-react';

const empty = { title: '', description: '', imageUrl: '', videoUrl: '', status: 2 };
const emptyMedia = { coverMediaId: null, videoMediaId: null };
const statusConfig = { Published: { label: 'Publicado', badge: 'success' }, Draft: { label: 'Rascunho', badge: 'warning' }, Inactive: { label: 'Inativo', badge: 'gray' } };

export function PostsPage() {
  const { toast } = useToast();
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editPost, setEditPost] = useState(null);
  const [form, setForm] = useState(empty);
  const [mediaIds, setMediaIds] = useState(emptyMedia);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [tab, setTab] = useState('all');

  const load = () => {
    setLoading(true);
    postService.getAll().then((r) => setPosts(r.data.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openEdit = (p) => {
    setEditPost(p);
    const sm = { Published: 1, Draft: 2, Inactive: 3 };
    setForm({ title: p.title, description: p.description || '', imageUrl: p.imageUrl || '', videoUrl: p.videoUrl || '', status: sm[p.status] || 2 });
    setMediaIds({ coverMediaId: p.coverMediaId || null, videoMediaId: p.videoMediaId || null });
    setModalOpen(true);
  };

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = { ...form, ...mediaIds };
      if (editPost) {
        await postService.update(editPost.id, payload);
        toast('Post atualizado!');
      } else {
        await postService.create(payload);
        toast('Post criado!');
      }
      setModalOpen(false);
      load();
    } catch (err) {
      toast(err.response?.data?.message || 'Erro', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await postService.delete(deleteTarget.id);
      toast('Post removido.');
      setDeleteTarget(null);
      load();
    } catch {
      toast('Erro', 'error');
    } finally {
      setDeleting(false);
    }
  };

  const feedItems = useMemo(() => posts.map((post) => mapPostToFeedItem(post)), [posts]);

  if (loading) return <LoadingState />;

  return (
    <PageContainer className="space-y-5">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Conteúdos</h1>
          <p className="text-sm text-slate-500 mt-1">{posts.length} publicações</p>
        </div>
        <div className="flex items-center gap-2">
          <Tabs tabs={feedTabs} value={tab} onChange={setTab} />
          <Button onClick={() => { setEditPost(null); setForm(empty); setModalOpen(true); }}><Plus size={16} />Novo post</Button>
        </div>
      </div>

      {posts.length === 0 ? (
        <EmptyState icon={FileText} title="Nenhum conteúdo" description="Crie posts e conteúdos para seus alunos." action={<Button onClick={() => { setEditPost(null); setForm(empty); setModalOpen(true); }}><Plus size={16} />Novo post</Button>} />
      ) : (
        <div className="space-y-3">
          {posts.map((post, idx) => {
            const cfg = statusConfig[post.status] || statusConfig.Draft;
            const item = feedItems[idx];
            return (
              <div key={post.id ?? post.postId ?? item?.id ?? `post-${idx}`} className="space-y-2 rounded-2xl border border-slate-200 bg-white p-3">
                <div className="flex items-center justify-between">
                  <Badge variant={cfg.badge}>{cfg.label}</Badge>
                  <div className="flex gap-1">
                    <button onClick={() => openEdit(post)} className="px-3 py-1.5 text-xs text-blue-600 hover:bg-blue-50 rounded-lg"><Pencil size={14} className="inline mr-1" />Editar</button>
                    <button onClick={() => setDeleteTarget(post)} className="px-3 py-1.5 text-xs text-red-500 hover:bg-red-50 rounded-lg"><Trash2 size={14} className="inline mr-1" />Excluir</button>
                  </div>
                </div>
                <FeedCard item={item} />
              </div>
            );
          })}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editPost ? 'Editar post' : 'Novo post'} size="lg">
        <form onSubmit={handleSave} className="space-y-4">
          <Input label="Título" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} required />
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Descrição</label>
            <textarea className="w-full px-3 py-2 border border-gray-300 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" rows={4} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} />
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <ImageUpload label="Imagem de capa" category={MediaCategory.PostImage} currentUrl={form.imageUrl} currentMediaId={mediaIds.coverMediaId} onUploaded={(m) => { setMediaIds((p) => ({ ...p, coverMediaId: m.id })); setForm((p) => ({ ...p, imageUrl: m.url })); }} onRemoved={() => { setMediaIds((p) => ({ ...p, coverMediaId: null })); setForm((p) => ({ ...p, imageUrl: '' })); }} />
            <VideoUpload label="Vídeo da aula" category={MediaCategory.PostVideo} currentUrl={form.videoUrl} onUploaded={(m) => { setMediaIds((p) => ({ ...p, videoMediaId: m.id })); setForm((p) => ({ ...p, videoUrl: m.url })); }} onRemoved={() => { setMediaIds((p) => ({ ...p, videoMediaId: null })); setForm((p) => ({ ...p, videoUrl: '' })); }} />
          </div>
          <Select label="Status" value={form.status} onChange={(e) => setForm((p) => ({ ...p, status: parseInt(e.target.value, 10) }))}>
            <option value={1}>Publicado</option>
            <option value={2}>Rascunho</option>
            <option value={3}>Inativo</option>
          </Select>
          <div className="flex gap-3 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)} className="flex-1">Cancelar</Button>
            <Button type="submit" loading={saving} className="flex-1">Salvar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)} onConfirm={handleDelete} loading={deleting} title="Excluir post" description={`Deseja excluir "${deleteTarget?.title}"?`} />
    </PageContainer>
  );
}

