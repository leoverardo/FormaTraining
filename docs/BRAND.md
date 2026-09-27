# UpCoach — Brand & Design System (front-end)

Página-index viva: `http://localhost:5173/brand` → `frontend/src/pages/brand/BrandPage.jsx`.
Rota interna, sem links públicos — referência do time, não dos usuários.

## Marca

- Nome: **UpCoach** (front-end renomeado de FitPlatform; backend/banco intactos).
- Símbolo: U com seta ascendente — `frontend/public/brand/upcoach-symbol.svg`.
- Favicon/app icon: `frontend/public/favicon.svg` (squircle índigo, U branco, seta ciano).
- Board oficial: salvar a imagem da marca em `frontend/public/brand/upcoach-brand-board.jpg` (a página exibe automaticamente).
- Componente: `frontend/src/components/brand/BrandLogo.jsx` (`horizontal` | `symbol` | `mono`, `tone` light/dark, tamanhos xs–lg).
- Título/SEO: `frontend/index.html` (pt-BR, description, theme-color `#4F46E5`).
- Tagline: **Mais alunos · Melhores resultados** · Assinatura: **Treine > Gerencie > Conquiste**.

## Paleta oficial

| Nome | Hex | RGB | Token |
|------|-----|-----|-------|
| Índigo | #6366F1 | 99, 102, 241 | `--brand-indigo` / `--primary` |
| Índigo forte | #4F46E5 | 79, 70, 229 | `--brand-indigo-strong` / `--primary-strong` |
| Índigo claro | #818CF8 | 129, 140, 248 | `--brand-indigo-soft` |
| Ciano | #06B6D4 | 6, 182, 212 | `--brand-cyan` (acento) |

## Tokens (`frontend/src/index.css`)

Espaçamento base 4px (`--space-1…--space-36`), radius (`--radius-sm/md/lg/pill`),
tipografia (`--text-xs…--text-hero`), motion (`--ease-apple`, `--duration-fast/medium/slow`),
layout (`--content-width: 1200px`) + utilitários `.uc-container`, `.uc-eyebrow`,
`.uc-display`, `.uc-heading`, `.uc-lead` e `prefers-reduced-motion` global.

## Primitivas (`frontend/src/components/ui/`)

`Button` (pill, 44px, primary→danger), `Badge`, `Input` (label/hint/error),
`Card`/`CardHeader`/`CardContent`. Ícones: Lucide 16/20/24, stroke 1.5–2px.

## Filosofia (benchmark Apple, identidade própria)

Content > decoration · whitespace > clutter · typography > efeitos ·
motion com propósito (CSS p/ micro, GSAP p/ scroll) · performance e a11y antes de decorar.
Nada de assets Apple/SF Symbols — tudo SVG/CSS próprio ou Lucide licenciado.

## Rollout

Fase 1 (esta): tokens + primitivas + rename + página `/brand`.
Fase 2 (incremental): aplicar `.uc-*` e narrativa editorial página a página,
sem quebrar lógica, rotas, auth ou backend.
