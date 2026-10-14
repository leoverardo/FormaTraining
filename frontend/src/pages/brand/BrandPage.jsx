import { Link } from 'react-router-dom';
import {
  ArrowRight, Lock, Download, Check, Ban, Type, Palette, Space,
  MousePointerClick, Shapes, Film, LayoutGrid, Accessibility, Gauge,
} from 'lucide-react';
import { BrandLogo, UpSymbol } from '../../components/brand/BrandLogo';
import { BRAND_ASSETS } from '../../components/brand/brandAssets';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { Input } from '../../components/ui/Input';
import { ThemeToggle } from '../../components/ui/ThemeToggle';
import { Card, CardHeader, CardContent } from '../../components/ui/Card';

const COLORS = [
  { name: 'Índigo', hex: '#6366F1', rgb: 'rgb(99, 102, 241)', token: '--brand-indigo / --primary', use: 'Cor primária da marca e do produto.' },
  { name: 'Índigo forte', hex: '#4F46E5', rgb: 'rgb(79, 70, 229)', token: '--brand-indigo-strong / --primary-strong', use: 'Ações, CTAs e eyebrow editorial.' },
  { name: 'Índigo claro', hex: '#818CF8', rgb: 'rgb(129, 140, 248)', token: '--brand-indigo-soft', use: 'Realces e dark mode.' },
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

const COVERS = [
  { src: BRAND_ASSETS.cover, title: 'Capa principal', description: 'Apresentação da plataforma na tela de login.', width: 1672, height: 941 },
  { src: BRAND_ASSETS.secondaryCover, title: 'Capa secundária', description: 'Boas-vindas ao cadastro de personal trainer.', width: 1672, height: 941 },
  { src: BRAND_ASSETS.subCover, title: 'Subcapa', description: 'Treine, gerencie, conquiste. A plataforma completa para o personal trainer.', width: 1916, height: 821 },
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
    <section id={id} className="scroll-mt-24 py-[var(--section-gap)] border-t border-slate-200 first:border-t-0 first:pt-0">
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
      <span className={`mt-0.5 grid h-5 w-5 shrink-0 place-items-center rounded-full ${ok ? 'bg-[var(--status-success-bg)] text-[var(--status-success-text)]' : 'bg-[var(--status-danger-bg)] text-[var(--status-danger-text)]'}`}>
        {ok ? <Check size={13} /> : <Ban size={13} />}
      </span>
      <span>{children}</span>
    </li>
  );
}

