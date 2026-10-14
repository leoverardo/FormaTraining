import { useEffect, useState } from 'react';
import { legalService } from '../../services/privacyService';
import { PublicHeader } from '../../components/brand/PublicHeader';

function LegalPage({ title, loadDocument }) {
  const [doc, setDoc] = useState(null);
  const [failed, setFailed] = useState(false);
  useEffect(() => {
    let active = true;
    loadDocument().then(response => {
      if (active) setDoc(response.data?.data);
    }).catch(() => { if (active) setFailed(true); });
    return () => { active = false; };
  }, [loadDocument]);
  return (
    <div className="min-h-screen bg-[var(--bg)]">
      <PublicHeader />
      <main className="uc-container uc-section max-w-4xl">
        <h1 className="uc-heading uc-heading">{title}</h1>
        {failed ? <p role="alert" className="mt-8 text-[var(--danger)]">Não foi possível carregar este documento. Tente novamente mais tarde.</p>
          : <div className="mt-8 whitespace-pre-wrap break-words uc-lead">{doc?.contentMarkdown || 'Carregando...'}</div>}
      </main>
    </div>
  );
}

export function PrivacyPolicyPage() {
  return <LegalPage title="Política de Privacidade" loadDocument={legalService.getPrivacyPolicy} />;
}

export function TermsOfUsePage() {
  return <LegalPage title="Termos de Uso" loadDocument={legalService.getTermsOfUse} />;
}
