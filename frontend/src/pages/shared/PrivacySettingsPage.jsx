import { useEffect, useState } from 'react';
import { privacyService } from '../../services/privacyService';
import { PageContainer } from '../../components/ui/PageContainer';
import { Button } from '../../components/ui/Button';

export function PrivacySettingsPage() {
  const [consents, setConsents] = useState([]);
  const [requests, setRequests] = useState([]);
  const [latestExport, setLatestExport] = useState(null);

  const load = async () => {
    const [c, r, e] = await Promise.all([privacyService.getConsents(), privacyService.myRequests(), privacyService.latestExport()]);
    setConsents(c.data?.data || []);
    setRequests(r.data?.data || []);
    setLatestExport(e.data?.data || null);
  };

  useEffect(() => { load(); }, []);

  const optionalConsents = consents.filter((c) =>
    c.code !== 'HEALTH_RELATED_DATA_PROCESSING');
  const healthNotice = consents.find((c) =>
    c.code === 'HEALTH_RELATED_DATA_PROCESSING');

  return (
    <PageContainer className="space-y-4">
      <h1 className="text-2xl font-bold">Privacidade e Dados</h1>
      <section className="rounded-xl border p-4">
        <h2 className="font-semibold">Consentimentos opcionais</h2>
        <div className="mt-3 space-y-2">
          {optionalConsents.map((c) => <label key={c.code} className="flex items-center justify-between text-sm"><span>{c.description}</span><input type="checkbox" checked={!!c.isGranted} onChange={(e) => privacyService.updateConsent(c.code, e.target.checked).then(load)} /></label>)}
        </div>
      </section>
      {healthNotice && (
        <section className="rounded-xl border p-4">
          <h2 className="font-semibold">Dados potencialmente sensiveis</h2>
          <p className="mt-2 text-sm text-slate-600">
            {healthNotice.description}
          </p>
          <p className="mt-1 text-xs text-amber-700">
            Texto e base legal final dependem de revisao juridica.
          </p>
        </section>
      )}
      <section className="rounded-xl border p-4 space-x-2">
        <Button onClick={() => privacyService.requestExport().then(load)}>Solicitar exportacao</Button>
        <Button variant="outline" onClick={() => privacyService.requestDeletion('Solicitacao enviada pela area de privacidade').then(load)}>Solicitar exclusao</Button>
        {latestExport?.id && <Button variant="ghost" onClick={() => privacyService.downloadExport(latestExport.id)}>Baixar ultimo export</Button>}
      </section>
      <section className="rounded-xl border p-4">
        <h2 className="font-semibold">Solicitacoes</h2>
        <ul className="mt-2 text-sm">{requests.map((r) => <li key={r.id}>{r.requestType} - {r.status}</li>)}</ul>
      </section>
    </PageContainer>
  );
}
