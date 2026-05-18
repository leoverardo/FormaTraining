# Onboarding + Payment State Machine

## Estados
- `Draft`
- `WaitingPayment`
- `PaymentApproved`
- `AccountCreated`
- `Completed`
- `Canceled`

## Transições permitidas (fluxo normal)
1. `Draft -> WaitingPayment` (seleção de plano/ciclo + checkout)
2. `WaitingPayment -> PaymentApproved` (webhook de pagamento confirmado)
3. `PaymentApproved -> AccountCreated` (conta trainer garantida/ativada)
4. `AccountCreated -> Completed` (onboarding finalizado)

## Transições bloqueadas
- Regressão de estado avançado para estado anterior por webhook tardio.
- `Completed -> qualquer outro`.
- `Canceled -> progressão automática por webhook`.

## Regras de idempotência
- Evento de webhook repetido com mesmo `EventId` é ignorado com log de idempotência.
- Estado do onboarding avança de forma monotônica por ranking interno; não ocorre downgrade.

## Papel do webhook
- Correlaciona assinatura por metadata/subscriptionId/checkoutId.
- Atualiza status da assinatura e pagamento.
- Avança onboarding apenas quando apropriado e sem sobrescrever estado mais avançado.

## Exemplo de fluxo normal
1. Checkout criado (`WaitingPayment`).
2. Webhook `subscription.completed`.
3. Assinatura ativa.
4. Onboarding avança `PaymentApproved -> AccountCreated -> Completed`.

## Exemplo de webhook duplicado
1. Evento já registrado em `PaymentWebhookLogs`.
2. Novo processamento é ignorado.
3. Sem duplicar efeitos de transição.
