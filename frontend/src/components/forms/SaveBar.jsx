import { Button } from '../ui/Button';

export function SaveBar({ saving, onSave, label = 'Salvar alteracoes', dirty = true }) {
  return (
    <div className="sticky bottom-3 z-20 rounded-[var(--radius-card)] border border-[var(--border)] bg-[var(--surface)]/95 p-3 shadow-[var(--shadow-soft)] backdrop-blur">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-600">{dirty ? 'Existem alteracoes prontas para salvar.' : 'Tudo salvo.'}</p>
        <Button type="button" onClick={onSave} loading={saving} disabled={!dirty}>{label}</Button>
      </div>
    </div>
  );
}
