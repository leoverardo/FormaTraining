# UpCoach — Brand & Design System (front-end)

Página-index viva: `http://localhost:5173/branding` (alias `/brand`) → `frontend/src/pages/brand/BrandPage.jsx`.
Rota interna, sem links públicos — referência do time, não dos usuários.

## Marca

- Nome: **UpCoach** (front-end renomeado de FitPlatform; backend/banco intactos).
- Assets oficiais: `frontend/public/brand/`, com caminhos centralizados em `frontend/src/components/brand/brandAssets.js`.
- Favicon/app icon: `IconePrincipalUpCoach.png`, também usado na variante `symbol`.
- Board oficial: `BrandinUpCoach.jpg`, exibido e disponível para download em `/brand`.
- Componente: `frontend/src/components/brand/BrandLogo.jsx` (`horizontal` | `symbol` | `mono`, `tone` auto/light/dark, tamanhos xs–lg). O padrão `auto` acompanha o tema do produto; `light`/`dark` permitem controlar o contraste sobre fundos fixos. A variante `mono` usa o ícone secundário oficial.
- Título/SEO: `frontend/index.html` (pt-BR, description, theme-color `#4F46E5`).
- Tagline: **Mais alunos · Melhores resultados** · Assinatura: **Treine > Gerencie > Conquiste**.

### Aplicação das imagens

| Arquivo | Local de uso |
| --- | --- |
| `Capa.png` | Painel de apresentação desktop do login |
| `CapaSecundariaUpCoach.png` | Topo do cartão de cadastro de personal trainer |
| `SubCapaUpCoach.png` | Faixa de assinatura abaixo da capa no login |
| `IconeFundoBrancoUpcoach.png` | Logo em superfícies claras |
| `IconeDarkModeUpCoach.png` | Logo em superfícies escuras |
| `IconeSecundarioUpCoach.png` | Variante secundária e demonstração sobre índigo |

As três capas também aparecem na galeria de `/brand`, com download dos originais.
Preservar a proporção das imagens e o texto incorporado às capas; não recortar,
esticar ou recolorir as artes. Os antigos SVGs permanecem disponíveis para compatibilidade.

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

## Referência visual aplicada ao frontend

