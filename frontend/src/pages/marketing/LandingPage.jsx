import { useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Check, ChevronRight, Dumbbell, HeartPulse, Menu, Plus, UsersRound, X } from 'lucide-react';
import { BrandLogo } from '../../components/brand/BrandLogo';
import { StudentDevicePreview } from './StudentDevicePreview';
import { ThemeToggle } from '../../components/ui/ThemeToggle';
import { Collapsible } from '../../components/ui/Collapsible';
import { FeatureCarousel } from './FeatureCarousel';
import { ProductStory } from './ProductStory';
import { PlatformOverview } from './PlatformOverview';
import { useLandingMotion } from './useLandingMotion';
import { useMotionPreference } from '../../features/motion/useMotionPreference';
import { useTheme } from '../../contexts/ThemeContext';
import { CardFanCarousel } from '../../components/ui/card-fan-carousel';
import { carouselScreens, screenSource } from './appScreens';
import './marketing.css';

const plans = [
  { name: 'Starter', price: '97', count: '20', description: 'O começo de uma rotina mais leve.' },
  { name: 'Pro', price: '197', count: '50', description: 'Mais espaço para o seu método.', featured: true },
  { name: 'Growth', price: '297', count: '100', description: 'Seu acompanhamento, em outra escala.' },
];
const faqs = [
  ['Meus alunos também precisam assinar?', 'Não. Só o personal assina. Seus alunos têm acesso à plataforma enquanto sua assinatura estiver ativa.'],
  ['Posso usar minha identidade visual?', 'Sim. Você pode configurar sua marca, logo, cores e uma página pública com seu próprio endereço.'],
  ['Como meus alunos recebem acesso?', 'Ao cadastrar um aluno, ele recebe um link seguro para definir a senha e começar a acompanhar seus treinos.'],
  ['O que acontece se a assinatura vencer?', 'O acesso ao painel e à área dos alunos é pausado até a regularização, sem perder seus dados.'],
];

