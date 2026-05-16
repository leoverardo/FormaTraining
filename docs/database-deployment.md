# Database Deployment

## Princípio
O schema é controlado por migrations EF Core. A API não deve executar patch SQL manual de alteração de schema no startup.

## Scripts oficiais
- `database/scripts/forma-training-create-from-scratch.sql`
  - Uso: banco novo/vazio.
  - Conceito: script linear desde migration `0` até a última migration.
- `database/scripts/forma-training-idempotent-update.sql`
  - Uso: atualização segura em bancos já existentes.
  - Conceito: script idempotente com checagem em `__EFMigrationsHistory`.
  - Implementação atual no repositório: blocos condicionais por migration (`IF NOT EXISTS ... MigrationId`), seguros para banco vazio e para banco parcialmente migrado.

## Como gerar os scripts
```bash
dotnet ef migrations script 0 20260515234913_V21_PrivacyLgpdFoundation --project backend/FitPlatform.Infrastructure --startup-project backend/FitPlatform.Api --output database/scripts/forma-training-create-from-scratch.sql
dotnet ef migrations script --idempotent --project backend/FitPlatform.Infrastructure --startup-project backend/FitPlatform.Api --output database/scripts/forma-training-idempotent-update.sql
```

## Como aplicar
1. Banco novo:
   - executar `forma-training-create-from-scratch.sql`.
2. Banco existente:
   - executar `forma-training-idempotent-update.sql`.

## Migrations locais
```bash
dotnet ef database update --project backend/FitPlatform.Infrastructure --startup-project backend/FitPlatform.Api
```

## Seeds
- Seeds de domínio continuam sendo executados no boot da API (`DatabaseSeeder.SeedAsync` e enriquecimento existente).
- Após criar o schema via script SQL, subir a API ao menos uma vez para garantir população dos dados essenciais de seed.

## Observação do ambiente atual
Houve instabilidade de conectividade NuGet (`NU1301`) durante parte da validação backend. A geração e o conteúdo dos scripts em `database/scripts` foram concluídos, mas recomenda-se regeneração oficial via `dotnet ef migrations script` em pipeline de release com conectividade estável.
