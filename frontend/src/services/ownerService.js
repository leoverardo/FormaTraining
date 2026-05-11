import api from './api';

export const ownerService = {
  getDashboard: () => api.get('/owner/dashboard'),
};
