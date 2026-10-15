import { Card, CardContent } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { Select } from '../../components/ui/Select';
import { Button } from '../../components/ui/Button';
import { useTheme } from '../../contexts/ThemeContext';

const THEME_TOKENS = [
  ['Canvas', '--bg', '#FFFFFF', '#0B1020'],
  ['Superfície', '--surface', '#FFFFFF', '#12192A'],
  ['Faixa suave', '--surface-muted', '#F5F5F7', '#171F32'],
  ['Superfície auxiliar', '--surface-subtle', '#F5F5F7', '#20293D'],
  ['Controles', '--surface-control', '#E6E6E8', '#2D374D'],
  ['Superfície elevada', '--surface-frost', '#FAFAFC', '#151D2F'],
  ['Texto principal', '--text', '#1D1D1F', '#F5F7FF'],
  ['Texto secundário', '--text-secondary', '#707070', '#B3BDCF'],
  ['Texto auxiliar', '--text-muted', '#707070', '#AEB7C7'],
  ['Borda', '--border', '#E6E6E8', '#2D374D'],
  ['Borda de controle', '--control-border', '#86868B', '#858FA6'],
  ['Links', '--link', '#4F46E5', '#A5B4FC'],
  ['Foco', '--focus-ring', '#6366F1', '#A5B4FC'],
];

function GuidelineCard({ title, children }) {
  return <Card><CardContent><h3 className="text-lg font-semibold text-[var(--text)]">{title}</h3><div className="mt-3 space-y-3 text-sm leading-relaxed text-[var(--text-secondary)]">{children}</div></CardContent></Card>;
}

export function ThemeTokens() {
  const { theme } = useTheme();
  return <>
    <p className="mb-5 text-sm text-[var(--text-secondary)]">Tema atual: <strong className="text-[var(--text)]">{theme === 'dark' ? 'escuro' : 'claro'}</strong>. Use o controle do cabeçalho para conferir as amostras reais. A geometria e a identidade permanecem as mesmas.</p>
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{THEME_TOKENS.map(([name, token, light, dark]) => <Card key={token}><CardContent>
      <div className="mb-4 h-12 rounded-xl border border-[var(--control-border)]" style={{ backgroundColor: `var(${token})` }} />
      <h3 className="text-sm font-semibold text-[var(--text)]">{name}</h3><code className="mt-1 block break-all text-xs text-[var(--link)]">{token}</code>
      <dl className="mt-3 grid grid-cols-2 gap-3 text-xs text-[var(--text-secondary)]"><div><dt>Claro</dt><dd className="mt-1 font-mono">{light}</dd></div><div><dt>Escuro</dt><dd className="mt-1 font-mono">{dark}</dd></div></dl>
    </CardContent></Card>)}</div>
    <div className="mt-4 grid gap-4 md:grid-cols-2">
      <GuidelineCard title="Ação e identidade"><p><code>--primary</code>: #6366F1 no claro e #818CF8 no escuro. O preenchimento de CTA <code>--primary-strong</code> mantém #4F46E5 nos dois temas, com texto <code>--on-primary</code>.</p><p><code>--primary-soft</code>: #EEF0FF / #24294D. Hover e foco usam tokens próprios; não substitua a paleta da marca por azul externo.</p></GuidelineCard>
      <GuidelineCard title="Identidade do personal"><p>A página pública aplica as cores configuradas pelo personal apenas no seu container. Os fundos preservam a cor original; texto, links e tags recebem contraste adequado ao tema.</p><p>Use os pares semânticos de sucesso, aviso, erro e informação. Ciano é acento; textos informativos usam <code>--status-info-text</code>.</p></GuidelineCard>
    </div>
  </>;
}

export function ComponentStates() {
  return <div className="mt-4 grid gap-4 lg:grid-cols-2">
    <GuidelineCard title="Estados de ação"><div className="flex flex-wrap gap-3"><Button disabled>Indisponível</Button><Button loading>Salvando</Button><Button variant="success">Concluído</Button></div><p>O carregamento bloqueia envios duplicados. Disabled preserva o contexto visual; foco usa contorno de 2px com afastamento de 4px.</p></GuidelineCard>
    <Card><CardContent className="grid gap-4 sm:grid-cols-2"><Input label="Campo desabilitado" value="Somente demonstração" disabled readOnly /><Select label="Preferência" defaultValue="weekly" hint="Seleção nativa com os mesmos tokens."><option value="weekly">Semanal</option><option value="monthly">Mensal</option></Select></CardContent></Card>
    <GuidelineCard title="Diálogos e navegação"><p>Modais e menu móvel recebem o foco, contêm Tab e Shift+Tab, fecham com Escape e devolvem o foco ao controle de abertura. O conteúdo do painel rola quando a altura disponível é pequena.</p><p>As abas devem acomodar o rótulo completo: na ficha do aluno são três colunas no celular e cinco a partir de 640px.</p></GuidelineCard>
    <GuidelineCard title="Feedback e dados"><p>Loading, vazio, erro e sucesso precisam de texto claro. Badges combinam rótulo e cor. Tabelas densas rolam dentro do seu container; a página inteira não deve rolar horizontalmente.</p><p>Use labels nos campos, mensagens de validação e nomes acessíveis nos botões que mostram apenas ícones.</p></GuidelineCard>
  </div>;
}

