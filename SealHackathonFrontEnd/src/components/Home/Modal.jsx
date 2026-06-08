import { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';

export default function Modal({
  open,
  onClose,
  title,
  subtitle,
  children,
  maxWidth = 'max-w-lg',
}) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e) => {
      if (e.key === 'Escape') onClose();
    };
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', onKey);
    return () => {
      document.body.style.overflow = '';
      window.removeEventListener('keydown', onKey);
    };
  }, [open, onClose]);

  if (!open) return null;

  return createPortal(
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center p-4 fade-in-up"
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
    >
      <button
        type="button"
        className="absolute inset-0 bg-[#04060A]/80 backdrop-blur-md transition-opacity duration-300"
        onClick={onClose}
        aria-label="Close"
      />
      <div
        className={`relative w-full ${maxWidth} max-h-[90vh] flex flex-col rounded-2xl overflow-hidden border border-white/[0.08] bg-[#0A0C14] shadow-[0_32px_64px_-12px_rgba(0,0,0,0.8)]`}
      >
        <div className="flex-shrink-0 flex items-start justify-between gap-4 px-6 pt-6 pb-4 border-b border-white/[0.06]">
          <div className="space-y-1">
            <h2
              id="modal-title"
              className="text-lg font-bold text-white tracking-wide"
              style={{ fontFamily: 'Montserrat, sans-serif' }}
            >
              {title}
            </h2>
            {subtitle && (
              <p className="text-xs text-slate-400 leading-normal">{subtitle}</p>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-white/5 transition-all duration-200"
            aria-label="Close"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
        <div className="flex-1 overflow-y-auto px-6 py-5">{children}</div>
      </div>
    </div>,
    document.body
  );
}
