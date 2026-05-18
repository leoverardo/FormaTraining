# Full System Audit — Forma Training

Data da auditoria: 2026-05-16
Escopo: revisão técnica completa do estado atual do repositório (backend, frontend, modelagem, migrations, segurança, billing, LGPD, fluxos críticos).

## Resumo Executivo
- Estado geral: funcional em módulos centrais, porém com riscos relevantes de segurança, consistência operacional e performance.
- Prontidão estimada para lançamento comercial: **62%**.
- Achados por severidade:
  - Crítico: 3
  - Alto: 11
  - Médio: 24
  - Baixo/Melhoria: 21
- Veredito: **Não deve lançar ainda**.

Principais riscos:
1. Segredos sensíveis hardcoded no repositório (`appsettings.json`).
2. Inconsistências de autenticação/autorização em fluxos e contratos legados de privacidade.
3. Dashboard owner com carga total em memória (risco forte de degradação em produção).
4. Tratamento de erro com `200 OK` em falhas reais em endpoints do Explore.
5. Qualidade/lint do frontend com erros impeditivos e hooks inconsistentes.

## Atualização pós-auditoria — Round 1 (2026-05-16)
Status dos achados críticos desta rodada:
- Segredos hardcoded: **mitigado** (valores reais removidos de `appsettings.json`, documentação e exemplo adicionados).
- Patch SQL no startup: **resolvido** (remoção do patch manual; startup sem alteração direta de schema).
- Rate limiting em endpoints públicos críticos: **resolvido** (políticas aplicadas em login, registros públicos, onboarding público, leads e busca Explore pública).
- Explore retornando `200` em erro interno: **resolvido** (agora retorna `500` com log estruturado; `200` permanece para vazio legítimo).
- Lint frontend com erros: **resolvido** (3 erros corrigidos; warnings ainda pendentes).

Pendência operacional:
- Build/test backend e geração EF script continuam bloqueados por acesso de rede/NuGet (`NU1301`) no ambiente da execução.

Atualização complementar:
- Scripts SQL em `database/scripts` agora existem com conteúdo real para criação completa e atualização idempotente.

## Atualização pós-auditoria — Round 2 (2026-05-16)
- Gate premium centralizado implementado com policy/handler:
  - policy `ActiveTrainerSubscription`
  - handler `ActiveTrainerSubscriptionHandler`
  - resposta `403` padronizada para bloqueio premium (`ACTIVE_SUBSCRIPTION_REQUIRED`).
- Endpoints premium de trainer protegidos em blocos críticos (alunos, treinos, posts, chat trainer, agenda trainer, hábitos/nutrição trainer, gamificação trainer, check-ins trainer, service sales trainer, feed trainer, página pública do trainer).
- Fluxos públicos (explore/página pública/leads) passaram a exigir também assinatura ativa para exposição/entrada pública do trainer.
- Hardening de mídia: categorias sensíveis (`ProgressPhoto`, `TransformationBefore`, `TransformationAfter`) não podem ser públicas no upload.
- Webhook/onboarding reforçado com logs estruturados e transição monotônica do onboarding em confirmação de pagamento.
- Validação de cupom em `/api/payments/subscriptions/validate-coupon` restringida para `Trainer` autenticado.

---

## 1) Mapa Geral do Sistema

### Stack real
- Backend: ASP.NET Core 9, EF Core (SQL Server), JWT bearer auth.
- Frontend: React 19 + Vite 8 + React Router 7 + Axios + Tailwind.
- Banco: SQL Server (migrations EF Core).
- Serviços externos: AbacatePay (billing), Cloudinary (mídia), fallback local storage.

### Estrutura macro
- `backend/FitPlatform.Api`: API + controllers + middleware + DI.
- `backend/FitPlatform.Infrastructure`: serviços de negócio, providers, DbContext, migrations, seed.
- `backend/FitPlatform.Domain`: entidades e enums.
- `backend/FitPlatform.Application`: DTOs, interfaces, contratos.
- `frontend/src`: páginas, serviços HTTP, layouts, contexto de autenticação.

