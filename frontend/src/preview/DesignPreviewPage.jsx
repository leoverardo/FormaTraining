import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, ArrowUpRight, Monitor, Smartphone, Search } from 'lucide-react';
import { BrandLogo } from '../components/brand/BrandLogo';
import { screenGroups, previewUrl } from './catalog';
import './preview.css';

export default function DesignPreviewPage() {
  const [selected, setSelected] = useState({ name: 'Landing page', path: '/landing', role: 'Trainer' });
  const [mobile, setMobile] = useState(false);
  const [search, setSearch] = useState('');
  return <div className="preview-gallery">
    <aside className="preview-directory"><Link to="/landing"><BrandLogo size="sm" /></Link><h1>Um novo olhar.<br /><span>Em cada tela.</span></h1><p>{screenGroups.reduce((sum, group) => sum + group.screens.length, 0)} telas · Forma Gallery</p><label className="preview-search"><Search size={15} /><input placeholder="Encontrar uma tela" value={search} onChange={e => setSearch(e.target.value)} aria-label="Encontrar uma tela" /></label><nav aria-label="Telas do projeto">{screenGroups.map(group => <section key={group.name}><h2>{group.name}</h2>{group.screens.filter(([name]) => `${group.name} ${name}`.toLocaleLowerCase('pt-BR').includes(search.toLocaleLowerCase('pt-BR'))).map(([name, path]) => <button key={path} onClick={() => setSelected({ name, path, role: group.role })} aria-current={selected.path === path && selected.role === group.role ? 'page' : undefined}>{name}<span>↗</span></button>)}</section>)}</nav><Link className="preview-back" to="/landing"><ArrowLeft size={14} />Voltar à plataforma</Link></aside>
    <main className="preview-workspace"><header><div><span>PRÉVIA VISUAL</span><h2>{selected.name}</h2></div><div className="preview-controls"><button aria-label="Visualização desktop" aria-pressed={!mobile} onClick={() => setMobile(false)}><Monitor size={17} /></button><button aria-label="Visualização celular" aria-pressed={mobile} onClick={() => setMobile(true)}><Smartphone size={17} /></button><a href={previewUrl(selected.path, selected.role)} target="_blank" rel="noreferrer" aria-label="Abrir tela em nova aba"><ArrowUpRight size={18} /></a></div></header><p className="preview-notice">Dados demonstrativos. Navegação e formulários disponíveis para revisão; envios e pagamentos desativados.</p><div className={`preview-frame ${mobile ? 'is-mobile' : ''}`}><iframe key={`${selected.path}-${selected.role}`} src={previewUrl(selected.path, selected.role)} title={`Prévia: ${selected.name}`} /></div></main>
  </div>;
}
