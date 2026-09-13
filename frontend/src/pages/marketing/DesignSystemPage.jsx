import { Check, ChevronRight, Dumbbell, HeartPulse, Sparkles, UsersRound } from 'lucide-react';
import { BrandLogo } from '../../components/brand/BrandLogo';
import './marketing.css';

const colors = [
  ['Ink 950', '#020617', 'Base escura e superfícies de alto contraste'],
  ['Slate 900', '#0f172a', 'Texto e fundos estruturais'],
  ['Indigo 500', '#6366f1', 'Ação principal e marca'],
  ['Cyan 400', '#22d3ee', 'Ênfase, dados e detalhes'],
  ['Slate 100', '#f1f5f9', 'Fundos suaves'],
  ['White', '#ffffff', 'Superfícies e respiro'],
];

const typeScale = [
  ['Display', '48–72px', 'Uma operação à altura.'],
  ['Heading', '28–40px', 'Informação hierarquizada com calma.'],
  ['Body', '16–18px', 'Texto direto, confortável e objetivo.'],
  ['Label', '12–14px', 'Contexto, estado e metadados.'],
];

const foundations = [
  ['Cor', '--color-*', 'Use intenção — canvas, surface, text e brand — em vez de repetir hexadecimais.'],
  ['Tipo', '--type-*', 'Uma escala curta cria contraste entre contexto, conteúdo e ação.'],
  ['Espaço', '--space-*', 'Múltiplos de 4 preservam ritmo em componentes e layouts.'],
  ['Material', '--shadow-*', 'Borda e elevação sutis distinguem camadas sem ruído visual.'],
  ['Motion', '--duration-*', 'Transições rápidas, previsíveis e desligáveis para reduzir movimento.'],
];

