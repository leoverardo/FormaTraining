export function PageContainer({ children, className = '', size = 'default' }) {
  const maxWidth =
    size === 'narrow' ? 'max-w-4xl'
      : size === 'wide' ? 'max-w-screen-2xl'
        : size === 'full' ? 'max-w-none'
          : 'max-w-7xl';
  return (
    <section className={`w-full ${maxWidth} mx-auto ${className}`}>
      {children}
    </section>
  );
}

