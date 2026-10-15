import { useLayoutEffect } from 'react';
import gsap from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';

gsap.registerPlugin(ScrollTrigger);

export function useLandingMotion(root, enabled) {
  useLayoutEffect(() => {
    if (!enabled) return undefined;
    const media = gsap.matchMedia();
    media.add('(prefers-reduced-motion: no-preference)', () => {
      const context = gsap.context(() => {
        gsap.from('.gallery-hero h1, .gallery-hero-description, .gallery-actions', { y: 22, duration: 0.85, stagger: 0.09, ease: 'power3.out', clearProps: 'transform' });
        root.current.querySelectorAll('.gallery-section-heading, .gallery-center > h2, .gallery-editorial > div:first-child, .gallery-story-chapter').forEach(element => {
          gsap.from(element, { y: 32, duration: 0.8, ease: 'power3.out', clearProps: 'transform', scrollTrigger: { trigger: element, start: 'top 92%', once: true } });
        });
      }, root);
      return () => context.revert();
    });
    media.add('(min-width: 768px) and (prefers-reduced-motion: no-preference)', () => {
      const context = gsap.context(() => {
        gsap.to('.gallery-desktop-device', { y: -48, rotateX: 3, ease: 'none', scrollTrigger: { trigger: '.gallery-product-stage', start: 'top 75%', end: 'bottom top', scrub: 0.8 } });
        gsap.to('.gallery-product-caption', { y: -85, ease: 'none', scrollTrigger: { trigger: '.gallery-product-stage', start: 'top 80%', end: 'bottom top', scrub: 1 } });
        gsap.fromTo('.gallery-student-stage .student-device', { y: 65, rotation: 3 }, { y: 0, rotation: 0, ease: 'none', scrollTrigger: { trigger: '.gallery-student-stage', start: 'top bottom', end: 'center center', scrub: 0.6 } });
      }, root);
      return () => context.revert();
    });
    return () => media.revert();
  }, [root, enabled]);
}
