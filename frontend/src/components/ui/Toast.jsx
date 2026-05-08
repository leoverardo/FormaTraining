import { createContext, useContext, useState, useCallback } from 'react';
import { CheckCircle, XCircle, X } from 'lucide-react';

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const addToast = useCallback((message, type = 'success') => {
    const id = Date.now();
    setToasts(prev => [...prev, { id, message, type }]);
    setTimeout(() => setToasts(prev => prev.filter(t => t.id !== id)), 4000);
  }, []);

  const removeToast = useCallback((id) => setToasts(prev => prev.filter(t => t.id !== id)), []);

  return (
    <ToastContext.Provider value={{ toast: addToast }}>
      {children}
      <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
        {toasts.map(t => (
          <div
            key={t.id}
            className={`flex items-center gap-3 px-4 py-3 rounded-2xl shadow-[0_12px_30px_rgba(15,23,42,0.2)] text-white text-sm max-w-sm border backdrop-blur-sm ${t.type === 'success' ? 'bg-emerald-600/95 border-emerald-400/50' : 'bg-red-600/95 border-red-400/50'}`}
          >
            {t.type === 'success' ? <CheckCircle size={18} /> : <XCircle size={18} />}
            <span className="flex-1">{t.message}</span>
            <button onClick={() => removeToast(t.id)} className="opacity-75 hover:opacity-100" aria-label="Fechar notificação"><X size={16} /></button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
};