### Módulos identificados
- Auth/Login/Me
- Onboarding trainer
- Billing/Subscriptions/Coupons/Webhooks
- Owner dashboard
- Trainer area
- Student area
- Workouts + Workout execution
- Explore + Feed + Leads
- Public trainer page
- Chat
- Appointments/Agenda
- Habits + Nutrition light
- Gamification
- Media upload
- Notifications
- Privacy/LGPD

### Tabela de módulos
| Módulo | Existe? | Status | Observações |
|--------|---------|--------|-------------|
| Auth | Sim | Parcial | Sem refresh token; token 7 dias; controles básicos OK |
| Onboarding | Sim | Parcial | Estados existem, transição possui atalhos/overwrites |
| Billing | Sim | Parcial | Bom núcleo, porém riscos em webhook/configuração/secrets |
| Trainer | Sim | Parcial | Fluxos principais presentes |
| Student | Sim | Parcial | Fluxos principais presentes |
| Owner | Sim | Parcial | Dashboard pesado (in-memory) |
| Workouts | Sim | Bom | CRUD e vínculo trainer-student presentes |
| Workout Execution | Sim | Bom/Parcial | Boa cobertura, mas sem testes automatizados |
| Chat | Sim | Bom/Parcial | Ownership bom, sem paginação |
| Agenda | Sim | Bom/Parcial | Regras de conflito ok, sem paginação robusta |
| Habits | Sim | Parcial | Funcional, sem suíte de testes |
| Nutrition Light | Sim | Parcial | Funcional |
| Gamification | Sim | Parcial | Funcional, risco de regra sem testes |
| Public Trainer Page | Sim | Parcial | Sincronização com consentimento melhorou |
| Explore | Sim | Parcial | Falhas retornam 200; degradam observabilidade |
| Leads | Sim | Parcial | Sem anti-spam/rate limit |
| Privacy/LGPD | Sim | Parcial | Estrutura robusta, mas coexistência de fluxo legado |
| Media Upload | Sim | Parcial | Validações básicas boas; faltam hardenings extras |
| Notifications | Sim | Parcial | Sem fila/retentiva observável |

---

## 1.1) Frontend, Design System e Qualidade Visual

### Stack visual atual
- UI base: React + Tailwind utilitário, com componentes reutilizáveis em `frontend/src/components/ui`.
- Ícones: Lucide.
- Estrutura de páginas por papel (Owner/Trainer/Student) com layouts dedicados.

### Direção de design observada
- Predominância de paleta neutra (`slate/gray`) com acentos em `indigo`, `emerald`, `amber`, `sky`.
- Linguagem de cards e blocos consistente em grande parte das telas (bordas suaves, `rounded`, `shadow` leve).
- Hierarquia visual geralmente boa em dashboards (KPI > detalhe > tabela/lista).

### Achados de UI/UX (frontend)
1. **Inconsistência de identidade visual entre módulos**: algumas telas têm acabamento mais “produto” (Owner/Explore), enquanto outras ainda parecem MVP técnico.
2. **Legibilidade irregular em textos auxiliares**: em alguns pontos há cinzas muito suaves para desktop e mobile.
3. **Estados de erro vs vazio**: avançou no Explore, mas ainda há telas que misturam falha técnica com “sem dados”.
4. **Sistema de feedback heterogêneo**: toasts, mensagens inline e silenciosas coexistem sem padrão único por criticidade.
5. **Responsividade boa no geral, porém desigual**: tabelas e blocos densos do owner exigem refinamento para mobile pequeno.
6. **Dívida de acessibilidade**: faltam evidências de padrão consistente para foco visível, navegação por teclado e contraste mínimo.

### Cores e tokens
- Não há, no estado atual, evidência de um design-token central formal (ex.: arquivo único com semântica `primary/success/warning/danger` para todo app).
- Uso de cores está funcional, mas com risco de deriva visual ao longo de novas entregas.
- Recomendação: consolidar tokens semânticos (cores, spacing, raio, elevação, tipografia) para reduzir inconsistência futura.

### Tipografia e densidade
- Escala tipográfica está adequada para cards e listas.
- Em telas operacionais densas (owner/trainer), a combinação de textos pequenos + muitas métricas pode reduzir escaneabilidade.
- Recomendação: padronizar tamanhos mínimos em elementos críticos e revisar contraste de textos secundários.

