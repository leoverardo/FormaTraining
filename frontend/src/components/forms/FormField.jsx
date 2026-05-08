import { HelperText } from './HelperText';
import { FieldError } from './FieldError';

export function FormField({ label, required, helper, error, children }) {
  return (
    <div className="space-y-1.5">
      {label ? (
        <label className="block text-sm font-semibold text-slate-700">
          {label}
          {required ? <span className="ml-1 text-cyan-600">*</span> : null}
        </label>
      ) : null}
      {children}
      {!error ? <HelperText>{helper}</HelperText> : null}
      <FieldError>{error}</FieldError>
    </div>
  );
}
