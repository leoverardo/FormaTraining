import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve('frontend/src');
const PT_PATH = path.resolve('frontend/src/i18n/locales/pt-BR.json');
const EN_PATH = path.resolve('frontend/src/i18n/locales/en-US.json');
const BAD_ENCODING = /Ã|Â|â€|�/;
const CODE_EXT = new Set(['.js', '.jsx', '.ts', '.tsx', '.json', '.css']);

function readUtf8(file) {
  return fs.readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
}

function walk(dir, acc = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, acc);
    else if (CODE_EXT.has(path.extname(entry.name))) acc.push(full);
  }
  return acc;
}

function flatten(obj, prefix = '', out = new Set()) {
  if (obj && typeof obj === 'object' && !Array.isArray(obj)) {
    for (const [k, v] of Object.entries(obj)) {
      const key = prefix ? `${prefix}.${k}` : k;
      flatten(v, key, out);
    }
    return out;
  }
  out.add(prefix);
  return out;
}

const files = walk(ROOT);
const encodingIssues = [];
const hardcodedIssues = [];

for (const file of files) {
  const raw = readUtf8(file);
  if (BAD_ENCODING.test(raw)) encodingIssues.push(file);

  if (file.endsWith('.jsx') || file.endsWith('.tsx')) {
    const lines = raw.split(/\r?\n/);
    lines.forEach((line, idx) => {
      if (line.includes('className=') || line.includes('import ') || line.includes('from ')) return;
      const hasVisibleString = />\s*[A-Za-zÀ-ÿ][^<{]*</.test(line) || /label=\"[A-Za-zÀ-ÿ]/.test(line) || /title=\"[A-Za-zÀ-ÿ]/.test(line);
      const ignores = line.includes('t(') || line.includes('{') || line.includes('//');
      if (hasVisibleString && !ignores) {
        hardcodedIssues.push(`${path.relative(process.cwd(), file)}:${idx + 1}`);
      }
    });
  }
}

const pt = JSON.parse(readUtf8(PT_PATH));
const en = JSON.parse(readUtf8(EN_PATH));
const ptKeys = flatten(pt);
const enKeys = flatten(en);

const missingInEn = [...ptKeys].filter((k) => !enKeys.has(k));
const missingInPt = [...enKeys].filter((k) => !ptKeys.has(k));

console.log('=== Encoding issues ===');
console.log(encodingIssues.length ? encodingIssues.join('\n') : 'None');
console.log('\n=== Missing i18n keys in en-US ===');
console.log(missingInEn.length ? missingInEn.join('\n') : 'None');
console.log('\n=== Missing i18n keys in pt-BR ===');
console.log(missingInPt.length ? missingInPt.join('\n') : 'None');
console.log('\n=== Potential hardcoded JSX strings ===');
console.log(hardcodedIssues.length ? hardcodedIssues.slice(0, 200).join('\n') : 'None');

if (encodingIssues.length || missingInEn.length || missingInPt.length) process.exit(1);
