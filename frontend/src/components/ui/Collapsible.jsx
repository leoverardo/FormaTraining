/** Keep content mounted so opening, closing and mid-flight reversal can animate. */
export function Collapsible({ open, id, labelledBy, motion = true, children }) {
  return (
    <div
      id={id}
      role="region"
      aria-labelledby={labelledBy}
      aria-hidden={!open}
      inert={!open}
      className="uc-disclosure"
      data-open={open}
      data-motion={motion ? 'enabled' : 'reduced'}
    >
      <div className="uc-disclosure__clip">{children}</div>
    </div>
  );
}
