import api from './api';

export const legalService = {
  getActiveDocuments: () => api.get('/legal/documents/active'),
  getPrivacyPolicy: () => api.get('/legal/privacy-policy'),
  getTermsOfUse: () => api.get('/legal/terms-of-use'),
};

export const privacyService = {
  getConsents: () => api.get('/privacy/consents'),
  updateConsent: (code, isGranted) => api.put(`/privacy/consents/${code}`, { isGranted }),
  requestExport: () => api.post('/privacy/data-export/request'),
  latestExport: () => api.get('/privacy/data-export/latest'),
  downloadExport: (id) => api.get(`/privacy/data-export/download/${id}`, { responseType: 'blob' }),
  requestDeletion: (description) => api.post('/privacy/account-deletion/request', { description }),
  myRequests: () => api.get('/privacy/requests/my'),
  ownerRequests: () => api.get('/owner/privacy/requests'),
  ownerUpdateRequestStatus: (id, payload) => api.put(`/owner/privacy/requests/${id}/status`, payload),
  ownerIncidents: () => api.get('/owner/privacy/incidents'),
  ownerCreateIncident: (payload) => api.post('/owner/privacy/incidents', payload),
  ownerUpdateIncident: (id, payload) => api.put(`/owner/privacy/incidents/${id}`, payload),
  ownerVendors: () => api.get('/owner/privacy/vendors'),
  ownerCreateVendor: (payload) => api.post('/owner/privacy/vendors', payload),
  ownerUpdateVendor: (id, payload) => api.put(`/owner/privacy/vendors/${id}`, payload),
};