export function MotionGuidelines() {
  return <div className="mt-4 grid gap-4 md:grid-cols-2">
    <GuidelineCard title="Entradas do produto · GSAP"><p>400ms, easing power2.out e sequência de 35ms. Títulos partem de −12px; cards e imagens, de +14px. Login e telas internas compartilham essa base.</p><p>Não animar tabelas inteiras nem ancestrais de elementos fixos. Não reiniciar a entrada a cada tecla digitada.</p></GuidelineCard>
    <GuidelineCard title="Landing · ScrollTrigger"><p>Hero: 850ms, sequência de 90ms e power3.out. Entradas editoriais: 800ms e deslocamento de 32px.</p><p>Parallax a partir de 768px: mockup até −48px e rotação de 3°, legenda até −85px. O scroll permanece nativo. A narrativa fixa começa em 1024px.</p></GuidelineCard>
    <GuidelineCard title="Carrossel · 6 segundos"><p>Scroll-snap nativo, toque, setas, indicadores e teclado. O avanço automático pausa com hover, foco, aba oculta ou carrossel fora da tela.</p><p>Navegação manual, inclusive trackpad, interrompe o autoplay. Retomar exige ação no controle de reprodução.</p></GuidelineCard>
    <GuidelineCard title="Three.js · carregamento sob demanda"><p>Somente na landing, a partir de 1024px, com movimento permitido e sem economia de dados. Limite de 30fps e DPR de 1.5.</p><p>Suspender o render fora da tela e em aba oculta. Liberar recursos ao sair. Sem WebGL ou em perda de contexto, manter a ilustração CSS estática.</p></GuidelineCard>
    <GuidelineCard title="Microinterações e diálogos"><p>CSS para hover, foco e feedback de pressão: 180–200ms. Button usa escala ativa de 0.99; controles de ícone e links de CTA usam 0.97.</p><p>Diálogos: 220ms e 8px de entrada. Não acrescentar bounce, sombras decorativas ou movimento contínuo a controles de formulário.</p></GuidelineCard>
    <GuidelineCard title="Pausa e movimento reduzido"><p><code>prefers-reduced-motion</code> é observado em tempo real. Desativar movimento não essencial, autoplay e 3D, mantendo conteúdo e navegação.</p><p>Na landing, “Pausar animações” controla os efeitos. No celular é um botão de 44px; quando o sistema já reduz movimento, esse controle não aparece.</p></GuidelineCard>
  </div>;
}

export function ResponsiveGuidelines() {
  return <div className="mt-4 grid gap-4 md:grid-cols-2">
    <GuidelineCard title="Containers e ritmo"><p><code>.uc-container</code>: até 1200px, com 32px de margem por lado. Até 1024px: limite de 960px e margens de 24px. Até 640px: margens de 20px.</p><p>Marketing usa <code>.gallery-shell</code> de até 1120px; abaixo de 768px, margens de 20px. Cards: padding de 28px, reduzido para 20px até 640px.</p></GuidelineCard>
    <GuidelineCard title="Celular · 320–639px"><p>Formulários em uma coluna, texto com quebra natural e controles de no mínimo 44px. Abaixo de 360px, o cabeçalho interno usa só o símbolo oficial com nome acessível.</p><p>Abas da ficha do aluno em duas linhas; planos em uma coluna. A arte lateral do login aparece apenas a partir de 1024px.</p></GuidelineCard>
    <GuidelineCard title="Tablet · 640–1023px"><p>A grade de planos passa para duas colunas a partir de 640px; o modal de escolha mantém no máximo duas. Sidebar interna a partir de 768px; antes disso, menu em drawer.</p><p>A narrativa da landing continua empilhada, com cada demonstração junto do seu texto. O 3D permanece substituído pela ilustração estática.</p></GuidelineCard>
    <GuidelineCard title="Desktop · 1024px ou mais"><p>Narrativa fixa e 3D elegível na landing. A grade de recursos usa três colunas; planos administrativos chegam a três somente a partir de 1280px.</p><p>Não use o tamanho da tela como sinônimo de largura disponível: sidebar, padding e modal também reduzem o espaço útil.</p></GuidelineCard>
    <GuidelineCard title="Imagens e marca"><p>Preservar proporção, texto incorporado e cores das imagens oficiais. Não recolorir o logo, aplicar sombras ou recortar informações essenciais.</p><p>Mockups são demonstrações ilustrativas; não apresentam métricas comerciais reais. Login recebe marca e acesso; a apresentação do produto fica em /landing.</p></GuidelineCard>
    <GuidelineCard title="Verificação em contexto"><p>Conferir 320, 360, 390, 600, 768, 820, 1024 e 1440px, nos dois temas. Incluir paisagem, nomes extensos, estados vazios e menus abertos.</p><p>Verificar foco, teclado, contraste, rótulos completos, scroll interno de tabelas e movimento reduzido. Os testes de interface usam dados simulados; não substituem testes reais do backend.</p></GuidelineCard>
  </div>;
}
