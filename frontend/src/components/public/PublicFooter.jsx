import { BrandLogo } from '../brand/BrandLogo';

export function PublicFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="rounded-2xl border border-slate-200 bg-white px-5 py-5 text-center text-sm text-slate-600">
      <div className="flex justify-center">
        <BrandLogo size="xs" />
      </div>
      <p className="mt-2">Página profissional criada com UpCoach</p>
      <p className="mt-1 text-xs text-slate-500">{year}</p>
    </footer>
  );
}
