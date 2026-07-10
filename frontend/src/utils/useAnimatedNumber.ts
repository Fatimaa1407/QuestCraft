import { useEffect, useRef, useState } from 'react';

export function useAnimatedNumber(target: number, durationMs = 600): number {
  const [displayed, setDisplayed] = useState(target);
  const frameRef = useRef<number | null>(null);

  useEffect(() => {
    const start = displayed;
    const delta = target - start;
    if (delta === 0) return;

    const startTime = performance.now();

    const tick = (now: number) => {
      const progress = Math.min(1, (now - startTime) / durationMs);
      const eased = 1 - (1 - progress) * (1 - progress);
      setDisplayed(Math.round(start + delta * eased));
      if (progress < 1) {
        frameRef.current = requestAnimationFrame(tick);
      }
    };

    frameRef.current = requestAnimationFrame(tick);
    return () => {
      if (frameRef.current) cancelAnimationFrame(frameRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [target]);

  return displayed;
}
