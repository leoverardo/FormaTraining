import { Card } from './Card';

export function SectionCard({ title, description, children, className = '', action }) {
  return (
    <Card className={`p-5 sm:p-6 ${className}`}>
      {(title || description || action) && (
        <div className="mb-4 flex items-start justify-between gap-3">
          <div>
            {title && <h3 className="text-base font-semibold text-slate-900">{title}</h3>}
            {description && <p className="text-sm text-slate-500 mt-1">{description}</p>}
          </div>
          {action}
        </div>
      )}
      {children}
    </Card>
  );
}


