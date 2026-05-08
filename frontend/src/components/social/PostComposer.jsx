import { Button } from '../ui/Button';
import { Card } from '../ui/Card';

export function PostComposer({ onCreate }) {
  return (
    <Card className="p-4 sm:p-5">
      <div className="rounded-xl border border-slate-200 bg-slate-50 p-3 text-sm text-slate-500 mb-3">Compartilhe algo com seus alunos...</div>
      <div className="flex flex-wrap gap-2">
        <Button size="sm" onClick={onCreate}>Criar conteúdo</Button>
        <Button size="sm" variant="outline" onClick={onCreate}>Adicionar mídia</Button>
      </div>
    </Card>
  );
}


