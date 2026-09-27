export function SuccessCheck({ size = 64, className = '', label = 'Concluído' }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 64 64"
      fill="none"
      className={className}
      role="img"
      aria-label={label}
    >
      <circle cx="32" cy="32" r="28" stroke="#10B981" strokeWidth="4" className="uc-check-circle" />
      <path
        d="M21 33.5 L28.5 41 L44 24"
        stroke="#10B981"
        strokeWidth="5"
        strokeLinecap="round"
        strokeLinejoin="round"
        className="uc-check-mark"
      />
    </svg>
  );
}
