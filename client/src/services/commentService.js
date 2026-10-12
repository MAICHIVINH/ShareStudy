import api from './api';

export const commentService = {
  getDocumentComments: async (documentId, params) => api.get(`/documents/${documentId}/comments`, { params }),
  createComment: async (documentId, data) => api.post(`/documents/${documentId}/comments`, data),
  addComment: async (payload) => {
    const docId = payload.documentId;
    const body = { content: payload.content, parentId: payload.parentId || null };
    return api.post(`/documents/${docId}/comments`, body);
  },
  updateComment: async (commentId, data) => api.put(`/comments/${commentId}`, data),
  deleteComment: async (commentId) => api.delete(`/comments/${commentId}`),
};

export default commentService;
