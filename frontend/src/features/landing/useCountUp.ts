import { useEffect, useState } from 'react';
import { useReducedMotion } from 'framer-motion';

// Counts up from 0 to `target` once, starting `delaySeconds` after mount —
// part of the page-load cascade, not a scroll trigger.
export function useCountUp(target: number, durationMs = 1200, delaySeconds = 0) {
  const reduceMotion = useReducedMotion();
  const [value, setValue] = useState(0);

  useEffect(() => {
    if (reduceMotion) {
      setValue(target);
      return;
    }

    let frame: number;
    const startTimeout = window.setTimeout(() => {
      const start = performance.now();
      const tick = (now: number) => {
        const progress = Math.min(1, (now - start) / durationMs);
        const eased = 1 - Math.pow(1 - progress, 3);
        setValue(Math.round(target * eased));
        if (progress < 1) {
          frame = requestAnimationFrame(tick);
        }
      };
      frame = requestAnimationFrame(tick);
    }, delaySeconds * 1000);

    return () => {
      window.clearTimeout(startTimeout);
      cancelAnimationFrame(frame);
    };
  }, [target, durationMs, delaySeconds, reduceMotion]);

  return value;
}
