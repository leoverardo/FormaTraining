import { useRef, useState } from 'react';
import { uploadService } from '../../services/uploadService';
import { useToast } from './Toast';
import { Video, X, Loader2, Play } from 'lucide-react';

export function VideoUpload({
  category,
  currentUrl,
  onUploaded,
  onRemoved,
  label = 'Vídeo',
  description = 'MP4, WebM · máx. 100 MB',
  disabled = false,
  className = '',
}) {
  const { toast } = useToast();
  const inputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState(currentUrl || null);
  const [progress, setProgress] = useState(0);

  const handleFile = async (file) => {
    if (!file) return;

    const allowed = ['video/mp4', 'video/webm', 'video/quicktime'];
    if (!allowed.includes(file.type)) {
      toast('Tipo de vídeo não permitido. Use MP4, WebM ou MOV.', 'error');
      return;
    }
    if (file.size > 100 * 1024 * 1024) {
      toast('Vídeo muito grande. Máximo: 100 MB.', 'error');
      return;
    }

    const localUrl = URL.createObjectURL(file);
    setPreview(localUrl);
    setUploading(true);
    setProgress(0);

    try {
      const res = await uploadService.upload(file, category);
      const media = res.data.data;
      setPreview(media.url);
      onUploaded?.(media);
      toast('Vídeo enviado com sucesso!');
    } catch (err) {
      setPreview(currentUrl || null);
      toast(err.response?.data?.message || 'Erro ao enviar vídeo.', 'error');
    } finally {
      setUploading(false);
    }
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
        <div className="relative rounded-2xl overflow-hidden border border-gray-200 bg-black">
          <video src={preview} className="w-full h-40 object-contain" controls={!uploading} />
          {uploading && (
            <div className="absolute inset-0 bg-black/60 flex flex-col items-center justify-center gap-2">
              <Loader2 size={28} className="text-white animate-spin" />
              <p className="text-white text-sm">Enviando vídeo...</p>
            </div>
          )}
          {!uploading && !disabled && (
            <button type="button" onClick={handleRemove}
              className="absolute top-2 right-2 bg-red-500 text-white p-1.5 rounded-lg hover:bg-red-600 transition-colors">
              <X size={14} />
            </button>
          )}
        </div>
      ) : (
        <div
          onClick={() => !disabled && !uploading && inputRef.current?.click()}
          onDrop={(e) => { e.preventDefault(); if (!disabled && !uploading) handleFile(e.dataTransfer.files?.[0]); }}
          onDragOver={(e) => e.preventDefault()}
          className={`border-2 border-dashed rounded-2xl px-4 py-8 text-center cursor-pointer transition-all ${
            disabled || uploading ? 'opacity-50 cursor-not-allowed border-gray-200' :
            'border-gray-300 hover:border-indigo-400 hover:bg-indigo-50/30'
          }`}
        >
          <div className="flex flex-col items-center gap-2">
            <div className="p-3 bg-gray-100 rounded-xl">
              <Video size={22} className="text-gray-400" />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-700">
                <span className="text-indigo-600">Clique para enviar</span> ou arraste aqui
              </p>
              <p className="text-xs text-gray-400 mt-0.5">{description}</p>
            </div>
          </div>
        </div>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="video/mp4,video/webm,video/quicktime"
        className="hidden"
        disabled={disabled || uploading}
        onChange={(e) => handleFile(e.target.files?.[0])}
      />
    </div>
  );
}

