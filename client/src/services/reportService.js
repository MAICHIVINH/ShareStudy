import api from './api';

export const reportService = {
  createReport: async (documentId, data) => api.post(`/documents/${documentId}/reports`, data),
  getMyReports: async (params) => api.get('/users/me/reports', { params }),
};