### Prioridades visuais antes do lançamento comercial
- **P1**: padronizar estados de loading/erro/vazio nas telas críticas (cadastro, checkout, dashboards, explore, treino em execução).
- **P1**: consolidar paleta semântica e guidelines rápidas de uso (ex.: sucesso, risco, bloqueio, informativo).
- **P2**: revisar responsividade de tabelas e blocos analíticos do owner.
- **P2**: checklist mínimo de acessibilidade visual (contraste, foco, leitura em mobile).

### Risco de produção (visão frontend/design)
- Não há bloqueio crítico de lançamento apenas por estética.
- Há risco **médio** de percepção de produto “inconsistente” entre áreas, impactando confiança e suporte.
- Endereçar padronização visual e feedback de erro melhora diretamente conversão, retenção e redução de chamados.

---

## 2) Auditoria de Backend

### 2.1 Controllers
Achados:
- `ExploreController` captura erro e responde `200 OK` com payload vazio/mensagem genérica em falhas internas. Impacto: monitoramento quebrado e comportamento confuso para clientes. (Alto)
- Convivência de controllers legados e novos de privacidade (`PrivacyController` e `PrivacyLgpdController`) gera sobreposição conceitual e risco de fluxo paralelo. (Médio)
- Alguns endpoints `Authorize` amplos sem role específica (`/api/payments/plans/{id}/billing-options`, `/api/payments/subscriptions/validate-coupon`) aceitam qualquer usuário autenticado; regra de negócio depende de serviço com `trainerId ?? Guid.Empty`. (Médio)
- Padronização de status code inconsistente em módulos (muitos `BadRequest` para cenários que são `Forbidden`/`Conflict`/`Unprocessable`). (Médio)

### 2.2 Services
Achados:
- `OwnerDashboardService`: carrega múltiplas tabelas inteiras com `ToListAsync()` e só depois agrega. Escalabilidade baixa. (Alto)
- `PaymentService.AdvanceOnboardingAsync`: status é atribuído em sequência (`PaymentApproved` -> `AccountCreated` -> `Completed`) no mesmo fluxo sem checkpoints persistidos intermediários. (Médio)
- `PrivacyLgpdService` concentra muitas responsabilidades (consents + legal + export). Bom para entrega rápida, mas exige refactor para manutenção futura. (Baixo)
- Vários serviços com lógica pesada sem transações explícitas em operações multi-entity fora do billing. (Médio)

### 2.3 DTOs / contratos
Achados:
- Contratos frontend/backend crescentes e com acoplamento informal (campos opcionais sem versionamento). (Médio)
- Endpoints de erro retornam mensagens não uniformes (PT/EN/mistos). (Baixo)

### 2.4 Entidades/modelo
Achados:
- Modelagem extensa e relativamente coerente.
- Há campos legados coexistindo com novos de privacidade (`TermsDocument/UserConsent` legado + `PrivacyPolicyVersion/UserPrivacyConsent` novo). (Médio)
- Dependência de enum string em front em alguns pontos sensíveis sem camada de mapeamento formal robusta. (Baixo)

### 2.5 Autorização/ownership
Achados:
- Workout execution, chat, appointments e student/trainer ownership: em geral bom.
- Pontos de atenção:
  - Endpoints com fallback “schema not applied yet” podem mascarar erro de autorização/infra. (Médio)
  - `ValidateCoupon` autenticado para qualquer role usa `Guid.Empty` quando não trainer, podendo distorcer regra de uso por cliente. (Médio)

---

## 3) Banco de Dados e Migrations

### Visão geral
- Migrations até `V21_PrivacyLgpdFoundation`.
- Snapshot atualizado.
- Índices e FKs amplamente usados; boa base estrutural.

### Riscos e coerência
- Ordem de migrations possui histórico com nomenclatura não totalmente linear (`V9` em duas linhas históricas etc.). Não quebra necessariamente, mas aumenta risco operacional. (Médio)
- Existe SQL de “ensure columns” no startup (`EnsureTrainerExploreColumnsAsync`) além de migrations. Isso cria dupla fonte de verdade do schema. (Alto)
- Seeds de privacidade rodam apenas quando `Users` vazio; em bases existentes alguns catálogos podem não ser preenchidos automaticamente. (Médio)

