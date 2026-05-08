import { Button } from './Button';
import { Modal } from './Modal';

export function ConfirmDialog({ open, onClose, onConfirm, title, description, loading }) {
  return (
    <Modal open={open} onClose={onClose} title={title} description={description} size="sm">
      <p className="text-sm text-gray-600 mb-6">Essa acao nao pode ser desfeita.</p>
      <div className="flex gap-3 justify-end">
        <Button variant="secondary" onClick={onClose}>Cancelar</Button>
        <Button variant="danger" onClick={onConfirm} loading={loading}>Confirmar</Button>
      </div>
    </Modal>
  );
}

