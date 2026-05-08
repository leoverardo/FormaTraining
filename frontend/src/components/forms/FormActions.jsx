import { Button } from '../ui/Button';

export function FormActions({ saving, onCancel, submitLabel = 'Salvar' }) {
  return (
    <div className="flex flex-col gap-2 sm:flex-row sm:justify-end">
      {onCancel ? <Button type="button" variant="outline" onClick={onCancel}>Cancelar</Button> : null}
      <Button type="submit" loading={saving}>{submitLabel}</Button>
    </div>
  );
}
