# Integração AbacatePay — Forma Training

Documentação da integração de assinaturas SaaS (trainer → plataforma) com a AbacatePay:
webhook, reconciliação, retry, cancelamento, troca de plano e cupons.

> A venda B2C de serviços do trainer (`ServiceSalesService`) usa Mercado Pago e **não**
> faz parte desta integração.

---

## 1. Endpoint oficial de webhook

Existe **um único** endpoint de webhook da AbacatePay:

```
POST {ApiBaseUrl}/api/webhooks/abacatepay?webhookSecret={secret}
```

| Ambiente | URL |
|---|---|
| Local | `POST http://localhost:5000/api/webhooks/abacatepay?webhookSecret={secret}` |
| Produção | `POST {Platform__ApiBaseUrl}/api/webhooks/abacatepay?webhookSecret={secret}` |

Servido por `WebhooksController` → `AbacatePayWebhookService`.

> O endpoint legado `POST /api/payments/webhooks/abacatepay` **foi removido**. Se ele ainda
> estiver cadastrado no painel da AbacatePay, atualize o cadastro antes do deploy — chamadas
> à rota antiga passarão a retornar 404 e os eventos serão perdidos.

A AbacatePay exige URL **HTTPS pública**. Para testes locais use [ngrok](https://ngrok.com)
ou um túnel equivalente.

### Definição do domínio de produção

O domínio **não é hardcoded**. Defina em:

| Variável | Uso |
|---|---|
| `Platform__ApiBaseUrl` | Host da API — é a base da URL de webhook cadastrada no painel da AbacatePay |
| `Platform__FrontendBaseUrl` | Host do SPA — origem dos redirects de checkout (`completionUrl` / `returnUrl`) |

Quando `AbacatePay__SuccessUrl` / `AbacatePay__ReturnUrl` ficam vazias, elas são derivadas de
`Platform__FrontendBaseUrl` + `OnboardingSuccessPath` / `OnboardingReturnPath`. Informe-as
explicitamente apenas se o redirect precisar apontar para outro host.

O redirect depende da origem do checkout (`PaymentService.BuildCheckoutRedirects`):

| Origem | `completionUrl` | `returnUrl` |
|---|---|---|
| Onboarding (cadastro) | `SuccessUrl?onboardingId={id}` | `ReturnUrl?onboardingId={id}` |
| Painel do trainer | `TrainerPanelUrl?checkout=success` | `TrainerPanelUrl?checkout=canceled` |

`AbacatePay__TrainerPanelUrl` vazio deriva de `Platform__FrontendBaseUrl` + `/trainer/subscription`.

---

## 2. Variáveis de ambiente

A forma canônica usa o separador padrão do ASP.NET Core (duplo underscore). Os nomes planos
antigos (`ABACATEPAY_*`) continuam aceitos como **alias de menor precedência** — uma variável
legada vazia nunca sobrescreve o `appsettings`.

```env
# ── Domínios ────────────────────────────────────────────────────────────
Platform__FrontendBaseUrl=https://app.seu-dominio.com
Platform__ApiBaseUrl=https://api.seu-dominio.com

# ── Credenciais (nunca versionar) ───────────────────────────────────────
AbacatePay__ApiKey=
AbacatePay__WebhookSecret=
AbacatePay__WebhookPublicKey=

# ── Segurança ───────────────────────────────────────────────────────────
# Obrigatoriamente true fora de Development — a aplicação não sobe com false.
AbacatePay__EnableSignatureValidation=true

# ── Ambiente ────────────────────────────────────────────────────────────
# true apenas quando a ApiKey for do ambiente de testes da AbacatePay.
AbacatePay__DevMode=false

# ── Opcionais ───────────────────────────────────────────────────────────
AbacatePay__BaseUrl=https://api.abacatepay.com/v2
AbacatePay__PaymentMethod=CARD          # CARD ou PIX
AbacatePay__EnableCouponSync=true
AbacatePay__SuccessUrl=                 # vazio = deriva de Platform__FrontendBaseUrl
AbacatePay__ReturnUrl=

# retryPolicy enviado à AbacatePay no checkout (limites da API: 1..10 e 1..30 dias)
AbacatePay__RetryPolicy__MaxRetry=3
AbacatePay__RetryPolicy__RetryEvery=1

# ── Reconciliação e retry de webhook ────────────────────────────────────
AbacatePay__Reconciliation__Enabled=true
AbacatePay__Reconciliation__IntervalMinutes=15
AbacatePay__Reconciliation__MinPendingAgeMinutes=20
AbacatePay__Reconciliation__MaxPendingAgeDays=7
AbacatePay__Reconciliation__BatchSize=25
AbacatePay__Reconciliation__MaxAttemptsPerSubscription=8
AbacatePay__Reconciliation__MaxListPages=5

# Verificação de assinaturas já ativas (rede de segurança para webhook perdido)
AbacatePay__Reconciliation__VerifyActiveSubscriptions=true
AbacatePay__Reconciliation__ActiveBatchSize=25
AbacatePay__Reconciliation__ActiveRecheckHours=12

AbacatePay__Reconciliation__WebhookRetryBatchSize=25
AbacatePay__Reconciliation__WebhookMaxRetries=5
AbacatePay__Reconciliation__WebhookRetryBackoffMinutes=5
```

Modelo completo versionado: `backend/FitPlatform.Api/appsettings.Example.json`.
`appsettings.json` e `appsettings.Development.json` são ignorados pelo git.

### Validação de startup

`AbacatePayOptionsValidator` roda com `ValidateOnStart()`. Fora de `Development` a aplicação
**não sobe** se faltar `ApiKey`, `WebhookSecret`, `WebhookPublicKey`, `SuccessUrl` ou `ReturnUrl`,
ou se `EnableSignatureValidation` estiver `false`. As mensagens citam apenas o nome da chave —
nunca o valor.

---

## 3. Autenticação do webhook

| Header / Param | Obrigatório | Descrição |
|---|---|---|
| `?webhookSecret=...` | Sempre | Query param comparado com `AbacatePay:WebhookSecret` |
| `X-Webhook-Signature` | Produção | `Base64( HMACSHA256( WebhookPublicKey, rawBody ) )` |
| `Content-Type` | — | `application/json` |

Ambas as comparações usam `CryptographicOperations.FixedTimeEquals`.

Webhooks não assinados são **sempre** rejeitados fora de `Development`, mesmo que
`EnableSignatureValidation` esteja `false` — e nesse caso a aplicação nem inicia.

---

## 4. Política de `devMode`

`AbacatePay:DevMode` declara contra qual ambiente esta instância opera.

| `AbacatePay:DevMode` | Evento com `devMode: true` | Evento com `devMode: false` |
|---|---|---|
| `false` (produção) | Registrado com `ProcessingStatus = IgnoredDevMode` e **descartado** | Processado |
| `true` (sandbox) | Processado | Processado |

Isso impede que um evento gerado no sandbox da AbacatePay ative uma assinatura real.

---

## 5. Eventos tratados

| Evento | Ação |
|---|---|
| `subscription.completed` | Ativa a assinatura, estende `EndDate`, aprova o pagamento, resgata o cupom e avança o onboarding |
| `subscription.renewed` | Registra o pagamento da renovação, estende `EndDate`, zera o contador de falhas e **aplica a troca de plano agendada** |
| `subscription.cancelled` | Marca `Canceled` preservando `EndDate` (acesso gracioso até o fim do período pago) |
| `subscription.payment_failed` | Registra a falha sem cancelar a assinatura (ver abaixo) |
| `checkout.completed` | Fallback: ativa a assinatura pendente caso `subscription.completed` não chegue |
| Qualquer outro | Registrado no log e respondido com `200 OK` |

> **Sobre `subscription.payment_failed` — NÃO CONFIRMADO, tratado defensivamente.**
>
> Investigação realizada: o evento não aparece no catálogo público de webhooks
> (`/pages/webhooks` lista `subscription.completed`, `subscription.renewed` e
> `subscription.cancelled`), não existe DTO, enum, fixture ou payload real dele no
> repositório, e a única origem rastreável é o fluxo legado removido na refatoração
> anterior (`PaymentService.HandleWebhookAsync`), que já o tratava. Nenhum payload real
> comprova que a AbacatePay o envia.
>
> Decisão: **o handler é mantido** — é idempotente, inofensivo se nunca disparar, e cobre
> a hipótese de a API evoluir. Mas o sistema **não depende dele**: a detecção de problemas
> de cobrança é responsabilidade da reconciliação (§9.2), que confere as assinaturas ativas
> diretamente contra o provedor. Se o evento nunca chegar, nada se perde.
>
> O comportamento documentado do provedor é retentar conforme o `retryPolicy` e emitir
> `subscription.cancelled` ao esgotar as tentativas — e esse caminho é coberto tanto pelo
> webhook quanto pela reconciliação.

### Por que uma falha de pagamento não cancela a assinatura

`TrainerSubscriptionStatus` tem apenas `Active`, `Pending`, `Expired` e `Canceled` — não existe
`PastDue`/`Suspended`, e introduzir um estado novo cortaria o acesso do trainer já na primeira
falha, antes de a AbacatePay terminar suas retentativas.

O tratamento adotado:

- `Status` permanece inalterado;
- `LastPaymentStatus = "failed"`;
- `PaymentFailureCount` incrementado e `LastPaymentFailureAt` registrado;
- o `TrainerPayment` correspondente passa a `Rejected`;
- `EndDate` preservado — o acesso expira naturalmente pela regra de
  `ActiveTrainerSubscriptionHandler` (`Status == Active && EndDate >= now`);
- uma renovação bem-sucedida zera `PaymentFailureCount` e `LastPaymentFailureAt`.

O cancelamento definitivo só ocorre quando a AbacatePay emite `subscription.cancelled`.

---

## 6. Formato do payload

```json
{
  "id": "log_abc123",
  "event": "subscription.completed",
  "apiVersion": 2,
  "devMode": false,
  "data": {
    "subscription": { "id": "subs_xyz" },
    "checkout":     { "id": "bill_xyz" },
    "customer":     { "id": "cust_xyz" },
    "payment":      { "id": "pay_xyz", "amount": 5990 },
    "externalId":   "trainer:{trainerId}:sub:{subscriptionId}",
    "metadata": {
      "trainerId":             "...",
      "trainerSubscriptionId": "...",
      "internalPlanId":        "...",
      "billingCycle":          "Monthly"
    }
  }
}
```

`data` nunca é validado de forma rígida — campos novos não quebram a desserialização.

### Resolução da assinatura interna

1. `data.metadata.trainerSubscriptionId` → id interno (mais preciso)
2. `data.subscription.id` → `AbacatePaySubscriptionId`
3. `data.checkout.id` ou `data.id` → `AbacatePayCheckoutId`
4. `data.externalId` (`trainer:{id}:sub:{id}`) → id interno

---

## 7. Idempotência — duas camadas

As duas camadas são independentes e ambas necessárias, porque **eventos distintos podem
referenciar o mesmo pagamento**.

| Camada | Chave | Onde | Problema que resolve |
|---|---|---|---|
| **Evento** | `PaymentWebhookLog.EventId` (= `payload.id`, o "ProviderEventId") | Índice UNIQUE `(Provider, EventId)` | A mesma entrega chegar duas vezes reexecutar a lógica de negócio |
| **Operação financeira** | `TrainerPayment.ProviderPaymentId` | `SubscriptionLifecycleService` | Dois eventos diferentes citando o mesmo pagamento criarem cobrança/crédito em duplicidade |

> `EventId` **é** o `ProviderEventId`: o campo já armazena o `id` do envelope da AbacatePay
> e já possui índice UNIQUE desde a migration V9. Nenhuma coluna nova foi criada.

### Fluxo de reivindicação do evento

```text
Webhook recebido
      ↓
Validação (secret + HMAC) → 401 se falhar
      ↓
devMode incompatível? → registra IgnoredDevMode → 200
      ↓
EventId já existe?
  ├─ estado terminal ──────────→ 200 "Duplicate webhook ignored"  (não reexecuta)
  ├─ Processing recente ───────→ 200 "Duplicate webhook ignored (concurrent delivery)"
  └─ Failed / Processing órfão → reivindica (RetryCount++)
      ↓
INSERT (se novo) → violação do índice UNIQUE? → perdedor relê e desiste
      ↓
COMMIT do log  ← fora da transação de negócio
      ↓
Processa negócio em transação Serializable
      ├─ sucesso → Processed (ou Ignored, se o tipo não é tratado)
      └─ falha   → rollback + Failed com backoff  (o log sobrevive)
```

Três barreiras, da mais barata à mais forte: leitura do registro → janela de *in-flight*
(5 min) → **índice UNIQUE no banco, que é a última barreira**.

### Outras garantias de idempotência no domínio

| Garantia | Mecanismo |
|---|---|
| Pagamento não duplica | `ProviderPaymentId` + `Status == Approved` |
| Cupom não resgata duas vezes | Índice único `(CouponId, TrainerId, SubscriptionId)` + verificação prévia |
| Cancelamento não repete | `CancelAsync` sai cedo se já `Canceled` |
| Falha de cobrança não conta duas vezes | `ProviderPaymentId` + `Status == Rejected` |
| Onboarding não regride | Avanço por ranking de status |

O `id` do payload é o identificador de idempotência — **nunca reutilize o mesmo id para
eventos diferentes**.

### Estados de `ProcessingStatus`

| Valor | Nome | Terminal | Descrição |
|---|---|:---:|---|
| 0 | `Pending` | | Recebido e registrado ("Received"), ainda não processado |
| 1 | `Processing` | | Em processamento; dentro da janela de 5 min bloqueia entregas concorrentes |
| 2 | `Processed` | ✅ | Processado com sucesso |
| 3 | `Failed` | | Falhou; **único** estado elegível a retry |
| 4 | `Duplicate` | ✅ | Reentrega concorrente descartada |
| 5 | `Abandoned` | ✅ | Excedeu o limite de tentativas — exige investigação manual |
| 6 | `IgnoredDevMode` | ✅ | Evento sandbox recebido por instância de produção |
| 7 | `Ignored` | ✅ | Evento autêntico sem ação de negócio (tipo não tratado) |

Estados terminais nunca são reprocessados: nem por uma reentrega do provedor, nem pela
rotina de retry. `ReprocessAsync` aplica a mesma regra mesmo se chamado diretamente.

---

## 8. Respostas HTTP

| Código | Situação | Corpo |
|---|---|---|
| `200` | Evento processado | `Webhook processado.` |
| `200` | Evento já em estado terminal | `Duplicate webhook ignored.` |
| `200` | Entrega concorrente do mesmo evento | `Duplicate webhook ignored (concurrent delivery).` |
| `200` | Tipo de evento não tratado | `Evento não tratado — registrado e ignorado.` |
| `200` | Evento sandbox em instância de produção | `Evento devMode ignorado nesta instância.` |
| `401` | Secret inválido ou assinatura HMAC inválida | `error` |
| `500` | Erro interno — a AbacatePay retenta, e o evento também entra na fila de retry interna | — |

Duplicatas **nunca** retornam erro: um `2xx` faz a AbacatePay parar de reenviar um evento
que já foi tratado.

---

## 9. Reconciliação e retry

`SubscriptionReconciliationBackgroundService` executa `SubscriptionReconciliationRunner` a
cada `IntervalMinutes`. Cada execução abre seu próprio escopo de DI e nunca derruba o host.

### 9.0 Qual endpoint consulta a assinatura, e por quê

A consulta usa **`GET /v2/subscriptions/list`**, não um endpoint de busca por id.

Investigação: a página de referência menciona `GET /subscriptions/get` em prosa, mas esse
endpoint **não tem página própria no índice oficial** (`llms.txt`), `/pages/subscriptions/get`
responde **404**, nenhum SDK oficial o implementa e o contrato de requisição (nome do
parâmetro) não está documentado em lugar nenhum. Implementá-lo seria construir sobre
documentação ambígua, então foi descartado.

Limitação aceita: `subscriptions/list` não oferece filtro por id nem por `externalId`
(os parâmetros documentados são `after`, `before`, `limit`, `startDate`, `endDate`,
`status`). Mitigações:

- `GetSubscriptionAsync` **para a paginação assim que encontra** o item — não lê todas as páginas;
- a janela `startDate`/`endDate` restringe a varredura quando o chamador conhece a data;
- a reconciliação **não** chama `GetSubscriptionAsync` por assinatura: resolve o lote inteiro
  com **uma única** varredura paginada, indexando o resultado por `id` e `externalId`;
- `MaxListPages` limita a paginação em qualquer caso.

Se a AbacatePay publicar um endpoint de busca individual com contrato documentado, trocar a
implementação é uma mudança local em `AbacatePayPaymentProvider.GetSubscriptionAsync`.

### 9.1 Assinaturas `Pending` órfãs

Seleciona assinaturas que satisfaçam **todos** os filtros:

- `Status == Pending` e `Provider == "AbacatePay"`;
- criadas há mais de `MinPendingAgeMinutes` (evita corrida com o webhook);
- criadas há menos de `MaxPendingAgeDays`;
- `ReconciliationAttempts < MaxAttemptsPerSubscription`;
- no máximo `BatchSize` por execução.

O lote inteiro é resolvido com **uma única** varredura paginada de `GET /subscriptions/list`
(limitada a `MaxListPages`), com match por `AbacatePayCheckoutId` ou `externalId`.

| Status remoto | Ação local |
|---|---|
| `PAID` | Ativa a assinatura (mesmo caminho do webhook — `SubscriptionLifecycleService`) |
| `CANCELLED` | Marca `Canceled` preservando `EndDate` |
| `EXPIRED` | Marca `Expired` |
| `REFUNDED` | Apenas registra o status remoto e alerta em log |
| `PENDING` | Nada — continua aguardando |

Indisponibilidade do provedor aborta a execução **sem consumir tentativas**.

### 9.2 Assinaturas `Active` divergentes — rede de segurança sem `payment_failed`

Segunda passada, controlada por `VerifyActiveSubscriptions` / `ActiveBatchSize` /
`ActiveRecheckHours`. É o que garante a detecção de problemas de cobrança **mesmo que a
AbacatePay nunca envie um evento de falha**.

| Situação | Ação |
|---|---|
| Provedor `CANCELLED`, local `Active` | Cancela localmente preservando `EndDate` (webhook de cancelamento perdido) |
| Provedor `EXPIRED`, local `Active` | Marca `Expired` |
| Provedor `REFUNDED` | Apenas sinaliza — estorno exige decisão humana |
| Provedor `PAID` mas `EndDate` local vencido | Apenas sinaliza — renovação não processada **ou** cobrança recusada |
| Sem correspondência no provedor | Apenas sinaliza |
| Coerente | Nada |

**Por que o caso "período vencido" não altera o status:** `subscriptions/list` devolve o
objeto do checkout inicial, não os ciclos de cobrança individuais, então não há como
distinguir "renovação paga que não processamos" de "cobrança recusada". Estender o
`EndDate` concederia acesso indevido; expirar cortaria acesso de quem pagou. A divergência
é registrada em log (`DivergencesFlagged`) para investigação.

Isso não abre brecha de acesso: `ActiveTrainerSubscriptionHandler` exige
`Status == Active && EndDate >= agora`, portanto uma assinatura com período vencido já
está bloqueada independentemente do status.

### 9.3 Retry de webhooks que falharam

Critério de elegibilidade — estritamente:

```text
ProcessingStatus == Failed
AND RetryCount    <  WebhookMaxRetries
AND NextRetryAt   <= agora
```

`Processed`, `Duplicate`, `Ignored`, `IgnoredDevMode` e `Abandoned` são terminais e **nunca**
entram na fila. `ReprocessAsync` repete essa verificação por conta própria, de modo que nem
uma chamada direta consegue reprocessar um estado terminal.

Reprocessa a partir do `RawPayload` já persistido e autenticado (até `WebhookRetryBatchSize`
por execução).

- Backoff exponencial: `WebhookRetryBackoffMinutes × 2^RetryCount`, limitado a 2⁶.
- Ao exceder `WebhookMaxRetries` o log vai para `Abandoned` e deixa de ser retentado.
- A assinatura **não** é revalidada no retry (já foi verificada no recebimento) e o método
  não é acessível por HTTP.

### 9.4 Sobrevivência do log a rollback

O registro do webhook é gravado e **commitado antes** da transação de negócio. Em caso de
falha, o rollback desfaz as alterações de domínio mas o log permanece, é marcado como
`Failed` com o erro sanitizado e entra na fila de retry.

Detalhe crítico da implementação: antes de escrever o `Failed`, o `ChangeTracker` é limpo e
o log é relido. Sem isso, as entidades inválidas que derrubaram a transação seriam
reenviadas no mesmo `SaveChanges` e a marcação falharia — apagando justamente o rastro que
se quer preservar.

### Monitoramento

```sql
-- Webhooks que falharam e ainda serão retentados
SELECT EventId, Type, RetryCount, NextRetryAt, ErrorMessage
FROM PaymentWebhookLogs
WHERE Provider = 'AbacatePay' AND ProcessingStatus = 3
ORDER BY NextRetryAt;

-- Webhooks abandonados — exigem ação manual
SELECT EventId, Type, RetryCount, ErrorMessage, LastAttemptAt
FROM PaymentWebhookLogs
WHERE Provider = 'AbacatePay' AND ProcessingStatus = 5
ORDER BY LastAttemptAt DESC;

-- Tipos de evento recebidos mas não tratados (candidatos a novo handler)
SELECT Type, COUNT(*) AS Total, MAX(CreatedAt) AS UltimoRecebimento
FROM PaymentWebhookLogs
WHERE Provider = 'AbacatePay' AND ProcessingStatus = 7
GROUP BY Type
ORDER BY Total DESC;

-- Assinaturas que a reconciliação desistiu de resolver
SELECT Id, TrainerId, CreatedAt, ReconciliationAttempts, ProviderStatus
FROM TrainerSubscriptions
WHERE Status = 2 AND ReconciliationAttempts >= 8
ORDER BY CreatedAt;

-- Divergências ativas: período vencido sem renovação processada
SELECT Id, TrainerId, EndDate, ProviderStatus, PaymentFailureCount, LastReconciledAt
FROM TrainerSubscriptions
WHERE Status = 1 AND EndDate < SYSUTCDATETIME()
ORDER BY EndDate;
```

> Se a consulta de `ProcessingStatus = 7` passar a mostrar um tipo recorrente — por exemplo
> um evento de falha de cobrança que a AbacatePay venha a introduzir — é o sinal para
> adicionar o handler correspondente.

---

## 10. Cancelamento e troca de plano

Ambos exigem JWT com papel `Trainer` e operam **exclusivamente** sobre o `TrainerId` extraído
do token — o identificador do trainer nunca é aceito pelo corpo ou pela rota.

### Cancelar assinatura

```
POST /api/subscriptions/cancel
Authorization: Bearer {jwt}     (Role = Trainer)
Body: —
```

```json
{
  "success": true,
  "message": "Assinatura cancelada. O acesso permanece liberado até o fim do período já pago.",
  "data": {
    "subscriptionId": "…",
    "status": "Canceled",
    "accessValidUntil": "2026-10-22T00:00:00Z",
    "alreadyCanceled": false
  }
}
```

- Chama `POST /v2/subscriptions/cancel` apenas quando existe `subs_...` no provedor.
- `EndDate` é preservado — o acesso vale até o fim do período pago.
- Idempotente: a segunda chamada retorna `200` com `alreadyCanceled: true` e **não** chama o
  provedor de novo.
- Falha do provedor **não** altera o estado local.

### Trocar de plano

```
POST /api/subscriptions/change-plan
Authorization: Bearer {jwt}     (Role = Trainer)
Content-Type: application/json

{ "platformPlanId": "guid", "billingCycle": "Monthly" }   // billingCycle é opcional
```

```json
{
  "success": true,
  "message": "Troca de plano agendada. O novo valor passa a valer no próximo ciclo de cobrança.",
  "data": {
    "subscriptionId": "…",
    "currentPlanId": "…", "currentPlanName": "Basic",
    "currentBillingCycle": "Monthly", "currentAmountInCents": 5990,
    "newPlanId": "…", "newPlanName": "Pro",
    "newBillingCycle": "Monthly", "newAmountInCents": 9990,
    "changeKind": "Upgrade",
    "effectiveAt": "2026-10-22T00:00:00Z",
    "providerStatus": "PENDING"
  }
}
```

O corpo aceita **apenas** o plano (e opcionalmente o ciclo). Preço e `productId` são resolvidos
pelo backend e conferidos contra `PlanBillingOptions.FinalPriceInCents`.

Validações: plano existe, ativo e disponível para compra; ciclo suportado; assinatura ativa e já
confirmada pelo provedor (`subs_...` presente); destino diferente do plano/ciclo atual; produto
AbacatePay configurado. `changeKind` (`Upgrade`/`Downgrade`/`CycleChange`) é derivado no backend.

**A troca não é imediata.** A AbacatePay agenda a alteração e a aplica no início do próximo
ciclo. Até lá a assinatura mantém o plano atual e o destino fica em
`PendingPlatformPlanId` / `PendingBillingCycle` / `PendingFinalAmountInCents` /
`PendingPlanChangeProviderId`. O evento `subscription.renewed` aplica a troca.
Um cancelamento descarta a troca agendada.

---

## 11. Cupons

A API aceita cupons nativamente no checkout de assinatura (campo `coupons`, array de códigos),
então **não é preciso criar um produto por faixa de preço**.

Divisão de responsabilidades:

| Responsabilidade | Onde |
|---|---|
| Elegibilidade (vigência, plano, ciclo, limites, valor mínimo) | Backend — `PaymentService.CalculateWithCouponAsync` |
| Aritmética do desconto sobre a cobrança | AbacatePay — campo `coupons` |

Fluxo:

1. O backend valida o cupom e calcula o valor final.
2. O cupom é espelhado de forma idempotente em `POST /v2/coupons/create`
   (`Percentage → PERCENTAGE`, `FixedAmount → FIXED` em centavos, `MaxUsesTotal → maxRedeems`,
   `-1` quando ilimitado). O resultado é gravado em `DiscountCoupons.ProviderCouponCode` /
   `ProviderSyncedAt`, e cupons já sincronizados não chamam o provedor de novo.
3. O código vai no campo `coupons` do checkout.

### Integridade de preço

A comparação com o produto da AbacatePay passou a usar o **subtotal antes do cupom**:

```
pricing.SubtotalAfterCycleDiscountInCents == PlanBillingOptions.FinalPriceInCents
```

Era a comparação com o valor **pós-cupom** que rejeitava todo cupom legítimo.

Exemplo: `R$ 59,90` com cupom de `20%` → backend calcula `R$ 47,92`; o produto continua valendo
`R$ 59,90` (bate com o cadastro) e a AbacatePay aplica os 20% no checkout.

### Proteções

| Cenário | Resultado |
|---|---|
| Cupom inexistente ou inativo | `Cupom inválido.` |
| Fora da vigência / expirado | `Cupom fora da vigência.` / `Cupom expirado.` |
| Plano ou ciclo diferente | `Cupom não aplicável a este plano/ciclo.` |
| Limite total ou por cliente atingido | `Cupom esgotado.` / `Limite de usos por cliente atingido.` |
| Valor mínimo não atingido | `Valor mínimo não atingido para o cupom.` |
| `DiscountValue <= 0` ou percentual `> 100` | `Cupom com desconto inválido.` |
| Desconto fixo maior que o valor | Clampado — o total vai a zero, nunca negativo |
| Falha ao espelhar no provedor | Checkout **bloqueado** (melhor que cobrar o valor cheio em silêncio) |

O frontend envia apenas `couponCode`; nenhum valor monetário vindo do cliente é aceito em
nenhum ponto do fluxo.

---

## 12. Segurança — checklist

- [x] Secret e assinatura comparados com `CryptographicOperations.FixedTimeEquals`
- [x] `EnableSignatureValidation = false` só aceito em `Development`; fora disso a app não sobe
- [x] Body raw lido com `EnableBuffering()` — stream não consumida antes da validação
- [x] Idempotência em duas camadas: `EventId` (evento) e `ProviderPaymentId` (financeiro)
- [x] Índice UNIQUE `(Provider, EventId)` como última barreira contra entrega concorrente
- [x] Entregas concorrentes não processam o mesmo evento em paralelo (janela de in-flight)
- [x] Estados terminais nunca reexecutam a lógica de negócio
- [x] Transação `Serializable` no processamento
- [x] Log do evento persistido **fora** da transação de negócio — uma falha deixa rastro e é retentável
- [x] Eventos `devMode` não afetam instância de produção
- [x] Endpoint de webhook `[AllowAnonymous]` — autenticado por secret + assinatura, não por JWT
- [x] Endpoints de assinatura exigem JWT `Trainer` e usam apenas o `TrainerId` do token
- [x] Preço sempre resolvido pelo backend e conferido contra o produto do provedor
- [x] Exceções do provedor encapsuladas em `PaymentProviderException` com mensagem segura
- [x] Nenhum log contém secret, assinatura, API key ou corpo de requisição/resposta

---

## 13. Logs estruturados

Campos emitidos (sem dados sensíveis):

```
EventId, EventType, DevMode, RetryCount, NextRetryAt
SubscriptionId, TrainerId, PreviousStatus, NewStatus, EndDate
CheckoutId, ProviderSubscriptionId, ProviderPaymentId, ProviderStatus
CouponCode, FailureCount, ChangeKind, Source
```

Pontos cobertos: webhook recebido, evento ignorado, evento já processado, assinatura localizada,
mudança de status, erro de processamento, retry, reconciliação, cancelamento e troca de plano.

---

## 14. Como testar localmente

### Com curl (sem validação de assinatura)

Rode a API com `ASPNETCORE_ENVIRONMENT=Development` e
`AbacatePay__EnableSignatureValidation=false`:

```bash
curl -X POST "http://localhost:5000/api/webhooks/abacatepay?webhookSecret=meu-secret-local" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "log_teste_001",
    "event": "subscription.completed",
    "apiVersion": 2,
    "devMode": false,
    "data": {
      "checkout": { "id": "bill_seu_checkout_id" },
      "subscription": { "id": "subs_teste" },
      "payment": { "id": "pay_teste", "amount": 5990 },
      "customer": { "id": "cust_teste" }
    }
  }'
```

> Com `AbacatePay__DevMode=false`, um payload com `"devMode": true` é registrado e ignorado —
> use `"devMode": false` no teste ou rode com `AbacatePay__DevMode=true`.

### Com ngrok (webhooks reais em desenvolvimento)

```bash
ngrok http 5000
# Cadastre no painel AbacatePay:
# https://abc123.ngrok.io/api/webhooks/abacatepay?webhookSecret=meu-secret
```

Com ngrok, mantenha `AbacatePay__EnableSignatureValidation=true` e configure a public key para
exercitar o fluxo completo.

---

## 15. Checklist de go-live

- [ ] `Platform__ApiBaseUrl` e `Platform__FrontendBaseUrl` definidos
- [ ] Webhook recadastrado no painel para `/api/webhooks/abacatepay` (a rota legada foi removida)
- [ ] `AbacatePay__ApiKey`, `WebhookSecret` e `WebhookPublicKey` definidos como secrets
- [ ] `AbacatePay__EnableSignatureValidation=true` e `AbacatePay__DevMode=false`
- [ ] Migration `V27_AbacatePaySubscriptionLifecycleAndReconciliation` aplicada
- [ ] `PlanBillingOptions.AbacatePayProductId` preenchido para todo plano/ciclo à venda, com
      `FinalPriceInCents` idêntico ao preço do produto na AbacatePay
- [ ] Cupons ativos conferidos no painel após o primeiro checkout que os utilizar
