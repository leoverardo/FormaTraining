import { Link } from 'react-router-dom';
import {
  ArrowRight, Lock, Download, Check, Ban, Type, Palette, Space,
  MousePointerClick, Shapes, Film, LayoutGrid, Accessibility, Gauge, Image as ImageIcon,
} from 'lucide-react';
import { BrandLogo, UpSymbol } from '../../components/brand/BrandLogo';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { Input } from '../../components/ui/Input';
import { Card, CardHeader, CardContent } from '../../components/ui/Card';

const COLORS = [
  { name: 'Índigo', hex: '#6366F1', rgb: 'rgb(99, 102, 241)', token: '--brand-indigo / --primary', use: 'Cor primária da marca e do produto.' },
  { name: 'Índigo forte', hex: '#4F46E5', rgb: 'rgb(79, 70, 229)', token: '--brand-indigo-strong / --primary-strong', use: 'Ações, CTAs e eyebrow editorial.' },
  { name: 'Índigo claro', hex: '#818CF8', rgb: 'rgb(129, 140, 248)', token: '--brand-indigo-soft', use: 'Realces, dark mode e gradientes.' },
  { name: 'Ciano', hex: '#06B6D4', rgb: 'rgb(6, 182, 212)', token: '--brand-cyan', use: 'Acento de energia (seta do símbolo). Uso moderado.' },
];

const TYPE_SCALE = [
  { label: 'Hero · 80px', cls: 'text-[clamp(40px,6vw,80px)] leading-[1.05] tracking-[-0.015em] font-semibold' },
  { label: 'Display · 56px', cls: 'text-[clamp(32px,5vw,56px)] leading-[1.05] tracking-[-0.015em] font-semibold' },
  { label: 'Heading · 40px', cls: 'text-[clamp(28px,4vw,40px)] leading-[1.1] tracking-[-0.01em] font-semibold' },
  { label: 'Heading SM · 28px', cls: 'text-[28px] leading-[1.15] tracking-[-0.01em] font-semibold' },
  { label: 'Product · 21px', cls: 'text-[21px] leading-[1.6]' },
  { label: 'Body · 17px', cls: 'text-[17px] leading-[1.65]' },
];

const NAV = [
  { to: '#marca', icon: Shapes, label: 'Marca' },
  { to: '#cores', icon: Palette, label: 'Cores' },
  { to: '#tipografia', icon: Type, label: 'Tipografia' },
  { to: '#espacamento', icon: Space, label: 'Espaço & Raio' },
  { to: '#componentes', icon: MousePointerClick, label: 'Componentes' },
  { to: '#icones', icon: Check, label: 'Ícones' },
  { to: '#motion', icon: Film, label: 'Motion' },
  { to: '#layout', icon: LayoutGrid, label: 'Layout' },
  { to: '#acessibilidade', icon: Accessibility, label: 'A11y & Performance' },
];

function Section({ id, kicker, title, lead, children }) {
  return (
    <section id={id} className="scroll-mt-24 py-14 border-t border-slate-200 first:border-t-0 first:pt-0">
      <p className="uc-eyebrow">{kicker}</p>
      <h2 className="uc-heading mt-3 text-slate-900">{title}</h2>
      {lead && <p className="uc-lead mt-4 max-w-2xl">{lead}</p>}
      <div className="mt-8">{children}</div>
    </section>
  );
}

function Rule({ ok, children }) {
  return (
    <li className="flex items-start gap-2.5 text-sm text-slate-600">
      <span className={`mt-0.5 grid h-5 w-5 shrink-0 place-items-center rounded-full ${ok ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-600'}`}>
        {ok ? <Check size={13} /> : <Ban size={13} />}
      </span>
      <span>{children}</span>
    </li>
  );
}

