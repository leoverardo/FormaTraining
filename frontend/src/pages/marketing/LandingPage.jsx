import { useLayoutEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { gsap } from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import {
  Activity, ArrowRight, BarChart3, CalendarDays, Check, ChevronRight, Clock3,
  ClipboardCheck, Dumbbell, Gauge, HeartPulse, Layers3, LineChart, Menu, MessageCircle,
  ShieldCheck, Sparkles, UsersRound, X, Zap
} from 'lucide-react';
import { BrandLogo } from '../../components/brand/BrandLogo';
import './marketing.css';

gsap.registerPlugin(ScrollTrigger);

const features = [
  { icon: UsersRound, label: 'Gestão de alunos', text: 'Centralize perfis, objetivos, anamnese, evolução e comunicação em um só lugar.' },
  { icon: Dumbbell, label: 'Treinos que escalam', text: 'Crie exercícios, rotinas e prescrições com uma experiência clara para cada aluno.' },
  { icon: CalendarDays, label: 'Agenda organizada', text: 'Enxergue sessões, compromissos e a rotina semanal sem trocar de ferramenta.' },
  { icon: HeartPulse, label: 'Evolução visível', text: 'Registros, check-ins, fotos e métricas para transformar acompanhamento em resultado.' },
];

const workflow = [
  ['01', 'Estruture sua operação', 'Cadastre alunos, defina sua identidade e organize sua base de exercícios.'],
  ['02', 'Prescreva com contexto', 'Monte treinos e acompanhe o que cada pessoa precisa fazer nesta semana.'],
  ['03', 'Acompanhe a evolução', 'Use check-ins, progresso e conteúdo para manter o aluno presente no processo.'],
];

const routineViews = [
  { icon: Layers3, label: 'Planeje', eyebrow: 'A sua semana, antes de começar', title: 'Tudo que pede atenção encontra o seu lugar.', text: 'Organize alunos, agenda e prioridades sem transformar a gestão em uma segunda jornada de trabalho.', notes: ['Agenda e compromissos em contexto', 'Visão rápida da sua base de alunos', 'Próximas decisões sem procurar informação'] },
  { icon: Dumbbell, label: 'Prescreva', eyebrow: 'Método que vira experiência', title: 'Cada treino carrega a intenção por trás dele.', text: 'Monte rotinas claras e faça o aluno entender o que fazer, como fazer e por que aquilo importa.', notes: ['Biblioteca organizada pelo seu método', 'Treino legível no momento da execução', 'Próximo passo sempre visível'] },
  { icon: LineChart, label: 'Acompanhe', eyebrow: 'Resultado que não fica solto', title: 'Evolução deixa de ser uma conversa perdida.', text: 'Check-ins, fotos e registros criam uma linha do tempo que ajuda você e o aluno a enxergarem a constância.', notes: ['Check-ins que convidam à resposta', 'Histórico visual de progresso', 'Conversas com contexto real'] },
];

const plans = [
  { name: 'Starter', price: 'R$ 97/mês', detail: 'Até 20 alunos · ideal para começar a operação.', cta: 'Começar no Starter' },
  { name: 'Pro', price: 'R$ 197/mês', detail: 'Até 50 alunos · para quem já acompanha todos os dias.', cta: 'Assinar o Pro', featured: true },
  { name: 'Growth', price: 'R$ 297/mês', detail: 'Até 100 alunos · para escalar sem perder presença.', cta: 'Falar sobre Growth' },
];

const faqs = [
  { q: 'Os meus alunos pagam pela plataforma?', a: 'Não. Só o personal assina. O acesso do aluno depende da sua assinatura ativa.' },
  { q: 'Posso usar minha identidade visual?', a: 'Sim. Marca, logo, cores da sua operação e página pública com seu slug.' },
  { q: 'Como o aluno recebe acesso?', a: 'Você cadastra o aluno e ele recebe um link seguro para definir a senha.' },
  { q: 'E se minha assinatura vencer?', a: 'O painel e o acesso dos alunos são pausados até a regularização — sem perder dados.' },
];

export function LandingPage() {
  const pageRef = useRef(null);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [activeRoutineView, setActiveRoutineView] = useState(0);
  const [openFaq, setOpenFaq] = useState(0);
  const activeView = routineViews[activeRoutineView];
  const ActiveViewIcon = activeView.icon;

  useLayoutEffect(() => {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduceMotion) return undefined;
    const context = gsap.context(() => {
      gsap.utils.toArray('.landing-reveal').forEach((element) => {
        gsap.from(element, {
          y: 32,
          opacity: 0,
          duration: 0.72,
          ease: 'power3.out',
          scrollTrigger: { trigger: element, start: 'top 86%' },
        });
      });
    }, pageRef);
    return () => context.revert();
  }, []);

  return (
    <div ref={pageRef} className="landing-page bg-[#070b1c] text-white selection:bg-indigo-400/40">
      <header className="landing-header-wrap">
        <div className="landing-header">
        <Link to="/" aria-label="Forma Training — início"><BrandLogo size="sm" textClassName="text-white font-bold" imageClassName="" /></Link>
        <nav className="hidden md:flex items-center gap-7 text-sm text-slate-300" aria-label="Navegação principal">
          <a href="#produto">Produto</a><a href="#como-funciona">Como funciona</a><a href="#planos">Planos</a><a href="#faq">FAQ</a>
        </nav>
        <Link to="/login" className="landing-nav-login">Entrar <ArrowRight size={15} /></Link>
        </div>
        <button
          type="button"
          className="landing-mobile-toggle md:hidden"
          aria-label={isMenuOpen ? 'Fechar menu' : 'Abrir menu'}
          aria-expanded={isMenuOpen}
          aria-controls="landing-mobile-navigation"
          onClick={() => setIsMenuOpen((isOpen) => !isOpen)}
        >
          {isMenuOpen ? <X size={19} /> : <Menu size={19} />}
        </button>
        <nav id="landing-mobile-navigation" className={`landing-mobile-panel md:hidden ${isMenuOpen ? 'is-open' : ''}`} aria-label="Navegação mobile">
          <a href="#produto" onClick={() => setIsMenuOpen(false)}>Produto</a>
          <a href="#para-personais" onClick={() => setIsMenuOpen(false)}>Experiência do aluno</a>
          <a href="#como-funciona" onClick={() => setIsMenuOpen(false)}>Como funciona</a>
          <Link to="/login" onClick={() => setIsMenuOpen(false)}>Acessar plataforma <ArrowRight size={16} /></Link>
        </nav>
      </header>

      <main>
        <section className="landing-hero landing-hero--premium overflow-hidden">
          <div className="landing-grid" aria-hidden="true" />
          <div className="landing-orb" aria-hidden="true" />
          <div className="landing-glow" aria-hidden="true" />
          <div className="landing-shell landing-hero-split">
            <div className="landing-hero-copy relative z-10">
              <div className="landing-eyebrow landing-pill"><Sparkles size={14} /> Gestão para personal trainers</div>
              <h1 className="landing-display mt-6 text-left">O seu método merece<br />uma <span>operação à altura.</span></h1>
              <p className="mt-6 text-lg leading-8 text-slate-300" style={{ maxWidth: '30rem' }}>Alunos, treinos, agenda e evolução em uma experiência feita para cuidar melhor — e crescer com clareza.</p>
              <div className="mt-8 flex flex-col sm:flex-row gap-3 items-start sm:items-center">
                <Link to="/register" className="landing-primary-cta landing-cta-pill">Começar minha operação <ArrowRight size={18} /></Link>
                <a href="#produto" className="landing-secondary-link">Conhecer a plataforma <ChevronRight size={17} /></a>
              </div>
              <div className="mt-10 grid grid-cols-2 gap-5 max-w-md">
                <div className="landing-mini-proof"><span className="landing-mini-icon"><Zap size={17} /></span><div><strong>Rápido de operar</strong><small>Prescreva em minutos, não em dias.</small></div></div>
                <div className="landing-mini-proof"><span className="landing-mini-icon"><ShieldCheck size={17} /></span><div><strong>Confiança visível</strong><small>O aluno percebe cada entrega.</small></div></div>
              </div>
            </div>
            <div className="landing-hero-visual landing-product-compo relative" aria-label="Prévia do painel Forma Training">
              <div className="landing-dash-window">
                <div className="landing-canvas-top"><span /><span /><span /><b>Forma Training · painel</b><em><BarChart3 size={14} /> Hoje</em></div>
                <div className="landing-dash-body">
                  <p className="landing-dash-label">Constância da semana</p>
                  <p className="landing-dash-value">4 de 5 treinos</p>
                  <svg viewBox="0 0 320 110" className="landing-dash-chart" aria-hidden="true">
                    <defs><linearGradient id="formaArea" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#818cf8" stopOpacity=".45" /><stop offset="1" stopColor="#818cf8" stopOpacity="0" /></linearGradient></defs>
                    <path d="M0,85 L25,78 L50,82 L75,60 L100,64 L125,45 L150,52 L175,35 L200,42 L225,28 L250,34 L275,20 L300,26 L320,16 L320,110 L0,110 Z" fill="url(#formaArea)" />
                    <path d="M0,85 L25,78 L50,82 L75,60 L100,64 L125,45 L150,52 L175,35 L200,42 L225,28 L250,34 L275,20 L300,26 L320,16" fill="none" stroke="#6366f1" strokeWidth="2.5" strokeLinejoin="round" strokeLinecap="round" />
                  </svg>
                  <div className="landing-dash-stats"><div><small>Alunos ativos</small><strong>24</strong></div><div><small>Check-ins</small><strong>94%</strong></div><div><small>Próximo</small><strong>Treino A</strong></div></div>
                </div>
              </div>
              <div className="landing-phone-window">
                <div className="landing-phone-notch" />
                <p className="landing-dash-label">Sua semana</p>
                <p className="landing-phone-value">Treino A · 45 min</p>
                <div className="landing-phone-card"><Dumbbell size={16} /><div><small>Hoje</small><strong>Inferiores · 6 exercícios</strong></div></div>
                <div className="landing-phone-card"><Clock3 size={16} /><div><small>Check-in</small><strong>Em 2 dias</strong></div></div>
              </div>
            </div>
          </div>
        </section>

        <section className="landing-trust"><div className="landing-shell"><p>UMA OPERAÇÃO MAIS CLARA PARA QUEM TRANSFORMA MOVIMENTO EM RESULTADO</p><div className="grid grid-cols-2 sm:grid-cols-4 gap-5 mt-6 text-slate-400 font-semibold tracking-[0.18em] text-xs sm:text-sm"><span>ALUNOS</span><span>TREINOS</span><span>EVOLUÇÃO</span><span>ROTINA</span></div></div></section>

        <section className="landing-statement landing-section--light">
          <div className="landing-shell landing-statement-grid">
            <div className="landing-reveal landing-statement-copy">
              <p className="landing-kicker">A FORMA DA SUA OPERAÇÃO</p>
              <h2 className="landing-heading">Quando cada detalhe se conecta, o seu trabalho ganha espaço para aparecer.</h2>
            </div>
            <div className="landing-statement-notes landing-reveal">
              <article><Activity size={19} /><div><strong>Menos ruído</strong><span>Informações, mensagens e rotinas deixam de competir pela sua atenção.</span></div></article>
              <article><Gauge size={19} /><div><strong>Mais ritmo</strong><span>Você encontra a próxima ação sem precisar reconstruir o contexto.</span></div></article>
              <article><ShieldCheck size={19} /><div><strong>Mais confiança</strong><span>O aluno percebe a qualidade do acompanhamento em cada ponto de contato.</span></div></article>
            </div>
          </div>
        </section>

        <section id="produto" className="landing-section landing-section--light">
          <div className="landing-shell">
            <div className="grid lg:grid-cols-[.8fr_1.2fr] gap-10 lg:gap-20 items-end">
              <div className="landing-reveal"><p className="landing-kicker">UMA PLATAFORMA, SEU MÉTODO</p><h2 className="landing-heading">Menos gestão manual. Mais presença no que importa.</h2></div>
              <p className="landing-reveal text-slate-600 text-lg leading-8 max-w-xl">A plataforma foi desenhada para deixar a operação silenciosa: dados fáceis de encontrar, rotinas fáceis de executar e uma experiência que dá valor ao seu trabalho.</p>
            </div>
            <div className="grid md:grid-cols-2 gap-px mt-14 overflow-hidden rounded-[2rem] border border-slate-200 bg-slate-200">
              {features.map(({ icon: Icon, label, text }, index) => <article key={label} className="landing-feature landing-reveal"><div className="landing-feature-number">0{index + 1}</div><div className="landing-feature-icon"><Icon size={22} /></div><h3>{label}</h3><p>{text}</p><span className="landing-feature-arrow"><ChevronRight size={20} /></span></article>)}
            </div>
          </div>
        </section>

        <section id="experiencia" className="landing-routine-section">
          <div className="landing-shell">
            <div className="landing-routine-heading landing-reveal">
              <div><p className="landing-kicker text-indigo-200">UMA ROTINA, VÁRIAS PERSPECTIVAS</p><h2 className="landing-heading text-white">O mesmo método. A tela certa para cada momento.</h2></div>
              <p>Uma operação bem resolvida não mostra tudo de uma vez. Ela entrega o que importa agora — para você decidir e para o aluno seguir.</p>
            </div>
            <div className="landing-routine-stage landing-reveal">
              <div className="landing-routine-tabs" role="tablist" aria-label="Etapas da rotina Forma">
                {routineViews.map(({ icon: Icon, label }, index) => <button key={label} type="button" role="tab" aria-selected={activeRoutineView === index} className={activeRoutineView === index ? 'is-active' : ''} onClick={() => setActiveRoutineView(index)}><Icon size={17} />{label}</button>)}
              </div>
              <div className="landing-routine-content">
                <div className="landing-routine-copy">
                  <p>{activeView.eyebrow}</p><h3>{activeView.title}</h3><span>{activeView.text}</span>
                  <ul>{activeView.notes.map((note) => <li key={note}><Check size={17} />{note}</li>)}</ul>
                </div>
                <div className="landing-routine-canvas" data-view={activeRoutineView} aria-label={`Demonstração: ${activeView.label}`}>
                  <div className="landing-canvas-top"><span /><span /><span /><b>Forma Training</b><em><ActiveViewIcon size={15} /> Hoje</em></div>
                  <div className="landing-canvas-body">
                    <div className="landing-canvas-hero"><div><small>VISÃO DA OPERAÇÃO</small><strong>{activeView.label === 'Planeje' ? 'Sua semana, em ordem.' : activeView.label === 'Prescreva' ? 'Treino A · inferiores' : 'Constância em construção.'}</strong></div><i /></div>
                    <div className="landing-canvas-grid"><div className="landing-canvas-card"><ClipboardCheck size={18} /><span>{activeView.label === 'Planeje' ? '4 prioridades' : activeView.label === 'Prescreva' ? '6 exercícios' : '3 check-ins'}</span><small>em foco hoje</small></div><div className="landing-canvas-card"><UsersRound size={18} /><span>{activeView.label === 'Acompanhe' ? '12 respostas' : '24 alunos'}</span><small>no seu acompanhamento</small></div></div>
                    <div className="landing-canvas-line"><span /><span /><span /><span /><i /></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section id="para-personais" className="landing-proof-section">
          <div className="landing-shell grid lg:grid-cols-2 gap-12 items-center">
            <div className="landing-product-window landing-reveal">
              <div className="landing-window-top"><span /><span /><span /><b>Visão do aluno</b></div>
              <div className="p-5 sm:p-7"><div className="flex justify-between items-start"><div><p className="text-slate-400 text-xs">Olá, Lucas</p><h3 className="text-xl font-bold mt-1">Sua semana de treino</h3></div><div className="landing-window-avatar">L</div></div><div className="landing-progress mt-7"><div className="flex justify-between text-xs"><span>Consistência da semana</span><b>4 de 5</b></div><div><i /></div></div><div className="grid grid-cols-2 gap-3 mt-6"><div className="landing-mini-card"><Dumbbell size={17} /><span>Treino A</span><small>Hoje · 45 min</small></div><div className="landing-mini-card"><Clock3 size={17} /><span>Check-in</span><small>Em 2 dias</small></div></div><div className="landing-message mt-5"><MessageCircle size={16} /><span>Seu treinador comentou seu progresso.</span></div></div>
            </div>
            <div className="landing-reveal"><p className="landing-kicker text-indigo-200">FEITO PARA OS DOIS LADOS</p><h2 className="landing-heading text-white">A experiência do aluno também é parte do seu serviço.</h2><p className="mt-6 text-slate-300 text-lg leading-8">Quando o aluno entende o próximo passo, vê a própria evolução e encontra seu treinador sem esforço, a constância deixa de depender de lembretes soltos.</p><ul className="mt-8 space-y-4 text-slate-200">{['Treinos acessíveis no momento certo', 'Progresso físico e histórico em uma linha do tempo', 'Conteúdos e comunicação no mesmo ambiente'].map((item) => <li key={item} className="flex gap-3"><Check className="text-cyan-300 shrink-0" size={20} />{item}</li>)}</ul></div>
          </div>
        </section>

        <section id="como-funciona" className="landing-section landing-section--cream">
          <div className="landing-shell"><div className="landing-reveal max-w-2xl"><p className="landing-kicker">COMECE COM O ESSENCIAL</p><h2 className="landing-heading">Uma rotina mais profissional começa em três movimentos.</h2></div><div className="grid md:grid-cols-3 gap-6 mt-14">{workflow.map(([number, title, text]) => <article key={number} className="landing-step landing-reveal"><span>{number}</span><h3>{title}</h3><p>{text}</p><div><ArrowRight size={20} /></div></article>)}</div></div>
        </section>

        <section id="planos" className="landing-section landing-section--light">
          <div className="landing-shell">
            <div className="landing-reveal max-w-2xl"><p className="landing-kicker">PLANOS PARA CADA FASE</p><h2 className="landing-heading">Só o personal assina. O aluno não paga nada.</h2><p className="mt-5 text-slate-600 text-lg leading-8">Ciclos mensal, trimestral e anual. Troque de plano quando sua base crescer.</p></div>
            <div className="grid md:grid-cols-3 gap-5 mt-12">
              {plans.map((plan) => (
                <article key={plan.name} className={`landing-step landing-reveal ${plan.featured ? 'ring-2 ring-indigo-500' : ''}`}>
                  <span>{plan.name.toUpperCase()}</span>
                  <h3 className="text-2xl">{plan.price}</h3>
                  <p>{plan.detail}</p>
                  <div className="mt-6"><Link to="/register" className="landing-primary-cta !bg-indigo-600 !text-white w-full">{plan.cta} <ArrowRight size={17} /></Link></div>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section id="faq" className="landing-section landing-section--light !pt-0">
          <div className="landing-shell grid lg:grid-cols-[.9fr_1.1fr] gap-10">
            <div className="landing-reveal"><p className="landing-kicker">PERGUNTAS FREQUENTES</p><h2 className="landing-heading">Claro desde o primeiro contato.</h2></div>
            <div className="ds-stack">
              {faqs.map((item, i) => (
                <div key={item.q} className="ds-card p-5">
                  <button type="button" onClick={() => setOpenFaq(i)} aria-expanded={openFaq === i} className="w-full flex justify-between items-center gap-4 text-left font-bold">
                    <span>{item.q}</span><ChevronRight size={18} className={openFaq === i ? 'rotate-90 transition' : 'transition'} />
                  </button>
                  {openFaq === i && <p className="mt-3 text-slate-600 leading-7">{item.a}</p>}
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="landing-rhythm-section">
          <div className="landing-shell">
            <div className="landing-rhythm-intro landing-reveal"><p className="landing-kicker">PRESENÇA QUE SE PERCEBE</p><h2 className="landing-heading">Cada etapa parece parte da mesma experiência.</h2><p>Da primeira prescrição ao próximo check-in, a Forma cria continuidade. Não é sobre adicionar telas: é sobre deixar o acompanhamento mais simples de sentir e mais fácil de sustentar.</p></div>
            <div className="landing-rhythm-grid">
              <article className="landing-rhythm-card landing-reveal"><span>01</span><CalendarDays size={25} /><h3>A rotina encontra o tempo</h3><p>Agenda, compromissos e tarefas em uma visão que respeita o seu dia.</p><div className="landing-rhythm-calendar"><i /><i /><i /><i /><i /></div></article>
              <article className="landing-rhythm-card landing-rhythm-card--indigo landing-reveal"><span>02</span><MessageCircle size={25} /><h3>O aluno encontra contexto</h3><p>O recado, a rotina e o progresso vivem no mesmo lugar.</p><div className="landing-rhythm-message"><b>Hoje você avançou.</b><small>Seu próximo passo já está pronto.</small></div></article>
              <article className="landing-rhythm-card landing-reveal"><span>03</span><HeartPulse size={25} /><h3>Você encontra evolução</h3><p>Registros viram uma história contínua, não planilhas soltas.</p><div className="landing-rhythm-pulse"><i /><i /><i /><i /><i /><i /></div></article>
            </div>
          </div>
        </section>

        <section className="landing-final-cta"><div className="landing-shell relative"><div className="landing-final-glow" aria-hidden="true" /><div className="landing-reveal max-w-3xl relative"><p className="landing-kicker text-indigo-200">SUA PRÓXIMA FASE</p><h2 className="landing-display text-5xl sm:text-6xl">Seu trabalho já é personalizado. Sua plataforma também pode ser.</h2><p className="mt-7 max-w-2xl text-lg leading-8 text-slate-300">Uma base mais clara para você atender com intenção, acompanhar com presença e crescer sem perder o seu método.</p><div className="mt-9 flex flex-col sm:flex-row gap-3"><Link to="/register" className="landing-primary-cta">Criar minha conta <ArrowRight size={18} /></Link><Link to="/login" className="landing-secondary-cta">Já tenho acesso</Link></div></div></div></section>
      </main>

      <footer className="landing-footer"><div className="landing-shell flex flex-col sm:flex-row justify-between gap-5"><BrandLogo size="sm" textClassName="text-white" /><p>Forma Training · estrutura para acompanhar evolução.</p><div className="flex gap-5"><Link to="/privacy-policy">Privacidade</Link><Link to="/terms-of-use">Termos</Link></div></div></footer>
    </div>
  );
}
