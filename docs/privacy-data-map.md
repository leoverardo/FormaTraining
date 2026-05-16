# privacy-data-map

## Dados potencialmente sensiveis
- StudentProgress (peso, medidas, composicao corporal): acompanhamento fisico.
- StudentWeeklyCheckIn (peso, humor, energia, sono, notas): rotina e saude.
- StudentAnamnesis (lesoes, restricoes de saude): informacoes de saude/autocuidado.
- StudentHabit/StudentHabitLog: comportamento de rotina pessoal.
- StudentNutritionGuidance: orientacoes alimentares.
- StudentProgressPhoto/StudentTransformation: imagens corporais.

## Decisao tecnica sobre HEALTH_RELATED_DATA_PROCESSING
- Tratado como aviso/registro destacado, nao como opt-in de marketing.
- UI de Privacidade exibe bloco informativo; nao oferece toggle livre.
- Revisao juridica final obrigatoria para texto e base legal.

## Geolocalizacao
- Uso apenas sob acao explicita do usuario no Explore.
- Finalidade: recomendacao de trainers proximos.
- Sem persistencia de latitude/longitude do usuario no banco na implementacao atual.
