# Media Privacy Map

| Tipo de mídia | Entidade | Pública/Privada | Como é armazenada | Quem acessa | Risco atual | Ajuste aplicado |
|--------------|----------|-----------------|-------------------|-------------|-------------|----------------|
| Foto de progresso | `MediaFile` + `StudentProgressPhoto` | Privada | Cloudinary/Local via `MediaService` | Student dono, Trainer dono do aluno, Owner | Exposição indevida por flag pública | Forçado `isPublic=false` por categoria sensível |
| Transformação antes/depois | `MediaFile` + `StudentTransformation` | Privada por padrão | Cloudinary/Local via `MediaService` | Student/Trainer/Owner | Uso indevido em fluxo público | Forçado `isPublic=false` para `TransformationBefore/After` |
| Foto de perfil trainer | `MediaFile` + `Trainer` | Controlada | Cloudinary/Local | Público somente em perfil público ativo | exposição fora de contexto | Fluxos públicos agora exigem assinatura ativa |
| Logo trainer | `MediaFile` + `Trainer` | Controlada | Cloudinary/Local | Público em contexto de página pública ativa | idem | condicionado à visibilidade pública + assinatura ativa |
| Banner público | `MediaFile` + `Trainer` | Pública controlada | Cloudinary/Local | Público | persistência após inativação | exibição pública condicionada a `PublicPageEnabled` e assinatura ativa |
| Capa/vídeo de post | `MediaFile` + `Post` | Pública quando post público | Cloudinary/Local | Público conforme visibilidade do post | confusão público/privado | permanece sob visibilidade do post e elegibilidade do trainer |
| Mídia de exercício | `MediaFile` + `Exercise` | Privada operacional | Cloudinary/Local | Trainer/Student vinculados | baixa | sem mudança estrutural nesta rodada |

## Regras consolidadas na rodada
- Categorias sensíveis agora são forçadas para privadas no backend (`MediaService`).
- Exposição pública de conteúdo do trainer depende de:
  - página pública ativa;
  - flags de busca/visibilidade aplicáveis;
  - assinatura ativa do trainer.

## Gap remanescente documentado
- URLs já públicas em provider externo podem continuar acessíveis até política completa de signed URLs/expiração por arquivo.
- Próximo passo recomendado: estratégia de URL assinada para mídia privada e revogação ativa de assets em despublicação/exclusão.
