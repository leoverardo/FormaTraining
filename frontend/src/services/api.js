import axios from 'axios';

/**
 * Resolve a base URL da API com regras de ambiente:
 *  1. VITE_API_URL (preferência)
 *  2. VITE_BACKEND_URL (legado)
 *  3. Em DEV sem env → http://localhost:5000 (aceito)
 *  4. Em PROD sem env → erro claro no console; não usa localhost silenciosamente.
 */
function resolveApiBaseUrl() {
  const fromEnv =
    import.meta.env.VITE_API_URL ||
    import.meta.env.VITE_BACKEND_URL;

  if (fromEnv) return fromEnv;

  if (import.meta.env.DEV) {
    return 'http://localhost:5000';
  }

  // Produção sem env configurada — avisa mas não quebra o render
  console.error(
    '[Forma Training] VITE_API_URL não está definida em produção. ' +
    'Requests de API podem falhar. ' +
    'Defina a variável de ambiente no painel do Vercel ou no .env.production.'
  );
  // Retorna string vazia: as requests vão falhar com erro de rede,
  // não com um localhost inacessível que produz erros silenciosos.
  return '';
}

const API_BASE_URL = resolveApiBaseUrl();

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  // Normaliza todos os caminhos para /api/... sem duplicar o prefixo
  if (config.url && !config.url.startsWith('/api')) {
    config.url = `/api${config.url.startsWith('/') ? config.url : `/${config.url}`}`;
  }

  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;

  if (import.meta.env.DEV) {
    console.log('[api] baseURL:', API_BASE_URL, '| url:', config.url);
  }

  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 403) {
      const errorCode = err.response?.data?.errors?.[0];
      if (errorCode === 'ACTIVE_SUBSCRIPTION_REQUIRED') {
        sessionStorage.setItem('premiumBlockMessage', err.response?.data?.message || 'Este recurso exige assinatura ativa.');
        if (!window.location.pathname.startsWith('/trainer/subscription')) {
          window.location.href = '/trainer/subscription';
        }
      }
    }

    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export default api;
