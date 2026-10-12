import api from './api';

export const ratingService = {
  getDocumentRatings: async (documentId, params) => api.get(`/documents/${documentId}/ratings`, { params }),
  createRating: async (documentId, data) => api.post(`/documents/${documentId}/ratings`, data),
  rateDocument: async (payload) => {
    const docId = payload.documentId;
    const body = { score: payload.score, comment: payload.comment || null };
    return api.post(`/documents/${docId}/ratings`, body);
  },
  updateRating: async (documentId, ratingId, data) => api.put(`/documents/${documentId}/ratings/${ratingId}`, data),
  deleteRating: async (documentId, ratingId) => api.delete(`/documents/${documentId}/ratings/${ratingId}`),
};

export default ratingService;
