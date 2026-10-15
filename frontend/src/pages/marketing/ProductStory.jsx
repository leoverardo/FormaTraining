import { useEffect, useRef, useState } from 'react';
import { Check, Dumbbell } from 'lucide-react';
import { ProductPreview, StudentPreview } from './ProductPreview';
import { useMotionPreference } from '../../features/motion/useMotionPreference';

const chapters = [
  { title: 'Organize', headline: 'Cada aluno. Um ponto de partida.', text: 'Cadastre seus alunos, configure sua marca e reúna sua biblioteca. O contexto de cada pessoa fica por perto, para você focar no que importa.', detail: 'Alunos, objetivos e histórico no mesmo lugar.' },
  { title: 'Prescreva', headline: 'Seu método ganha movimento.', text: 'Crie treinos com intenção e planeje a semana de cada aluno. Transforme seu conhecimento em uma rotina clara para acompanhar.', detail: 'Uma biblioteca que cresce com o seu trabalho.' },
  { title: 'Acompanhe', headline: 'A evolução continua entre os treinos.', text: 'Conecte check-ins, registros e conversas para ver a evolução. Para o aluno, o próximo passo está sempre à mão.', detail: 'Mais presença em cada etapa da jornada.' },
];

function StoryPreview({ index }) {
  return index === 0 ? <ProductPreview /> : index === 1 ? <div className="gallery-prescription-preview">
              <div><Dumbbell size={26} /><span>Seu método, em prática</span></div><h3>Força e mobilidade.</h3><p>Uma rotina clara. Um novo passo.</p>
              {['Agachamento livre', 'Remada com halteres', 'Mobilidade de quadril'].map((exercise, index) => <article key={exercise}><i>0{index + 1}</i><div><b>{exercise}</b><small>3 séries · Movimento com intenção</small></div><Check size={16} /></article>)}
              <span className="gallery-story-note">Demonstração ilustrativa</span>
            </div> : <StudentPreview />;
}

export function ProductStory({ enabled }) {
  const root = useRef(null);
  const [active, setActive] = useState(0);
  const reduced = useMotionPreference();
  useEffect(() => {
    const observer = new IntersectionObserver(entries => {
      entries.filter(entry => entry.isIntersecting).forEach(entry => setActive(Number(entry.target.dataset.chapter)));
    }, { rootMargin: '-25% 0px -40% 0px' });
    root.current.querySelectorAll('.gallery-story-chapter').forEach(chapter => observer.observe(chapter));
    return () => observer.disconnect();
  }, []);
  return <section id="como-funciona" ref={root} className="gallery-section gallery-story" role="region" aria-label="Uma rotina melhor" data-chapter={active}>
    <div className="gallery-shell">
      <p className="gallery-eyebrow">Simples desde o primeiro dia</p>
      <h2>Uma rotina melhor.<br /><span>Em três movimentos.</span></h2>
      <div className="gallery-story-layout">
        <div className="gallery-story-stage">
          <div className="gallery-story-nav" aria-label="Etapas da plataforma">
            {chapters.map((chapter, index) => <button type="button" key={chapter.title} aria-pressed={active === index}
              onClick={() => root.current.querySelector(`[data-chapter="${index}"].gallery-story-chapter`).scrollIntoView({ behavior: reduced || !enabled ? 'instant' : 'smooth', block: 'center' })}>
              <span aria-hidden="true">0{index + 1}</span>{chapter.title}
            </button>)}
          </div>
          <div className="gallery-story-screen" key={active}>
            <StoryPreview index={active} />
          </div>
          <p className="gallery-footnote">Uma prévia da plataforma. Dados ilustrativos.</p>
        </div>
        <div className="gallery-story-chapters">{chapters.map((chapter, index) => <article className="gallery-story-chapter" data-chapter={index} key={chapter.title}>
          <span className="gallery-story-number">0{index + 1}</span><h3>{chapter.headline}</h3><p>{chapter.text}</p><div><Check size={18} />{chapter.detail}</div><div className="gallery-story-mobile-demo gallery-story-screen"><StoryPreview index={index} /></div>
        </article>)}</div>
      </div>
    </div>
  </section>;
}