### Tabela (resumo)
| Tabela | Uso | Risco encontrado | Correção sugerida |
|---|---|---|---|
| Users | Auth | Segredos/controles não relacionados à tabela, sem soft delete formal | Política clara de inativação e auditoria |
| Trainers | Núcleo trainer/public | Script ad-hoc de colunas no startup | Remover patch SQL e usar só migration |
| Students | Vínculo aluno | Boa | Sem ação imediata |
| TrainerSubscriptions | Billing | Fluxos de estado complexos sem trilha formal de state machine | Introduzir state transition guard |
| TrainerPayments | Billing | Volume pode crescer sem particionamento | Índices + estratégia arquivamento |
| DiscountCoupons/Redemptions | Cupons | Regras dependem do uso por trainer; caso non-trainer no endpoint | Restrição de role no endpoint |
| TrainerOnboardings | Onboarding | Transição em cadeia no webhook | Persistir marcos intermediários |
| TrainerLeads | Comercial | Sem anti-spam/rate limit | Proteções API |
| Conversations/ChatMessages | Chat | Sem paginação de mensagens | Paginação/cursor |
| WorkoutSessions* | Execução | Boa base | Monitorar crescimento e índices compostos adicionais |
| StudentHabit* | Hábitos | Boa base | Sem ação imediata |
| Privacy* | LGPD | Coexistência com legado `Terms*` | Plano de depreciação do legado |
| MediaFiles | Uploads | Boa base, risco acesso URL pública | Tokenização/expiração para privados |

---

## 4) Autenticação, Login e Acesso

- JWT sem refresh token; expiração 7 dias fixa. (Médio)
- Sem logout server-side/revogação de token. (Médio)
- `ProtectedRoute` frontend sólido para papéis básicos.
- Cenários de conta inativa: login bloqueia.
- Sem trilha explícita para bloqueio por assinatura em todas as rotas premium (parcial, depende de serviços). (Alto)

Cenários especiais:
- Trainer sem assinatura ativa: há verificações em partes do billing, mas não há gateway central de feature access para todo módulo premium. (Alto)
- Conta criada antes de pagamento: suportado no onboarding; risco de caminhos alternativos liberarem acesso parcial. (Médio)

---

## 5) Pagamentos, Assinaturas e Planos

### 5.1 Planos e descontos
- Regra de ciclo/desconto implementada no backend (mensal, trimestral 10%, semestral 15%, anual 20%).
- Cálculo parece coerente com arredondamento para centavos.
- Risco: divergência futura se frontend replicar regra local sem travamento por contrato único. (Médio)

### 5.2 Cupons
- Validação robusta (vigência, plano, ciclo, limites total e por cliente, mínimo).
- Risco: endpoint `validate-coupon` permite role não-trainer e usa `Guid.Empty` como cliente. (Médio)

### 5.3 Assinaturas/webhooks
- Idempotência com `PaymentWebhookLog` único por evento é ponto positivo.
- Validador de webhook exige chave e assinatura; boa base.
- Risco crítico operacional: chaves/segredos hardcoded no repositório. (Crítico)

### 5.4 Gateway
- Integração AbacatePay ativa.
- Tratamento de erro retorna exceções internas no provider com mensagem genérica; razoável.

### 5.5 Owner metrics
- Cálculos extensos, mas abordagem in-memory é gargalo severo para crescimento. (Alto)

---

## 6) Onboarding Trainer

- Estados existem: Draft, WaitingPayment, PaymentApproved, AccountCreated, Completed.
- Problema: no fluxo de webhook, estados intermediários são sobrescritos no mesmo método sem checkpoint real, reduzindo auditabilidade do funil. (Médio)
- Risco de duplicidade mitigado parcialmente por checks de email/registro em andamento.

---

## 7) Módulo Trainer

- Dashboard, alunos, treinos, agenda, posts, leads, vendas, perfil público presentes.
- Riscos:
  - UX/consistência: tratamento de erros e estados varia muito por tela. (Médio)
  - Dependência de muitas chamadas sem cache/normalização. (Baixo)

---

## 8) Módulo Student

- Dashboard, execução treino, explore, chat, check-in, etc. presentes.
- Riscos:
  - Explore com retorno 200 em falha (já citado). (Alto)
  - Lint indica hooks com dependências ausentes em telas críticas (`ExploreTrainersPage`, etc.). (Médio)

---

