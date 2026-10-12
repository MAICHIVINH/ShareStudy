import api from './api';

export const educationService = {
  getEducationLevels: async () => api.get('/education-levels'),
  getAllSchools: async (params) => api.get('/schools', { params }),
  getSchoolsByLevel: async (levelId) => api.get(`/education-levels/${levelId}/schools`),
  getSchoolDetail: async (id) => api.get(`/schools/${id}`),
  getFacultiesBySchool: async (schoolId) => api.get(`/schools/${schoolId}/faculties`),
  getMajorsByFaculty: async (facultyId) => api.get(`/faculties/${facultyId}/majors`),
  getSubjectsByMajor: async (majorId) => api.get(`/majors/${majorId}/subjects`),
  getAllSubjects: async (params) => api.get('/subjects', { params }),
  getDocumentTypes: async () => api.get('/document-types'),
  getAcademicYears: async () => api.get('/academic-years'),
};

export default educationService;
