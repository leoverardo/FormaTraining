import api from './api';

export const ownerService = {
  getDashboard: (range = 30) => api.get('/owner/dashboard', { params: { range } }),
};
