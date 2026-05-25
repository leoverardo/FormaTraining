import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { exerciseLibraryService } from '../../services/exerciseLibraryService';
import { useToast } from '../../components/ui/Toast';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Select } from '../../components/ui/Select';
import { Badge } from '../../components/ui/Badge';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { FormPage } from '../../components/forms/FormPage';
import { FormHeader } from '../../components/forms/FormHeader';
import { FormSection } from '../../components/forms/FormSection';
import { FormGrid } from '../../components/forms/FormGrid';
import { Dumbbell, Search, Download, ExternalLink, Library } from 'lucide-react';
import { useDomainLabels } from '../../i18n/domainLabels';
import { themeClasses } from '../../styles/themeClasses';

const levelOptions = [
  { value: 'all', label: 'Todos os niveis' },
  { value: 'Beginner', label: 'Iniciante' },
  { value: 'Intermediate', label: 'Intermediario' },
  { value: 'Advanced', label: 'Avancado' },
];

function normalized(value) {
  return String(value || '').toLowerCase().trim();
}

function Placeholder({ name }) {
  return (
    <div className="flex h-40 items-center justify-center bg-[linear-gradient(130deg,_#0f172a,_#0c4a6e,_#14532d)] text-cyan-100">
      <div className="text-center">
        <Dumbbell className="mx-auto" size={24} />
        <p className="mt-2 text-xs">{name || 'Exercicio base'}</p>
      </div>
    </div>
  );
}

export function ExerciseLibraryPage() {
  const { toast } = useToast();
  const { levelLabel } = useDomainLabels();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [items, setItems] = useState([]);
  const [importingId, setImportingId] = useState(null);
  const [search, setSearch] = useState('');
  const [muscleFilter, setMuscleFilter] = useState('all');
  const [levelFilter, setLevelFilter] = useState('all');

  const load = () => {
    setLoading(true);
    exerciseLibraryService.getAll().then((response) => setItems(response.data.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const muscleGroups = useMemo(() => {
    const unique = new Set(items.map((item) => item.muscleGroup).filter(Boolean));
    return Array.from(unique).sort();
  }, [items]);

  const filtered = useMemo(() => {
    return items.filter((item) => {
      const matchesSearch = !search || normalized(item.name).includes(normalized(search)) || normalized(item.description).includes(normalized(search));
      const matchesMuscle = muscleFilter === 'all' || normalized(item.muscleGroup) === normalized(muscleFilter);
      const matchesLevel = levelFilter === 'all' || item.level === levelFilter;
      return matchesSearch && matchesMuscle && matchesLevel;
    });
  }, [items, levelFilter, muscleFilter, search]);

  const handleImport = async (item) => {
    setImportingId(item.id);
    try {
      await exerciseLibraryService.duplicateToMyLibrary(item.id);
      toast('Exercicio adicionado aos seus exercicios!');
      setItems((prev) => prev.map((entry) => (entry.id === item.id ? { ...entry, alreadyAdded: true } : entry)));
    } catch (err) {
      toast(err.response?.data?.message || 'Erro ao adicionar exercicio', 'error');
    } finally {
      setImportingId(null);
    }
  };

  if (loading) return <LoadingState />;

  return (
    <FormPage>
      <FormHeader
        title="Biblioteca Base"
        description="Explore exercicios prontos e adicione a sua biblioteca pessoal."
        actions={<Button variant="outline" onClick={() => navigate('/trainer/exercises')}><ExternalLink size={15} />Ver meus exercicios</Button>}
      />

      <FormSection icon={Search} title="Filtros da biblioteca" description="Use filtros para descobrir exercicios prontos com mais rapidez.">
        <FormGrid cols="3">
          <Input placeholder="Buscar exercicio base" value={search} onChange={(e) => setSearch(e.target.value)} />
          <Select value={muscleFilter} onChange={(e) => setMuscleFilter(e.target.value)}>
            <option value="all">Todos os grupos</option>
            {muscleGroups.map((group) => <option key={group} value={group}>{group}</option>)}
          </Select>
          <Select value={levelFilter} onChange={(e) => setLevelFilter(e.target.value)}>
            {levelOptions.map((level) => <option key={level.value} value={level.value}>{level.label}</option>)}
          </Select>
        </FormGrid>
      </FormSection>

      {filtered.length === 0 ? (
        <EmptyState
          icon={Library}
          title="Nenhum exercicio encontrado"
          description="Ajuste os filtros ou tente outro termo de busca."
          action={<Button onClick={() => { setSearch(''); setMuscleFilter('all'); setLevelFilter('all'); }}>Limpar filtros</Button>}
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {filtered.map((item) => (
            <article key={item.id} className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-[0_8px_22px_rgba(15,23,42,0.07)] transition hover:-translate-y-0.5 hover:shadow-[0_14px_30px_rgba(15,23,42,0.12)]">
              {item.imageUrl ? <img src={item.imageUrl} alt={item.name} className="h-40 w-full object-cover" /> : <Placeholder name={item.name} />}
              <div className="p-4">
                <div className="mb-2 flex items-center justify-between gap-2">
                  <Badge variant="info">Base Forma Training</Badge>
                  <Badge variant="gray">{item.level ? levelLabel(item.level) : 'NÃ­vel livre'}</Badge>
                </div>
                <h3 className={`truncate text-sm font-semibold ${themeClasses.cardTitle}`}>{item.name}</h3>
                <p className="mt-0.5 text-xs text-slate-500">{item.muscleGroup || 'Grupo nao informado'}</p>
                <p className="mt-2 line-clamp-2 text-xs text-slate-600">{item.description || 'Exercicio da biblioteca base pronto para uso.'}</p>
                <div className="mt-4 flex gap-2 border-t border-slate-100 pt-3">
                  <Button
                    className="flex-1"
                    size="sm"
                    disabled={item.alreadyAdded}
                    loading={importingId === item.id}
                    onClick={() => handleImport(item)}
                  >
                    <Download size={14} />
                    {item.alreadyAdded ? 'Adicionado' : 'Adicionar'}
                  </Button>
                  <Button className="flex-1" size="sm" variant="outline" onClick={() => navigate('/trainer/exercises')}>
                    Ver detalhes
                  </Button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </FormPage>
  );
}

