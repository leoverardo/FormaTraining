import api from './api';

export const gamificationService = {
  getStudentSummary: () => api.get('/student/gamification/summary'),
  getStudentAchievements: () => api.get('/student/gamification/achievements'),
  getStudentMonthlyGoals: (year, month) => api.get('/student/gamification/monthly-goals', { params: { year, month } }),

  getTrainerSummary: (studentId) => api.get(`/trainer/students/${studentId}/gamification/summary`),
  getTrainerAchievements: (studentId) => api.get(`/trainer/students/${studentId}/gamification/achievements`),
  getTrainerMonthlyGoals: (studentId, year, month) => api.get(`/trainer/students/${studentId}/gamification/monthly-goals`, { params: { year, month } }),
  updateTrainerMonthlyGoals: (studentId, year, month, data) => api.put(`/trainer/students/${studentId}/gamification/monthly-goals/${year}/${month}`, data),
};
