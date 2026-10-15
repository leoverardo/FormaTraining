import { useEffect, useRef, useSyncExternalStore } from 'react';
import { useMotionPreference } from '../../features/motion/useMotionPreference';

const desktopQuery = '(min-width: 1024px)';
const subscribe = callback => {
  const media = window.matchMedia(desktopQuery);
  media.addEventListener('change', callback);
  navigator.connection?.addEventListener('change', callback);
  return () => { media.removeEventListener('change', callback); navigator.connection?.removeEventListener('change', callback); };
};
const eligible = () => window.matchMedia(desktopQuery).matches && !navigator.connection?.saveData;

export function BrandSculpture({ enabled }) {
  const host = useRef(null);
  const reduced = useMotionPreference();
  const desktop = useSyncExternalStore(subscribe, eligible, () => false);
  useEffect(() => {
    const element = host.current;
    element.dataset.renderer = 'fallback';
    if (!enabled || reduced || !desktop) return undefined;
    let cancelled = false;
    let started = false;
    let dispose;
    const observer = new IntersectionObserver(async ([entry]) => {
      if (!entry.isIntersecting || started) return;
      started = true;
      try {
        const { createBrandSculpture } = await import('./createBrandSculpture');
        if (cancelled) return;
        dispose = createBrandSculpture(element, { onFailure: () => { element.dataset.renderer = 'unavailable'; } });
      } catch {
        if (!cancelled) element.dataset.renderer = 'unavailable';
      }
    }, { rootMargin: '120px' });
    observer.observe(element);
    return () => { cancelled = true; observer.disconnect(); dispose?.(); element.dataset.renderer = 'fallback'; };
  }, [enabled, reduced, desktop]);
  return <div ref={host} className="brand-sculpture" aria-hidden="true" data-renderer="fallback">
    <div className="brand-sculpture-fallback"><i /><i /><i /><span /></div>
  </div>;
}