export function DesignSystemPage() {
  return (
    <main className="ds-page min-h-screen bg-slate-50 text-slate-950">
      <div className="ds-topline" />
      <div className="max-w-7xl mx-auto px-5 sm:px-8 lg:px-12 py-8 sm:py-12">
        <header className="flex flex-col sm:flex-row sm:items-center justify-between gap-6 border-b border-slate-200 pb-8">
          <BrandLogo size="md" textClassName="text-slate-950 font-bold" />
          <div className="flex gap-6 text-xs font-semibold uppercase tracking-[0.14em] text-slate-500"><span>System Design</span><span>v0.1</span><span>Interno</span></div>
        </header>

        <section className="py-14 sm:py-20 max-w-4xl">
          <p className="ds-kicker"><Sparkles size={14} /> Forma foundations</p>
          <h1 className="mt-5 text-5xl sm:text-7xl font-black tracking-[-0.06em] leading-[.95]">O sistema que faz a Forma parecer uma só.</h1>
          <p className="mt-7 text-lg sm:text-xl leading-8 text-slate-600 max-w-2xl">Esta rota é um espaço de trabalho interno. Ela traduz nossas decisões em padrões reutilizáveis antes de elas virarem produto.</p>
        </section>

        <section className="ds-section">
          <SectionTitle number="00" title="Contrato de interface" description="A Forma usa tokens semânticos: a intenção fica no componente e a decisão visual fica centralizada no sistema." />
          <div className="ds-token-grid">
            {foundations.map(([name, token, description]) => <article key={name} className="ds-token-card"><strong>{name}</strong><code>{token}</code><p>{description}</p></article>)}
          </div>
        </section>

        <section className="ds-section">
          <SectionTitle number="01" title="Paleta fixa" description="A cor é nossa âncora. Todo o restante evolui ao redor dela." />
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {colors.map(([name, hex, use]) => <div className="ds-color-card" key={name}><div style={{ backgroundColor: hex }} /><div><strong>{name}</strong><code>{hex}</code><p>{use}</p></div></div>)}
          </div>
        </section>

        <section className="ds-section">
          <SectionTitle number="02" title="Tipografia" description="Plus Jakarta Sans: geométrica, clara e humana o bastante para saúde e performance." />
          <div className="ds-type-card">
            {typeScale.map(([name, size, example]) => <div className="ds-type-row" key={name}><div><small>{name}</small><code>{size}</code></div><p className={`ds-type-${name.toLowerCase()}`}>{example}</p></div>)}
          </div>
        </section>

        <section className="ds-section">
          <SectionTitle number="03" title="Imagens" description="Fotografia de treino deve parecer acompanhamento real: gesto preciso, luz editorial e contexto humano — nunca banco de imagens genérico." />
          <div className="ds-image-card">
            <div className="ds-image-preview"><img src="/images/forma-training-hero.png" alt="Referência de direção fotográfica para a Forma Training" /></div>
            <div className="ds-image-rules">
              <div><small>Tom</small><strong>Editorial, humano, em movimento</strong></div>
              <div><small>Enquadramento</small><strong>Vertical 4:5 ou horizontal 16:10</strong></div>
              <div><small>Uso</small><strong>Com espaço negativo para mensagem e contraste</strong></div>
              <p>A luz profunda em azul-marinho e índigo conecta as imagens à interface sem depender de filtros pesados.</p>
            </div>
          </div>
        </section>

        <section className="ds-section">
          <SectionTitle number="04" title="Espaço e superfícies" description="O ritmo é construído em múltiplos de 4. Espaço é parte da hierarquia." />
          <div className="grid lg:grid-cols-2 gap-5">
            <div className="ds-panel"><h3>Escala de espaçamento</h3><div className="mt-7 space-y-4">{[4, 8, 12, 16, 24, 32, 48, 64].map((value) => <div key={value} className="ds-space-row"><code>{value}px</code><i style={{ width: `${Math.min(value * 3, 210)}px` }} /></div>)}</div></div>
            <div className="ds-panel"><h3>Elevação</h3><div className="grid grid-cols-3 gap-4 mt-7"><div className="ds-elevation ds-elevation--flat">Base</div><div className="ds-elevation ds-elevation--soft">Soft</div><div className="ds-elevation ds-elevation--raised">Raised</div></div><p className="mt-8 text-sm text-slate-500">Use sombra como sinal de interação e agrupamento — não como decoração constante.</p></div>
          </div>
        </section>

        <section className="ds-section">
          <SectionTitle number="05" title="Componentes e ícones" description="Ícones Lucide, traço 1.5–2px, sempre acompanhados de texto quando a ação for ambígua." />
          <div className="grid lg:grid-cols-[1.25fr_.75fr] gap-5">
            <div className="ds-panel"><h3>Ações</h3><div className="flex flex-wrap gap-3 mt-7"><button className="ds-button ds-button--primary">Criar treino <ChevronRight size={17} /></button><button className="ds-button ds-button--secondary">Salvar rascunho</button><button className="ds-button ds-button--ghost">Cancelar</button></div><div className="mt-9 border-t border-slate-100 pt-7"><h3>Estados</h3><div className="flex flex-wrap gap-2 mt-4"><span className="ds-status ds-status--success"><Check size={14} /> Ativo</span><span className="ds-status ds-status--info">Em acompanhamento</span><span className="ds-status ds-status--warning">Atenção</span></div></div></div>
            <div className="ds-panel"><h3>Família de ícones</h3><div className="grid grid-cols-2 gap-3 mt-7">{[[UsersRound, 'Alunos'], [Dumbbell, 'Treinos'], [HeartPulse, 'Evolução'], [Sparkles, 'Experiência']].map(([Icon, name]) => <div className="ds-icon-item" key={name}><Icon size={20} /><span>{name}</span></div>)}</div></div>
          </div>
        </section>

        <section className="ds-section mb-0">
          <SectionTitle number="06" title="Motion" description="Movimento orienta atenção. Deve ser breve, responsivo e opcional para quem reduz animações." />
          <div className="ds-motion-card"><div><strong>Entrada</strong><p>opacity + translateY · 500–800ms · ease-out</p></div><div><strong>Interação</strong><p>150–200ms · mudança de cor, borda ou 1–2% de escala</p></div><div><strong>Parallax</strong><p>Somente elementos decorativos, distância curta e nunca no conteúdo crítico.</p></div></div>
        </section>
      </div>
    </main>
  );
}

function SectionTitle({ number, title, description }) {
  return <div className="ds-section-heading"><p>{number}</p><div><h2>{title}</h2><span>{description}</span></div></div>;
}