export function BrandPage() {
  return (
    <div className="min-h-screen bg-[var(--bg)] text-slate-900">
      <header className="border-b border-slate-200 bg-[var(--surface)] sticky top-0 z-40">
        <div className="uc-container flex h-16 items-center justify-between gap-4">
          <BrandLogo size="sm" />
          <div className="flex items-center gap-3">
            <ThemeToggle />
            <span className="inline-flex items-center gap-1.5 rounded-full bg-[var(--primary-strong)] px-3 py-1.5 text-xs font-semibold text-white">
              <Lock size={13} /> Uso interno
            </span>
            <Link to="/login" className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-[var(--link)] hover:text-[var(--link)]">
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
            <a href={BRAND_ASSETS.icons.primary} download className="inline-flex">
              <Button><Download size={16} /> Baixar favicon</Button>
            </a>
            <a href={BRAND_ASSETS.board} download className="inline-flex">
              <Button variant="outline"><Download size={16} /> Baixar board oficial</Button>
            </a>
          </div>
        </div>

        <nav className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-9 gap-2 pb-4" aria-label="Seções do design system">
          {NAV.map(({ to, icon: Icon, label }) => (
            <a key={to} href={to} className="flex items-center gap-2 rounded-xl border border-slate-200 bg-[var(--surface)] px-3 py-2.5 text-[13px] font-semibold text-slate-600 hover:border-[var(--primary)] hover:text-[var(--link)] transition">
              <Icon size={15} className="text-[var(--link)] shrink-0" /> {label}
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
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Primária · fundo claro</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-[#ffffff]"><BrandLogo size="lg" tone="light" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Secundária · sobre índigo</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-[var(--primary-strong)]"><BrandLogo size="lg" variant="mono" tone="dark" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Dark mode · sobre noite</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center bg-[#0B1027]"><BrandLogo size="lg" tone="dark" /></CardContent></Card>
          </div>

          <div className="grid gap-4 md:grid-cols-3 mt-4">
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Ícone principal</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center"><UpSymbol primary className="h-20 w-20" /></CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">App icon / favicon</p></CardHeader>
              <CardContent className="flex h-36 items-center justify-center gap-5">
                <img src={BRAND_ASSETS.icons.primary} alt="Ícone UpCoach 48px" width={48} height={48} className="h-12 w-12 object-contain" />
                <img src={BRAND_ASSETS.icons.primary} alt="Ícone UpCoach 32px" width={32} height={32} className="h-8 w-8 object-contain" />
                <img src={BRAND_ASSETS.icons.primary} alt="Ícone UpCoach 16px" width={16} height={16} className="h-4 w-4 object-contain" />
              </CardContent></Card>
            <div className="rounded-2xl uc-inverse bg-[var(--brand-ink)] p-6 text-white flex flex-col justify-center">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--link)]">Treine <span className="text-cyan-400">&gt;</span> Gerencie <span className="text-cyan-400">&gt;</span> Conquiste</p>
              <p className="mt-3 text-sm text-slate-300">A plataforma completa para o personal trainer.</p>
            </div>
          </div>

          <Card className="mt-4">
            <CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Board oficial da marca</p></CardHeader>
            <CardContent>
              <img
                src={BRAND_ASSETS.board}
                alt="Board oficial da marca UpCoach: logo, símbolo, paleta e tagline"
                className="w-full rounded-xl border border-slate-200"
                width={1536}
                height={1024}
                loading="lazy"
                decoding="async"
              />
              <p className="mt-3 text-sm text-slate-500">
                Referência oficial de logo, ícones, paleta e assinaturas da UpCoach.
              </p>
              <ul className="mt-5 grid gap-2.5 sm:grid-cols-2">
                <Rule ok>Área de respiro mínima = altura da seta do símbolo.</Rule>
                <Rule ok>Tamanho mínimo digital: 24px (símbolo) / 96px (lockup).</Rule>
                <Rule ok>Fundo claro: ícone branco. Fundo noite: ícone dark mode. Fundo índigo: variante secundária.</Rule>
                <Rule ok>Ciano aparece só como acento (seta, detalhes).</Rule>
                <Rule>Não esticar, girar, trocar cores ou aplicar sombras no logo.</Rule>
                <Rule>Não usar SF Symbols nem assets da Apple — Lucide e imagens oficiais UpCoach.</Rule>
              </ul>
            </CardContent>
          </Card>

          <div className="mt-8">
            <h3 className="text-xl font-semibold text-slate-900">Capas e assinatura</h3>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              {COVERS.map((cover) => (
                <Card key={cover.src} className={`overflow-hidden ${cover.src === BRAND_ASSETS.subCover ? 'md:col-span-2' : ''}`}>
                  <img src={cover.src} alt={`UpCoach — ${cover.description}`} width={cover.width} height={cover.height}
                    className="block h-auto w-full" loading="lazy" decoding="async" />
                  <CardContent>
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <div>
                        <p className="font-semibold text-slate-900">{cover.title}</p>
                        <p className="mt-1 text-sm text-slate-500">{cover.description}</p>
                      </div>
                      <a href={cover.src} download className="inline-flex items-center gap-2 text-sm font-semibold text-[var(--link)]">
                        <Download size={16} /> Baixar imagem
                      </a>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
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
                  <p className="font-semibold text-slate-900">{c.name}</p>
                  <p className="mt-1 font-mono text-xs text-slate-500">{c.hex} · {c.rgb}</p>
                  <p className="mt-1 font-mono text-[11px] text-[var(--link)]">{c.token}</p>
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
          title="SF Pro · Inter"
          lead="SF Pro quando disponível no sistema; Inter carregada localmente como alternativa web. Pesos moderados, tracking negativo cuidadoso nos títulos, sem gradiente/sombra em texto."
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
          lead="Base de 4px, radius por hierarquia (cards 28px, pills total) e superfícies sem sombra — separação por espaço e superfície."
        >
          <div className="grid gap-4 md:grid-cols-3">
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Espaçamento (4px)</p></CardHeader>
              <CardContent className="flex flex-wrap items-end gap-3">
                {[4, 8, 16, 24, 40, 64].map((v) => (
                  <div key={v} className="text-center">
                    <div className="mx-auto bg-[var(--brand-indigo)]/80" style={{ width: v, height: v }} />
                    <p className="mt-1 font-mono text-[11px] text-slate-500">{v}</p>
                  </div>
                ))}
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Radius</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-3">
                <span className="bg-[var(--primary-soft)] px-4 py-2 text-sm font-semibold" style={{ borderRadius: '10px' }}>10px</span>
                <span className="bg-[var(--primary-soft)] px-4 py-2 text-sm font-semibold" style={{ borderRadius: '20px' }}>20px</span>
                <span className="bg-[var(--primary-soft)] px-4 py-2 text-sm font-semibold" style={{ borderRadius: '28px' }}>28px</span>
                <span className="bg-[var(--primary-strong)] px-4 py-2 text-sm font-semibold text-white" style={{ borderRadius: '9999px' }}>pill</span>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Superfícies planas</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-4">
                <div className="rounded-[28px] border border-slate-200 bg-[var(--surface)] px-5 py-4 text-sm font-semibold">sem sombra</div>
                <div className="rounded-[28px] bg-[var(--surface)] px-5 py-4 text-sm font-semibold" >faixa suave</div>
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
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Buttons · pill 44px</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-3">
                <Button>Primary</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="outline">Outline</Button>
                <Button variant="ghost">Ghost</Button>
                <Button variant="danger">Danger</Button>
                <Button size="sm">Small</Button>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Badges</p></CardHeader>
              <CardContent className="flex flex-wrap items-center gap-2">
                <Badge variant="success">Ativo</Badge>
                <Badge variant="warning">Pendente</Badge>
                <Badge variant="danger">Vencido</Badge>
                <Badge variant="info">Base UpCoach</Badge>
                <Badge>Neutro</Badge>
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Input</p></CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <Input label="Nome" placeholder="Como te chamam?" hint="Visível no seu perfil público." />
                <Input label="E-mail" placeholder="voce@studio.com" error="Use um e-mail válido." />
              </CardContent></Card>
            <Card><CardHeader><p className="text-xs font-semibold uppercase tracking-[0.12em] text-slate-400">Card editorial</p></CardHeader>
              <CardContent>
                <p className="uc-eyebrow">Exemplo</p>
                <p className="mt-2 text-lg font-semibold text-slate-900">Cards grandes, poucos por tela.</p>
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
          <Card><CardContent className="flex flex-wrap items-center gap-6 text-[var(--link)]">
            <ArrowRight size={16} /><ArrowRight size={20} /><ArrowRight size={24} />
            <Check size={20} /><Download size={20} /><Lock size={20} />
            <Palette size={20} /><Type size={20} /><Film size={20} />
          </CardContent></Card>
        </Section>

        <Section
          id="motion"
          kicker="07 · Motion"
          title="Movimento com propósito"
          lead="Toda animação responde: por que isso está se movendo? CSS para respostas discretas de hover e foco. Calma, precisão, fluidez."
        >
          <Card><CardContent className="grid gap-3 font-mono text-[13px] text-slate-600 sm:grid-cols-2">
            <p><span className="text-[var(--link)]">--ease-apple</span> cubic-bezier(0.22, 1, 0.36, 1)</p>
            <p><span className="text-[var(--link)]">--duration-fast</span> 180ms · micro</p>
            <p><span className="text-[var(--link)]">--duration-medium</span> 400ms · reveals</p>
            <p><span className="text-[var(--link)]">--duration-slow</span> 800ms · storytelling</p>
          </CardContent></Card>
          <ul className="mt-5 grid gap-2.5 sm:grid-cols-2">
            <Rule ok>Hover: scale 1.02 · active: 0.98 — nada de bounce.</Rule>
            <Rule ok>Respeitar <code>prefers-reduced-motion</code> (já global no CSS).</Rule>
            <Rule>Nada de parallax excessivo, particles ou gamer UI.</Rule>
            <Rule>Botão/link: CSS. Movimento discreto, respeitando a preferência do visitante.</Rule>
          </ul>
        </Section>

        <Section
          id="layout"
          kicker="08 · Layout"
          title="Um container, um ritmo"
          lead="Desktop 1200px · tablet 960px · mobile full com respiro. Breakpoints centralizados, tipografia com clamp()."
        >
          <Card><CardContent className="grid gap-3 font-mono text-[13px] text-slate-600 sm:grid-cols-2">
            <p><span className="text-[var(--link)]">.uc-container</span> min(100% − 64px, 1200px)</p>
            <p><span className="text-[var(--link)]">--content-width-tablet</span> 960px</p>
            <p><span className="text-[var(--link)]">.uc-display</span> clamp(40px, 6vw, 80px)</p>
            <p><span className="text-[var(--link)]">.uc-section</span> ritmo vertical por clamp()</p>
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
          <div className="mt-8 flex items-center justify-between rounded-2xl uc-inverse bg-[var(--brand-ink)] p-6 text-white">
            <div className="flex items-center gap-3">
              <Gauge size={20} className="text-cyan-400" />
              <p className="text-sm text-slate-300">Referências: Apple HIG · Design Resources · Fonts · SF Symbols · Motion · Typography · Color · Accessibility.</p>
            </div>
            <Link to="/login" className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-[var(--link)] hover:text-white">
              Voltar ao app <ArrowRight size={15} />
            </Link>
          </div>
        </Section>
      </main>
    </div>
  );
}
