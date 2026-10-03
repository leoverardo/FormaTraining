import { ArrowUpRight, CalendarDays, Check, ChevronRight, Dumbbell, LayoutDashboard, MessageCircle, UsersRound } from 'lucide-react';

/** Illustrative product view, intentionally independent of account data. */
export function ProductPreview({ compact = false }) {
  return <div className={`product-preview ${compact ? 'product-preview--compact' : ''}`} aria-label="Exemplo ilustrativo do painel Forma Training">
    <aside className="product-sidebar">
      <b className="product-wordmark">forma<span>training</span></b>
      <small>SEU ESPAÇO</small>
      {[[LayoutDashboard, 'Visão geral'], [UsersRound, 'Alunos'], [Dumbbell, 'Treinos'], [CalendarDays, 'Agenda'], [MessageCircle, 'Mensagens']].map(([Icon, label], i) => <div key={label} className={i === 0 ? 'selected' : ''}><Icon size={15} />{label}</div>)}
      <p><span className="product-avatar">MC</span>Marina Costa<small>Personal trainer</small></p>
    </aside>
    <div className="product-main">
      <div className="product-topline"><span>Meu acompanhamento</span><span>SEX, 02 OUT <span className="product-avatar">MC</span></span></div>
      <div className="product-greeting"><div><small>UM BOM DIA PARA EVOLUIR</small><h3>Mais perto de cada aluno.</h3><p>Sua rotina organizada. Seu próximo passo, claro.</p></div><span className="product-add">+ Novo treino</span></div>
      <div className="product-metrics">{[['24', 'Alunos ativos', '+3 neste mês'], ['86%', 'Constância', '+12% no mês'], ['06', 'Sessões hoje', 'Tudo organizado']].map(([value, label, note]) => <div key={label}><span>{label}</span><strong>{value}</strong><small><ArrowUpRight size={12} />{note}</small></div>)}</div>
      <div className="product-bottom"><section><div className="product-section-label"><h4>Um pouco melhor. A cada semana.</h4><span>Últimos 7 dias</span></div><div className="product-chart" aria-label="Gráfico ilustrativo de constância crescente">{[42, 60, 48, 76, 64, 86, 95].map((height, i) => <div key={i}><i style={{ height: `${height}%` }} /><span>{['S', 'T', 'Q', 'Q', 'S', 'S', 'D'][i]}</span></div>)}</div></section><section><div className="product-section-label"><h4>Na sua agenda</h4><CalendarDays size={15} /></div>{[['09:00', 'Lucas Almeida', 'Treino de força'], ['10:30', 'Ana Oliveira', 'Avaliação física'], ['14:00', 'Pedro Santos', 'Treino funcional']].map(([time, name, title]) => <div className="product-appointment" key={time}><time>{time}</time><div><b>{name}</b><small>{title}</small></div><ChevronRight size={14} /></div>)}</section></div>
    </div>
  </div>;
}

export function StudentPreview() {
  return <div className="student-device" aria-label="Exemplo ilustrativo da experiência do aluno"><div className="device-speaker" /><div className="student-device-top"><span>9:41</span><span>••• ▰</span></div><p>Olá, Lucas.</p><h3>Seu melhor ritmo<br />começa aqui.</h3><div className="student-device-progress"><div><span>Sua semana</span><b>4 de 5</b></div><div className="device-week">{['S', 'T', 'Q', 'Q', 'S'].map((day, i) => <span key={i} className={i < 4 ? 'done' : ''}>{i < 4 ? <Check size={16} /> : day}</span>)}</div><small>Um treino mais perto do seu objetivo.</small></div><div className="student-device-workout"><Dumbbell size={26} /><small>SEU TREINO DE HOJE</small><h4>Força. Foco.<br />Evolução.</h4><p>Inferiores · 6 exercícios · 45 min</p><span>Ver meu treino <ChevronRight size={15} /></span></div><div className="student-device-message"><MessageCircle size={17} /><span>Seu personal está com você.<small>Converse sobre o próximo passo.</small></span></div><div className="device-home" /></div>;
}
