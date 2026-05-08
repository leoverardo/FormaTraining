import api from './api';

export const studentService = {
  getAll: () => api.get('/students'),
  getById: (id) => api.get(`/students/${id}`),
  create: (data) => api.post('/students', data),
  update: (id, data) => api.put(`/students/${id}`, data),
  delete: (id) => api.delete(`/students/${id}`),
  activate: (id) => api.put(`/students/${id}/activate`),
  deactivate: (id) => api.put(`/students/${id}/deactivate`),
  resendAccessEmail: (id) => api.post(`/students/${id}/resend-access-email`),
};

