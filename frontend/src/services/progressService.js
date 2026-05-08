import api from './api';

export const progressService = {
  // Trainer
  getByStudent: (studentId) => api.get(`/students/${studentId}/progress`),
  createForStudent: (studentId, data) => api.post(`/students/${studentId}/progress`, data),
  updateForStudent: (studentId, id, data) => api.put(`/students/${studentId}/progress/${id}`, data),
  deleteForStudent: (studentId, id) => api.delete(`/students/${studentId}/progress/${id}`),
  getPhotosByStudent: (studentId) => api.get(`/students/${studentId}/progress-photos`),
  addPhotoForStudent: (studentId, data) => api.post(`/students/${studentId}/progress-photos`, data),
  deletePhotoForStudent: (studentId, id) => api.delete(`/students/${studentId}/progress-photos/${id}`),

  // Student
  getOwn: () => api.get('/student/progress'),
  createOwn: (data) => api.post('/student/progress', data),
  updateOwn: (id, data) => api.put(`/student/progress/${id}`, data),
  deleteOwn: (id) => api.delete(`/student/progress/${id}`),
  getOwnPhotos: () => api.get('/student/progress-photos'),
  addOwnPhoto: (data) => api.post('/student/progress-photos', data),
  deleteOwnPhoto: (id) => api.delete(`/student/progress-photos/${id}`),
};

