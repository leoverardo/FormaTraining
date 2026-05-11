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
  description = 'JPG, PNG ou WebP · máx. 5 MB',
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
      toast('Tipo de arquivo não permitido. Use JPG, PNG ou WebP.', 'error');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      toast('Imagem muito grande. Máximo permitido: 5 MB.', 'error');
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
      {label && <label className="block text-sm font-medium text-gray-700">{label}</label>}

      {preview ? (
        <div className="relative inline-block group">
          <img src={preview} alt="preview" className="w-32 h-32 object-cover rounded-2xl border border-gray-200" />
          {uploading && (
            <div className="absolute inset-0 bg-black/40 flex items-center justify-center rounded-2xl">
              <Loader2 size={24} className="text-white animate-spin" />
            </div>
          )}
          {!uploading && !disabled && (
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 flex items-end justify-center rounded-2xl transition-all pb-2 gap-2 opacity-0 group-hover:opacity-100">
              <button type="button" onClick={() => inputRef.current?.click()}
                className="bg-white text-gray-700 text-xs px-2 py-1 rounded-lg shadow font-medium hover:bg-gray-50">
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
            disabled || uploading ? 'opacity-50 cursor-not-allowed border-gray-200' :
            'border-gray-300 hover:border-indigo-400 hover:bg-indigo-50/30'
          }`}
        >
          {uploading ? (
            <div className="flex flex-col items-center gap-2">
              <Loader2 size={28} className="text-indigo-500 animate-spin" />
              <p className="text-sm text-gray-500">Enviando...</p>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-2">
              <div className="p-3 bg-gray-100 rounded-xl">
                <ImageIcon size={22} className="text-gray-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">
                  <span className="text-indigo-600">Clique para enviar</span> ou arraste aqui
                </p>
                <p className="text-xs text-gray-400 mt-0.5">{description}</p>
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

