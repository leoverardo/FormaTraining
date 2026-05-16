# Round 1 — Production Hardening (P0/P1)

Data: 2026-05-16

## Objetivo da rodada
Corrigir riscos críticos de segurança, estabilidade e deploy antes do lançamento.

## Riscos corrigidos nesta rodada
1. Segredos hardcoded removidos de arquivos versionados principais.
2. Patch SQL manual de schema removido do startup da API.
3. Rate limiting aplicado em endpoints públicos críticos.
4. Explore corrigido para não mascarar falhas internas com `200 OK`.
5. Frontend Explore diferenciado entre vazio legítimo e erro de carregamento.
6. 3 erros de lint corrigidos (restaram apenas warnings).

## Alterações aplicadas
- `Program.cs`
  - Removido `EnsureTrainerExploreColumnsAsync`.
  - Adicionado `UseRateLimiter` e políticas nomeadas.
- Controllers com rate limiting:
  - `AuthController`: `login`, `register-student`, `register-trainer`
  - `PublicOnboardingController`: endpoints de mutação/checkout
  - `PublicPageController`: criação de lead público
  - `ExploreController`: `feed` e `trainers` públicos
- Explore backend:
  - Falhas internas agora retornam `500` + log estruturado.
- Frontend:
  - Explore mostra estado de erro separado de “sem resultados”.
  - Lint errors removidos com ajustes seguros.

## Secrets externalizados
- JWT secret
- AbacatePay API key/webhook secrets
- Cloudinary credentials

Referência de configuração: `docs/production-configuration.md` e `backend/FitPlatform.Api/appsettings.Example.json`.

## SQL scripts
- Caminhos preparados:
  - `database/scripts/forma-training-create-from-scratch.sql`
  - `database/scripts/forma-training-idempotent-update.sql`
- `forma-training-create-from-scratch.sql` gerado via EF migrations script.
- `forma-training-idempotent-update.sql` gerado com blocos idempotentes por migration, condicionados por `__EFMigrationsHistory`, cobrindo bancos vazios e parcialmente migrados.

## Status dos comandos
- `dotnet restore FitPlatform.sln`: falhou por `NU1301` (bloqueio NuGet/rede).
- `dotnet build FitPlatform.sln -v minimal`: falhou por `NU1301`.
- `dotnet test FitPlatform.sln -v minimal`: falhou por `NU1301`.
- `npm run build`: sucesso.
- `npm run lint`: sucesso com 0 erros e 7 warnings.

## Pendências restantes
- Validar backend build/test de forma estável após normalizar conectividade NuGet (ocorrência intermitente de `NU1301` neste ambiente).
- Rotacionar credenciais reais previamente expostas fora do código.
