import { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Sparkles } from 'lucide-react';
import { useToastStore } from '../../app/toastStore';
import { Z_INDEX } from '../../styles/zIndex';

const AUTO_DISMISS_MS = 3200;

// Mounted once in AppLayout; reads from the global toastStore so any mutation anywhere
// (Shop, Profile) can call showToast(...) without needing this component in scope.
export function ToastStack() {
  const toasts = useToastStore((s) => s.toasts);
  const dismiss = useToastStore((s) => s.dismiss);

  return createPortal(
    <div
      className="pointer-events-none fixed right-4 top-4 flex w-full max-w-sm flex-col gap-2 sm:right-6 sm:top-6"
      style={{ zIndex: Z_INDEX.toast }}
    >
      <AnimatePresence>
        {toasts.map((toast) => (
          <ToastCard key={toast.id} toast={toast} onDismiss={() => dismiss(toast.id)} />
        ))}
      </AnimatePresence>
    </div>,
    document.body,
  );
}

function ToastCard({ toast, onDismiss }: { toast: { id: number; title: string; message?: string; imageUrl?: string | null; emoji?: string }; onDismiss: () => void }) {
  useEffect(() => {
    const timer = window.setTimeout(onDismiss, AUTO_DISMISS_MS);
    return () => window.clearTimeout(timer);
  }, [onDismiss]);

  return (
    <motion.div
      layout
      initial={{ opacity: 0, x: 40, scale: 0.9 }}
      animate={{ opacity: 1, x: 0, scale: 1 }}
      exit={{ opacity: 0, x: 40, scale: 0.9, transition: { duration: 0.2 } }}
      transition={{ type: 'spring', stiffness: 340, damping: 26 }}
      className="glass-card pointer-events-auto flex items-center gap-3 overflow-hidden rounded-2xl p-3.5 shadow-xl"
    >
      <span className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-xl bg-gradient-to-br from-app-accent to-app-accent-2 text-white">
        {toast.imageUrl ? (
          <img src={toast.imageUrl} alt="" className="h-full w-full object-cover" />
        ) : toast.emoji ? (
          <span className="text-lg">{toast.emoji}</span>
        ) : (
          <Sparkles size={18} />
        )}
      </span>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-slate-900 dark:text-white">{toast.title}</p>
        {toast.message && <p className="truncate text-xs text-slate-500 dark:text-slate-400">{toast.message}</p>}
      </div>
    </motion.div>
  );
}
