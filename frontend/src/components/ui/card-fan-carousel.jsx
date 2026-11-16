import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import gsap from 'gsap';
import { Modal } from './Modal';
import { useMotionPreference } from '../../features/motion/useMotionPreference';

const POSITIONS = [
  { rotation: -21, scale: .7756, x: -30, y: 7.3, zIndex: 1 },
  { rotation: -14, scale: .8498, x: -22, y: 4, zIndex: 2 },
  { rotation: -7, scale: .9346, x: -11, y: 1.3, zIndex: 3 },
  { rotation: 0, scale: 1, x: 0, y: 0, zIndex: 10 },
  { rotation: 7, scale: .9346, x: 11, y: 1.3, zIndex: 3 },
  { rotation: 14, scale: .8498, x: 22, y: 4, zIndex: 2 },
  { rotation: 21, scale: .7756, x: 30, y: 7.3, zIndex: 1 },
];

function responsiveMultiplier(width) {
  return width < 480 ? .28 : width < 640 ? .38 : width < 768 ? .5 : width < 1024 ? .75 : 1;
}

export function CardFanCarousel({ cards, theme = 'light', enabled = true }) {
  const rootRef = useRef(null);
  const fanRef = useRef(null);
  const layoutRef = useRef(null);
  const centerRef = useRef(3);
  const enteredRef = useRef(false);
  const focusCardAfterChange = useRef(false);
  const [inView, setInView] = useState(false);
  const [center, setCenter] = useState(cards.length > 7 ? 3 : Math.floor(cards.length / 2));
  const [preview, setPreview] = useState(null);
  const reduced = useMotionPreference();
  const moving = enabled && !reduced;
  const count = cards.length;

  const visibleMap = useCallback(index => {
    const map = new Map();
    for (let slot = 0; slot < Math.min(7, count); slot++) {
      map.set(count > 7 ? ((index + slot - 3) % count + count) % count : slot, slot);
    }
    return map;
  }, [count]);
  const visible = useMemo(() => visibleMap(center), [visibleMap, center]);
  const cycle = direction => setCenter(previous => (previous + direction + count) % count);

  useLayoutEffect(() => {
    centerRef.current = center;
    layoutRef.current?.(center);
    if (focusCardAfterChange.current) {
      fanRef.current?.querySelector(`[data-card-index="${center}"]`)?.focus({ preventScroll: true });
      focusCardAfterChange.current = false;
    }
  }, [center]);

  useEffect(() => {
    const observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting)) {
        setInView(true);
        observer.disconnect();
      }
    }, { threshold: .2 });
    if (rootRef.current) observer.observe(rootRef.current);
    return () => observer.disconnect();
  }, []);

  useLayoutEffect(() => {
    const root = rootRef.current;
    const fan = fanRef.current;
    if (!root || !fan || !count) return undefined;
    const elements = [...fan.querySelectorAll('.cf-card')];
    const context = gsap.context(() => {}, root);
    let hovered = null;
    const slotCount = Math.min(7, count);
    const config = slot => count >= 7 ? POSITIONS[slot] : (() => {
      const distance = slotCount > 1 ? (slot - (slotCount - 1) / 2) / ((slotCount - 1) / 2) : 0;
      return { rotation: distance * 21, scale: 1 - .2244 * distance ** 2, x: distance * 30, y: distance ** 2 * 7.3, zIndex: 10 - Math.abs(distance) };
    })();

    const layout = (index, entrance = false) => context.add(() => {
      const map = visibleMap(index);
      const multiplier = responsiveMultiplier(root.clientWidth);
      const vertical = Math.min(1, fan.clientHeight / 608) * multiplier;
      gsap.killTweensOf(elements);
      elements.forEach((element, cardIndex) => {
        const slot = map.get(cardIndex);
        if (slot === undefined) {
          if (moving && inView) gsap.to(element, { opacity: 0, scale: .9, duration: .2, overwrite: true });
          else gsap.set(element, { opacity: 0 });
          return;
        }
        const base = config(slot);
        const distance = hovered === null ? 0 : Math.abs(slot - hovered);
        const nudge = hovered === null || slot === hovered ? 0 : (slot < hovered ? -1 : 1) * 3 * multiplier / Math.max(1, distance);
        const target = {
          x: `${base.x * multiplier + nudge}rem`,
          y: `${base.y * vertical - (slot === hovered ? 1.8 * vertical : 0)}rem`,
          rotation: base.rotation,
          scale: base.scale * (slot === hovered ? 1.035 : 1),
          opacity: 1,
          zIndex: slot === hovered ? 20 : base.zIndex,
        };
        if (!moving || !inView) gsap.set(element, target);
        else if (entrance) gsap.fromTo(element,
          { x: 0, y: 72, rotation: 0, scale: .9, opacity: 0 },
          { ...target, duration: 1.05, delay: slot * .045, ease: 'elastic.out(1,.78)', overwrite: true });
        else gsap.to(element, { ...target, duration: .45, ease: 'power3.out', overwrite: true });
      });
    });
    layoutRef.current = layout;
    const entrance = inView && moving && !enteredRef.current;
    if (inView) enteredRef.current = true;
    layout(centerRef.current, entrance);

    const listeners = elements.map((element, index) => {
      const enter = () => {
        if (!moving || !window.matchMedia('(hover: hover) and (pointer: fine)').matches) return;
        hovered = visibleMap(centerRef.current).get(index) ?? null;
        layout(centerRef.current);
      };
      element.addEventListener('mouseenter', enter);
      return () => element.removeEventListener('mouseenter', enter);
    });
    const leave = () => { hovered = null; layout(centerRef.current); };
    fan.addEventListener('mouseleave', leave);
    let initialSize = true;
    const resize = new ResizeObserver(() => {
      if (initialSize) { initialSize = false; return; }
      layout(centerRef.current);
    });
    resize.observe(root);
    return () => {
      layoutRef.current = null;
      listeners.forEach(remove => remove());
      fan.removeEventListener('mouseleave', leave);
      resize.disconnect();
      context.revert();
    };
  }, [count, moving, inView, visibleMap]);

  if (!count) return null;
  const current = cards[center % count];
  return <>
    <div ref={rootRef} className="cf-root" data-theme={theme} data-motion={moving ? 'enabled' : 'reduced'} role="region" aria-roledescription="carrossel" aria-label="Telas reais do aplicativo"
      onKeyDown={event => {
        const next = event.key === 'ArrowRight' ? (center + 1) % count
          : event.key === 'ArrowLeft' ? (center - 1 + count) % count
            : event.key === 'Home' ? 0 : event.key === 'End' ? count - 1 : null;
        if (next === null) return;
        event.preventDefault();
        if (event.target.closest('.cf-card')) {
          if (next === center) fanRef.current?.querySelector(`[data-card-index="${next}"]`)?.focus({ preventScroll: true });
          else focusCardAfterChange.current = true;
        }
        setCenter(next);
      }}>
      <style dangerouslySetInnerHTML={{ __html: CF_CSS }} />
      <div className="cf-section">
        <div className="cf-frame"><div ref={fanRef} className="cf-fan">
          {cards.map((card, index) => <button key={card.imgUrl} type="button" className="cf-card" data-card-index={index} aria-label={`Ampliar ${card.alt}`} aria-hidden={!visible.has(index)} inert={!visible.has(index)} tabIndex={visible.has(index) ? 0 : -1} onClick={() => setPreview(index)}>
            <span className="cf-media"><img src={card.imgUrl} width="390" height="844" loading="lazy" decoding="async" alt={card.alt} className="cf-img" /></span>
          </button>)}
        </div></div>
        <div className="cf-controls">
          <button type="button" className="cf-arrow" onClick={() => cycle(-1)} aria-label="Tela anterior"><ChevronLeft size={20} /></button>
          <div className="cf-dots" aria-hidden="true">{cards.map((card, index) => <span key={card.imgUrl} className={`cf-dot${index === center ? ' cf-dot--active' : ''}`} />)}</div>
          <button type="button" className="cf-arrow" onClick={() => cycle(1)} aria-label="Próxima tela"><ChevronRight size={20} /></button>
        </div>
        <p className="cf-caption" aria-live="polite">{current.alt}<span>{center + 1} de {count} · Toque em uma tela para ampliar</span></p>
      </div>
    </div>
    <Modal open={preview !== null} onClose={() => setPreview(null)} title={preview !== null ? cards[preview]?.alt : 'Tela do aplicativo'} description="Captura real da interface com dados demonstrativos." size="sm">
      {preview !== null && <img src={cards[preview]?.imgUrl} alt={cards[preview]?.alt} width="390" height="844" className="mx-auto max-h-[65vh] w-auto rounded-2xl object-contain" />}
    </Modal>
  </>;
}