export function BrandPage() {
  return (
    <div className="min-h-screen bg-[#FAFAFC] text-slate-900">
      <header className="border-b border-slate-200 bg-white/90 backdrop-blur sticky top-0 z-40">
        <div className="uc-container flex h-16 items-center justify-between gap-4">
          <BrandLogo size="sm" />
          <div className="flex items-center gap-3">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-slate-900 px-3 py-1.5 text-xs font-semibold text-white">
              <Lock size={13} /> Uso interno
            </span>
            <Link to="/login" className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-indigo-600 hover:text-indigo-800">
              Ir para o app <ArrowRight size={15} />
            </Link>
          </div>
        </div>
      </header>

      <main className="uc-container pb-24">
        <div className="py-14 sm:py-20 max-w-3xl">
          <p className="uc-eyebrow">UpCoach · Brand & Design System</p>
          <h1 className="uc-display mt-4 text-slate-900">Treine, gerencie, conquiste.</h1>
          <p className="uc-lead mt-6">
            Esta é a página-index do nosso sistema de design: marca, cores, tipografia,
            componentes, motion e regras de uso. Ela é a referência para evoluir o produto
            com consistência — sem copiar a Apple, mas com o mesmo rigor de qualidade.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <a href="/favicon.svg" download className="inline-flex">
              <Button><Download size={16} /> Baixar favicon</Button>
            </a>
            <a href="/brand/upcoach-symbol.svg" download className="inline-flex">
              <Button variant="outline"><Download size={16} /> Baixar símbolo</Button>
            </a>
          </div>
        </div>

        <nav className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-9 gap-2 pb-4" aria-label="Seções do design system">
          {NAV.map(({ to, icon: Icon, label }) => (
            <a key={to} href={to} className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-[13px] font-semibold text-slate-600 hover:border-indigo-300 hover:text-indigo-700 transition">
              <Icon size={15} className="text-indigo-500 shrink-0" /> {label}
            </a>
          ))}
        </nav>

        <Section
          id="marca"
          kicker="01 · Marca"
          title="UpCoach"
          lead="Símbolo U com seta ascendente: evolução e resultado. Tagline oficial: Mais alunos · Melhores resultados. Assinatura de campanha: Treine > Gerencie > Conquiste."
        >
          <div className="grid gap-4 md:grid-cols-3">
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Primária · fundo claro</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-white"><BrandLogo size="lg" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Mono · sobre índigo</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-indigo-600"><BrandLogo size="lg" variant="mono" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Mono · sobre noite</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-[#0B1027]"><BrandLogo size="lg" variant="mono" /></CardContent></Card>
          </div>

          <div className="grid gap-4 md:grid-cols-3 mt-4">
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Símbolo isolado</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center"><UpSymbol className="h-20 w-20" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">App icon / favicon</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center gap-5">
                <img src="/favicon.svg" alt="Ícone UpCoach 48px" className="h-12 w-12" />
                <img src="/favicon.svg" alt="Ícone UpCoach 32px" className="h-8 w-8" />
                <img src="/favicon.svg" alt="Ícone UpCoach 16px" className="h-4 w-4" />
              </CardContent></Card>
            <div className="rounded-2xl bg-[#0B1027] p-6 text-white flex flex-col justify-center">
              <p className="text-xs font-bold uppercase tracking-[0.2em] text-indigo-300">Treine <span className="text-cyan-400">&gt;</span> Gerencie <span className="text-cyan-400">&gt;</span> Conquiste</p>
              <p className="mt-3 text-sm text-slate-300">A plataforma completa para o personal trainer.</p>
            </div>
          </div>

          <Card className="mt-4">
            <CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Board oficial da marca</p></CardHeader>
            <CardContent>
              <img
                src="/brand/upcoach-brand-board.jpg"
                alt="Board oficial da marca UpCoach: logo, símbolo, paleta e tagline"
                className="w-full rounded-xl border border-slate-200"
                onError={(e) => { e.currentTarget.style.display = 'none'; }}
              />
              <p className="mt-3 flex items-start gap-2 text-sm text-slate-500">
                <ImageIcon size={16} className="mt-0.5 shrink-0 text-indigo-500" />
                Para exibir o board oficial aqui, salve a imagem da marca como <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs font-semibold text-slate-700">frontend/public/brand/upcoach-brand-board.jpg</code>.
              </p>
              <ul className="mt-5 grid gap-2.5 sm:grid-cols-2">
                <Rule ok>Área de respiro mínima = altura da seta do símbolo.</Rule>
                <Rule ok>Tamanho mínimo digital: 24px (símbolo) / 96px (lockup).</Rule>
                <Rule ok>Fundo claro: lockup primária. Fundo índigo/noite/foto: mono branca.</Rule>
                <Rule ok>Ciano aparece só como acento (seta, detalhes).</Rule>
                <Rule>Não esticar, girar, trocar cores ou aplicar sombras no logo.</Rule>
                <Rule>Não usar SF Symbols nem assets da Apple — Lucide + SVG próprio.</Rule>
              </ul>
            </CardContent>
          </Card>
        </Section>

        <Section
          id="cores"
          kicker="02 · Cor"
          title="Paleta UpCoach"
          lead="Índigo conduz, ciano acentua. Fundo claro, texto escuro, cor de ação com moderação — nada de interface inteira em azul."
        >
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {COLORS.map((c) => (
              <Card key={c.hex} className="overflow-hidden">
                <div className="h-24" style={{ background: c.hex }} />
                <CardContent>
                  <p className="font-bold text-slate-900">{c.name}</p>
                  <p className="mt-1 font-mono text-xs text-slate-500">{c.hex} · {c.rgb}</p>
                  <p className="mt-1 font-mono text-[11px] text-indigo-600">{c.token}</p>
                  <p className="mt-2 text-[13px] text-slate-600">{c.use}</p>
                </CardContent>
              </Card>
            ))}
          </div>
          <ul className="mt-5 grid gap-2.5 sm:grid-cols-2">
            <Rule ok>Texto principal sempre escuro (#0F172A) sobre fundo claro.</Rule>
            <Rule ok>Tokens em <code>src/index.css</code> — nunca hardcodar hex em componente.</Rule>
            <Rule>Não criar novas cores de marca sem atualizar esta página.</Rule>
            <Rule>Dark mode troca superfície, não a identidade (tokens --primary viram #818CF8).</Rule>
          </ul>
        </Section>

        <Section
          id="tipografia"
          kicker="03 · Tipografia"
          title="Plus Jakarta Sans"
          lead="Alternativa web legal e consistente à SF Pro. Pesos moderados, tracking negativo cuidadoso nos títulos, sem gradiente/sombra em texto."
        >
          <Card>
            <CardContent className="divide-y divide-slate-100">
              {TYPE_SCALE.map((t) => (
                <div key={t.label} className="grid gap-1 py-5 sm:grid-cols-[180px_1fr] sm:items-baseline">
                  <p className="font-mono text-xs text-slate-400">{t.label}</p>
                  <p className={`${t.cls} text-slate-900`}>O seu método merece uma operação à altura.</p>
                </div>
              ))}
              <div className="grid gap-1 py-5 sm:grid-cols-[180px_1fr] sm:items-baseline">
                <p className="font-mono text-xs text-slate-400">Eyebrow · 12px caps</p>
                <p className="uc-eyebrow">Gestão para personal trainers</p>
              </div>
            </CardContent>
          </Card>
        </Section>

        <Section
          id="espacamento"
          kicker="04 · Espaço, raio e sombra"
          title="Respiro é design"
          lead="Base de 4px, radius por hierarquia (cards 28px, pills total) e sombra só com função — separação por espaço e superfície primeiro."
        >
          <div className="grid gap-4 md:grid-cols-3">
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Espaçamento (4px)</p></CardHeader>
              <CardContent className="flex flex-wrap items-end gap-3">
                {[4, 8, 16, 24, 40, 64].map((v) => (
                  <div key={v} className="text-center">
                    <div className="mx-auto bg-indigo-500/80" style={{ width: v, height: v }} />
                    <p className="mt-1 font-mono text-[11px] text-slate-500">{v}</p>
                  </div>
                ))}
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Radius</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-3">
                <span className="bg-indigo-100 px-4 py-2 text-sm font-semibold" style={{ borderRadius: '10px' }}>10px</span>
                <span className="bg-indigo-100 px-4 py-2 text-sm font-semibold" style={{ borderRadius: '20px' }}>20px</span>
                <span className="bg-indigo-100 px-4 py-2 text-sm font-semibold" style={{ borderRadius: '28px' }}>28px</span>
                <span className="bg-indigo-600 px-4 py-2 text-sm font-semibold text-white" style={{ borderRadius: '9999px' }}>pill</span>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Sombra funcional</p></CardHeader>
              <CardContent className="flex items-center gap-4">
                <div className="rounded-[28px] border border-slate-200 bg-white px-5 py-4 text-sm font-semibold">sem sombra</div>
                <div className="rounded-[28px] bg-white px-5 py-4 text-sm font-semibold" style={{ boxShadow: '0 8px 30px rgba(15,23,42,0.06)' }}>soft</div>
              </CardContent></Card>
          </div>
        </Section>

        <Section
          id="componentes"
          kicker="05 · Componentes"
          title="Primitivas do produto"
          lead="Mesma API em todas as páginas: Button, Badge, Input, Card. Estados obrigatórios: default, hover, active, focus, disabled."
        >
          <div className="grid gap-4 lg:grid-cols-2">
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Buttons · pill 44px</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-3">
                <Button>Primary</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="outline">Outline</Button>
                <Button variant="ghost">Ghost</Button>
                <Button variant="danger">Danger</Button>
                <Button size="sm">Small</Button>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Badges</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-2">
                <Badge variant="success">Ativo</Badge>
                <Badge variant="warning">Pendente</Badge>
                <Badge variant="danger">Vencido</Badge>
                <Badge variant="info">Base UpCoach</Badge>
                <Badge>Neutro</Badge>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Input</p></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <Input label="Nome" placeholder="Como te chamam?" hint="Visível no seu perfil público." />
                <Input label="E-mail" placeholder="voce@studio.com" error="Use um e-mail válido." />
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-bold uppercase tracking-[0.12em] text-slate-400">Card editorial</p></CardHeader>
              <CardContent>
                <p className="uc-eyebrow">Exemplo</p>
                <p className="mt-2 text-lg font-bold text-slate-900">Cards grandes, poucos por tela.</p>
                <p className="mt-1 text-sm text-slate-600">Whitespace separa; superfície confirma. Nem todo conteúdo precisa de card.</p>
              </CardContent></Card>
          </div>
        </Section>

        <Section
          id="icones"
          kicker="06 · Ícones"
          title="Lucide, traço consistente"
          lead="Tamanhos 16/20/24, stroke 1.5–2px, cantos arredondados. Equivalente web aos princípios SF Symbols — sem violar licença."
        >
          <Card><CardContent className="flex flex-wrap items-center gap-6 text-indigo-600">
            <ArrowRight size={16} /><ArrowRight size={20} /><ArrowRight size={24} />
            <Check size={20} /><Download size={20} /><Lock size={20} />
            <Palette size={20} /><Type size={20} /><Film size={20} />
          </CardContent></Card>
        </Section>

        <Section
          id="motion"
          kicker="07 · Motion"
          title="Movimento com propósito"
          lead="Toda animação responde: por que isso está se movendo? CSS para hover/focus; GSAP para scroll e coreografia. Calma, precisão, fluidez."
        >
          <Card><CardContent className="grid gap-3 font-mono text-[13px] text-slate-600 sm:grid-cols-2">
            <p><span className="text-indigo-600">--ease-apple</span> cubic-bezier(0.22, 1, 0.36, 1)</p>
            <p><span className="text-indigo-600">--duration-fast</span> 180ms · micro</p>
            <p><span className="text-indigo-600">--duration-medium</span> 400ms · reveals</p>
            <p><span className="text-indigo-600">--duration-slow</span> 800ms · storytelling</p>
          </CardContent></Card>
          <ul className="mt-5 grid gap-2.5 sm:grid-cols-2">
            <Rule ok>Hover: scale 1.02 · active: 0.98 — nada de bounce.</Rule>
            <Rule ok>Respeitar <code>prefers-reduced-motion</code> (já global no CSS).</Rule>
            <Rule>Nada de parallax excessivo, particles ou gamer UI.</Rule>
            <Rule>Botão/link: CSS. Sticky scenes e scroll: GSAP + ScrollTrigger.</Rule>
          </ul>
        </Section>

        <Section
          id="layout"
          kicker="08 · Layout"
          title="Um container, um ritmo"
          lead="Desktop 1200px · tablet 960px · mobile full com respiro. Breakpoints centralizados, tipografia com clamp()."
        >
          <Card><CardContent className="grid gap-3 font-mono text-[13px] text-slate-600 sm:grid-cols-2">
            <p><span className="text-indigo-600">.uc-container</span> min(100% − 48px, 1200px)</p>
            <p><span className="text-indigo-600">--content-width-tablet</span> 960px</p>
            <p><span className="text-indigo-600">.uc-display</span> clamp(40px, 6vw, 80px)</p>
            <p><span className="text-indigo-600">.uc-section</span> ritmo vertical por clamp()</p>
          </CardContent></Card>
        </Section>

        <Section
          id="acessibilidade"
          kicker="09 · Acessibilidade & performance"
          title="Premium inclui todo mundo"
          lead="Ordem de prioridade: funcionalidade → acessibilidade → performance → responsividade → consistência → motion."
        >
          <ul className="grid gap-2.5 sm:grid-cols-2">
            <Rule ok>Touch mínimo 44px · foco visível em todo interativo.</Rule>
            <Rule ok>HTML semântico, ARIA só quando necessário, alt em imagens.</Rule>
            <Rule ok>Animações em transform/opacity, lazy em mídia, sem vídeo gigante no LCP.</Rule>
            <Rule ok>Contraste AA: texto slate-900/600 sobre branco; índigo-600 para ação.</Rule>
          </ul>
          <div className="mt-8 flex items-center justify-between rounded-2xl bg-[#0B1027] p-6 text-white">
            <div className="flex items-center gap-3">
              <Gauge size={20} className="text-cyan-400" />
              <p className="text-sm text-slate-300">Referências: Apple HIG · Design Resources · Fonts · SF Symbols · Motion · Typography · Color · Accessibility.</p>
            </div>
            <Link to="/login" className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-indigo-300 hover:text-white">
              Voltar ao app <ArrowRight size={15} />
            </Link>
          </div>
        </Section>
      </main>
    </div>
  );
}
