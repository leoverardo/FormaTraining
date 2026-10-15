import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import gsap from 'gsap';
import { useMotionPreference } from './useMotionPreference';
import './motion.css';

// Animate content, never ancestors of fixed menus, dialogs or toasts.
export function MotionRoute() {
  const { pathname } = useLocation();
  const reduced = useMotionPreference();
  useEffect(() => {
    if (pathname === '/landing') return undefined;
    const root = document.getElementById('root');
    const seen = new WeakSet();
    const context = gsap.context(() => {}, root);
    const scan = () => {
      const surface = root.querySelector('.app-shell, .auth-screen, .public-page') || root.firstElementChild;
      if (surface) surface.dataset.motion = reduced ? 'reduced' : 'enabled';
      if (reduced) return;
      const targets = [...root.querySelectorAll('.app-content h1, .app-content .ds-card, .auth-screen h1, .auth-screen h2, .auth-screen img, main h1, main h2, [role="dialog"] h2')]
        .filter(element => !seen.has(element) && element.getClientRects().length && !element.closest('[aria-hidden="true"]') && !element.querySelector('table, [role="grid"]'))
        .slice(0, 8);
      targets.forEach(element => seen.add(element));
      if (targets.length) context.add(() => gsap.from(targets, {
        y: (_, element) => element.matches('h1, h2') ? -12 : 14,
        duration: 0.4, stagger: 0.035, ease: 'power2.out', clearProps: 'transform',
      }));
    };
    scan();
    const observer = new MutationObserver(scan);
    observer.observe(root, { childList: true, subtree: true });
    return () => { observer.disconnect(); context.revert(); };
  }, [pathname, reduced]);
  return null;
}
