import api from './api';

export const workoutSessionService = {
  // Student
  getOwn: () => api.get('/student/workout-sessions'),
  start: (data) => api.post('/student/workout-sessions/start', data),
  complete: (id, data) => api.put(`/student/workout-sessions/${id}/complete`, data),
  getExecution: (id) => api.get(`/student/workout-sessions/${id}/execution`),
  updateSet: (sessionId, setId, data) => api.patch(`/student/workout-sessions/${sessionId}/sets/${setId}`, data),
  completeExercise: (sessionId, exerciseSessionId, data) =>
    api.put(`/student/workout-sessions/${sessionId}/exercises/${exerciseSessionId}/complete`, data),
  skip: (id) => api.put(`/student/workout-sessions/${id}/skip`),

  // Trainer
  getByStudent: (studentId) => api.get(`/students/${studentId}/workout-sessions`),
};

