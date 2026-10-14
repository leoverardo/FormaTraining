export function FormHeader({ title, description, actions }) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 className="uc-heading text-[var(--text)] uc-heading">{title}</h1>
        {description ? <p className="mt-3 uc-lead">{description}</p> : null}
      </div>
      {actions ? <div className="flex items-center gap-2">{actions}</div> : null}
    </div>
  );
}
