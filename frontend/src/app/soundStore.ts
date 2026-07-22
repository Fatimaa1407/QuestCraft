import { create } from 'zustand';

const STORAGE_KEY = 'questcraft-sound-enabled';

function getInitialSoundEnabled(): boolean {
  const stored = localStorage.getItem(STORAGE_KEY);
  return stored === null ? true : stored === 'true';
}

interface SoundState {
  soundEnabled: boolean;
  toggleSound: () => void;
}

export const useSoundStore = create<SoundState>((set, get) => ({
  soundEnabled: getInitialSoundEnabled(),
  toggleSound: () => {
    const next = !get().soundEnabled;
    localStorage.setItem(STORAGE_KEY, String(next));
    set({ soundEnabled: next });
  },
}));

// Non-hook accessor for use inside utils/sounds.ts, which isn't a component and can't call
// useSoundStore() directly — reads the same Zustand store's current state synchronously.
export function isSoundEnabled(): boolean {
  return useSoundStore.getState().soundEnabled;
}
