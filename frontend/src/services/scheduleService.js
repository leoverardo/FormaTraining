import api from './api';

export const scheduleService = {
  getByStudent: (studentId) => api.get(`/students/${studentId}/schedule`),
  create: (studentId, data) => api.post(`/students/${studentId}/schedule`, data),
  update: (id, data) => api.put(`/schedule/${id}`, data),
  delete: (id) => api.delete(`/schedule/${id}`),
};

