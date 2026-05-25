import axios from 'axios';

const API_BASE_URL =
  import.meta.env.VITE_API_URL ||
  import.meta.env.VITE_BACKEND_URL ||
  'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  // Normalize all service paths to backend API routes without duplicating /api.
  if (config.url && !config.url.startsWith('/api')) {
    config.url = `/api${config.url.startsWith('/') ? config.url : `/${config.url}`}`;
  }

  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;

  if (import.meta.env.DEV) {
    console.log('API_BASE_URL:', API_BASE_URL);
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

