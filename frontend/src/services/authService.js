import api from './api';

export const authService = {
  login: (data) => api.post('/auth/login', data),
  registerTrainer: (data) => api.post('/auth/register-trainer', data),
  registerStudent: (data) => api.post('/public/student-register', data),
  me: () => api.get('/auth/me'),
};

