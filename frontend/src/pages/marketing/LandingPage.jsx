import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Check, ChevronRight, Dumbbell, HeartPulse, Menu, Plus, UsersRound, X } from 'lucide-react';
import { BrandLogo } from '../../components/brand/BrandLogo';
import { ProductPreview, StudentPreview } from './ProductPreview';
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
  const [menuOpen, setMenuOpen] = useState(false);
  const [active, setActive] = useState(0);
  return <div className="gallery-landing">
    <header className="gallery-nav"><div className="gallery-shell">
      <Link to="/landing" aria-label="Forma Training — início"><BrandLogo size="sm" /></Link>
      <nav aria-label="Navegação principal" className={menuOpen ? 'is-open' : ''}>
        {[['#produto', 'A plataforma'], ['#experiencia', 'Para seus alunos'], ['#planos', 'Planos'], ['#faq', 'Dúvidas']].map(([href, label]) => <a key={href} href={href} onClick={() => setMenuOpen(false)}>{label}</a>)}
      </nav>
      <div className="gallery-nav-actions"><Link to="/login">Entrar</Link><Link to="/register" className="gallery-button">Começar agora</Link><button className="gallery-menu" onClick={() => setMenuOpen(!menuOpen)} aria-label={menuOpen ? 'Fechar menu' : 'Abrir menu'} aria-expanded={menuOpen}>{menuOpen ? <X size={20} /> : <Menu size={20} />}</button></div>
    </div></header>
    <main>
      <section className="gallery-hero">
        <div className="gallery-shell"><p className="gallery-eyebrow">Forma Training</p><h1>Seu método.<br /><span>Em sua melhor forma.</span></h1><p className="gallery-hero-description">Mais presença no acompanhamento.<br className="mobile-break" /> Mais espaço para evoluir.<br />Alunos, treinos e resultados. Tudo conectado.</p><div className="gallery-actions"><Link to="/register" className="gallery-button">Conhecer os planos <ArrowRight size={15} /></Link><a href="#produto" className="gallery-link">Explore a plataforma <ChevronRight size={17} /></a></div>
          <div className="gallery-product-stage"><div className="gallery-desktop-device"><div className="gallery-device-camera" /><ProductPreview /></div><div className="gallery-device-foot" /><div className="gallery-product-caption"><span>Feito para personal trainers.<br /><b>Pensado para cada aluno.</b></span><a href="#experiencia" aria-label="Conheça a experiência do aluno"><Plus size={20} /></a></div></div>
          <p className="gallery-footnote">Uma prévia do que você e seus alunos podem construir juntos. Dados ilustrativos.</p>
        </div>
      </section>
      <section id="produto" className="gallery-band gallery-section"><div className="gallery-shell"><div className="gallery-section-heading"><h2>Menos tarefas.<br /><span>Mais possibilidades.</span></h2><a href="#como-funciona" className="gallery-link">Veja como funciona <ChevronRight size={17} /></a></div>
        <div className="gallery-features"><article><p>ALUNOS</p><h3>Cada pessoa.<br />Todo o contexto.</h3><span>Objetivos, histórico e conversas reunidos para um acompanhamento próximo.</span><div className="gallery-student-list">{[['LA', 'Lucas Almeida', 'Foco em força'], ['AO', 'Ana Oliveira', 'Em evolução'], ['PS', 'Pedro Santos', 'Mais constância']].map(([initials, name, label]) => <div key={name}><i>{initials}</i><div><b>{name}</b><small>{label}</small></div><ChevronRight size={16} /></div>)}</div><UsersRound className="gallery-feature-icon" size={22} /></article>
        <article><p>TREINOS</p><h3>Seu conhecimento.<br />Em movimento.</h3><span>Monte rotinas claras e organize sua biblioteca de exercícios do seu jeito.</span><div className="gallery-workout-card"><Dumbbell size={32} /><small>TREINO A</small><b>Uma boa base.<br />Um novo limite.</b><div><span>6 exercícios</span><span>45 minutos</span></div></div><Dumbbell className="gallery-feature-icon" size={22} /></article>
        <article><p>EVOLUÇÃO</p><h3>Pequenos passos.<br />Avanços visíveis.</h3><span>Check-ins, fotos e registros transformam constância em uma história.</span><div className="gallery-progress-visual"><strong>86<span>%</span></strong><small>constância da semana</small><div>{[36, 52, 47, 70, 61, 86, 96].map((h, i) => <i key={i} style={{ height: `${h}%` }} />)}</div></div><HeartPulse className="gallery-feature-icon" size={22} /></article></div>
      </div></section>
      <section id="experiencia" className="gallery-section"><div className="gallery-shell gallery-editorial"><div><p className="gallery-eyebrow">A experiência do aluno</p><h2>O próximo passo.<br /><span>Sempre por perto.</span></h2><p className="gallery-copy">Seu cuidado continua entre uma sessão e outra. Uma experiência simples de usar, com o treino do dia, a evolução e a conversa com você no mesmo lugar.</p><ul className="gallery-checks">{['Treinos claros, na palma da mão', 'Uma linha do tempo de conquistas', 'Conexão direta com seu personal'].map(text => <li key={text}><Check size={18} />{text}</li>)}</ul><Link to="/student/register" className="gallery-link">Conheça seu espaço <ChevronRight size={17} /></Link></div><div className="gallery-student-stage"><StudentPreview /></div></div></section>
      <section id="como-funciona" className="gallery-band gallery-section"><div className="gallery-shell"><p className="gallery-eyebrow">Simples desde o primeiro dia</p><h2>Uma rotina melhor.<br /><span>Em três movimentos.</span></h2><div className="gallery-workflow">{[['01', 'Organize.', 'Cadastre seus alunos, configure sua marca e reúna sua biblioteca.'], ['02', 'Prescreva.', 'Crie treinos com intenção e planeje a semana de cada aluno.'], ['03', 'Acompanhe.', 'Conecte check-ins, registros e conversas para ver a evolução.']].map(([number, title, text]) => <article key={number}><span>{number}</span><h3>{title}</h3><p>{text}</p></article>)}</div></div></section>
      <section id="planos" className="gallery-section"><div className="gallery-shell"><div className="gallery-center"><p className="gallery-eyebrow">Espaço para crescer</p><h2>O plano muda.<br /><span>Seu cuidado permanece.</span></h2><p className="gallery-copy">Só o personal assina. Para o aluno, o acesso está incluído.</p></div><div className="gallery-plans">{plans.map(plan => <article key={plan.name} className={plan.featured ? 'is-featured' : ''}><span className="gallery-plan-label">{plan.featured ? 'Para ir além' : 'Feito para o seu momento'}</span><h3>{plan.name}</h3><p>{plan.description}</p><div className="gallery-price"><small>R$</small> {plan.price}<span>/mês</span></div><ul><li><Check size={15} />Até {plan.count} alunos</li><li><Check size={15} />Treinos, agenda e evolução</li><li><Check size={15} />Sua página pública</li></ul><Link to="/register" className={`gallery-button ${plan.featured ? '' : 'gallery-button--outline'}`}>Escolher {plan.name} <ChevronRight size={15} /></Link></article>)}</div><p className="gallery-footnote">Valores do ciclo mensal. Consulte as opções trimestral e anual no cadastro.</p></div></section>
      <section id="faq" className="gallery-section gallery-band"><div className="gallery-shell gallery-faq"><h2>Bom saber.<br /><span>Antes de começar.</span></h2><div>{faqs.map(([question, answer], i) => <article key={question}><button onClick={() => setActive(active === i ? null : i)} aria-expanded={active === i} aria-controls={`faq-${i}`}>{question}<Plus size={19} className={active === i ? 'is-open' : ''} /></button><div id={`faq-${i}`} hidden={active !== i}><p>{answer}</p></div></article>)}</div></div></section>
      <section className="gallery-section gallery-final gallery-center"><div className="gallery-shell"><p className="gallery-eyebrow">Seu trabalho merece esse espaço.</p><h2>Pronto para a sua<br />melhor forma?</h2><Link to="/register" className="gallery-button">Começar agora <ArrowRight size={16} /></Link></div></section>
    </main>
    <footer className="gallery-footer"><div className="gallery-shell"><BrandLogo size="sm" /><p>Forma Training. Evolução que se acompanha.</p><div><Link to="/privacy-policy">Privacidade</Link><Link to="/terms-of-use">Termos de uso</Link>{import.meta.env.DEV && <Link to="/design-preview">Ver todas as telas</Link>}</div></div></footer>
  </div>;
}
