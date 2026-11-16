import libraryImages from './libraryImages.json';

const squat = {
  src: '/exercise-examples/agachamento-livre.jpg',
  alt: 'Exemplo de agachamento livre com barra em uma academia',
  author: 'Em Bhoo',
  source: 'https://commons.wikimedia.org/wiki/File:Squat_woman.jpg',
  license: 'CC BY 2.0',
  licenseUrl: 'https://creativecommons.org/licenses/by/2.0/',
};

const legPress = {
  src: '/exercise-examples/leg-press.jpg',
  alt: 'Exemplo de exercício em uma máquina de leg press',
  author: 'Nenad Stojkovic',
  source: 'https://commons.wikimedia.org/wiki/File:Girl_playing_weights_with_legs_on_the_exercise_machine.jpg',
  license: 'CC BY 2.0',
  licenseUrl: 'https://creativecommons.org/licenses/by/2.0/',
};

// Presentation only: examples never enter the form or API payload.
const examples = new Map([
  ...libraryImages.map(image => [normalizedName(image.name), image]),
  ['agachamento livre', squat],
  ['agachamento com barra', squat],
  ['leg press', legPress],
  ['leg press horizontal', legPress],
]);

function normalizedName(name) {
  return String(name || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim().replace(/\s+/g, ' ');
}

export function getExerciseExample(name) {
  const key = normalizedName(name);
  return examples.get(key) || null;
}
