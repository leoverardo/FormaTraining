import { FormField } from './FormField';
import { Input } from '../ui/Input';

export function ColorPickerField({ label, value, onChange }) {
  return (
    <FormField label={label}>
      <div className="flex items-center gap-2">
        <input type="color" value={value} onChange={(e) => onChange(e.target.value)} className="h-11 w-11 rounded-lg border border-slate-300 bg-white" />
        <Input value={value} onChange={(e) => onChange(e.target.value)} className="flex-1" />
      </div>
    </FormField>
  );
}
