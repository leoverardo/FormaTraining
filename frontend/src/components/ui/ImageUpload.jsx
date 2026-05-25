import { useRef, useState } from 'react';
import { uploadService } from '../../services/uploadService';
import { useToast } from './Toast';
import { X, Image as ImageIcon, Loader2 } from 'lucide-react';

export function ImageUpload({
  category,
  currentUrl,
  onUploaded,
  onRemoved,
  label = 'Imagem',
  description = 'JPG, PNG ou WebP Â· mÃ¡x. 5 MB',
  disabled = false,
  studentId,
  isPublic = false,
  className = '',
}) {
  const { toast } = useToast();
  const inputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState(currentUrl || null);

  const handleFile = async (file) => {
    if (!file) return;

    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type)) {
      toast('Tipo de arquivo nÃ£o permitido. Use JPG, PNG ou WebP.', 'error');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      toast('Imagem muito grande. MÃ¡ximo permitido: 5 MB.', 'error');
      return;
    }

    // Instant local preview
    const localUrl = URL.createObjectURL(file);
    setPreview(localUrl);
    setUploading(true);

    try {
      const res = await uploadService.upload(file, category, { studentId, isPublic });
      const media = res.data.data;
      setPreview(media.url);
      onUploaded?.(media);
      toast('Imagem enviada com sucesso!');
    } catch (err) {
      setPreview(currentUrl || null);
      toast(err.response?.data?.message || 'Erro ao enviar imagem.', 'error');
    } finally {
      setUploading(false);
    }
  };

  const handleDrop = (e) => {
    e.preventDefault();
    if (disabled || uploading) return;
    const file = e.dataTransfer.files?.[0];
    if (file) handleFile(file);
  };

  const handleRemove = () => {
    setPreview(null);
    onRemoved?.();
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <div className={`space-y-1 ${className}`}>
      {label && <label className="block text-sm font-medium text-slate-700 dark:text-slate-200">{label}</label>}

      {preview ? (
        <div className="relative inline-block group">
          <img src={preview} alt="preview" className="h-32 w-32 rounded-2xl border border-slate-200 object-cover dark:border-white/10" />
          {uploading && (
            <div className="absolute inset-0 bg-black/40 flex items-center justify-center rounded-2xl">
              <Loader2 size={24} className="text-white animate-spin" />
            </div>
          )}
          {!uploading && !disabled && (
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 flex items-end justify-center rounded-2xl transition-all pb-2 gap-2 opacity-0 group-hover:opacity-100">
              <button type="button" onClick={() => inputRef.current?.click()}
                className="rounded-lg bg-white px-2 py-1 text-xs font-medium text-slate-700 shadow hover:bg-slate-50 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800">
                Trocar
              </button>
              <button type="button" onClick={handleRemove}
                className="bg-red-500 text-white text-xs px-2 py-1 rounded-lg shadow font-medium hover:bg-red-600">
                <X size={12} />
              </button>
            </div>
          )}
        </div>
      ) : (
        <div
          onClick={() => !disabled && !uploading && inputRef.current?.click()}
          onDrop={handleDrop}
          onDragOver={(e) => e.preventDefault()}
          className={`border-2 border-dashed rounded-2xl px-4 py-8 text-center cursor-pointer transition-all ${
            disabled || uploading ? 'cursor-not-allowed border-slate-200 opacity-50 dark:border-white/10' :
            'border-slate-300 hover:border-indigo-400 hover:bg-indigo-50/30 dark:border-white/15 dark:hover:bg-indigo-500/10'
          }`}
        >
          {uploading ? (
            <div className="flex flex-col items-center gap-2">
              <Loader2 size={28} className="text-indigo-500 animate-spin" />
              <p className="text-sm text-slate-500 dark:text-slate-400">Enviando...</p>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-2">
              <div className="rounded-xl bg-slate-100 p-3 dark:bg-slate-800">
                <ImageIcon size={22} className="text-slate-400 dark:text-slate-300" />
              </div>
              <div>
                <p className="text-sm font-medium text-slate-700 dark:text-slate-200">
                  <span className="text-indigo-600">Clique para enviar</span> ou arraste aqui
                </p>
                <p className="mt-0.5 text-xs text-slate-400 dark:text-slate-500">{description}</p>
              </div>
            </div>
          )}
        </div>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        className="hidden"
        disabled={disabled || uploading}
        onChange={(e) => handleFile(e.target.files?.[0])}
      />
    </div>
  );
}


