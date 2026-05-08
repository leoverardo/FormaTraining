import { useEffect, useMemo, useState } from 'react';
import { exerciseService } from '../../services/exerciseService';
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
import { MediaCategory } from '../../services/uploadService';
import { FormPage } from '../../components/forms/FormPage';
import { FormHeader } from '../../components/forms/FormHeader';
import { FormSection } from '../../components/forms/FormSection';
import { FormGrid } from '../../components/forms/FormGrid';
import { FormField } from '../../components/forms/FormField';
import { Textarea } from '../../components/forms/Textarea';
import { PreviewCard } from '../../components/forms/PreviewCard';
import { FormActions } from '../../components/forms/FormActions';
import { Dumbbell, Plus, Pencil, Trash2, PlayCircle, Search, Download } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const empty = { name: '', muscleGroup: '', description: '', instructions: '', imageUrl: '', videoUrl: '', level: 1 };
const emptyMedia = { imageMediaId: null, videoMediaId: null };

const levelOptions = [
  { value: 1, label: 'Iniciante', apiValue: 'Beginner', badge: 'success' },
  { value: 2, label: 'Intermediario', apiValue: 'Intermediate', badge: 'warning' },
  { value: 3, label: 'Avancado', apiValue: 'Advanced', badge: 'danger' },
];

const muscleOptions = ['Peito', 'Costas', 'Ombros', 'Bracos', 'Pernas', 'Core', 'Gluteos', 'Panturrilhas'];

function levelFromApi(level) {
  return levelOptions.find((item) => item.apiValue === level) || levelOptions[0];
}

function normalized(value) {
  return String(value || '').toLowerCase().trim();
}

function CardMedia({ imageUrl, name }) {
  if (imageUrl) return <img src={imageUrl} alt={name} className="h-40 w-full object-cover" />;
  return (
    <div className="flex h-40 items-center justify-center bg-[linear-gradient(135deg,_#0f172a,_#0e7490,_#155e75)] text-cyan-100">
      <div className="text-center">
        <Dumbbell className="mx-auto" size={24} />
        <p className="mt-2 text-xs">Sem imagem</p>
      </div>
    </div>
  );
}

