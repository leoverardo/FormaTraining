export function PublicFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="rounded-2xl border border-slate-200 bg-white px-5 py-5 text-center text-sm text-slate-600">
      <p className="font-semibold text-slate-800">FitPlatform</p>
      <p className="mt-1">Pagina profissional criada com FitPlatform</p>
      <p className="mt-1 text-xs text-slate-500">{year}</p>
    </footer>
  );
}
