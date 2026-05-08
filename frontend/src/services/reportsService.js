import api from './api';

export const reportsService = {
  getOverview: () => api.get('/trainer/reports/overview'),
};

