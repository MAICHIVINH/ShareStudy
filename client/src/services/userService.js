import apiClient from './api';

export const userService = {
  updateProfile: async (data) => {
    return apiClient.put('/users/profile', data);
  },

  uploadAvatar: async (fileOrBlob) => {
    const formData = new FormData();
    formData.append('file', fileOrBlob, 'avatar.jpg');
    return apiClient.post('/users/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
};
