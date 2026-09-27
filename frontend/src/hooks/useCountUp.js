import { useEffect, useRef, useState } from 'react';

export function useCountUp(target, { duration = 900 } = {}) {
  const [value, setValue] = useState(0);
  const currentRef = useRef(0);
  const rafRef = useRef(0);

  useEffect(() => {
    const end = Number.isFinite(Number(target)) ? Number(target) : 0;
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduce || currentRef.current === end) {
      currentRef.current = end;
      setValue(end);
      return undefined;
    }
    const from = currentRef.current;
    const start = performance.now();
    const tick = (now) => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 4);
      const v = from + (end - from) * eased;
      currentRef.current = v;
      setValue(v);
      if (t < 1) rafRef.current = requestAnimationFrame(tick);
    };
    rafRef.current = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(rafRef.current);
  }, [target, duration]);

  return value;
}
