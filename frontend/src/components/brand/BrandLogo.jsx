export function BrandLogo({
  className = '',
  showText = true,
  size = 'md',
  textClassName = '',
  imageClassName = '',
}) {
  const logoUrl = import.meta.env.VITE_BRAND_LOGO_URL;

  const sizeClasses = {
    sm: 'h-8 w-8 rounded-xl',
    md: 'h-9 w-9 rounded-2xl',
    lg: 'h-12 w-12 rounded-2xl',
  };

  const avatarClass = sizeClasses[size] || sizeClasses.md;

  return (
    <div className={`flex items-center gap-2 min-w-0 ${className}`}>
      <div className={`${avatarClass} overflow-hidden border border-slate-200 dark:border-white/10 bg-white/90 dark:bg-white/10 flex items-center justify-center shrink-0`}>
        {logoUrl ? (
          <img
            src={logoUrl}
            alt="Forma Training"
            className={`h-full w-full object-contain p-1 ${imageClassName}`}
          />
        ) : (
          <span className="text-xs font-black text-white bg-gradient-to-br from-indigo-600 to-cyan-500 h-full w-full flex items-center justify-center">
            F
          </span>
        )}
      </div>
      {showText ? (
        <span className={`font-semibold truncate ${textClassName}`} title="Forma Training">
          Forma Training
        </span>
      ) : null}
    </div>
  );
}
