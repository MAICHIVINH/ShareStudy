import React, { useState } from 'react';
import { Link, Outlet, useLocation, useNavigate, Navigate } from 'react-router-dom';
import { Layout, Menu, Button, Avatar, Dropdown, Input, Tooltip } from 'antd';
import {
  DashboardOutlined,
  FileTextOutlined,
  UserOutlined,
  BankOutlined,
  FlagOutlined,
  MenuUnfoldOutlined,
  MenuFoldOutlined,
  HomeOutlined,
  SunOutlined,
  MoonOutlined,
  LogoutOutlined,
  HistoryOutlined,
  SettingOutlined,
  SearchOutlined,
  BellOutlined,
  ShopOutlined,
  BookOutlined
} from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../contexts/ThemeContext';
import { useSettings } from '../../contexts/SettingsContext';
import { formatAvatarUrl } from '../../utils/avatarUtils';

const { Header, Sider, Content } = Layout;

export const AdminLayout = () => {
  const [collapsed, setCollapsed] = useState(true); // Default to mini icon sidebar as in screenshot
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { isDarkMode, toggleDarkMode } = useTheme();
  const { settings } = useSettings();

  const userRoles = user?.roles || [];
  const isSystemAdmin = userRoles.includes('Admin');
  const isModeratorOnly = userRoles.includes('Moderator') && !isSystemAdmin;

  const menuItems = [
    {
      key: '/admin',
      icon: <DashboardOutlined className="text-base" />,
      label: <Link to="/admin">Dashboard</Link>,
      title: 'Dashboard',
    },
    {
      key: '/admin/documents',
      icon: <FileTextOutlined className="text-base" />,
      label: <Link to="/admin/documents">Quản lý tài liệu</Link>,
      title: 'Quản lý tài liệu',
    },
    ...(isSystemAdmin ? [
      {
        key: '/admin/users',
        icon: <UserOutlined className="text-base" />,
        label: <Link to="/admin/users">Quản lý người dùng</Link>,
        title: 'Quản lý người dùng',
      },
      {
        key: '/admin/education',
        icon: <BankOutlined className="text-base" />,
        label: <Link to="/admin/education">Danh mục giáo dục</Link>,
        title: 'Danh mục giáo dục',
      },
    ] : []),
    {
      key: '/admin/reports',
      icon: <FlagOutlined className="text-base" />,
      label: <Link to="/admin/reports">Báo cáo vi phạm</Link>,
      title: 'Báo cáo vi phạm',
    },
    ...(isSystemAdmin ? [
      {
        key: '/admin/audit-logs',
        icon: <HistoryOutlined className="text-base" />,
        label: <Link to="/admin/audit-logs">Nhật ký hệ thống</Link>,
        title: 'Nhật ký hệ thống',
      },
      {
        key: '/admin/settings',
        icon: <SettingOutlined className="text-base" />,
        label: <Link to="/admin/settings">Cài đặt hệ thống</Link>,
        title: 'Cài đặt hệ thống',
      },
    ] : []),
  ];

  const adminOnlyRoutes = ['/admin/users', '/admin/education', '/admin/audit-logs', '/admin/settings'];
  const isAccessingRestrictedRoute = isModeratorOnly && adminOnlyRoutes.some((route) => location.pathname.startsWith(route));

  if (isAccessingRestrictedRoute) {
    return <Navigate to="/admin" replace />;
  }

  const getBreadcrumbTitle = () => {
    const path = location.pathname;
    if (path === '/admin') return 'Dashboard';
    if (path.startsWith('/admin/documents')) return 'Quản lý tài liệu';
    if (path.startsWith('/admin/users')) return 'Quản lý người dùng';
    if (path.startsWith('/admin/education')) return 'Danh mục giáo dục';
    if (path.startsWith('/admin/reports')) return 'Báo cáo vi phạm';
    if (path.startsWith('/admin/audit-logs')) return 'Nhật ký hệ thống';
    if (path.startsWith('/admin/settings')) return 'Cài đặt hệ thống';
    return 'Quản trị';
  };

  const userMenuItems = [
    {
      key: 'home',
      label: <Link to="/"><HomeOutlined className="mr-2" />Về Trang chủ</Link>,
    },
    {
      key: 'profile',
      label: <Link to="/profile"><UserOutlined className="mr-2" />Trang cá nhân</Link>,
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      danger: true,
      label: <span onClick={logout} className="font-semibold"><LogoutOutlined className="mr-2" />Đăng xuất</span>,
    },
  ];

  return (
    <Layout className="h-screen overflow-hidden bg-[#F4F5F7] dark:bg-slate-950 font-sans flex flex-row">
      
      {/* ERP Dark Navy Mini Icon Sidebar (Matching Screenshot Tone) */}
      <Sider 
        trigger={null} 
        collapsible 
        collapsed={collapsed}
        collapsedWidth={60}
        width={220}
        theme="dark"
        className="h-screen sticky top-0 left-0 bg-[#0A1325] border-r border-[#152238] z-30 shrink-0 select-none"
      >
        <style>{`
          .custom-sidebar-menu.ant-menu-dark {
            background-color: #0A1325 !important;
          }
          .custom-sidebar-menu.ant-menu-dark .ant-menu-item-selected {
            background-color: #2563EB !important;
            color: #ffffff !important;
            border-radius: 6px;
          }
          .custom-sidebar-menu.ant-menu-dark .ant-menu-item:hover:not(.ant-menu-item-selected) {
            background-color: #16233B !important;
            color: #ffffff !important;
            border-radius: 6px;
          }
        `}</style>

        {/* Top Blue Store/Brand Icon Box */}
        <div className="h-14 flex items-center justify-center border-b border-[#152238] bg-[#0A1325]">
          <Link to="/admin" className="flex items-center justify-center">
            <div className="w-9 h-9 rounded-lg bg-blue-600 text-white flex items-center justify-center text-lg font-bold shadow-sm hover:bg-blue-500 transition-colors">
              <BookOutlined />
            </div>
            {!collapsed && (
              <span className="font-bold text-white text-sm ml-2.5 truncate">
                {settings?.siteName ? settings.siteName.split('-')[0].trim() : 'ShareStudy'}
              </span>
            )}
          </Link>
        </div>

        {/* Clean Icon Navigation Menu */}
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          items={menuItems}
          className="bg-[#0A1325] border-0 mt-2 font-medium custom-sidebar-menu text-xs px-1"
        />
      </Sider>

      {/* Main Layout Container */}
      <Layout className="h-screen flex flex-col flex-1 min-w-0 overflow-hidden bg-[#F4F5F7] dark:bg-slate-950">
        
        {/* Crisp Top Header Bar (Matching Screenshot) */}
        <Header className="bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800 px-4 flex items-center justify-between h-13 leading-normal shrink-0 z-20">
          
          {/* Left: Sidebar Toggle & Breadcrumb */}
          <div className="flex items-center gap-3">
            <button
              onClick={() => setCollapsed(!collapsed)}
              className="p-1.5 rounded text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 border-0 bg-transparent cursor-pointer"
            >
              {collapsed ? <MenuUnfoldOutlined className="text-base" /> : <MenuFoldOutlined className="text-base" />}
            </button>

            {/* Breadcrumb with rounded white tag background matching screenshot */}
            <div className="flex items-center gap-1.5 text-xs text-slate-500">
              <span className="bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded text-slate-600 dark:text-slate-300 font-medium">
                Trang chủ
              </span>
              <span className="text-slate-400">/</span>
              <span className="font-bold text-slate-800 dark:text-white">
                {getBreadcrumbTitle()}
              </span>
            </div>
          </div>

          {/* Right: Top Search Input & Action Icons */}
          <div className="flex items-center gap-3">
            <Input
              prefix={<SearchOutlined className="text-slate-400 text-xs" />}
              placeholder="Tìm kiếm nhanh sản phẩm, tài liệu..."
              className="w-56 sm:w-64 rounded-md text-xs bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700 h-8"
            />

            <button 
              onClick={toggleDarkMode}
              className="p-1.5 rounded text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 border-0 bg-transparent cursor-pointer"
              title="Đổi giao diện"
            >
              {isDarkMode ? <SunOutlined className="text-base text-amber-500" /> : <MoonOutlined className="text-base text-slate-600" />}
            </button>

            <Tooltip title="Thông báo">
              <button className="p-1.5 rounded text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 border-0 bg-transparent cursor-pointer">
                <BellOutlined className="text-base" />
              </button>
            </Tooltip>

            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" trigger={['click']}>
              <div className="flex items-center gap-2 cursor-pointer p-1 rounded hover:bg-slate-100 dark:hover:bg-slate-800">
                <Avatar 
                  size={28}
                  src={user?.avatarUrl ? formatAvatarUrl(user.avatarUrl, user.fullName || user.username) : undefined} 
                  icon={!user?.avatarUrl && <UserOutlined />} 
                  className="bg-blue-600 text-white shrink-0 font-bold" 
                />
              </div>
            </Dropdown>
          </div>
        </Header>

        {/* Content Body */}
        <Content className="flex-1 overflow-y-auto p-4 sm:p-5 bg-[#F4F5F7] dark:bg-slate-950">
          <Outlet />
        </Content>

      </Layout>

    </Layout>
  );
};

export default AdminLayout;
