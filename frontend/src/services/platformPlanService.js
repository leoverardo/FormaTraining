import api from './api';

export const platformPlanService = {
  getAll: () => api.get('/platform-plans'),
  getById: (id) => api.get(`/platform-plans/${id}`),
  create: (data) => api.post('/platform-plans', data),
  update: (id, data) => api.put(`/platform-plans/${id}`, data),
  delete: (id) => api.delete(`/platform-plans/${id}`),
};

