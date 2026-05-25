# Webhook AbacatePay — Forma Training

Documentação de integração do webhook AbacatePay com o backend da plataforma.

---

## Endpoint

| Ambiente    | URL                                                                 |
|-------------|---------------------------------------------------------------------|
| Local       | `POST http://localhost:5000/api/webhooks/abacatepay?webhookSecret={secret}` |
| Produção    | `POST https://api.formatraining.com.br/api/webhooks/abacatepay?webhookSecret={secret}` |

> ⚠️ A AbacatePay exige URL **HTTPS pública** para produção. Use um proxy reverso (Nginx/Cloudflare) ou um serviço como [ngrok](https://ngrok.com) para testes locais.

---

## Configuração no Dashboard AbacatePay

1. Acesse o painel da AbacatePay → **Webhooks**.
2. Adicione o endpoint com a URL de produção.
3. Copie o **Webhook Secret** gerado e configure em `AbacatePay:WebhookSecret` (ver variáveis abaixo).
4. Copie a **Public Key** e configure em `AbacatePay:WebhookPublicKey`.
5. Selecione os eventos que deseja receber (recomendado: todos os listados na seção **Eventos** abaixo).

---

## Variáveis de ambiente / appsettings

| Variável de ambiente              | appsettings key                    | Descrição                                                        |
|-----------------------------------|------------------------------------|------------------------------------------------------------------|
| `ABACATEPAY_WEBHOOK_SECRET`       | `AbacatePay:WebhookSecret`         | Secret enviado na query string `?webhookSecret=...`               |
| `ABACATEPAY_WEBHOOK_PUBLIC_KEY`   | `AbacatePay:WebhookPublicKey`      | Chave para validar assinatura HMAC-SHA256 do header               |
| — (apenas appsettings)            | `AbacatePay:EnableSignatureValidation` | `false` em dev, **`true` em produção** (obrigatório)         |

### Exemplo em `appsettings.json` (local/dev)

```json
"AbacatePay": {
  "WebhookSecret": "meu-secret-local",
  "WebhookPublicKey": "",
  "EnableSignatureValidation": false
}
```

### Exemplo em produção (variáveis de ambiente)

```bash
ABACATEPAY_WEBHOOK_SECRET=<secret-do-dashboard>
ABACATEPAY_WEBHOOK_PUBLIC_KEY=<public-key-do-dashboard>
# No appsettings.Production.json:
# "AbacatePay:EnableSignatureValidation": true
```

---

## Headers e autenticação

| Header / Param            | Obrigatório | Descrição                                                         |
|---------------------------|-------------|-------------------------------------------------------------------|
| `?webhookSecret=...`      | ✅ Sempre   | Query param com o secret configurado no dashboard                  |
| `X-Webhook-Signature`     | ✅ Produção | HMAC-SHA256 do body raw, codificado em Base64                      |
| `Content-Type`            | —           | `application/json`                                                 |

### Como a assinatura é calculada

```
X-Webhook-Signature = Base64( HMACSHA256( PublicKey, rawBody ) )
```

O sistema rejeita qualquer webhook não assinado em produção, mesmo que `EnableSignatureValidation` esteja como `false` nas configs — isso é uma proteção de segurança obrigatória.

---

## Eventos tratados

| Evento                      | Ação                                                                            |
|-----------------------------|---------------------------------------------------------------------------------|
| `subscription.completed`    | Ativa assinatura interna, salva IDs externos, libera acesso, avança onboarding  |
| `subscription.renewed`      | Registra novo pagamento de renovação, estende `EndDate`, mantém ativo           |
| `subscription.cancelled`    | Marca assinatura como Cancelada; `EndDate` é preservado para acesso gracioso    |
| `checkout.completed`        | Fallback: ativa assinatura pendente caso `subscription.completed` não chegue    |
| Qualquer outro evento        | Salvo no log de webhooks e retorna `200 OK` (não quebra)                        |

---

## Formato do payload

```json
{
  "id": "log_abc123",
  "event": "subscription.completed",
  "apiVersion": 2,
  "devMode": false,
  "data": {
    "subscription": { "id": "sub_xyz" },
    "checkout":     { "id": "chk_xyz" },
    "customer":     { "id": "cus_xyz" },
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

> O campo `data` pode receber novos campos em versões futuras da API. O sistema nunca valida rigidamente a estrutura de `data`.

### Estratégia de resolução da assinatura interna

O serviço tenta encontrar a `TrainerSubscription` interna na seguinte ordem de prioridade:

1. `data.metadata.trainerSubscriptionId` → lookup por ID interno (mais preciso)
2. `data.subscription.id` → lookup por `AbacatePaySubscriptionId`
3. `data.checkout.id` ou `data.id` → lookup por `AbacatePayCheckoutId`
4. `data.externalId` (formato `trainer:{id}:sub:{id}`) → lookup por ID interno

---

## Idempotência

O sistema usa a tabela `PaymentWebhookLogs` com índice único em `(Provider, EventId)`:

- Se o `id` do evento já foi processado com sucesso (`ProcessingStatus = Processed`), retorna `200 OK` imediatamente sem reprocessar.
- Eventos em `Failed` podem ser reprocessados (o `RetryCount` é incrementado).
- O campo `id` do payload JSON é o identificador de idempotência — **nunca reutilize o mesmo ID para eventos diferentes**.

### Estados do ProcessingStatus

| Valor | Nome        | Descrição                                        |
|-------|-------------|--------------------------------------------------|
| 0     | Pending     | Recebido, ainda não processado                   |
| 1     | Processing  | Em processamento (proteção contra re-entradas)   |
| 2     | Processed   | Processado com sucesso                           |
| 3     | Failed      | Falha; pode ser reprocessado                     |
| 4     | Duplicate   | Duplicata; ignorado                              |

---

## Respostas HTTP

| Código | Situação                                                               |
|--------|------------------------------------------------------------------------|
| `200`  | Evento aceito (inclui duplicatas — AbacatePay não vai retentar)         |
| `401`  | Secret inválido ou assinatura HMAC inválida                            |
| `500`  | Erro interno — AbacatePay **vai retentar** automaticamente             |

---

## Como testar localmente

### Com curl (sem validação de assinatura, `EnableSignatureValidation: false`)

```bash
curl -X POST "http://localhost:5000/api/webhooks/abacatepay?webhookSecret=meu-secret-local" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "log_teste_001",
    "event": "subscription.completed",
    "apiVersion": 2,
    "devMode": true,
    "data": {
      "checkout": { "id": "chk_seu_checkout_id" },
      "subscription": { "id": "sub_teste" },
      "payment": { "id": "pay_teste", "amount": 5990 },
      "customer": { "id": "cus_teste" }
    }
  }'
