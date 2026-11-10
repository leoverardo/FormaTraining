# Configuração de Produção — Secrets e Hardening

## Objetivo
Centralizar configuração sensível fora do repositório.

> `appsettings.json` e `appsettings.Development.json` são ignorados pelo `.gitignore`, mas
> **são copiados para a imagem Docker** (`COPY . .`). Por isso o `appsettings.json` versionável
> não contém segredo real nem domínio de produção: tudo vem de variável de ambiente.

## Secrets externalizados
- `Jwt:Secret`
- `AbacatePay:ApiKey`
- `AbacatePay:WebhookSecret`
- `AbacatePay:WebhookPublicKey`
- `Cloudinary:CloudName`
- `Cloudinary:ApiKey`
- `Cloudinary:ApiSecret`
- `ConnectionStrings:DefaultConnection`

## Convenção de variáveis de ambiente

A forma **canônica** usa o separador padrão do ASP.NET Core (duplo underscore):

```env
Platform__FrontendBaseUrl=https://app.seu-dominio.com
Platform__ApiBaseUrl=https://api.seu-dominio.com

Jwt__Secret=
ConnectionStrings__DefaultConnection=

AbacatePay__ApiKey=
AbacatePay__WebhookSecret=
AbacatePay__WebhookPublicKey=
AbacatePay__EnableSignatureValidation=true
AbacatePay__DevMode=false

Cloudinary__CloudName=
Cloudinary__ApiKey=
Cloudinary__ApiSecret=
Cloudinary__Folder=fitplatform
```

A lista completa das chaves da AbacatePay (retryPolicy, reconciliação, retry de webhook) está
em [`abacatepay-webhook.md`](abacatepay-webhook.md#2-variáveis-de-ambiente).

### Nomes legados (ainda suportados)

Mantidos como **alias de menor precedência** — se a chave canônica estiver definida, o alias é
ignorado; se o alias estiver vazio, ele não apaga o valor do `appsettings`:

`CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`, `CLOUDINARY_FOLDER`,
`ABACATEPAY_BASE_URL`, `ABACATEPAY_API_KEY`, `ABACATEPAY_WEBHOOK_SECRET`,
`ABACATEPAY_WEBHOOK_PUBLIC_KEY`, `ABACATEPAY_SUCCESS_URL`, `ABACATEPAY_RETURN_URL`,
`ABACATEPAY_DEV_MODE`, `ABACATEPAY_ENABLE_SIGNATURE_VALIDATION`,
`PLATFORM_FRONTEND_BASE_URL`, `PLATFORM_API_BASE_URL`.

> Antes da padronização, esses nomes eram gravados no provider de maior precedência **mesmo
> quando ausentes**, apagando os valores do `appsettings.json`. Esse comportamento foi corrigido.

## Validação de startup

`AbacatePayOptionsValidator` roda com `ValidateOnStart()`. Fora de `Development` a aplicação
**não sobe** se faltar `AbacatePay:ApiKey`, `WebhookSecret`, `WebhookPublicKey`, `SuccessUrl`
ou `ReturnUrl`, ou se `EnableSignatureValidation` estiver `false`. As mensagens citam apenas o
nome da chave, nunca o valor.

## Arquivos de referência
- Exemplo seguro versionado: `backend/FitPlatform.Api/appsettings.Example.json`
- Local (ignorado pelo git): `backend/FitPlatform.Api/appsettings.json`
- Ajustes locais: `appsettings.Local.json` / `appsettings.*.local.json` (já ignorados)

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
Credenciais que já foram expostas historicamente em código devem ser rotacionadas manualmente
fora do repositório antes do go-live. Isso inclui o `Jwt:Secret` de desenvolvimento presente
no `appsettings.json` local — defina `Jwt__Secret` em produção.
