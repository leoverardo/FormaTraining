const squat = {
  src: '/exercise-examples/agachamento-livre.jpg',
  alt: 'Exemplo de agachamento livre com barra em uma academia',
  author: 'Em Bhoo',
  source: 'https://commons.wikimedia.org/wiki/File:Squat_woman.jpg',
};

const legPress = {
  src: '/exercise-examples/leg-press.jpg',
  alt: 'Exemplo de exercício em uma máquina de leg press',
  author: 'Nenad Stojkovic',
  source: 'https://commons.wikimedia.org/wiki/File:Girl_playing_weights_with_legs_on_the_exercise_machine.jpg',
};

// Presentation only: examples never enter the form or API payload.
const examples = {
  'agachamento livre': squat,
  'agachamento com barra': squat,
  'leg press': legPress,
  'leg press horizontal': legPress,
};

export function getExerciseExample(name) {
  const key = String(name || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim().replace(/\s+/g, ' ');
  return examples[key] || null;
}
