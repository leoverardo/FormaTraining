export function PageContainer({ children, className = '', size = 'default' }) {
  const maxWidth =
    size === 'narrow' ? 'ds-container--narrow'
      : size === 'wide' ? 'ds-container--wide'
        : size === 'full' ? 'w-full px-4 sm:px-6'
          : 'ds-container';
  return (
    <section className={`${maxWidth} ${className}`}>
      {children}
    </section>
  );
}

