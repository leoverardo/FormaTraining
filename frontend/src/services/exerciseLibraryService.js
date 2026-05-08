import api from './api';

export const exerciseLibraryService = {
  getAll: () => api.get('/exercise-library'),
  create: (data) => api.post('/exercise-library', data),
  update: (id, data) => api.put(`/exercise-library/${id}`, data),
  delete: (id) => api.delete(`/exercise-library/${id}`),
  duplicateToMyLibrary: (id) => api.post(`/exercise-library/${id}/duplicate-to-my-library`),
  getTemplates: () => api.get('/workout-templates'),
  createTemplate: (data) => api.post('/workout-templates', data),
  duplicateTemplate: (id) => api.post(`/workout-templates/${id}/duplicate-to-my-workouts`),
};

