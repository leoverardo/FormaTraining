import { FormField } from './FormField';

export function UploadField({ label, helper, children }) {
  return <FormField label={label} helper={helper}>{children}</FormField>;
}
