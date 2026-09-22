# Forma Training — Design System (Apple-inspired, identidade própria)

> Cores da marca preservadas. Este sistema traduz princípios do Apple HIG
> (hierarquia, whitespace, materiais, motion, acessibilidade) para a stack
> atual: React + Vite + Tailwind 4. Nada de fonte proprietária Apple.

## Princípios

1. Menos elementos, mais impacto. Whitespace é hierarquia.
2. Tipografia primeiro, depois cor, depois sombra.
3. Materiais com função (header/drawer/float), não vidro decorativo.
4. Motion rápido e discreto, sempre com `prefers-reduced-motion`.
5. Tokens semânticos — componente nunca hardcodar hex.

## Tokens — `frontend/src/design-system/tokens.css`

- Cor: `--color-canvas/surface/text-primary/text-secondary/border-subtle/brand/brand-strong/accent/success/warning/danger` (hex base inalterado: `--primary #6366f1`, `--primary-strong #4f46e5`).
- Tipo: `--type-display/hero/h1/h2/h3/body-lg/body/caption/label`, `--tracking-display/heading`, `--leading-*`.
- Espaço: `--space-1..10`, `--section-y`, `--section-y-sm` (múltiplos de 4).
- Layout: `--content-width 75rem`, `--content-narrow`, `--content-wide`, `--gutter`.
- Radius: `--radius-sm/md/lg/xl/pill` (refinado, sem excesso de pill).
- Shadow: `--shadow-1/2/3/soft` (profundidade suave).
- Motion: `--duration-fast/normal/slow`, `--ease-standard/out/spring`.
- Dark via `html.dark` / `[data-theme=dark]`.

## Arquivos

```
frontend/src/design-system/
  tokens.css      # cores, tipo, espaço, radius, shadow, motion
  typography.css  # .ds-display .ds-hero .ds-h1/h2/h3 .ds-body .ds-caption .ds-label .ds-kicker
  layout.css      # .ds-container .ds-section .ds-grid-2/3 .ds-stack
  motion.css      # .ds-transition .ds-pressable .ds-reveal + reduced-motion
  components.css  # .ds-btn .ds-card .ds-input .ds-badge .ds-material .ds-skeleton
  index.css       # importa tudo (importado por src/index.css)
```

## Componentes React (API preservada, visual pelo DS)

- `components/ui/Button.jsx` → usa `.ds-btn .ds-btn--primary/secondary/outline/ghost`
- `components/ui/Card.jsx` → `.ds-card`
- `components/ui/Input.jsx` → `.ds-input` + `label htmlFor` (a11y)
- `components/ui/PageContainer.jsx` → `.ds-container/--narrow/--wide`
- `components/ui/PageHeader.jsx` → `.ds-h1` + descrição
- `layouts/AppShell.jsx` → header glass `bg-white/95 backdrop-blur`, sidebar ativa indigo preservada

Não criar botão/card/input inline nas páginas — reutilizar os acima.

## Landing — referência iPhone Duo (princípios, não cópia)

- Hero: headline gigante, sub curta, 1 CTA primário + 1 secundário, visual produto + float cards, parallax GSAP com scrub, grid + orb decorativos.
- Seções: trust → statement → produto → rotina (tabs) → aluno → como funciona → **planos** → **FAQ** → final CTA → footer.
- Scroll storytelling só onde agrega: hero parallax, `.landing-reveal`, canvas por view.
- `prefers-reduced-motion`: GSAP desativado, CSS com transição 0.01ms.
- Imagem `/images/forma-training-hero.png` com `object-fit cover`, shade editorial, caption.

## Imagens / vídeo

- Aspect consistente, `object-fit cover`, lazy fora do hero, poster em vídeo, `playsInline muted` quando autoplay.
- Fotos progresso sempre privadas (regra backend preservada).

## Acessibilidade

- `focus-visible` com `var(--focus-ring)`, alvos ≥44px no mobile, `aria-expanded/controls` no menu mobile e FAQ, `role=tablist/tab` com `aria-selected`.
- Contraste: texto secundário nunca abaixo de `#64748b` no light.
- Alt específico (evitar "Banner", "Post", "preview").

## Performance

- GSAP só na landing, `ScrollTrigger` com scrub, sem lib 3D forçada (sem objeto 3D que justifique).
- `transform/opacity` para animação, nunca layout. Bundle monitorado (`vite build` avisa >500kB).

## Uso

```jsx
import { PageContainer } from '../components/ui/PageContainer';
import { PageHeader } from '../components/ui/PageHeader';
import { Button } from '../components/ui/Button';

<PageContainer>
  <PageHeader title="Alunos" description="..." actions={<Button> Novo aluno </Button>} />
</PageContainer>
```

```css
.minha-secao { background: var(--color-surface); border: 1px solid var(--color-border-subtle); border-radius: var(--radius-xl); }
```

## QA antes de entregar

- `npm run lint` passando, `npm run build` passando, sem TODO da tarefa.
- Revisar desktop/tablet/mobile, overflow, sticky, contraste, reduced-motion.