const CF_CSS = `
/* o que o projeto pode trocar com uma classe (largura, margem): na camada base, que vem antes das classes de componente
   e dos utilitários (Tailwind v4) e perde para o CSS fora de camada (Tailwind v3, CSS Modules, CSS comum) */
@layer base {
  .cf-root { position: relative; width: 100%; margin: 0; }
}
.cf-root {
  container-type: inline-size;
  box-sizing: border-box;
  isolation: isolate;
  -webkit-tap-highlight-color: transparent;
  --cf-card-border: rgba(255, 255, 255, .08);
  --cf-card-shadow: 0 15px 35px rgba(0, 0, 0, .5);
  --cf-arrow-border: rgba(255, 255, 255, .1);
  --cf-arrow-border-hover: rgba(255, 255, 255, .25);
  --cf-arrow-bg: rgba(255, 255, 255, .05);
  --cf-arrow-color: rgba(255, 255, 255, .55);
  --cf-arrow-color-hover: rgba(255, 255, 255, .8);
  --cf-arrow-ring: rgba(255, 255, 255, .04);
  --cf-arrow-shadow: 0 4px 20px rgba(0, 0, 0, .4);
  --cf-dot: rgba(255, 255, 255, .15);
  --cf-dot-active: rgba(255, 255, 255, .8);
}
.cf-root[data-theme="light"] {
  --cf-card-border: rgba(0, 0, 0, .08);
  --cf-card-shadow: 0 15px 35px rgba(0, 0, 0, .15);
  --cf-arrow-border: rgba(0, 0, 0, .1);
  --cf-arrow-border-hover: rgba(0, 0, 0, .25);
  --cf-arrow-bg: rgba(0, 0, 0, .05);
  --cf-arrow-color: rgba(0, 0, 0, .4);
  --cf-arrow-color-hover: rgba(0, 0, 0, .7);
  --cf-arrow-ring: rgba(0, 0, 0, .04);
  --cf-arrow-shadow: 0 4px 20px rgba(0, 0, 0, .1);
  --cf-dot: rgba(0, 0, 0, .15);
  --cf-dot-active: rgba(0, 0, 0, .7);
}
.cf-root *, .cf-root *::before, .cf-root *::after { box-sizing: border-box; }

.cf-section {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  margin: 0;
  padding: 1rem;
}
.cf-frame { display: flex; align-items: center; justify-content: center; width: 100%; max-width: 90rem; }

/* o leque: os cartões ficam todos no centro e o GSAP abre cada um no lugar dele. A altura é a ideal da faixa de
   largura, até 70% da tela (--cf-vh é 1% da altura de referência, a da tela por padrão) */
.cf-fan {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  max-width: 80rem;
  height: min(38rem, calc(70 * var(--cf-vh, 1vh)));
}
.cf-card {
  position: absolute;
  display: block;
  width: min(20rem, calc(36.84 * var(--cf-vh, 1vh)));
  height: min(35rem, calc(64.47 * var(--cf-vh, 1vh)));
  margin: 0;
  padding: 0;
  overflow: clip;
  border: 1px solid var(--cf-card-border);
  border-radius: min(3.316rem, calc(6.11 * var(--cf-vh, 1vh)));
  background: transparent;
  box-shadow: var(--cf-card-shadow);
  opacity: 0;
  will-change: transform;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
}
.cf-card--link { cursor: pointer; }
/* uma faixa invisível embaixo de cada cartão: com o cartão erguido no hover, o mouse logo abaixo dele continua nele */
.cf-card::after {
  content: "";
  position: absolute;
  left: 0;
  right: 0;
  bottom: -4rem;
  z-index: 100;
  height: 4rem;
  pointer-events: auto;
}
.cf-media { position: relative; width: 100%; height: 100%; overflow: hidden; }
.cf-img {
  position: absolute;
  inset: 0;
  z-index: 10;
  display: block;
  width: 100%;
  height: 100%;
  max-width: none;
  margin: 0;
  object-fit: cover;
}

.cf-controls { position: relative; z-index: 30; display: flex; align-items: center; justify-content: center; gap: 1rem; margin-top: 1rem; }
.cf-dots { display: flex; align-items: center; gap: .5rem; }
.cf-dot { display: block; width: .5rem; height: .5rem; border-radius: 9999px; background: var(--cf-dot); transition: all .3s cubic-bezier(.4, 0, .2, 1); }
.cf-dot--active { background: var(--cf-dot-active); transform: scale(1.3); }

.cf-arrow {
  position: relative;
  z-index: 30;
  display: flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  margin: 0;
  padding: 0;
  border: 1.5px solid var(--cf-arrow-border);
  border-radius: 9999px;
  background: var(--cf-arrow-bg);
  -webkit-backdrop-filter: blur(16px);
  backdrop-filter: blur(16px);
  color: var(--cf-arrow-color);
  box-shadow: var(--cf-arrow-shadow);
  font: inherit;
  cursor: pointer;
  outline: none;
  transition: color .3s cubic-bezier(.4, 0, .2, 1), border-color .3s cubic-bezier(.4, 0, .2, 1);
}
.cf-arrow::before {
  content: "";
  position: absolute;
  inset: 3px;
  border: 1px solid var(--cf-arrow-ring);
  border-radius: 9999px;
  pointer-events: none;
}
.cf-arrow:hover { border-color: var(--cf-arrow-border-hover); color: var(--cf-arrow-color-hover); }
.cf-arrow:focus-visible { border-color: var(--cf-arrow-border-hover); color: var(--cf-arrow-color-hover); }
.cf-arrow:active { opacity: .7; }
.cf-chevron { position: relative; z-index: 2; display: block; width: 1rem; height: 1rem; }

/* as faixas de largura do carrossel, as mesmas do getResponsiveMultiplier */
@container (min-width: 768px) {
  .cf-section { padding-inline: 2rem; }
  .cf-controls { margin-top: 1.5rem; }
  .cf-arrow { width: 3rem; height: 3rem; }
  .cf-chevron { width: 1.25rem; height: 1.25rem; }
}
@container (min-width: 1024px) {
  .cf-section { padding-block: 2rem; }
}
@container (max-width: 1023.98px) {
  .cf-fan { height: min(34rem, calc(70 * var(--cf-vh, 1vh))); }
  .cf-card {
    width: min(16rem, calc(32.94 * var(--cf-vh, 1vh)));
    height: min(28rem, calc(57.65 * var(--cf-vh, 1vh)));
    border-radius: min(2.6rem, calc(5.35 * var(--cf-vh, 1vh)));
  }
}
@container (max-width: 767.98px) {
  .cf-fan { height: min(28rem, calc(70 * var(--cf-vh, 1vh))); }
  .cf-card {
    width: min(12rem, calc(30 * var(--cf-vh, 1vh)));
    height: min(21rem, calc(52.5 * var(--cf-vh, 1vh)));
    border-radius: min(2rem, calc(5 * var(--cf-vh, 1vh)));
  }
}
@container (max-width: 639.98px) {
  .cf-fan { height: min(26rem, calc(70 * var(--cf-vh, 1vh))); }
  .cf-card {
    width: min(9rem, calc(24.23 * var(--cf-vh, 1vh)));
    height: min(15.75rem, calc(42.4 * var(--cf-vh, 1vh)));
    border-radius: min(1.5rem, calc(4.04 * var(--cf-vh, 1vh)));
  }
}
@container (max-width: 479.98px) {
  .cf-fan { height: min(22rem, calc(70 * var(--cf-vh, 1vh))); }
  .cf-card {
    width: min(7rem, calc(22.27 * var(--cf-vh, 1vh)));
    height: min(12.25rem, calc(38.98 * var(--cf-vh, 1vh)));
    border-radius: min(1.2rem, calc(3.82 * var(--cf-vh, 1vh)));
  }
}

/* App screenshots preserve their full mobile viewport. */
.cf-card { height: auto; aspect-ratio: 390 / 844; opacity: 1; border-radius: 22px; background: var(--surface); will-change: auto; }
.cf-media { display: block; }
.cf-img { object-fit: contain; }
.cf-section { overflow: clip; padding-top: 3rem; }
.cf-card:focus-visible, .cf-arrow:focus-visible { outline: 2px solid var(--focus-ring); outline-offset: 4px; }
.cf-caption { margin: 20px 0 0; font-size: 14px; font-weight: 500; text-align: center; color: var(--text); }
.cf-caption span { display: block; margin-top: 6px; font-size: 12px; font-weight: 400; color: var(--text-secondary); }
.cf-root, .cf-root[data-theme="light"] { --cf-arrow-color: var(--text-secondary); --cf-arrow-color-hover: var(--text); }
@container (min-width: 1024px) { .cf-card { width: min(17rem, calc(29 * var(--cf-vh, 1vh))); } }
`;
