export function LoadingState({ text = 'Carregando...' }) {
  return (
    <div className="flex items-center justify-center py-20">
      <div className="flex flex-col items-center gap-4">
        <div className="relative">
          <div className="w-10 h-10 rounded-full border-2 border-indigo-200" />
          <div className="w-10 h-10 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin absolute inset-0" />
        </div>
        <p className="text-sm text-slate-500 font-medium">{text}</p>
      </div>
    </div>
  );
}

