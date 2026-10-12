import api from './api';

export const authService = {
  login: async (emailOrUsername, password) => {
    return await api.post('/auth/login', { emailOrUsername, password });
  },
  register: async (data) => {
    return await api.post('/auth/register', data);
  },
  logout: async () => {
    try {
      await api.post('/auth/logout');
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
    }
  },
  getCurrentUser: async () => {
    return await api.get('/auth/me');
  },
};
