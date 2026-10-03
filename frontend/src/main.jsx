import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { I18nProvider } from './i18n'

// Preview fixtures are imported only in development, in explicitly marked frames.
let previewUser;
if (import.meta.env.DEV && new URLSearchParams(window.location.search).get('designPreview') === '1') {
  const { installDesignPreview } = await import('./preview/fixtures');
  previewUser = await installDesignPreview();
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <I18nProvider>
      <App previewUser={previewUser} />
    </I18nProvider>
  </StrictMode>,
)

