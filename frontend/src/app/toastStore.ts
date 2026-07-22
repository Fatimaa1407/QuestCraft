import { create } from 'zustand';

export interface ToastItem {
  id: number;
  title: string;
  message?: string;
  imageUrl?: string | null;
  emoji?: string;
}

interface ToastState {
  toasts: ToastItem[];
  show: (toast: Omit<ToastItem, 'id'>) => void;
  dismiss: (id: number) => void;
}

let nextId = 1;

// Standalone store (not a hook-scoped queue) so any mutation anywhere in the app — Shop, Profile,
// wherever an equip/unequip happens — can fire a toast without prop-drilling a callback down to it.
export const useToastStore = create<ToastState>((set) => ({
  toasts: [],
  show: (toast) =>
    set((state) => ({ toasts: [...state.toasts, { ...toast, id: nextId++ }] })),
  dismiss: (id) => set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) })),
}));

export function showToast(toast: Omit<ToastItem, 'id'>) {
  useToastStore.getState().show(toast);
}
