import api from './api';

export const internalNotesService = {
  getByStudent: (studentId) => api.get(`/students/${studentId}/internal-notes`),
  create: (studentId, data) => api.post(`/students/${studentId}/internal-notes`, data),
  update: (studentId, noteId, data) => api.put(`/students/${studentId}/internal-notes/${noteId}`, data),
  delete: (studentId, noteId) => api.delete(`/students/${studentId}/internal-notes/${noteId}`),
};