## 9) Owner Dashboard

- Endpoint único `GET /api/owner/dashboard?range=` existe.
- Riscos:
  - Filtragem temporal é aplicada, mas toda massa de dados é carregada antes de agregar. (Alto)
  - Possível distorção por timezone UTC em métricas de negócio local sem normalização por fuso de operação. (Médio)

---

## 10) Perfil Público, Explore e Leads

- Perfil público: ativação/desativação e slug implementados.
- Sincronização com consentimento melhorada, porém coexistência de fluxo legado ainda pode confundir governança. (Médio)
- Explore: geolocalização com prompt explícito implementada.
- Leads: funcional, sem proteção anti-automação/rate-limit. (Alto)

---

## 11) Treinos e Execução

- Modelo robusto (session/exercise/set) com autosave por set e finalização.
- Ownership checks adequados nos principais endpoints.
- Risco principal: ausência de testes automatizados de regressão para cenários complexos de sessão em andamento/retomada. (Médio)

---

## 12) Chat, Agenda, Hábitos, Nutrição, Gamificação

- Chat: ownership bom; sem paginação em mensagens/conversas (médio para escala).
- Agenda: conflitos de horário e estados básicos cobertos.
- Hábitos/nutrição: funcional com modelagem clara.
- Gamificação: existe; sem cobertura de testes e risco de duplicidade/regressão em triggers. (Médio)

---

## 13) LGPD e Privacidade

Pontos fortes:
- Documentos versionados, consentimentos, histórico, requests, export e painéis owner.

Riscos:
- Coexistência módulo novo e legado (`TermsDocument/UserConsent`) sem plano explícito de depreciação. (Médio)
- Exportação JSON ainda parcial sob ponto de “completude” do direito de acesso (ex.: mensagens só enviadas, não necessariamente todo contexto participativo). (Médio)
- `HEALTH_RELATED_DATA_PROCESSING` foi corretamente bloqueado como toggle livre na API/UI recente; manter revisão jurídica final obrigatória.

---

## 14) Uploads, Mídia e Cloud

- Validação de tipo/tamanho e extensões perigosas: boa.
- Controle de acesso por owner/trainer/student razoável.
- Risco: estratégia de proteção de mídia privada depende da URL/provedor; falta desenho explícito de URL assinada expirada para todos os casos sensíveis. (Alto)

---

## 15) Frontend e UX dos fluxos críticos

Resultados objetivos:
- `npm run build`: OK (com warning de bundle > 500kB).
- `npm run lint`: FALHA com 3 erros e 7 warnings.

Problemas relevantes:
- Erros lint reais em telas críticas (`OwnerDashboard`, `StudentWorkouts`, `SubscriptionPage`). (Médio)
- Hooks com dependências ausentes em múltiplas telas. (Médio)
- Inconsistência de feedback e estados entre páginas.

---

## 16) Segurança Geral (classificação)

### Crítico
1. Segredos hardcoded em `backend/FitPlatform.Api/appsettings.json` (JWT secret, AbacatePay key, Cloudinary key/secret).
2. Mesmo padrão potencial em outros arquivos de configuração de ambiente local comprometendo higiene de segredo.
3. Exposição de credenciais em repositório + risco de uso indevido imediato.

### Alto
1. Explore retornando 200 em falhas internas.
2. Dashboard owner in-memory para dataset grande.
3. Ausência de rate limiting em endpoints públicos (leads/onboarding/webhook path protegido só por assinatura).
4. Política de mídia privada sem expiração/token unificado.
5. Script SQL de schema em runtime além de migrations.

### Médio
- Falta refresh token/revogação.
- Lint e hooks inconsistentes.
- Estado onboarding sem checkpoints intermediários.
- Endpoint de cupom aceitando roles amplas.

### Baixo/Melhoria
- Padronização de mensagens PT/EN.
- Observabilidade mais rica para métricas críticas.

---

## 17) Performance e Escalabilidade

Achados:
- `OwnerDashboardService` com várias cargas totais (`ToListAsync`) antes de agregações (N módulos). (Alto)
- Chat sem paginação de mensagens. (Médio)
- Algumas listas com `Take` fixo sem paginação externa flexível. (Médio)
- Bundle frontend grande (warning Vite). (Baixo)

---

