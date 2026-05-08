export function Textarea({ className = '', ...props }) {
  return <textarea className={`w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-slate-800 placeholder:text-slate-400 transition hover:border-slate-400 focus:border-cyan-300 focus:outline-none focus:ring-2 focus:ring-cyan-200 ${className}`} {...props} />;
}
