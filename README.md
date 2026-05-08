# FitPlatform MVP

Plataforma SaaS para personal trainers gerenciarem alunos, treinos e consultorias online.

## Modelo de negócio

- **B2B**: o personal trainer contrata a plataforma e paga uma assinatura mensal, trimestral ou anual
- Alunos **não pagam** diretamente pela plataforma
- O personal controla seus próprios alunos, exercícios, treinos, conteúdos e progresso físico dos alunos
- O acesso dos alunos depende da assinatura ativa do personal trainer

## Funcionalidades do MVP

- Onboarding multi-etapas do personal trainer (dados pessoais, profissionais, endereço, plano, pagamento)
- Definição de senha por link/token seguro
- Painel do personal com gestão de alunos, exercícios, treinos, rotina semanal, posts e assinatura
- Perfil profissional completo do personal (CREF, bio, especialidades, endereço, identidade visual)
- Planos com ciclos de cobrança (mensal, trimestral, anual)
- Cadastro de alunos com envio de link de acesso por e-mail
- Progresso físico dos alunos (peso, medidas, % gordura)
- Fotos de progresso com comparador lado a lado
- Área do aluno com treinos, conteúdos, progresso e fotos
- Controle de bloqueio por assinatura vencida

## Stack

**Backend:** C# / .NET 8 · ASP.NET Core · Entity Framework Core · SQL Server · JWT · BCrypt · Swagger

**Frontend:** React 18 · Vite · TailwindCSS 4 · React Router · Axios · Lucide Icons

## Estrutura de pastas

```
AppTreino/
├── backend/
│   ├── FitPlatform.Api/          # Controllers, Program.cs, Middlewares
│   ├── FitPlatform.Application/  # DTOs, Interfaces, Common
│   ├── FitPlatform.Domain/       # Entities, Enums
│   └── FitPlatform.Infrastructure/ # DbContext, Services, Seed, Migrations, PaymentProviders, ExternalServices
├── frontend/
│   └── src/
│       ├── pages/auth/           # Login, Registro (onboarding multi-step), Definir senha
│       ├── pages/owner/          # Dashboard Owner, Planos
│       ├── pages/trainer/        # Dashboard, Alunos, Detalhe do Aluno, Exercícios, Treinos, etc.
│       ├── pages/student/        # Dashboard, Treinos, Posts, Progresso, Fotos, Acesso
│       ├── components/ui/        # Componentes reutilizáveis
│       ├── contexts/             # AuthContext
│       ├── layouts/              # TrainerLayout, StudentLayout, OwnerLayout
│       ├── routes/               # ProtectedRoute
│       └── services/             # Service layer (API calls)
└── README.md
```

## Configuração do SQL Server

