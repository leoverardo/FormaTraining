# Round 2 — Segurança de Negócio, Escala e Proteção de Dados

Data: 2026-05-18

## Escopo executado
- Gate premium centralizado no backend (policy/handler + payload 403 padronizado).
- Aplicação do gate em endpoints críticos de trainer e visibilidade pública dependente de assinatura ativa.
- Tratamento de bloqueio premium no frontend (`ACTIVE_SUBSCRIPTION_REQUIRED` com redirecionamento para assinatura).
- Hardening de mídia sensível (categorias corporais forçadas como privadas no upload).
- Hardening de webhook/onboarding (idempotência por `EventId`, transição monotônica e logs estruturados).
- Endurecimento de autorização em cupom autenticado de assinatura (role `Trainer`).
- Refatoração do owner dashboard para reduzir agregações in-memory.

## Owner dashboard (escala)
- Endpoint mantido: `GET /api/owner/dashboard?range=7|30|90`.
- Mudanças técnicas:
  - agregações críticas movidas para banco com `CountAsync`/`SumAsync`/`GroupBy` traduzível;
  - projeções enxutas com `AsNoTracking` e `Take` para listas operacionais;
  - remoção de carregamentos completos de tabelas apenas para totalizadores.
- Contrato de resposta preservado (`OwnerDashboardResponse`).

## Artefatos da rodada
- `docs/premium-access-matrix.md`
- `docs/media-privacy-map.md`
- `docs/onboarding-payment-state-machine.md`
- `docs/billing-discount-rules.md`
