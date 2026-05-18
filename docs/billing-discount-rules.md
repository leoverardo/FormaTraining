# Billing Discount Rules

## Fonte da verdade
- Cálculo de preço final é centralizado no backend (`PaymentService`).
- Frontend apenas exibe preview e resultado retornado da API.

## Regras de ciclo
- Mensal: sem desconto de ciclo.
- Trimestral: 10% de desconto sobre base de 3 meses.
- Semestral: 15% de desconto sobre base de 6 meses.
- Anual: 20% de desconto sobre base de 12 meses.

## Regras de cupom
- Cupom é validado no backend com vigência, escopo, limites de uso e mínimo de compra.
- Cupom pode reduzir subtotal já descontado do ciclo.
- Desconto final é limitado ao subtotal (não gera valor negativo).
- Resgate definitivo incrementa uso apenas em confirmação de pagamento.

## Stacking (ciclo + cupom)
- Suportado: primeiro aplica desconto de ciclo, depois desconto de cupom.
- Persistência inclui:
  - `BaseAmountInCents`
  - `CycleDiscountAmountInCents`
  - `CouponDiscountAmountInCents`
  - `FinalAmountInCents`

## Arredondamento
- Arredondamento em centavos com `MidpointRounding.AwayFromZero`.

## Acesso
- Validação de cupom de assinatura interna endurecida para role `Trainer` no endpoint autenticado.
- Validação pública de cupom para onboarding permanece no fluxo público específico de onboarding.
