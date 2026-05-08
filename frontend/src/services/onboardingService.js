import api from './api';

export const onboardingService = {
  start: (data) => api.post('/public/trainer-onboarding', data),
  get: (id) => api.get(`/public/trainer-onboarding/${id}`),
  updateProfessional: (id, data) => api.put(`/public/trainer-onboarding/${id}/professional-data`, data),
  updateAddress: (id, data) => api.put(`/public/trainer-onboarding/${id}/address`, data),
  selectPlan: (id, data) => api.put(`/public/trainer-onboarding/${id}/select-plan`, data),
  simulatePayment: (id) => api.post(`/public/trainer-onboarding/${id}/simulate-payment-approved`),
};

