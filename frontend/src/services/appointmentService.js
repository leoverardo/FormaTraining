import api from './api';

export const appointmentService = {
  // Trainer
  trainerList: (params = {}) => api.get('/trainer/appointments', { params }),
  trainerGetById: (id) => api.get(`/trainer/appointments/${id}`),
  trainerCreate: (data) => api.post('/trainer/appointments', data),
  trainerUpdate: (id, data) => api.put(`/trainer/appointments/${id}`, data),
  trainerReschedule: (id, data) => api.patch(`/trainer/appointments/${id}/reschedule`, data),
  trainerCancel: (id, data) => api.patch(`/trainer/appointments/${id}/cancel`, data || {}),
  trainerComplete: (id) => api.patch(`/trainer/appointments/${id}/complete`),

  // Student
  studentList: (params = {}) => api.get('/student/appointments', { params }),
  studentGetById: (id) => api.get(`/student/appointments/${id}`),
  studentConfirm: (id) => api.patch(`/student/appointments/${id}/confirm`),
};
