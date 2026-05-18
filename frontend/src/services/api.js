import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
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

