import api from './api';

export const adminService = {
  // Dashboard Stats & Charts
  getDashboardStats: async () => {
    return api.get('/admin/dashboard/summary');
  },

  getDashboardCharts: async () => {
    return api.get('/admin/dashboard/charts');
  },

  // Documents Management
  getDocuments: async (params) => {
    return api.get('/admin/documents', { params });
  },

  approveDocument: async (id) => {
    return api.put(`/admin/documents/${id}/approve`);
  },

  rejectDocument: async (id, reason) => {
    return api.put(`/admin/documents/${id}/reject`, { reason });
  },

  hideDocument: async (id) => {
    return api.put(`/admin/documents/${id}/hide`);
  },

  deleteDocument: async (id) => {
    return api.delete(`/admin/documents/${id}`);
  },

  // Users Management
  getUsers: async (params) => {
    return api.get('/admin/users', { params });
  },

  updateUserStatus: async (id, status) => {
    return api.put(`/admin/users/${id}/status`, { status });
  },

  updateUserRoles: async (id, roles) => {
    return api.put(`/admin/users/${id}/roles`, { roles });
  },

  resetUserPassword: async (id) => {
    return api.post(`/admin/users/${id}/reset-password`);
  },

  deleteUser: async (id) => {
    return api.delete(`/admin/users/${id}`);
  },

  // Reports Management
  getReports: async (params) => {
    return api.get('/admin/reports', { params });
  },

  resolveReport: async (id, status, notes) => {
    return api.put(`/admin/reports/${id}/resolve`, { status, notes });
  },

  // Education Hierarchy CRUD
  // Schools
  createSchool: async (data) => api.post('/admin/schools', data),
  updateSchool: async (id, data) => api.put(`/admin/schools/${id}`, data),
  deleteSchool: async (id) => api.delete(`/admin/schools/${id}`),

  // Faculties
  createFaculty: async (data) => api.post('/admin/faculties', data),
  updateFaculty: async (id, data) => api.put(`/admin/faculties/${id}`, data),
  deleteFaculty: async (id) => api.delete(`/admin/faculties/${id}`),

  // Majors
  createMajor: async (data) => api.post('/admin/majors', data),
  updateMajor: async (id, data) => api.put(`/admin/majors/${id}`, data),
  deleteMajor: async (id) => api.delete(`/admin/majors/${id}`),

  // Subjects
  createSubject: async (data) => api.post('/admin/subjects', data),
  updateSubject: async (id, data) => api.put(`/admin/subjects/${id}`, data),
  deleteSubject: async (id) => api.delete(`/admin/subjects/${id}`),

  // Audit Logs
  getAuditLogs: async (params) => {
    return api.get('/admin/audit-logs', { params });
  },

  // System Settings
  getSettings: async () => {
    return api.get('/admin/settings');
  },

  updateSettings: async (settings) => {
    return api.put('/admin/settings', settings);
  },
};

export default adminService;
