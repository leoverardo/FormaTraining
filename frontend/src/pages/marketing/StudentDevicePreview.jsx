import { useEffect, useRef, useState } from 'react';
import { useTheme } from '../../contexts/ThemeContext';
import { screenSource } from './appScreens';

const screens = [
  { key: 'aluno-painel', alt: 'Tela do aluno: painel' },
  { key: 'aluno-treino', alt: 'Tela do aluno: treinos' },
  { key: 'aluno-progresso', alt: 'Tela do aluno: progresso' },
];

export function StudentDevicePreview({ scrollScreens = false, screenKey }) {
  const { theme } = useTheme();
  const device = useRef(null);
  const [active, setActive] = useState(0);

  useEffect(() => {
    if (!scrollScreens) return undefined;
    const section = device.current.closest('#experiencia');
    if (!section) return undefined;
    let frame = 0;
    const update = () => {
      frame = 0;
      const bounds = section.getBoundingClientRect();
      const progress = (window.innerHeight * 0.8 - bounds.top) / (bounds.height + window.innerHeight * 0.45);
      setActive(Math.max(0, Math.min(screens.length - 1, Math.floor(progress * screens.length))));
    };
    const schedule = () => { if (!frame) frame = requestAnimationFrame(update); };
    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', schedule);
    schedule();
    return () => {
      window.removeEventListener('scroll', schedule);
      window.removeEventListener('resize', schedule);
      if (frame) cancelAnimationFrame(frame);
    };
  }, [scrollScreens]);

  const screen = screens.find(item => item.key === screenKey) || screens[scrollScreens ? active : 0];
  const src = screenSource(screen.key, theme);
  return <div ref={device} className="student-device">
    <div className="device-speaker" aria-hidden="true" />
    <img key={src} className="student-device-screen" src={src} width="390" height="844" alt={screen.alt} loading="lazy" />
  </div>;
}
