import api from './api';

export const studentAreaService = {
  getAccessStatus: () => api.get('/student/access-status'),
  getDashboard: () => api.get('/student/dashboard'),
  getWorkouts: () => api.get('/student/workouts'),
  getWorkoutById: (id) => api.get(`/student/workouts/${id}`),
  getPosts: () => api.get('/student/posts'),
  getPostById: (id) => api.get(`/student/posts/${id}`),
};