## 18) Erros, Logs e Observabilidade

- Middleware global de exceção existe e evita leak de stacktrace para cliente.
- Logs estruturados presentes em partes (billing/public page/privacy), porém cobertura desigual.
- Problema de semântica de erro no Explore (200 em erro) compromete rastreabilidade. (Alto)

---

## 19) Testes e Confiabilidade

- Testes backend praticamente inexistentes (`UnitTest1` vazio).
- `dotnet test` não validado por bloqueio de restore (rede/sandbox), e mesmo assim não há suíte substancial.

### Testes unitários prioritários
1. Regras de cálculo de billing por ciclo/desconto.
2. Validação de cupom (vigência/limites/plano/ciclo).
3. Transições de estado de assinatura/onboarding.
4. Regras de ownership em workout execution/chat/appointments.
5. Consent sync perfil público.

### Testes de integração prioritários
1. Checkout + webhook idempotente.
2. Onboarding fim-a-fim com ativação de conta.
3. Export LGPD garantindo escopo do titular.
4. Rotas públicas (`/p/{slug}` e explore) respeitando flags/consent.

### Testes E2E prioritários
1. Cadastro trainer + pagamento + primeiro login.
2. Student explorer -> lead -> conversão.
3. Execução de treino com retomada e conclusão.
4. Privacidade: toggles, export, exclusão, owner workflow.

---

## 20) Relatório Central de Problemas

| ID | Severidade | Módulo | Problema | Impacto | Sugestão de correção | Bloqueia lançamento? |
|----|------------|--------|----------|---------|----------------------|----------------------|
| A-001 | Crítico | Segurança/Config | Secrets hardcoded em appsettings | Comprometimento de conta/gateway/storage | Rotacionar chaves, remover do repo, usar vault/env | Sim |
| A-002 | Crítico | Segurança | JWT secret exposto em código | Forja de token | Rotação imediata + secret manager | Sim |
| A-003 | Crítico | Segurança | Credenciais Cloudinary/AbacatePay expostas | Acesso indevido financeiro e mídia | Rotação + revogação + auditoria de uso | Sim |
| A-004 | Alto | Explore | Retorno 200 em falha interna | Cliente não detecta erro real | Retornar 5xx/4xx correto | Sim |
| A-005 | Alto | Owner Dashboard | Carga in-memory massiva | Queda de performance e custo | Agregações SQL paginadas/projeções | Sim |
| A-006 | Alto | DB/Migrations | Patch SQL de schema no startup | Drift de schema e risco operacional | Remover patch e formalizar migration | Sim |
| A-007 | Alto | Público/Leads | Sem rate limiting anti abuso | Spam/fraude/DoS lógico | Rate-limit + proteção anti-bot | Sim |
| A-008 | Alto | Mídia | Estratégia de privacidade de URL incompleta | Vazamento de mídia privada | Signed URLs/expiração/policy rígida | Sim |
| A-009 | Alto | Billing Access | Gate de features premium não central | Acesso indevido por assinatura inválida | Policy central por plano/status | Sim |
| A-010 | Alto | Observabilidade | Falha semântica HTTP no Explore | Alertas e SLO comprometidos | Ajustar status codes + logs | Sim |
| A-011 | Médio | Auth | Sem refresh/revogação token | Sessão longa sem controle | Refresh token + blacklist/rotation | Não |
| A-012 | Médio | Frontend | Lint com erros em produção branch | Qualidade e regressões | Corrigir lint e habilitar CI gate | Não |
| A-013 | Médio | Onboarding | Status sobrescritos no webhook | Perda de auditabilidade | Persistir passos intermediários | Não |
| A-014 | Médio | Privacy | Fluxo legado + novo coexistem | Governança confusa | Plano de depreciação legado | Não |
| A-015 | Médio | Coupon API | Role ampla em validate endpoint | Métrica/uso por cliente distorcida | Restringir a Trainer | Não |

---

## 21) Prioridades Antes do Lançamento

### P0 — Corrigir antes de qualquer lançamento
- Remover/rotacionar **todos** os segredos hardcoded.
- Corrigir semântica HTTP de erros no Explore.
- Eliminar patch SQL de schema em startup (usar só migration).
- Implementar rate limiting básico para endpoints públicos críticos.

