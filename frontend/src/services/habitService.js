import api from './api';

export const habitService = {
  // Trainer
  getTrainerHabits: (studentId) => api.get(`/trainer/students/${studentId}/habits`),
  createHabit: (studentId, data) => api.post(`/trainer/students/${studentId}/habits`, data),
  updateHabit: (studentId, habitId, data) => api.put(`/trainer/students/${studentId}/habits/${habitId}`, data),
  updateHabitStatus: (studentId, habitId, data) => api.patch(`/trainer/students/${studentId}/habits/${habitId}/status`, data),
  deleteHabit: (studentId, habitId) => api.delete(`/trainer/students/${studentId}/habits/${habitId}`),
  getAdherence: (studentId, days = 7) => api.get(`/trainer/students/${studentId}/habits/adherence?days=${days}`),
  getTrainerGuidance: (studentId) => api.get(`/trainer/students/${studentId}/nutrition-guidance`),
  upsertTrainerGuidance: (studentId, data) => api.put(`/trainer/students/${studentId}/nutrition-guidance`, data),

  // Student
  getToday: () => api.get('/student/habits/today'),
  updateToday: (habitId, data) => api.patch(`/student/habits/${habitId}/today`, data),
  getStudentGuidance: () => api.get('/student/nutrition-guidance'),
};
