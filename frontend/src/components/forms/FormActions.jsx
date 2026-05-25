import { Button } from '../ui/Button';
import { useI18n } from '../../i18n';

export function FormActions({ saving, onCancel, submitLabel = 'Salvar' }) {
  const { t } = useI18n();
  return (
    <div className="flex flex-col gap-2 sm:flex-row sm:justify-end">
      {onCancel ? <Button type="button" variant="outline" onClick={onCancel}>{t('common.cancel')}</Button> : null}
      <Button type="submit" loading={saving}>{submitLabel}</Button>
    </div>
  );
}
