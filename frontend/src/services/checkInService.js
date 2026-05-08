import api from './api';

export const checkInService = {
  // Student
  getOwn: () => api.get('/student/check-ins'),
  getCurrentWeek: () => api.get('/student/check-ins/current-week'),
  create: (data) => api.post('/student/check-ins', data),
  update: (id, data) => api.put(`/student/check-ins/${id}`, data),

  // Trainer
  getByStudent: (studentId) => api.get(`/students/${studentId}/check-ins`),
  getRecent: (limit = 10) => api.get(`/trainer/check-ins/recent?limit=${limit}`),
  getMissingCurrentWeek: () => api.get('/trainer/check-ins/missing-current-week'),
  addComment: (studentId, checkInId, data) => api.post(`/students/${studentId}/check-ins/${checkInId}/comments`, data),
};

