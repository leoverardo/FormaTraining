import api from './api';

export const paymentService = {
  getBillingOptions: (planId) => api.get(`/payments/plans/${planId}/billing-options`),
  validateCoupon: (data) => api.post('/payments/subscriptions/validate-coupon', data),
  createCheckout: (data) => api.post('/payments/subscriptions/checkout', data),
};