export function ExercisesPage() {
  const { toast } = useToast();
  const navigate = useNavigate();
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editEx, setEditEx] = useState(null);
  const [form, setForm] = useState(empty);
  const [mediaIds, setMediaIds] = useState(emptyMedia);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [search, setSearch] = useState('');
  const [muscleFilter, setMuscleFilter] = useState('all');
  const [levelFilter, setLevelFilter] = useState('all');

  const load = () => {
    setLoading(true);
    exerciseService.getAll().then((response) => setExercises(response.data.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setEditEx(null);
    setForm(empty);
    setMediaIds(emptyMedia);
    setModalOpen(true);
  };

  const openEdit = (exercise) => {
    setEditEx(exercise);
    setForm({
      name: exercise.name || '',
      muscleGroup: exercise.muscleGroup || '',
      description: exercise.description || '',
      instructions: exercise.instructions || '',
      imageUrl: exercise.imageUrl || '',
      videoUrl: exercise.videoUrl || '',
      level: levelFromApi(exercise.level).value,
    });
    setMediaIds({ imageMediaId: exercise.imageMediaId || null, videoMediaId: exercise.videoMediaId || null });
    setModalOpen(true);
  };

  const handleSave = async (event) => {
    event.preventDefault();
    setSaving(true);
    try {
      const payload = { ...form, ...mediaIds };
      if (editEx) {
        await exerciseService.update(editEx.id, payload);
        toast('Exercicio atualizado!');
      } else {
        await exerciseService.create(payload);
        toast('Exercicio criado!');
      }
      setModalOpen(false);
      load();
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao salvar exercicio', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await exerciseService.delete(deleteTarget.id);
      toast('Exercicio removido.');
      setDeleteTarget(null);
      load();
    } catch {
      toast('Erro ao remover exercicio', 'error');
    } finally {
      setDeleting(false);
    }
  };

  const filtered = useMemo(() => {
    return exercises.filter((exercise) => {
      const level = levelFromApi(exercise.level).value;
      const matchesSearch = !search || normalized(exercise.name).includes(normalized(search)) || normalized(exercise.description).includes(normalized(search));
      const matchesMuscle = muscleFilter === 'all' || normalized(exercise.muscleGroup) === normalized(muscleFilter);
      const matchesLevel = levelFilter === 'all' || String(level) === String(levelFilter);
      return matchesSearch && matchesMuscle && matchesLevel;
    });
  }, [exercises, levelFilter, muscleFilter, search]);

  if (loading) return <LoadingState />;

  return (
    <FormPage>
      <FormHeader
        title="Meus Exercicios"
        description="Gerencie os exercicios que voce usa nos treinos dos seus alunos."
        actions={<Button onClick={openCreate}><Plus size={16} />Novo exercicio</Button>}
      />

      <FormSection icon={Search} title="Filtros" description="Encontre exercicios por nome, grupo muscular e nivel.">
        <FormGrid cols="3">
          <Input placeholder="Buscar exercicio" value={search} onChange={(e) => setSearch(e.target.value)} />
          <Select value={muscleFilter} onChange={(e) => setMuscleFilter(e.target.value)}>
            <option value="all">Todos os grupos</option>
            {muscleOptions.map((group) => <option key={group} value={group}>{group}</option>)}
          </Select>
          <Select value={levelFilter} onChange={(e) => setLevelFilter(e.target.value)}>
            <option value="all">Todos os niveis</option>
            {levelOptions.map((level) => <option key={level.value} value={level.value}>{level.label}</option>)}
          </Select>
        </FormGrid>
      </FormSection>

      {filtered.length === 0 ? (
        <EmptyState
          icon={Dumbbell}
          title="Voce ainda nao cadastrou exercicios"
          description="Crie seu primeiro exercicio ou importe da biblioteca base."
          action={<div className="flex flex-col gap-2 sm:flex-row"><Button onClick={openCreate}><Plus size={16} />Criar primeiro exercicio</Button><Button variant="outline" onClick={() => navigate('/trainer/library')}><Download size={16} />Importar da biblioteca base</Button></div>}
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {filtered.map((exercise) => {
            const level = levelFromApi(exercise.level);
            return (
              <article key={exercise.id} className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-[0_8px_22px_rgba(15,23,42,0.07)] transition hover:-translate-y-0.5 hover:shadow-[0_14px_30px_rgba(15,23,42,0.12)]">
                <CardMedia imageUrl={exercise.imageUrl} name={exercise.name} />
                <div className="p-4">
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <h3 className="truncate text-sm font-semibold text-slate-900">{exercise.name}</h3>
                      <p className="mt-0.5 text-xs text-slate-500">{exercise.muscleGroup || 'Grupo nao informado'}</p>
                    </div>
                    <Badge variant={level.badge}>{level.label}</Badge>
                  </div>
                  <p className="mt-2 line-clamp-2 text-xs text-slate-600">{exercise.description || 'Sem descricao cadastrada.'}</p>
                  <div className="mt-3 flex items-center gap-2 text-xs text-slate-500">
                    <PlayCircle size={14} className={exercise.videoUrl ? 'text-emerald-600' : 'text-slate-400'} />
                    {exercise.videoUrl ? 'Video disponivel' : 'Sem video'}
                  </div>
                  <div className="mt-4 flex gap-2 border-t border-slate-100 pt-3">
                    <button onClick={() => openEdit(exercise)} className="flex-1 rounded-lg bg-blue-50 px-3 py-1.5 text-xs font-semibold text-blue-700 transition hover:bg-blue-100"><Pencil size={13} className="mr-1 inline" />Editar</button>
                    <button onClick={() => setDeleteTarget(exercise)} className="flex-1 rounded-lg bg-rose-50 px-3 py-1.5 text-xs font-semibold text-rose-700 transition hover:bg-rose-100"><Trash2 size={13} className="mr-1 inline" />Excluir</button>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editEx ? 'Editar exercicio' : 'Novo exercicio'} size="xl">
        <form onSubmit={handleSave} className="space-y-4">
          <div className="grid gap-4 lg:grid-cols-[1.15fr_0.85fr]">
            <div className="space-y-4">
              <FormSection title="Dados basicos" description="Informacoes principais para identificar o exercicio.">
                <FormGrid>
                  <Input label="Nome" required value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} />
                  <Select label="Nivel" value={form.level} onChange={(e) => setForm((p) => ({ ...p, level: parseInt(e.target.value, 10) }))}>
                    {levelOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                  </Select>
                  <div className="md:col-span-2"><Input label="Grupo muscular" placeholder="Ex: Peito" value={form.muscleGroup} onChange={(e) => setForm((p) => ({ ...p, muscleGroup: e.target.value }))} /></div>
                  <div className="md:col-span-2">
                    <FormField label="Descricao">
                      <Textarea rows={3} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} />
                    </FormField>
                  </div>
                </FormGrid>
              </FormSection>

              <FormSection title="Instrucoes" description="Detalhes de execucao e observacoes para o aluno.">
                <FormField label="Instrucoes de execucao">
                  <Textarea rows={5} value={form.instructions} onChange={(e) => setForm((p) => ({ ...p, instructions: e.target.value }))} />
                </FormField>
              </FormSection>

              <FormSection title="Midia" description="Adicione imagem e video de demonstracao.">
                <FormGrid>
                  <ImageUpload
                    label="Imagem do exercicio"
                    category={MediaCategory.ExerciseImage}
                    currentUrl={form.imageUrl}
                    currentMediaId={mediaIds.imageMediaId}
                    onUploaded={(media) => { setMediaIds((p) => ({ ...p, imageMediaId: media.id })); setForm((p) => ({ ...p, imageUrl: media.url })); }}
                    onRemoved={() => { setMediaIds((p) => ({ ...p, imageMediaId: null })); setForm((p) => ({ ...p, imageUrl: '' })); }}
                  />
                  <VideoUpload
                    label="Video demonstrativo"
                    category={MediaCategory.ExerciseVideo}
                    currentUrl={form.videoUrl}
                    onUploaded={(media) => { setMediaIds((p) => ({ ...p, videoMediaId: media.id })); setForm((p) => ({ ...p, videoUrl: media.url })); }}
                    onRemoved={() => { setMediaIds((p) => ({ ...p, videoMediaId: null })); setForm((p) => ({ ...p, videoUrl: '' })); }}
                  />
                </FormGrid>
              </FormSection>
            </div>

            <div className="space-y-4 lg:sticky lg:top-3 lg:self-start">
              <PreviewCard title="Preview do exercicio" subtitle="Como o card aparece para o aluno.">
                <div className="overflow-hidden rounded-xl border border-slate-200">
                  <CardMedia imageUrl={form.imageUrl} name={form.name || 'Exercicio'} />
                  <div className="p-3">
                    <p className="text-sm font-semibold text-slate-900">{form.name || 'Nome do exercicio'}</p>
                    <p className="text-xs text-slate-500">{form.muscleGroup || 'Grupo muscular'}</p>
                    <p className="mt-2 line-clamp-3 text-xs text-slate-600">{form.description || 'Descricao do exercicio.'}</p>
                  </div>
                </div>
              </PreviewCard>
            </div>
          </div>
          <FormActions saving={saving} onCancel={() => setModalOpen(false)} submitLabel={editEx ? 'Salvar alteracoes' : 'Criar exercicio'} />
        </form>
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        loading={deleting}
        title="Excluir exercicio"
        description={`Deseja excluir "${deleteTarget?.name}"? Esta acao nao pode ser desfeita.`}
      />
    </FormPage>
  );
}
