import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from '../components/layout/MainLayout';
import { ProtectedRoute } from './ProtectedRoute';
import { AdminRoute } from '../components/common/AdminRoute';

import HomePage from '../pages/Home/HomePage';
import DocumentsPage from '../pages/Documents/DocumentsPage';
import DocumentDetailPage from '../pages/DocumentDetail/DocumentDetailPage';
import UploadDocumentPage from '../pages/UploadDocument/UploadDocumentPage';
import SubjectsPage from '../pages/Subjects/SubjectsPage';
import SchoolsPage from '../pages/Schools/SchoolsPage';
import LoginPage from '../pages/Login/LoginPage';
import RegisterPage from '../pages/Register/RegisterPage';
import ProfilePage from '../pages/Profile/ProfilePage';

// Admin Portal Components
import AdminLayout from '../pages/Admin/AdminLayout';
import AdminDashboard from '../pages/Admin/Dashboard/AdminDashboard';
import AdminDocumentsPage from '../pages/Admin/Documents/AdminDocumentsPage';
import AdminUsersPage from '../pages/Admin/Users/AdminUsersPage';
import AdminEducationPage from '../pages/Admin/Education/AdminEducationPage';
import AdminReportsPage from '../pages/Admin/Reports/AdminReportsPage';
import AdminAuditLogsPage from '../pages/Admin/AuditLogs/AdminAuditLogsPage';
import AdminSettingsPage from '../pages/Admin/Settings/AdminSettingsPage';

export const AppRoutes = () => {
  return (
    <Routes>
      {/* Public Pages with MainLayout */}
      <Route path="/" element={<MainLayout><HomePage /></MainLayout>} />
      <Route path="/documents" element={<MainLayout><DocumentsPage /></MainLayout>} />
      <Route path="/documents/:slug" element={<MainLayout><DocumentDetailPage /></MainLayout>} />
      <Route path="/subjects" element={<MainLayout><SubjectsPage /></MainLayout>} />
      <Route path="/schools" element={<MainLayout><SchoolsPage /></MainLayout>} />

      {/* Protected User Pages */}
      <Route 
        path="/upload" 
        element={
          <ProtectedRoute>
            <MainLayout><UploadDocumentPage /></MainLayout>
          </ProtectedRoute>
        } 
      />
      <Route 
        path="/profile" 
        element={
          <ProtectedRoute>
            <MainLayout><ProfilePage /></MainLayout>
          </ProtectedRoute>
        } 
      />

      {/* Auth Pages (No MainLayout) */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* Admin Portal Routes (Protected by AdminRoute) */}
      <Route element={<AdminRoute />}>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<AdminDashboard />} />
          <Route path="documents" element={<AdminDocumentsPage />} />
          <Route path="users" element={<AdminUsersPage />} />
          <Route path="education" element={<AdminEducationPage />} />
          <Route path="reports" element={<AdminReportsPage />} />
          <Route path="audit-logs" element={<AdminAuditLogsPage />} />
          <Route path="settings" element={<AdminSettingsPage />} />
        </Route>
      </Route>

      {/* Fallback */}
      <Route path="*" element={<MainLayout><HomePage /></MainLayout>} />
    </Routes>
  );
};

export default AppRoutes;
