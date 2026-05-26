/**
 * textEncoding.js — Proteção visual contra mojibake em payloads externos.
 *
 * AVISO: estas funções são uma camada de proteção TEMPORÁRIA no frontend.
 * Elas tentam corrigir textos UTF-8 que foram interpretados incorretamente
 * como Latin-1/Windows-1252 durante o trânsito API → DB → resposta.
 *
 * A correção DEFINITIVA deve ser feita no backend/DB:
 *  - Garantir que a conexão com o banco use utf8mb4 (MySQL/MariaDB)
 *    ou UTF-8 explícito (PostgreSQL: client_encoding = 'UTF8')
 *  - Verificar cabeçalhos Content-Type: application/json; charset=utf-8
 *  - Certificar que o ORM/driver não faça double-encode
 *
 * Padrões detectados (UTF-8 lido como Latin-1):
 *   Ã£  → ã      (U+00E3)
 *   Ã§  → ç      (U+00E7)
 *   Ãª  → ê      (U+00EA)
 *   Ã©  → é      (U+00E9)
 *   Ã³  → ó      (U+00F3)
 *   Ã¢  → â      (U+00E2)
 *   Ã   → Ã / À / Á (contexto)
 *   Â   → artefato de byte 0xC2 lido como Latin-1
 *   â€" → —      (U+2014 em dash)
 *   â€" → –      (U+2013 en dash)
 *   â€™ → '      (U+2019 aspas curvas)
 *   â€œ → "      (U+201C)
 *   â€  → "      (U+201D)
 *   â€¦ → …      (U+2026 reticências)
 *   ?   → caractere de substituição (U+FFFD)
 */

/** Mapa de substituições ordenado do mais específico para o mais genérico. */
const MOJIBAKE_MAP = [
  // Em dash e en dash (frequentes em nomes de exercícios, ex.: "treino â€" frente")
  ['â€"', '—'],   // —
  ['â€"', '–'],   // –  (segunda ocorrência = en dash; cobrimos ambos)
  ['â€™', '’'],   // '
  ['â€œ', '“'],   // "
  ['â€', '”'], // " (byte 0x9D como Latin-1)
  ['â€¦', '…'],   // …
  ['â€™', '’'],   // ' (variante)

  // Vogais e consoantes latinas comuns em português
  ['Ã£o', 'ão'],
  ['Ã£', 'ã'],
  ['Ã§Ã£o', 'ção'],   // "ação", "atenção"
  ['Ã§', 'ç'],
  ['Ãª', 'ê'],
  ['Ã©', 'é'],
  ['Ã³', 'ó'],
  ['Ã¢', 'â'],
  ['Ã¡', 'á'],
  ['Ã\xad', 'í'],     // í (byte 0xAD)
  ['Ã\xba', 'ú'],     // ú
  ['Ã\x83', 'Ã'],     // Ã maiúsculo legítimo
  ['Ãµ', 'õ'],
  ['Ã\xb5', 'õ'],
  ['Ã\xa3', 'ã'],
  ['Ã\xa7', 'ç'],
  ['Ã\xaa', 'ê'],
  ['Ã\xa9', 'é'],
  ['Ã\xb3', 'ó'],
  ['Ã\xa2', 'â'],
  ['Ã\xa1', 'á'],

  // Byte 0xC2 sozinho vira "Â" em Latin-1 — geralmente artefato
  ['Ã', 'Á'],
  ['Ã', 'Í'],
  ['Ã', 'Ú'],
  ['Ã', 'Ý'],

  // Caractere de substituição visível
  ['�', ''],

  // "Â" isolado antes de espaço ou no fim — artefato puro
  // (aplicado ao final para não atrapalhar as substituições acima)
  [/Â(?=\s|$)/g, ''],
];

/** Padrões que indicam mojibake — checagem rápida. */
const MOJIBAKE_PATTERNS = [
  /Ã[£§ªµ©³¡¢]/,
  /â€/,
  /Â(?!\w)/,
  /�/,
  /mÃ/,
  /aÃ/,
  /Ã§Ã/,
];

/**
 * Retorna true se o texto provavelmente contém mojibake.
 * @param {unknown} value
 * @returns {boolean}
 */
export function hasMojibake(value) {
  if (typeof value !== 'string') return false;
  return MOJIBAKE_PATTERNS.some((re) => re.test(value));
}

/**
 * Tenta corrigir mojibake comum.
 * Não aplica correções em cascata — cada regra é aplicada uma vez.
 * @param {unknown} value
 * @returns {unknown} string corrigida, ou o valor original se não for string
 */
export function fixMojibake(value) {
  if (typeof value !== 'string') return value;

  let result = value;
  for (const [pattern, replacement] of MOJIBAKE_MAP) {
    if (typeof pattern === 'string') {
      result = result.split(pattern).join(replacement);
    } else {
      // RegExp
      result = result.replace(pattern, replacement);
    }
  }
  return result;
}

/**
 * Normaliza texto para exibição:
 * - Se não for string, retorna o valor original.
 * - Se detectar mojibake, corrige e retorna.
 * - Caso contrário, retorna o original sem modificação.
 *
 * Use este helper nos pontos de renderização de dados vindos da API/mock:
 *   posts de atividade, títulos de check-in, orientações, feed, etc.
 *
 * NÃO use em chaves do i18n — os arquivos de tradução já estão corretos.
 *
 * @param {unknown} value
 * @returns {unknown}
 */
export function normalizeDisplayText(value) {
  if (typeof value !== 'string') return value;
  if (!hasMojibake(value)) return value;
  return fixMojibake(value);
}
