# Configuração de Produção — Secrets e Hardening

## Objetivo
Centralizar configuração sensível fora do repositório.

## Secrets externalizados
- `Jwt:Secret`
- `AbacatePay:ApiKey`
- `AbacatePay:WebhookSecret`
- `AbacatePay:WebhookPublicKey`
- `Cloudinary:CloudName`
- `Cloudinary:ApiKey`
- `Cloudinary:ApiSecret`

## Variáveis de ambiente suportadas
- `CLOUDINARY_CLOUD_NAME`
- `CLOUDINARY_API_KEY`
- `CLOUDINARY_API_SECRET`
- `CLOUDINARY_FOLDER`
- `ABACATEPAY_BASE_URL`
- `ABACATEPAY_API_KEY`
- `ABACATEPAY_WEBHOOK_SECRET`
- `ABACATEPAY_WEBHOOK_PUBLIC_KEY`
- `ABACATEPAY_SUCCESS_URL`
- `ABACATEPAY_RETURN_URL`
- `ABACATEPAY_DEV_MODE`

## Arquivos de referência
- Exemplo seguro: `backend/FitPlatform.Api/appsettings.Example.json`
- Base versionada sem segredo real: `backend/FitPlatform.Api/appsettings.json`
- Ajustes locais: `appsettings.Local.json` / `appsettings.*.local.json` (já ignorados por `.gitignore`)

## Rate limiting configurável por ambiente
Seção: `RateLimiting:Policies`
- `AuthLogin`
- `StudentRegister`
- `TrainerOnboarding`
- `PublicLead`
- `ExplorePublicSearch`

Cada política possui:
- `PermitLimit`
- `WindowSeconds`
- `QueueLimit`

## Rotação obrigatória
Credenciais que já foram expostas historicamente em código devem ser rotacionadas manualmente fora do repositório antes do go-live.
