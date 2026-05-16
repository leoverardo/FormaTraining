import { useEffect, useState } from 'react';
import { PageContainer } from '../../components/ui/PageContainer';
import { privacyService } from '../../services/privacyService';
import { Button } from '../../components/ui/Button';

export function OwnerPrivacyPage() {
  const [requests, setRequests] = useState([]);
  const [incidents, setIncidents] = useState([]);
  const [vendors, setVendors] = useState([]);

  const load = async () => {
    const [r, i, v] = await Promise.all([privacyService.ownerRequests(), privacyService.ownerIncidents(), privacyService.ownerVendors()]);
    setRequests(r.data?.data || []);
    setIncidents(i.data?.data || []);
    setVendors(v.data?.data || []);
  };

  useEffect(() => { load(); }, []);

  return <PageContainer className="space-y-4">
    <h1 className="text-2xl font-bold">Privacidade / LGPD</h1>
    <section className="rounded-xl border p-4"><h2 className="font-semibold">Solicitacoes</h2>{requests.map((r) => <div key={r.id} className="text-sm flex gap-2 items-center"><span>{r.requestType} - {r.status}</span><Button size="sm" variant="outline" onClick={() => privacyService.ownerUpdateRequestStatus(r.id, { status: 'InProgress' }).then(load)}>Iniciar</Button></div>)}</section>
    <section className="rounded-xl border p-4"><h2 className="font-semibold">Incidentes</h2>{incidents.map((i) => <div key={i.id} className="text-sm">{i.title} - {i.status}</div>)}</section>
    <section className="rounded-xl border p-4"><h2 className="font-semibold">Fornecedores</h2>{vendors.map((v) => <div key={v.id} className="text-sm">{v.name} - {v.countryOrRegion}</div>)}</section>
  </PageContainer>;
}
