import { BarChart2, CalendarDays, MessageCircle, Palette, Users, Zap } from 'lucide-react';

const features = [
  { icon: Users, title: 'Alunos, treinos e metas', description: 'Gerencie alunos, treinos e metas em um só lugar. Reúna objetivos e histórico para planejar um acompanhamento próximo de cada pessoa.' },
  { icon: Zap, title: 'Progresso e check-ins', description: 'Acompanhe o progresso e check-ins semanais. Conecte registros, fotos e constância para entender a evolução entre uma sessão e outra.' },
  { icon: BarChart2, title: 'Relatórios e evolução', description: 'Relatórios de engajamento e evolução física ajudam a acompanhar a jornada e orientar os próximos passos do seu trabalho.' },
  { icon: CalendarDays, title: 'Agenda e conversas', description: 'Organize os horários e mantenha as conversas com seus alunos no mesmo espaço. Seu cuidado continua além do treino do dia.' },
  { icon: MessageCircle, title: 'Conteúdos que aproximam', description: 'Publique conteúdos para compartilhar conhecimento e manter seus alunos conectados ao seu método e à rotina de treinos.' },
  { icon: Palette, title: 'A sua identidade', description: 'Configure sua marca, suas cores e sua página pública. Apresente seu trabalho com uma identidade que os alunos reconhecem.' },
];

export function PlatformOverview() {
  return <section className="gallery-section gallery-overview" aria-labelledby="platform-overview-title">
    <div className="gallery-shell">
      <div className="gallery-section-heading"><div>
        <p className="gallery-eyebrow">Uma plataforma para a sua consultoria</p>
        <h2 id="platform-overview-title">Gerencie sua consultoria fitness<br /><span>em um só lugar.</span></h2>
        <p className="gallery-copy">Treinos, alunos, progresso e conteúdos organizados em uma plataforma profissional e fácil de usar.</p>
      </div></div>
      <div className="gallery-overview-grid">{features.map(({ icon: Icon, title, description }) => <article key={title}>
        <Icon size={24} aria-hidden="true" /><h3>{title}</h3><p>{description}</p>
      </article>)}</div>
    </div>
  </section>;
}
