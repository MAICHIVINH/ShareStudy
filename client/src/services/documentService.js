import api from './api';

export const documentService = {
  getDocuments: async (params) => api.get('/documents', { params }),
  getDocumentById: async (id) => api.get(`/documents/${id}`),
  getDocumentBySlug: async (slug) => api.get(`/documents/slug/${slug}`),
  uploadDocument: async (formData) => api.post('/documents', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  recordView: async (id) => api.post(`/documents/${id}/view`),
  downloadDocument: async (id) => api.get(`/documents/${id}/download`),
  getPopularDocuments: async (limit = 5) => api.get('/documents/popular', { params: { limit } }),
  getLatestDocuments: async (limit = 8) => api.get('/documents/latest', { params: { limit } }),
  getTopRatedDocuments: async (limit = 5) => api.get('/documents/top-rated', { params: { limit } }),
};