Referência: [Apple — Refero](https://styles.refero.design/style/a73148b9-449b-42cd-9f38-86ef694f500e).
Galeria clara, produto e imagens em destaque, tipografia editorial, faixas neutras e superfícies planas. Usar as cores UpCoach para ações e acentos; o azul Apple não é uma cor da plataforma.

- Fonte: SF Pro disponível no sistema, com **Inter local** como fallback principal. Fontes servidas pelo próprio frontend via `@fontsource/inter`; nenhuma chamada externa necessária para carregá-las.
- Hero: 80px/600/1.05 e tracking -0.015em no desktop; títulos editoriais 40px/600; corpo 17px/1.47. Interfaces densas usam 14px e auxiliares 12px.
- Canvas claro: `#ffffff`; faixas e superfícies secundárias `#f5f5f7`; texto principal `#1d1d1f`, secundário `#707070`. O texto auxiliar deve manter contraste AA no fundo real onde aparece.
- Card/mídia: 28px, padding 28px no desktop e 20px no celular; gaps 20px. Separação por espaço/superfície, sem sombras decorativas ou gradientes CSS.
- Seções editoriais: 90px no desktop, 56px no celular. Títulos de telas operacionais usam `.uc-heading`, preservando espaço para dados e ações.
- Controles pill, mínimo 44×44px de toque, foco visível e estados disabled/error. Inputs com raio 980px; textarea 20px.
- Modos claro e escuro compartilham geometria e hierarquia. O escuro adapta a base existente `#0b1020/#12192a/#171f32`, com acentos legíveis.
- Imagens oficiais permanecem intactas; efeitos incorporados na arte não devem ser removidos para imitar superfícies CSS.

Aplicação: `/landing`, `/branding`, autenticação/onboarding, áreas Owner/Trainer/Student, exploração, páginas públicas e legais. Tokens semânticos e primitives atendem todas as telas; CSS de marketing utiliza a mesma fundação. Cores configuradas pelo personal continuam sendo dados da sua identidade, não os defaults da plataforma.

## Validação visual

Execute `npm run test:design` em `frontend/`. Chromium usa o Chrome instalado. O runner reutiliza o Vite em 5173 ou inicia uma instância para a suíte. Fixtures em `frontend/tests/` isolam estados de dados e papéis; não há bypass de autorização no frontend de produção. A suíte visual não depende do backend ou de uma assinatura ativa no banco local.

Os testes exercitam rotas, temas, overflow de página, menus, FAQ, modal/teclado, persistência de tema e contraste AA. Evidências locais ficam em `artifacts/design/` (ignorado pelo Git). Tabelas podem rolar dentro do próprio container; a página inteira não deve rolar horizontalmente.

A página pública aplica as cores configuradas pelo personal somente no seu próprio container. A paleta global UpCoach permanece intacta. O texto dos CTAs personalizados é escolhido para manter contraste; links e hover usam variações legíveis da mesma cor.

As cores de fundo personalizadas permanecem exatas. Textos de links e tags usam tons derivados com contraste AA nas superfícies do tema; o hover dos CTAs preserva a leitura mesmo com cores claras ou intermediárias. Modais e menu móvel mantêm o foco dentro do painel e o devolvem ao controle de abertura ao fechar.


## Motion e narrativa do produto

A landing usa GSAP/ScrollTrigger para entradas e parallax contido, carrossel de destaques com scroll-snap nativo e uma narrativa de três etapas com demonstração fixa no desktop. Celulares e tablets apresentam as demonstrações junto dos respectivos textos. Setas, indicadores, teclado e toque continuam funcionando sem depender da animação. A reprodução automática pode ser pausada e interrompe quando a pessoa navega manualmente.

Three.js é carregado sob demanda somente na landing, em desktop com movimento permitido e sem economia de dados. A escultura geométrica usa a paleta UpCoach; as imagens oficiais permanecem intactas. O render limita DPR a 1.5 e frames a 30 por segundo, suspende fora da tela/em aba oculta e libera recursos ao sair. Sem WebGL, uma ilustração CSS ocupa o mesmo espaço.

O botão “Pausar animações” controla os efeitos da landing. A preferência de movimento reduzido do sistema é respeitada em tempo real, deixando a página estática e sem autoplay. Login e demais telas compartilham entradas curtas em títulos e elementos de apresentação; tabelas, campos e regras de negócio permanecem estáveis. Primitivas de diálogo recebem entrada breve, preservando foco e teclado.

Testes dedicados: `npm run test:design -- tests/motion-foundation.spec.js tests/motion-landing.spec.js tests/motion-three.spec.js`. A suíte de design estático usa movimento reduzido para comparação consistente; esses testes de interação exercitam o movimento normal e suas alternativas.


## Responsividade

A revisão inclui larguras de 320, 360, 600, 768, 820, 1024 e 1440px, claro/escuro e amostras em paisagem. Em celulares abaixo de 360px, o cabeçalho interno apresenta apenas o símbolo oficial com nome acessível, preservando espaço para os controles. O controle de animações da landing fica compacto em 44px no celular e não aparece quando a preferência do sistema já desativa movimento.

Cartões de planos usam uma coluna no celular, duas em telas intermediárias e três apenas no desktop largo. A seleção de plano em modal limita a grade a duas colunas, considerando a largura do painel. Perguntas do FAQ mantêm texto flexível e espaço para a rotação do ícone. O selo de uso interno no branding mantém formato pill sem quebra de linha. Tabelas continuam com rolagem interna quando necessária.

Na ficha do aluno, as cinco abas se distribuem em duas linhas no celular e voltam a uma linha a partir de 640px, preservando rótulos completos e alvos de toque.
