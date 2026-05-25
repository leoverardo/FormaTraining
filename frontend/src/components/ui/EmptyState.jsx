export function EmptyState({ icon: Icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center rounded-2xl border border-dashed border-slate-300 dark:border-white/10 bg-white/75 dark:bg-slate-900">
      {Icon && <div className="p-4 bg-slate-100 dark:bg-white/10 rounded-2xl mb-4"><Icon size={30} className="text-slate-500 dark:text-slate-300" /></div>}
      <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-1">{title}</h3>
      {description && <p className="text-sm text-slate-500 dark:text-slate-400 mb-5 max-w-sm">{description}</p>}
      {action}
    </div>
  );
}

