import { create } from 'zustand';
import type { User } from '../types/auth';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isInitializing: boolean;
  setAuth: (user: User, accessToken: string) => void;
  clearAuth: () => void;
  setInitializing: (value: boolean) => void;
  updateUser: (patch: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,
  isInitializing: true,
  setAuth: (user, accessToken) => set({ user, accessToken }),
  clearAuth: () => set({ user: null, accessToken: null }),
  setInitializing: (value) => set({ isInitializing: value }),
  updateUser: (patch) => set((state) => (state.user ? { user: { ...state.user, ...patch } } : state)),
}));
