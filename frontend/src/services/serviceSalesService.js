import api from './api';

export const serviceSalesService = {
  getOffers: () => api.get('/trainer/service-offers'),
  getOfferById: (offerId) => api.get(`/trainer/service-offers/${offerId}`),
  createOffer: (data) => api.post('/trainer/service-offers', data),
  updateOffer: (offerId, data) => api.put(`/trainer/service-offers/${offerId}`, data),
  updateOfferStatus: (offerId, isActive) => api.patch(`/trainer/service-offers/${offerId}/status`, { isActive }),
  updateOfferVisibility: (offerId, isPublic) => api.patch(`/trainer/service-offers/${offerId}/visibility`, { isPublic }),

  getOrders: (params) => api.get('/trainer/service-orders', { params }),
  getOrderById: (orderId) => api.get(`/trainer/service-orders/${orderId}`),
  getOrdersSummary: () => api.get('/trainer/service-orders-summary'),

  getPublicOffers: (slugOrId) => api.get(`/public/trainers/${slugOrId}/service-offers`),
  createPublicOrder: (slugOrId, offerId, data) => api.post(`/public/trainers/${slugOrId}/service-orders`, data, { params: { offerId } }),
  createStudentOrder: (trainerId, offerId) => api.post(`/student/trainers/${trainerId}/service-orders`, { serviceOfferId: offerId }),
};