Edite `backend/FitPlatform.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FitPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Com usuário/senha:
```json
"DefaultConnection": "Server=localhost;Database=FitPlatformDb;User Id=sa;Password=Sua_senha123;TrustServerCertificate=True;"
```

## Como rodar o Backend

```bash
cd backend/FitPlatform.Api
dotnet run
```

O banco é criado, as migrations são aplicadas e os seeds são executados automaticamente ao iniciar.

## Como rodar o Frontend

```bash
cd frontend
npm install
npm run dev
```

## URLs principais

| URL | Descrição |
|-----|-----------|
| http://localhost:5173 | Frontend |
| http://localhost:5000 | Backend API |
| http://localhost:5000/swagger | Swagger/OpenAPI |

## Usuários de teste

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Owner | admin@test.com | 123456 |
| Trainer | trainer@test.com | 123456 |
| Aluno | aluno@test.com | 123456 |

## Planos e ciclos de cobrança

| Plano | Mensal | Trimestral | Anual | Limite |
|-------|--------|------------|-------|--------|
| Starter | R$ 97 | R$ 267 | R$ 997 | 20 alunos |
| Pro | R$ 197 | R$ 547 | R$ 1.997 | 50 alunos |
| Growth | R$ 297 | R$ 797 | R$ 2.997 | 100 alunos |

## Fluxo de onboarding do personal

1. Acessar `/register`
2. Preencher dados pessoais (nome, e-mail, CPF)
3. Preencher dados profissionais (marca, CREF, bio, Instagram)
4. Preencher endereço
5. Selecionar plano e ciclo de cobrança
6. Clicar em "Simular pagamento aprovado"
7. Ver no **console do servidor** o link de definição de senha
8. Acessar `/set-password?token=<token>`
9. Definir a senha e logar

## Fluxo de criação de aluno

1. Personal acessa **Alunos** → **Novo aluno**
2. Preenche nome, e-mail, objetivo, etc.
3. O sistema cria o User com `MustChangePassword = true`
4. O **console do servidor** exibe o link de acesso do aluno
5. O aluno acessa `/set-password?token=<token>` e define sua senha
6. O aluno faz login em `/login`

Para reenviar o link: botão **"Reenviar acesso"** na aba Dados do detalhe do aluno.

## Como testar pagamento simulado

Via tela de **Assinatura** no painel do trainer:
- **"Simular pagamento aprovado"** → ativa assinatura
- **"Simular vencimento"** → expira assinatura (bloqueia painel e alunos)

Via API:
```
POST /api/trainer/subscription/simulate-approved
POST /api/trainer/subscription/simulate-expired
```

## Como testar progresso do aluno

**Como trainer:**
1. Acesse **Alunos** → clique no ícone `>` para abrir o detalhe
2. Clique na aba **Progresso** → **Registrar**
3. Preencha peso, medidas, % gordura

**Como aluno:**
1. Acesse **Meu Progresso** no menu
2. Clique em **Registrar**

## Como testar fotos de progresso

**Como aluno:**
1. Acesse **Fotos** no menu
2. Clique em **Adicionar** e informe a URL da imagem
3. Para comparar: selecione uma foto "Antes" e uma "Depois" no painel de comparação

## Como testar bloqueio por assinatura

1. Simule o vencimento: **Assinatura → Simular vencimento**
2. Tente criar/editar alunos → botões bloqueados
3. Faça login como aluno → tela de bloqueio exibida
4. Simule pagamento aprovado para restaurar acesso

## Novos endpoints (v2)

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/public/trainer-onboarding | Iniciar onboarding |
| GET | /api/public/trainer-onboarding/{id} | Consultar onboarding |
| PUT | /api/public/trainer-onboarding/{id}/professional-data | Dados profissionais |
| PUT | /api/public/trainer-onboarding/{id}/address | Endereço |
| PUT | /api/public/trainer-onboarding/{id}/select-plan | Selecionar plano |
| POST | /api/public/trainer-onboarding/{id}/simulate-payment-approved | Simular pagamento |
| POST | /api/auth/set-password | Definir senha por token |
| POST | /api/auth/request-password-reset | Solicitar reset de senha |
| POST | /api/students/{id}/resend-access-email | Reenviar e-mail de acesso ao aluno |
| GET | /api/students/{id}/progress | Progresso do aluno (trainer) |
| POST | /api/students/{id}/progress | Criar progresso (trainer) |
| GET | /api/students/{id}/progress-photos | Fotos do aluno (trainer) |
| POST | /api/students/{id}/progress-photos | Adicionar foto (trainer) |
| GET | /api/student/progress | Meu progresso (aluno) |
| POST | /api/student/progress | Registrar progresso (aluno) |
| GET | /api/student/progress-photos | Minhas fotos (aluno) |
| POST | /api/student/progress-photos | Adicionar foto (aluno) |

## Migrations

- `InitialCreate` — estrutura inicial do banco
- `AddOnboardingProgressAndExtendedProfile` — onboarding, progresso, fotos, campos estendidos de trainer/student/user, PlatformPlanPrice, PasswordSetupToken

## Próximos passos para integração real com Mercado Pago

1. Preencher `MercadoPago:AccessToken` e `MercadoPago:PublicKey` em `appsettings.json`
2. Implementar `MercadoPagoPaymentProvider.CreateSubscriptionAsync` — POST para `https://api.mercadopago.com/preapproval`
3. Implementar `PaymentService.HandleWebhookAsync` — validar assinatura e processar eventos
4. Validar assinatura do webhook com `MercadoPago:WebhookSecret` em `PaymentsController.Webhook`
5. Para e-mail real: substituir `ConsoleEmailService` por implementação SMTP/SendGrid/Resend

## Segurança

- JWT Bearer com claims de UserId, Role, TrainerId, StudentId
- `PasswordHash` nunca exposto nas respostas
- `PasswordSetupToken` salvo como hash BCrypt, não puro
- Token de setup expira em 24h (trainer) ou 7 dias (aluno)
- Isolamento multi-tenant por `TrainerId` em todas as consultas
- Alunos de um trainer não são visíveis para outros trainers
- Fotos de progresso são privadas por regra de acesso no backend

## Upload de midia (Cloudinary + fallback local)

### Configuracao

No `backend/FitPlatform.Api/appsettings.json` e `appsettings.Development.json`:

```json
"Storage": {
  "Provider": "Cloudinary",
  "FallbackProvider": "Local",
  "MaxImageSizeMB": 5,
  "MaxVideoSizeMB": 100,
  "AllowedImageTypes": ["image/jpeg", "image/png", "image/webp"],
  "AllowedVideoTypes": ["video/mp4", "video/webm", "video/quicktime"],
  "LocalBasePath": "wwwroot/uploads",
  "PublicBaseUrl": "https://localhost:5001/uploads"
},
"Cloudinary": {
  "CloudName": "",
  "ApiKey": "",
  "ApiSecret": "",
  "Folder": "fitplatform"
}
```

- `Storage:Provider=Cloudinary` com credenciais validas usa Cloudinary.
- Em `Development`, sem credenciais, cai automaticamente para `Local` quando `FallbackProvider=Local`.
- Fora de `Development`, sem credenciais, o backend retorna erro claro de configuracao.
- Nao commitar credenciais reais.

### Variaveis de ambiente

Voce pode configurar por:

- `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`, `Cloudinary__Folder`
- ou `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`, `CLOUDINARY_FOLDER`

### Estrutura de pastas no Cloudinary

Base: `fitplatform/`

