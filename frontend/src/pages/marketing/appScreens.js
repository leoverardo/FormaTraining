export const appScreens = [
  { key: 'personal-alunos', label: 'Gestão de alunos', alt: 'Tela do personal: alunos' },
  { key: 'aluno-treino', label: 'Treino em andamento', alt: 'Tela do aluno: treinos' },
  { key: 'aluno-progresso', label: 'Evolução ao longo do tempo', alt: 'Tela do aluno: progresso' },
  { key: 'aluno-painel', label: 'Seu dia de treino', alt: 'Tela do aluno: painel' },
  { key: 'aluno-checkin', label: 'Check-in semanal', alt: 'Tela do aluno: check-in' },
];

export function screenSource(key, theme) {
  return `/marketing/mobile/${key}-${theme}.png`;
}

export const carouselScreens = appScreens.flatMap((screen, index) => (index % 2 ? ['dark', 'light'] : ['light', 'dark']).map(theme => ({
  imgUrl: screenSource(screen.key, theme),
  alt: `${screen.label} — tema ${theme === 'light' ? 'claro' : 'escuro'}`,
})));
