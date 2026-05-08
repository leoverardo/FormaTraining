import api from './api';

export const workoutSessionService = {
  // Student
  getOwn: () => api.get('/student/workout-sessions'),
  start: (data) => api.post('/student/workout-sessions/start', data),
  complete: (id, data) => api.put(`/student/workout-sessions/${id}/complete`, data),
  skip: (id) => api.put(`/student/workout-sessions/${id}/skip`),

  // Trainer
  getByStudent: (studentId) => api.get(`/students/${studentId}/workout-sessions`),
};