- `trainers/{trainerId}/profile`
- `trainers/{trainerId}/logo`
- `trainers/{trainerId}/banner`
- `trainers/{trainerId}/exercises`
- `trainers/{trainerId}/posts`
- `trainers/{trainerId}/students/{studentId}/progress`
- `trainers/{trainerId}/transformations`

### Endpoints

- `POST /api/media/upload` (multipart: `file`, `category`, `studentId?`, `isPublic?`)
- `POST /api/media/upload/profile-photo`
- `POST /api/media/upload/logo`
- `POST /api/media/upload/banner`
- `POST /api/media/upload/exercise-image`
- `POST /api/media/upload/exercise-video`
- `POST /api/media/upload/post-cover`
- `POST /api/media/upload/progress-photo`
- `DELETE /api/media/{id}`

### Validacoes

- Imagens: `image/jpeg`, `image/png`, `image/webp` ate 5 MB
- Videos: `video/mp4`, `video/webm`, `video/quicktime` ate 100 MB
- Bloqueio de extensoes perigosas (`.exe`, `.bat`, `.cmd`, `.ps1`, etc)
- Nomes de arquivo seguros com GUID

### Como testar no Swagger/Postman

1. Autentique com JWT.
2. Chame `POST /api/media/upload` com `multipart/form-data`.
3. Informe `file` + `category` (enum `MediaCategory`).
4. Para progresso, use `category=ProgressPhoto` (sempre privado).
5. Verifique retorno com `url`, `secureUrl`, `thumbnailUrl`, `provider` e `sizeInBytes`.

### Seguranca e acesso

- Trainer acessa apenas midias do proprio `TrainerId`.
- Student acessa apenas as proprias midias (`StudentId`).
- Midia publica depende de `IsPublic=true` e regras da categoria.
- `PublicId`/`ProviderKey` do Cloudinary e salvo para exclusao futura.

## Assinaturas Mercado Pago (Trainer B2B)

O pagador da assinatura e o personal trainer. Alunos nao pagam na plataforma.

### Configuracao no backend

`backend/FitPlatform.Api/appsettings.json`:

```json
"MercadoPago": {
  "AccessToken": "",
  "PublicKey": "",
  "WebhookSecret": "",
  "NotificationUrl": "",
  "SuccessUrl": "",
  "FailureUrl": "",
  "PendingUrl": ""
}
```

### Variaveis de ambiente

- `MERCADOPAGO_ACCESS_TOKEN`
- `MERCADOPAGO_PUBLIC_KEY`
- `MERCADOPAGO_WEBHOOK_SECRET`
- `MERCADOPAGO_NOTIFICATION_URL`
- `MERCADOPAGO_SUCCESS_URL`
- `MERCADOPAGO_FAILURE_URL`
- `MERCADOPAGO_PENDING_URL`

### Endpoints de pagamento

- `POST /api/payments/create-trainer-subscription` (trainer autenticado)
- `POST /api/payments/webhook` (Mercado Pago)
- `GET /api/trainer/subscription` (status local da assinatura)
- `POST /api/payments/simulate-approved` (teste)
- `POST /api/payments/simulate-expired` (teste)

### Fluxo de criacao de assinatura

1. Front chama `create-trainer-subscription`.
2. Backend cria `TrainerSubscription` com status `Pending`.
3. Backend cria assinatura no Mercado Pago (preapproval).
4. Backend salva `MercadoPagoSubscriptionId`, `MercadoPagoPayerId` e `InitPoint`.
5. Backend retorna `checkoutUrl` (`init_point`) para o frontend redirecionar.

### Webhook (idempotente + reconciliacao)

1. Mercado Pago chama `POST /api/payments/webhook`.
2. Backend valida assinatura quando `WebhookSecret` estiver configurado.
3. Evento e salvo em `PaymentWebhookLog` com chave idempotente (`Provider + EventId`).
4. Backend consulta `GET /preapproval/{id}` no Mercado Pago para confirmar status.
5. Status `authorized/approved` ativa assinatura e cria `TrainerPayment`.
6. Status `cancelled/paused/expired` atualiza assinatura local.

### Billing cycle mapeado para preapproval

- `Monthly` => `frequency=1`, `frequency_type=months`
- `Quarterly` => `frequency=3`, `frequency_type=months`
- `Yearly` => `frequency=12`, `frequency_type=months`

### Testar webhook com ngrok

1. Rode a API localmente.
2. Abra tunel: `ngrok http 5000`.
3. Configure no Mercado Pago:
   - `NotificationUrl = https://SEU-NGROK/api/payments/webhook`
4. Atualize tambem `MERCADOPAGO_NOTIFICATION_URL` no backend.
5. Dispare eventos no painel sandbox do Mercado Pago e valide o `PaymentWebhookLog`.

### Testar em sandbox

1. Use `AccessToken` e `PublicKey` de sandbox.
2. Crie assinatura via endpoint.
3. Acesse `checkoutUrl` retornado e complete o fluxo de teste.
4. Confira:
   - `TrainerSubscription` (status, `LastPaymentStatus`)
   - `TrainerPayment` (historico de pagamento)
   - `PaymentWebhookLog` (idempotencia/auditoria)