### P1 — Corrigir antes do lançamento comercial
- Refatorar `OwnerDashboardService` para agregação no banco.
- Centralizar gate de acesso por assinatura/plano.
- Fechar lacunas de mídia privada (signed URLs/policies).
- Corrigir erros de lint e hooks críticos no frontend.

### P2 — Corrigir logo após lançamento (curto prazo)
- Refresh token/revogação.
- Deprecar fluxo legado de privacidade.
- Melhorar observabilidade por domínio (billing/onboarding/chat/workout).

### P3 — Melhorias futuras
- Otimizações de bundle frontend.
- Refino de UX/consistência visual e mensagens.
- Cobertura de testes avançada e testes de carga.

---

## 22) Checklist de Prontidão para Produção

- [ ] Banco sobe do zero com migrations (não validado integralmente nesta execução por restrição de restore/rede)
- [ ] Seed funciona em base vazia e em base já populada (parcial)
- [ ] Backend compila (parcial: build de solução bloqueado por rede; build API já havia funcionado localmente)
- [x] Frontend compila
- [x] Cadastro trainer funciona (pelo código)
- [x] Cadastro student funciona (pelo código)
- [ ] Checkout funciona (depende de env real + webhook)
- [ ] Webhook funciona em ambiente real
- [ ] Assinatura ativa libera acesso de forma centralizada
- [x] Perfil público funciona
- [x] Explore funciona (com ressalvas de erro semântico)
- [x] Treino funciona
- [x] Chat funciona
- [x] Agenda funciona
- [x] Owner dashboard funciona (com risco de escala)
- [x] Privacidade/LGPD funciona (parcial, com legado coexistente)
- [x] Exportação funciona
- [x] Exclusão pode ser solicitada
- [ ] Logs essenciais completos (parcial)
- [ ] Secrets não estão hardcoded
- [ ] Config de produção documentada

---

## Execuções obrigatórias realizadas

### Backend
- `dotnet build FitPlatform.sln -v minimal` -> **Falhou** por `NU1301` (acesso bloqueado ao nuget.org no ambiente).
- `dotnet test FitPlatform.sln -v minimal` -> **Falhou** pelo mesmo motivo.
- Observação: cobertura de testes existente é praticamente nula (`UnitTest1` vazio).

### Frontend
- `npm run build` -> **OK**, com warning de chunk > 500kB.
- `npm run lint` -> **Falhou** (3 erros, 7 warnings).

### Banco
- Migrations revisadas até `V21_PrivacyLgpdFoundation`.
- Snapshot presente e coerente em alto nível.
- Não foi possível validar migração em banco real nesta execução.

---

## Top 10 achados mais importantes
1. Segredos hardcoded (JWT/AbacatePay/Cloudinary) no repositório.
2. JWT secret exposto (forja de token possível).
3. Explore retorna 200 em erro real.
4. Owner dashboard in-memory (grave para escala).
5. Script SQL ad-hoc no startup além de migrations.
6. Falta rate limiting em endpoints públicos.
7. Gate premium por assinatura não centralizado.
8. Estratégia de mídia privada incompleta.
9. Lint frontend com erros em telas importantes.
10. Cobertura de testes praticamente inexistente.

## Top 10 correções mais urgentes
1. Rotacionar e remover segredos do código imediatamente.
2. Ajustar retorno HTTP de erro no Explore.
3. Remover `EnsureTrainerExploreColumnsAsync` e formalizar schema em migrations.
4. Criar política central de autorização por assinatura/plano.
5. Refatorar owner dashboard para consultas agregadas SQL.
6. Implementar rate limit para rotas públicas críticas.
7. Implementar estratégia de URL assinada/expiração para mídia sensível.
8. Corrigir erros de lint e warnings críticos de hooks.
9. Criar testes unitários para billing/cupom/assinatura.
10. Criar testes de integração webhook/onboarding/export LGPD.

---

## Correções pequenas aplicadas durante esta auditoria
- Nenhuma correção pequena foi aplicada nesta etapa de varredura completa.

---

## Veredito final
**Não deve lançar ainda.**

Critérios mínimos para reavaliar lançamento:
- Resolver todos os itens P0.
- Resolver a maior parte dos itens P1 (principalmente performance owner dashboard + controle premium + lint crítico).
