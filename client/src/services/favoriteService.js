import api from './api';

export const favoriteService = {
  getMyFavorites: async (params) => api.get('/users/me/favorites', { params }),
  addFavorite: async (documentId) => api.post(`/favorites/${documentId}`),
  removeFavorite: async (documentId) => api.delete(`/favorites/${documentId}`),
};
