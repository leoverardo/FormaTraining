export const screenGroups = [
  { name: 'Apresentação e acesso', role: 'Trainer', screens: [
    ['Landing page', '/landing'], ['Entrar', '/login'], ['Cadastro do personal', '/register'], ['Cadastro do aluno', '/student/register'], ['Definir senha', '/set-password?token=preview'], ['Retorno de pagamento', '/onboarding/return?onboardingId=preview'], ['Página pública', '/p/marina'], ['Privacidade', '/privacy-policy'], ['Termos de uso', '/terms-of-use'], ['Sistema visual', '/design-system'],
  ] },
  { name: 'Personal trainer', role: 'Trainer', screens: [
    ['Visão geral', '/trainer/dashboard'], ['Alunos', '/trainer/students'], ['Detalhes do aluno', '/trainer/students/1'], ['Treinos', '/trainer/workouts'], ['Exercícios', '/trainer/exercises'], ['Biblioteca', '/trainer/library'], ['Programação', '/trainer/schedule'], ['Agenda', '/trainer/appointments'], ['Relatórios', '/trainer/reports'], ['Conteúdos', '/trainer/posts'], ['Página pública · edição', '/trainer/public-page'], ['Interessados', '/trainer/leads'], ['Vendas de serviços', '/trainer/sales'], ['Assinatura', '/trainer/subscription'], ['Perfil', '/trainer/profile'], ['Mensagens', '/trainer/messages'], ['Privacidade da conta', '/trainer/privacy'],
  ] },
  { name: 'Aluno', role: 'Student', screens: [
    ['Meu dia', '/student/dashboard'], ['Meus treinos', '/student/workouts'], ['Detalhe do treino', '/student/workouts/1'], ['Agendamentos', '/student/appointments'], ['Check-in', '/student/check-in'], ['Anamnese', '/student/anamnesis'], ['Conteúdos', '/student/posts'], ['Detalhe do conteúdo', '/student/posts/1'], ['Evolução', '/student/progress'], ['Fotos', '/student/photos'], ['Meu acesso', '/student/access'], ['Meu perfil', '/student/profile'], ['Mensagens', '/student/messages'], ['Privacidade da conta', '/student/privacy'],
  ] },
  { name: 'Explorar', role: 'Explorer', screens: [
    ['Feed', '/explore'], ['Personais', '/explore/trainers'], ['Salvos', '/explore/saved'], ['Seguindo', '/explore/following'],
  ] },
  { name: 'Administração', role: 'Owner', screens: [
    ['Painel da plataforma', '/owner'], ['Planos', '/owner/plans'], ['Privacidade e solicitações', '/owner/privacy'],
  ] },
];

export const previewUrl = (path, role) => `${path}${path.includes('?') ? '&' : '?'}designPreview=1&previewRole=${role}`;
