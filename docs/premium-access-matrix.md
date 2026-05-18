# Premium Access Matrix

| Recurso | Público | Student | Trainer pendente/inativo | Trainer assinatura ativa | Owner |
|--------|---------|---------|----------------------------|--------------------------|-------|
| Dashboard trainer (`/api/trainer/dashboard`) | N | N | N | S | S |
| Gestão de alunos (`/api/students/*`) | N | N | N | S | S |
| Treinos trainer (`/api/workouts/*`) | N | N | N | S | S |
| Exercícios trainer (`/api/exercises/*`) | N | N | N | S | S |
| Sessões do aluno pelo trainer (`/api/students/{id}/workout-sessions`) | N | N | N | S | S |
| Chat (uso trainer) (`/api/chat/*`) | N | S (fluxo student normal) | N | S | S |
| Agenda trainer (`/api/trainer/appointments/*`, `schedule`, `calendar trainer`) | N | N | N | S | S |
| Hábitos/Nutrição trainer (`/api/trainer/students/{id}/habits*`) | N | N | N | S | S |
| Gamificação trainer (`/api/trainer/students/{id}/gamification*`) | N | N | N | S | S |
| Posts/publicações trainer (`/api/posts/*`) | N | N | N | S | S |
| Leads internos trainer (`/api/trainer/leads*`) | N | N | N | S | S |
| Ofertas e pedidos B2C do trainer (`/api/trainer/service-*`) | N | N | N | S | S |
| Perfil/assinatura do trainer (`/api/trainer/profile`, `/api/trainer/subscription`) | N | N | S | S | S |
| Checkout/regularização assinatura (`/api/payments/subscriptions/checkout`) | N | N | S | S | S |
| Página pública por slug (`/api/public/trainers/{slug}`) | S* | S* | N/A | S* | S |
| Explore/listagem pública (`/api/explore/*`) | S* | S* | N/A | S* | S |

Notas:
- `S*` público condicionado: trainer precisa estar com assinatura ativa para permanecer visível em página pública, explore e geração de lead.
- Onboarding público (`/api/public/trainer-onboarding/*`) permanece acessível sem assinatura ativa, por ser fluxo pré-ativação.
- Regras de bloqueio premium são aplicadas no backend via policy centralizada (`ActiveTrainerSubscription`) com resposta `403` padronizada.
