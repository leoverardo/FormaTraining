import { useEffect, useState } from 'react';
import { legalService } from '../../services/privacyService';

export function PrivacyPolicyPage() {
  const [doc, setDoc] = useState(null);
  useEffect(() => { legalService.getPrivacyPolicy().then((r) => setDoc(r.data?.data)); }, []);
  return <div className="mx-auto max-w-4xl p-6"><h1 className="text-2xl font-bold">Politica de Privacidade</h1><div className="mt-4 whitespace-pre-wrap text-sm text-slate-700">{doc?.contentMarkdown || 'Carregando...'}</div></div>;
}

export function TermsOfUsePage() {
  const [doc, setDoc] = useState(null);
  useEffect(() => { legalService.getTermsOfUse().then((r) => setDoc(r.data?.data)); }, []);
  return <div className="mx-auto max-w-4xl p-6"><h1 className="text-2xl font-bold">Termos de Uso</h1><div className="mt-4 whitespace-pre-wrap text-sm text-slate-700">{doc?.contentMarkdown || 'Carregando...'}</div></div>;
}
