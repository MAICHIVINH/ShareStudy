import api from './api';

export const notificationService = {
  getNotifications: async (params) => api.get('/notifications', { params }),
  getUnreadCount: async () => api.get('/notifications/unread-count'),
  markAsRead: async (id) => api.patch(`/notifications/${id}/read`),
  markAllAsRead: async () => api.patch('/notifications/read-all'),
};
