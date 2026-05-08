import api from './api';

export const anamnesisService = {
  getOwn: () => api.get('/student/anamnesis'),
  save: (data) => api.post('/student/anamnesis', data),
  getByStudent: (studentId) => api.get(`/students/${studentId}/anamnesis`),
};

