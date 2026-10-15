import { Children, cloneElement, useCallback, useEffect, useRef, useState } from 'react';
import { ChevronLeft, ChevronRight, Pause, Play } from 'lucide-react';
import { useMotionPreference } from '../../features/motion/useMotionPreference';

export function FeatureCarousel({ children, enabled }) {
  const slides = Children.toArray(children);
  const viewport = useRef(null);
  const [active, setActive] = useState(0);
  const [playing, setPlaying] = useState(true);
  const [hovered, setHovered] = useState(false);
  const [focused, setFocused] = useState(false);
  const [visible, setVisible] = useState(false);
  const [hidden, setHidden] = useState(() => document.hidden);
  const reduced = useMotionPreference();
  const moving = enabled && !reduced;
  const running = moving && playing && visible && !hovered && !focused && !hidden;
  const goTo = useCallback(index => {
    const element = viewport.current;
    const slide = element?.children[(index + slides.length) % slides.length];
    if (slide) element.scrollTo({ left: slide.offsetLeft, behavior: moving ? 'smooth' : 'instant' });
  }, [slides.length, moving]);
  const navigate = index => { setPlaying(false); goTo(index); };

  useEffect(() => {
    const element = viewport.current;
    const observer = new IntersectionObserver(([entry]) => setVisible(entry.isIntersecting), { threshold: 0.15 });
    observer.observe(element);
    const onVisibility = () => setHidden(document.hidden);
    document.addEventListener('visibilitychange', onVisibility);
    return () => { observer.disconnect(); document.removeEventListener('visibilitychange', onVisibility); };
  }, []);
  useEffect(() => {
    if (!running) return undefined;
    const timer = window.setTimeout(() => goTo(active + 1), 6000);
    return () => window.clearTimeout(timer);
  }, [running, active, goTo]);

  const onScroll = () => {
    const element = viewport.current;
    const offsets = [...element.children].map(slide => Math.abs(slide.offsetLeft - element.scrollLeft));
    setActive(offsets.indexOf(Math.min(...offsets)));
  };
  return <div className="gallery-carousel" role="region" aria-roledescription="carrossel" aria-label="Destaques da plataforma"
    onMouseEnter={() => setHovered(true)} onMouseLeave={() => setHovered(false)}
    onFocusCapture={() => setFocused(true)} onBlurCapture={event => { if (!event.currentTarget.contains(event.relatedTarget)) setFocused(false); }}>
    <div ref={viewport} className="gallery-features" tabIndex={0} aria-label="Cartões de funcionalidades" onScroll={onScroll}
      onPointerDown={() => setPlaying(false)}
      onWheel={event => { if (Math.abs(event.deltaX) > 0 || event.shiftKey) setPlaying(false); }}
      onKeyDown={event => {
        if (event.key === 'ArrowRight' || event.key === 'ArrowLeft') { event.preventDefault(); navigate(active + (event.key === 'ArrowRight' ? 1 : -1)); }
        if (event.key === 'Home' || event.key === 'End') { event.preventDefault(); navigate(event.key === 'Home' ? 0 : slides.length - 1); }
      }}>
      {slides.map((slide, index) => cloneElement(slide, { role: 'group', 'aria-roledescription': 'slide', 'aria-label': `${index + 1} de ${slides.length}` }))}
    </div>
    <div className="gallery-carousel-controls">
      <div className="gallery-carousel-arrows">
        <button type="button" className="uc-icon-button" aria-label="Destaque anterior" onClick={() => navigate(active - 1)}><ChevronLeft size={19} /></button>
        <button type="button" className="uc-icon-button" aria-label="Próximo destaque" onClick={() => navigate(active + 1)}><ChevronRight size={19} /></button>
      </div>
      <div className="gallery-carousel-playback" data-playing={running}>
        {slides.map((_, index) => <button type="button" key={index} aria-label={`Mostrar destaque ${index + 1}`} aria-pressed={active === index} onClick={() => navigate(index)}>
          <span className={active === index ? 'is-selected' : ''}><i key={`${active}-${running}`} /></span>
        </button>)}
        <button type="button" aria-label={moving && playing ? 'Pausar destaques' : 'Reproduzir destaques'} disabled={!moving} onClick={() => setPlaying(value => !value)}>
          {moving && playing ? <Pause size={16} /> : <Play size={16} />}
        </button>
      </div>
      <span className="gallery-carousel-count" aria-live="off">{String(active + 1).padStart(2, '0')} / {String(slides.length).padStart(2, '0')}</span>
    </div>
  </div>;
}
