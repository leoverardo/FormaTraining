import { useEffect, useRef, useState } from 'react';
import { useTheme } from '../../contexts/ThemeContext';
import { screenSource } from './appScreens';
import { useMotionPreference } from '../../features/motion/useMotionPreference';

const screens = [
  { key: 'aluno-painel', alt: 'Tela do aluno: painel' },
  { key: 'aluno-treino', alt: 'Tela do aluno: treinos' },
  { key: 'aluno-progresso', alt: 'Tela do aluno: progresso' },
  { key: 'aluno-checkin', alt: 'Tela do aluno: check-in' },
];

export function StudentDevicePreview({ autoPlay = false, screenKey }) {
  const { theme } = useTheme();
  const reduced = useMotionPreference();
  const device = useRef(null);
  const [active, setActive] = useState(0);
  const [visible, setVisible] = useState(false);
  const [pageVisible, setPageVisible] = useState(() => !document.hidden);

  useEffect(() => {
    if (!autoPlay) return undefined;
    const observer = new IntersectionObserver(([entry]) => setVisible(entry.isIntersecting && entry.intersectionRatio >= .25), { threshold: .25 });
    observer.observe(device.current);
    const updatePageVisibility = () => setPageVisible(!document.hidden);
    updatePageVisibility();
    document.addEventListener('visibilitychange', updatePageVisibility);
    return () => {
      observer.disconnect();
      document.removeEventListener('visibilitychange', updatePageVisibility);
    };
  }, [autoPlay]);

  useEffect(() => {
    if (!autoPlay || reduced || !visible || !pageVisible || screenKey) return undefined;
    const timer = window.setInterval(() => setActive(index => (index + 1) % screens.length), 3000);
    return () => window.clearInterval(timer);
  }, [autoPlay, reduced, visible, pageVisible, screenKey]);

  const screen = screens.find(item => item.key === screenKey) || screens[reduced ? 0 : active];
  const src = screenSource(screen.key, theme);
  return <div ref={device} className="student-device">
    <div className="device-speaker" aria-hidden="true" />
    <img key={src} className="student-device-screen" src={src} width="390" height="844" alt={screen.alt} loading="lazy" />
  </div>;
}