export function LandingPage() {
  const root = useRef(null);
  const { theme } = useTheme();
  const reduced = useMotionPreference();
  const [motionEnabled, setMotionEnabled] = useState(true);
  const moving = motionEnabled && !reduced;
  useLandingMotion(root, moving);
  const [menuOpen, setMenuOpen] = useState(false);
  const [active, setActive] = useState(0);
  return <div ref={root} className="gallery-landing" data-motion={moving ? 'enabled' : 'reduced'}>
    <header className="gallery-nav"><div className="gallery-shell">
      <Link to="/landing" aria-label="UpCoach — início"><BrandLogo size="sm" /></Link>
      <nav aria-label="Navegação principal" className={menuOpen ? 'is-open' : ''}>
        {[['#produto', 'A plataforma'], ['#experiencia', 'Para seus alunos'], ['#planos', 'Planos'], ['#faq', 'Dúvidas']].map(([href, label]) => <a key={href} href={href} onClick={() => setMenuOpen(false)}>{label}</a>)}
      </nav>
      <div className="gallery-nav-actions"><Link to="/login">Entrar</Link><Link to="/register" className="gallery-button">Começar agora</Link><ThemeToggle /><button type="button" className="gallery-menu" onClick={() => setMenuOpen(!menuOpen)} aria-label={menuOpen ? 'Fechar menu' : 'Abrir menu'} aria-expanded={menuOpen}>{menuOpen ? <X size={20} /> : <Menu size={20} />}</button></div>
    </div></header>
    <button type="button" className="gallery-motion-toggle" aria-label={moving ? 'Pausar animações' : 'Ativar animações'} aria-pressed={moving} disabled={reduced} onClick={() => setMotionEnabled(value => !value)}><span aria-hidden="true">{moving ? 'Ⅱ' : '▷'}</span><span className="gallery-motion-label">{reduced ? 'Movimento reduzido' : moving ? 'Pausar animações' : 'Ativar animações'}</span></button>
    <main>
      <section className="gallery-hero">
        <div className="gallery-shell"><p className="gallery-eyebrow">UpCoach</p><h1>Seu método.<br /><span>Em sua melhor forma.</span></h1><p className="gallery-hero-description">Mais presença no acompanhamento.<br className="mobile-break" /> Mais espaço para evoluir.<br />Alunos, treinos e resultados. Tudo conectado.</p><div className="gallery-actions"><Link to="/register" className="gallery-button">Conhecer os planos <ArrowRight size={15} /></Link><a href="#produto" className="gallery-link">Explore a plataforma <ChevronRight size={17} /></a></div>
          <div className="gallery-product-stage"><div className="gallery-desktop-device"><div className="gallery-device-camera" /><img className="gallery-dashboard-preview" src="/marketing/painel-personal.jpg" width="1440" height="900" alt="Prévia do painel do personal UpCoach" fetchPriority="high" /></div><div className="gallery-device-foot" /><div className="gallery-product-caption"><span>Feito para personal trainers.<br /><b>Pensado para cada aluno.</b></span><a href="#experiencia" aria-label="Conheça a experiência do aluno"><Plus size={20} /></a></div></div>
          <p className="gallery-footnote">Uma prévia do que você e seus alunos podem construir juntos. Dados ilustrativos.</p>
        </div>
      </section>
      <PlatformOverview />
      <section id="por-dentro" className="gallery-section gallery-mobile-showcase" aria-label="Explore o aplicativo">
        <div className="gallery-shell gallery-center"><h2>Seu acompanhamento.<br /><span>Em cada detalhe.</span></h2><p className="gallery-copy">Explore as telas do personal e do aluno, nos modos claro e escuro. Um cuidado que acompanha a sua rotina.</p></div>
        <div className="gallery-screens-fan"><CardFanCarousel cards={carouselScreens} theme={theme} enabled={moving} /></div>
        <p className="gallery-footnote gallery-center">Capturas reais da interface mobile. Dados demonstrativos.</p>
      </section>
      <section id="produto" className="gallery-band gallery-section"><div className="gallery-shell"><div className="gallery-section-heading"><h2>Menos tarefas.<br /><span>Mais possibilidades.</span></h2><a href="#como-funciona" className="gallery-link">Veja como funciona <ChevronRight size={17} /></a></div>
        <FeatureCarousel enabled={moving}><article><p>ALUNOS</p><h3>Cada pessoa.<br />Todo o contexto.</h3><span>Objetivos, histórico e conversas reunidos para um acompanhamento próximo.</span><img className="gallery-feature-screen" src={screenSource('personal-alunos', theme)} width="390" height="844" alt="Tela do personal: gestão de alunos no celular" loading="lazy" /><UsersRound className="gallery-feature-icon" size={22} /></article>
        <article><p>TREINOS</p><h3>Seu conhecimento.<br />Em movimento.</h3><span>Monte rotinas claras e organize sua biblioteca de exercícios do seu jeito.</span><img className="gallery-feature-screen" src={screenSource('aluno-treino', theme)} width="390" height="844" alt="Tela do aluno: execução do treino no celular" loading="lazy" /><Dumbbell className="gallery-feature-icon" size={22} /></article>
        <article><p>EVOLUÇÃO</p><h3>Pequenos passos.<br />Avanços visíveis.</h3><span>Check-ins, fotos e registros transformam constância em uma história.</span><img className="gallery-feature-screen" src={screenSource('aluno-progresso', theme)} width="390" height="844" alt="Tela do aluno: registros de evolução no celular" loading="lazy" /><HeartPulse className="gallery-feature-icon" size={22} /></article></FeatureCarousel>
      </div></section>
      <section id="experiencia" className="gallery-section"><div className="gallery-shell gallery-editorial"><div><p className="gallery-eyebrow">A experiência do aluno</p><h2>O próximo passo.<br /><span>Sempre por perto.</span></h2><p className="gallery-copy">Seu cuidado continua entre uma sessão e outra. Uma experiência simples de usar, com o treino do dia, a evolução e a conversa com você no mesmo lugar.</p><ul className="gallery-checks">{['Treinos claros, na palma da mão', 'Uma linha do tempo de conquistas', 'Conexão direta com seu personal'].map(text => <li key={text}><Check size={18} />{text}</li>)}</ul><Link to="/student/register" className="gallery-link">Conheça seu espaço <ChevronRight size={17} /></Link></div><div className="gallery-student-stage"><StudentDevicePreview scrollScreens={moving} /></div></div></section>
      <ProductStory enabled={moving} />
      <section id="planos" className="gallery-section"><div className="gallery-shell"><div className="gallery-center"><p className="gallery-eyebrow">Espaço para crescer</p><h2>O plano muda.<br /><span>Seu cuidado permanece.</span></h2><p className="gallery-copy">Só o personal assina. Para o aluno, o acesso está incluído.</p></div><div className="gallery-plans">{plans.map(plan => <article key={plan.name} className={plan.featured ? 'is-featured' : ''}><span className="gallery-plan-label">{plan.featured ? 'Para ir além' : 'Feito para o seu momento'}</span><h3>{plan.name}</h3><p>{plan.description}</p><div className="gallery-price"><small>R$</small> {plan.price}<span>/mês</span></div><ul><li><Check size={15} />Até {plan.count} alunos</li><li><Check size={15} />Treinos, agenda e evolução</li><li><Check size={15} />Sua página pública</li></ul><Link to="/register" className={`gallery-button ${plan.featured ? '' : 'gallery-button--outline'}`}>Escolher {plan.name} <ChevronRight size={15} /></Link></article>)}</div><p className="gallery-footnote">Valores do ciclo mensal. Consulte as opções trimestral e anual no cadastro.</p></div></section>
      <section id="faq" className="gallery-section gallery-band"><div className="gallery-shell gallery-faq"><h2>Bom saber.<br /><span>Antes de começar.</span></h2><div>{faqs.map(([question, answer], i) => <article key={question}><h3><button type="button" id={`faq-toggle-${i}`} onClick={() => setActive(active === i ? null : i)} aria-expanded={active === i} aria-controls={`faq-${i}`}><span>{question}</span><Plus size={19} aria-hidden="true" className={active === i ? 'is-open' : ''} /></button></h3><Collapsible id={`faq-${i}`} labelledBy={`faq-toggle-${i}`} open={active === i} motion={moving}><p>{answer}</p></Collapsible></article>)}</div></div></section>
      <section className="gallery-section gallery-final gallery-center"><div className="gallery-shell"><p className="gallery-eyebrow">Seu trabalho merece esse espaço.</p><h2>Pronto para a sua<br />melhor forma?</h2><Link to="/register" className="gallery-button">Começar agora <ArrowRight size={16} /></Link></div></section>
    </main>
    <footer className="gallery-footer"><div className="gallery-shell"><BrandLogo size="sm" /><p>© {new Date().getFullYear()} UpCoach. Todos os direitos reservados.</p><div><Link to="/privacy-policy">Privacidade</Link><Link to="/terms-of-use">Termos de uso</Link></div></div></footer>
  </div>;
}
