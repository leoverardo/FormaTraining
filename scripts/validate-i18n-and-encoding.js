#!/usr/bin/env node
/**
 * validate-i18n-and-encoding.js
 *
 * Validações estáticas para o projeto Forma Training:
 *  1. Valida JSON dos arquivos de tradução (pt-BR e en-US).
 *  2. Compara chaves recursivamente entre os dois locales.
 *  3. Busca padrões de mojibake em arquivos .js, .jsx, .ts, .tsx dentro de src/.
 *
 * Sai com código 1 se encontrar problema grave em arquivos estáticos.
 * Padrões em src/ são reportados como WARN (não encerram com erro),
 * pois podem ser comentários documentando o problema.
 */

import { readFileSync, readdirSync, statSync } from 'fs';
import { join, relative, extname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const ROOT = join(__dirname, '..');
const LOCALES_DIR = join(ROOT, 'frontend', 'src', 'i18n', 'locales');
const SRC_DIR = join(ROOT, 'frontend', 'src');

let hasError = false;
let warnCount = 0;

function log(level, ...args) {
  const prefix = level === 'ERROR' ? '\x1b[31m[ERROR]\x1b[0m' :
                 level === 'WARN'  ? '\x1b[33m[WARN]\x1b[0m'  :
                 '\x1b[32m[OK]\x1b[0m';
  console.log(prefix, ...args);
}

/* ─── 1. Validar JSON ─── */

function loadJson(filePath) {
  const label = relative(ROOT, filePath);
  try {
    const raw = readFileSync(filePath, 'utf-8');
    const parsed = JSON.parse(raw);
    log('OK', `JSON válido: ${label}`);
    return parsed;
  } catch (err) {
    log('ERROR', `JSON inválido em ${label}: ${err.message}`);
    hasError = true;
    return null;
  }
}

/* ─── 2. Comparar chaves recursivamente ─── */

function collectKeys(obj, prefix = '') {
  const keys = new Set();
  for (const [k, v] of Object.entries(obj)) {
    const full = prefix ? `${prefix}.${k}` : k;
    if (v && typeof v === 'object' && !Array.isArray(v)) {
      for (const sub of collectKeys(v, full)) keys.add(sub);
    } else {
      keys.add(full);
    }
  }
  return keys;
}

function compareKeys(ptBR, enUS) {
  if (!ptBR || !enUS) return;
  const ptKeys = collectKeys(ptBR);
  const enKeys = collectKeys(enUS);
  let diffCount = 0;

  for (const key of ptKeys) {
    if (!enKeys.has(key)) {
      log('ERROR', `Chave presente em pt-BR mas ausente em en-US: ${key}`);
      hasError = true;
      diffCount++;
    }
  }
  for (const key of enKeys) {
    if (!ptKeys.has(key)) {
      log('ERROR', `Chave presente em en-US mas ausente em pt-BR: ${key}`);
      hasError = true;
      diffCount++;
    }
  }

  if (diffCount === 0) {
    log('OK', `Chaves sincronizadas: ${ptKeys.size} em pt-BR, ${enKeys.size} em en-US`);
  } else {
    log('ERROR', `${diffCount} divergência(s) de chaves entre pt-BR e en-US`);
  }
}

/* ─── 3. Buscar mojibake em src/ ─── */

const MOJIBAKE_PATTERNS = [
  { pattern: /Ã[£§ªµ©³¡¢]/,    label: 'Ã+vogal (UTF-8 como Latin-1)' },
  { pattern: /â€/,              label: 'â€ (dash/aspas UTF-8 como Latin-1)' },
  { pattern: /Â(?!\w)/,         label: 'Â isolado (byte 0xC2 como Latin-1)' },
  { pattern: /mÃ/,              label: 'mÃ (início de "mês" mojibake)' },
  { pattern: /aÃ/,              label: 'aÃ (início de "ação" mojibake)' },
];

/** Arquivos que definem intencionalmente os padrões de mojibake — ignorar no scan. */
const SCAN_EXCLUDES = ['textEncoding.js'];

function walkSrc(dir, callback) {
  for (const entry of readdirSync(dir)) {
    if (SCAN_EXCLUDES.includes(entry)) continue;
    const full = join(dir, entry);
    const stat = statSync(full);
    if (stat.isDirectory()) {
      if (entry === 'node_modules' || entry === '.git' || entry === 'dist') continue;
      walkSrc(full, callback);
    } else {
      const ext = extname(entry);
      if (['.js', '.jsx', '.ts', '.tsx'].includes(ext)) {
        callback(full);
      }
    }
  }
}

function scanForMojibake(srcDir) {
  let fileHits = 0;

  walkSrc(srcDir, (filePath) => {
    const label = relative(ROOT, filePath);
    const content = readFileSync(filePath, 'utf-8');
    const lines = content.split('\n');

    lines.forEach((line, idx) => {
      // Ignora linhas que são puramente comentário (documentação do mojibake map)
      const stripped = line.replace(/\/\/[^\n]*/g, '').trim();
      if (!stripped) return;

      for (const { pattern, label: patLabel } of MOJIBAKE_PATTERNS) {
        if (pattern.test(stripped)) {
          log('WARN', `${label}:${idx + 1} — ${patLabel}`);
          warnCount++;
          fileHits++;
        }
      }
    });
  });

  if (fileHits === 0) {
    log('OK', 'Nenhum padrão de mojibake encontrado em arquivos estáticos de src/');
  } else {
    log('WARN', `${fileHits} ocorrência(s) em src/ — verificar se são strings hardcoded ou dados em tempo de execução`);
  }
}

/* ─── Main ─── */

console.log('\n=== Forma Training — Validação i18n + Encoding ===\n');

const ptBR = loadJson(join(LOCALES_DIR, 'pt-BR.json'));
const enUS = loadJson(join(LOCALES_DIR, 'en-US.json'));

console.log('');
compareKeys(ptBR, enUS);

console.log('');
scanForMojibake(SRC_DIR);

console.log('');
if (hasError) {
  console.log('\x1b[31m✗ Validação falhou. Corrija os erros acima.\x1b[0m\n');
  process.exit(1);
} else if (warnCount > 0) {
  console.log(`\x1b[33m⚠ Validação passou com ${warnCount} aviso(s).\x1b[0m\n`);
} else {
  console.log('\x1b[32m✓ Tudo ok!\x1b[0m\n');
}
