# privacy-lgpd-implementation

## Atualizacao final pre-lancamento

### Geolocalizacao / Explore
- Consentimento `GEOLOCATION_FOR_EXPLORE` confirmado no seed (`ConsentDefinitions`), backend e frontend.
- Explore (`ExploreFeedPage` e `ExploreTrainersPage`) agora exige etapa explicita antes de `navigator.geolocation`.
- Fluxo: explicacao + botoes `Permitir localizacao` e `Continuar sem localizacao`.
- Ao permitir: registra consentimento `GEOLOCATION_FOR_EXPLORE`.
- Ao negar: registra revogacao e segue sem bloquear experiencia.
- Nenhuma persistencia de latitude/longitude do usuario foi adicionada em banco.

### Perfil publico do trainer
- Sincronizacao bidirecional concluida:
- Ativar pagina publica -> concede/sincroniza `PUBLIC_PROFILE_VISIBILITY` (backend `PublicPageService`).
- Desativar pagina publica -> revoga/sincroniza `PUBLIC_PROFILE_VISIBILITY`.
- Revogar consentimento em Privacidade -> desativa pagina publica e remove do Explore.
- Conceder consentimento em Privacidade -> decisao tecnica: autoriza publicacao, mas nao ativa automaticamente a pagina publica (evita publicacao acidental). Ativacao continua no fluxo de configuracao da pagina publica.
- Confirmacao explicita adicionada na ativacao da pagina publica.

### HEALTH_RELATED_DATA_PROCESSING
- Nao tratado como toggle opcional comum.
- Removido da lista de toggles de consentimentos opcionais da UI de privacidade.
- Mantido como bloco informativo com aviso de revisao juridica.
- Endpoint de update bloqueia alteracao direta desse item via UI.
- Onboarding continua com aviso destacado/registravel, com observacao de revisao juridica obrigatoria.

## Pontos pendentes de revisao juridica humana
- Texto final de politica/termos.
- Base legal final para dados potencialmente sensiveis de saude/evolucao.
- Politica de retencao e descarte.
- Procedimento operacional e comunicacao de incidentes (ANPD/titulares).