```

### Com ngrok (para receber webhooks reais da AbacatePay em desenvolvimento)

```bash
# Instalar ngrok: https://ngrok.com/download
ngrok http 5000

# Copiar a URL gerada (ex: https://abc123.ngrok.io) e configurar no dashboard AbacatePay:
# https://abc123.ngrok.io/api/webhooks/abacatepay?webhookSecret=meu-secret
```

> Com ngrok, ative `EnableSignatureValidation: true` localmente e configure a public key para testar o fluxo completo.

### Com AbacatePay CLI (se disponível)

```bash
abacatepay webhook forward \
  --url http://localhost:5000/api/webhooks/abacatepay \
  --secret meu-secret-local
```

---

## Segurança — checklist

- [x] Secret comparado com `CryptographicOperations.FixedTimeEquals` (sem timing attack)
- [x] Assinatura HMAC comparada com `CryptographicOperations.FixedTimeEquals`
- [x] `EnableSignatureValidation = false` só aceito em `Development` — em produção sempre rejeita webhooks não assinados
- [x] Nenhum dado sensível nos logs (secret, assinatura, CPF/CNPJ, tokens)
- [x] Body raw lido com `EnableBuffering()` — stream não consumida antes da validação
- [x] Eventos `devMode: true` processados normalmente (a separação de ambiente é feita pelo dashboard AbacatePay)
- [x] Endpoint `[AllowAnonymous]` — não requer JWT, apenas secret + assinatura
- [x] Idempotência garantida via transação `Serializable` + índice único

---

## Logs estruturados

O sistema registra os seguintes campos estruturados (sem dados sensíveis):

```
EventId={...}   EventType={...}   DevMode={...}
SubscriptionId={...}   TrainerId={...}
CheckoutId={...}   AbacateSubId={...}   PaymentId={...}
```

Para monitorar webhooks que falharam:

```sql
SELECT EventId, Type, ProcessingStatus, ErrorMessage, RetryCount, CreatedAt
FROM PaymentWebhookLogs
WHERE Provider = 'AbacatePay'
  AND ProcessingStatus = 3  -- Failed
ORDER BY CreatedAt DESC;
```
