import api from './api';

export const trainerService = {
  getDashboard: () => api.get('/trainer/dashboard'),
  getProfile: () => api.get('/trainer/profile'),
  updateProfile: (data) => api.put('/trainer/profile', data),
  getSubscription: () => api.get('/trainer/subscription'),
  createSubscription: (data) => api.post('/trainer/subscription/create', data),
  simulateApproved: () => api.post('/trainer/subscription/simulate-approved'),
  simulateExpired: () => api.post('/trainer/subscription/simulate-expired'),
  getLeads: () => api.get('/trainer/leads'),
  updateLeadStatus: (id, status) => api.put(`/trainer/leads/${id}/status`, { status }),
  convertLeadToStudent: (id) => api.post(`/trainer/leads/${id}/convert-to-student`),
};

