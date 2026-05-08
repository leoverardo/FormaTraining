import api from './api';

export const workoutService = {
  getAll: () => api.get('/workouts'),
  getById: (id) => api.get(`/workouts/${id}`),
  create: (data) => api.post('/workouts', data),
  update: (id, data) => api.put(`/workouts/${id}`, data),
  delete: (id) => api.delete(`/workouts/${id}`),
  addExercise: (id, data) => api.post(`/workouts/${id}/exercises`, data),
  updateExercise: (id, weId, data) => api.put(`/workouts/${id}/exercises/${weId}`, data),
  removeExercise: (id, weId) => api.delete(`/workouts/${id}/exercises/${weId}`),
};

