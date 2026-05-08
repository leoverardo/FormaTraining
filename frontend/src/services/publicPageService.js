import api from './api';

export const publicPageService = {
  getBySlug: (slug) => api.get(`/public/trainers/${slug}`),
  updatePage: (data) => api.put('/trainer/public-page', data),
  getTestimonials: () => api.get('/testimonials'),
  createTestimonial: (studentId, data) => api.post(`/students/${studentId}/testimonials`, data),
  getTransformations: () => api.get('/transformations'),
  createTransformation: (studentId, data) => api.post(`/students/${studentId}/transformations`, data),
  // Student
  getStudentTestimonials: () => api.get('/student/testimonial-requests'),
  approveTestimonial: (id) => api.put(`/student/testimonials/${id}/approve`),
  revokeTestimonial: (id) => api.put(`/student/testimonials/${id}/revoke`),
};

