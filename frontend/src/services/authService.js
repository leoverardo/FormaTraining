import api from './api';

export const authService = {
  login: (data) => api.post('/auth/login', data),
  registerTrainer: (data) => api.post('/auth/register-trainer', data),
  me: () => api.get('/auth/me'),
};

