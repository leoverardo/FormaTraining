import api from './api';

export const exploreService = {
  getFeed: (params) => api.get('/explore/feed', { params }),
  getTrainers: (params) => api.get('/explore/trainers', { params }),
  getRecommended: (params) => api.get('/explore/trainers/recommended', { params }),
  followTrainer: (trainerId) => api.post(`/explore/trainers/${trainerId}/follow`),
  unfollowTrainer: (trainerId) => api.delete(`/explore/trainers/${trainerId}/follow`),
  saveTrainer: (trainerId) => api.post(`/explore/trainers/${trainerId}/save`),
  unsaveTrainer: (trainerId) => api.delete(`/explore/trainers/${trainerId}/save`),
  getFollowing: () => api.get('/student/following-trainers'),
  getSaved: () => api.get('/student/saved-trainers'),
};
