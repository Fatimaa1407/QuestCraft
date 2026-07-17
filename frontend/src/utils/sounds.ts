let ctx: AudioContext | null = null;

function getContext(): AudioContext | null {
  if (typeof window === 'undefined') return null;
  ctx ??= new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
  return ctx;
}

function playTone(freq: number, startOffset: number, duration: number, type: OscillatorType = 'sine', peakGain = 0.15) {
  const audioCtx = getContext();
  if (!audioCtx) return;

  const oscillator = audioCtx.createOscillator();
  const gain = audioCtx.createGain();
  oscillator.type = type;
  oscillator.frequency.value = freq;

  const startTime = audioCtx.currentTime + startOffset;
  gain.gain.setValueAtTime(0, startTime);
  gain.gain.linearRampToValueAtTime(peakGain, startTime + 0.02);
  gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);

  oscillator.connect(gain);
  gain.connect(audioCtx.destination);
  oscillator.start(startTime);
  oscillator.stop(startTime + duration);
}

export function playSuccessSound() {
  playTone(660, 0, 0.14, 'sine');
  playTone(990, 0.08, 0.18, 'sine');
}

export function playErrorSound() {
  playTone(180, 0, 0.22, 'sawtooth', 0.08);
}

export function playFanfareSound() {
  playTone(523, 0, 0.14, 'sine');
  playTone(659, 0.12, 0.14, 'sine');
  playTone(784, 0.24, 0.22, 'sine');
}

// A bigger moment than the quiz fanfare: a rising C-E-G-C major arpeggio that
// resolves into a soft two-note shimmer, instead of just repeating playFanfareSound.
export function playLevelUpSound() {
  playTone(523.25, 0, 0.16, 'sine');
  playTone(659.25, 0.1, 0.16, 'sine');
  playTone(783.99, 0.2, 0.18, 'sine');
  playTone(1046.5, 0.32, 0.5, 'sine', 0.18);
  playTone(1318.51, 0.36, 0.45, 'sine', 0.09);
}
