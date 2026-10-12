import React, { useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Badge, Dropdown, Avatar, Button, Popover, List, Empty, Drawer, Tag } from 'antd';
import { 
  BellOutlined, 
  MoonOutlined, 
  SunOutlined, 
  UserOutlined, 
  DownOutlined,
  LogoutOutlined,
  DashboardOutlined,
  HeartOutlined,
  CloudUploadOutlined,
  MenuOutlined,
  HomeOutlined,
  FileTextOutlined,
  BookOutlined,
  BankOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { useAuth } from '../../contexts/AuthContext';
import { useTheme } from '../../contexts/ThemeContext';
import { useSettings } from '../../contexts/SettingsContext';
import { notificationService } from '../../services/notificationService';
import { formatAvatarUrl } from '../../utils/avatarUtils';

dayjs.extend(relativeTime);

export const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout, isAdmin, isModerator, openLoginModal, openRegisterModal } = useAuth();
  const { isDarkMode, toggleDarkMode } = useTheme();
  const { settings } = useSettings();
  
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  useEffect(() => {
    if (user) {
      fetchNotifications();
    }
  }, [user]);

  const fetchNotifications = async () => {
    try {
      const resCount = await notificationService.getUnreadCount();
      if (resCount?.data !== undefined) {
        setUnreadCount(resCount.data);
      }
      const resList = await notificationService.getNotifications({ pageNumber: 1, pageSize: 5 });
      if (resList?.data?.items) {
        setNotifications(resList.data.items);
      }
    } catch (err) {
      console.error('Error fetching notifications:', err);
    }
  };

  const handleMarkAsRead = async (id) => {
    try {
      await notificationService.markAsRead(id);
      fetchNotifications();
    } catch (err) {
      console.error(err);
    }
  };

  const navItems = [
    { label: 'Trang chủ', path: '/', icon: <HomeOutlined /> },
    { label: 'Tài liệu', path: '/documents', icon: <FileTextOutlined /> },
    { label: 'Môn học', path: '/subjects', icon: <BookOutlined /> },
    { label: 'Trường & Ngành', path: '/schools', icon: <BankOutlined /> },
  ];

  const userMenuItems = [
    {
      key: 'header_user_info',
      label: (
        <div className="py-1 px-1 border-b border-slate-100">
          <div className="font-bold text-slate-900 text-sm truncate">{user?.fullName || user?.username}</div>
          <div className="text-[11px] text-slate-400 truncate">{user?.email}</div>
          {isAdmin && <Tag color="red" className="mt-1 font-bold text-[10px] m-0">Quản trị viên</Tag>}
          {isModerator && !isAdmin && <Tag color="gold" className="mt-1 font-bold text-[10px] m-0">Kiểm duyệt viên</Tag>}
        </div>
      ),
    },
    {
      key: 'profile',
      label: (
        <Link to="/profile" className="flex items-center gap-2 text-xs font-semibold py-1">
          <UserOutlined className="text-blue-600" /> Trang cá nhân
        </Link>
      ),
    },
    {
      key: 'favorites',
      label: (
        <Link to="/profile?tab=favorites" className="flex items-center gap-2 text-xs font-semibold py-1">
          <HeartOutlined className="text-rose-500" /> Tài liệu yêu thích
        </Link>
      ),
    },
    ...(isAdmin || isModerator ? [{
      key: 'admin',
      label: (
        <Link to="/admin" className="flex items-center gap-2 text-xs font-semibold py-1">
          <DashboardOutlined className="text-purple-600" /> Quản trị Admin
        </Link>
      ),
    }] : []),
    {
      type: 'divider',
    },
    {
      key: 'logout',
      danger: true,
      label: (
        <span onClick={logout} className="flex items-center gap-2 text-xs font-semibold py-1">
          <LogoutOutlined /> Đăng xuất
        </span>
      ),
    },
  ];

  const notificationContent = (
    <div className="w-80 sm:w-96 max-h-96 overflow-y-auto">
      <div className="flex justify-between items-center px-4 py-3 border-b border-slate-100 font-bold text-slate-900 text-xs">
        <span className="flex items-center gap-2">
          <BellOutlined className="text-blue-600" /> Thông báo hệ thống
        </span>
        {unreadCount > 0 && (
          <span 
            className="text-[11px] text-blue-600 cursor-pointer hover:underline font-semibold"
            onClick={async () => {
              await notificationService.markAllAsRead();
              fetchNotifications();
            }}
          >
            Đánh dấu tất cả đã đọc
          </span>
        )}
      </div>
      <List
        itemLayout="horizontal"
        dataSource={notifications}
        locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không có thông báo mới" /> }}
        renderItem={(item) => (
          <List.Item 
            className={`cursor-pointer hover:bg-slate-50 px-4 py-3 transition-colors border-b border-slate-50 ${!item.isRead ? 'bg-blue-50/40 font-medium' : ''}`}
            onClick={() => {
              handleMarkAsRead(item.id);
              if (item.link) navigate(item.link);
            }}
          >
            <List.Item.Meta
              avatar={
                <div className={`w-8 h-8 rounded-full flex items-center justify-center text-xs ${!item.isRead ? 'bg-blue-600 text-white' : 'bg-slate-100 text-slate-500'}`}>
                  <BellOutlined />
                </div>
              }
              title={
                <div className="flex justify-between items-center">
                  <span className="text-xs font-bold text-slate-800 line-clamp-1">{item.title}</span>
                  <span className="text-[10px] text-slate-400 font-normal">{dayjs(item.createdAt).fromNow()}</span>
                </div>
              }
              description={<span className="text-[11px] text-slate-500 line-clamp-2 mt-0.5">{item.message}</span>}
            />
          </List.Item>
        )}
      />
    </div>
  );

  return (
    <header className="sticky top-0 z-50 bg-white/95 dark:bg-slate-900/95 backdrop-blur-md border-b border-slate-200/80 dark:border-slate-800 shadow-sm transition-colors">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 sm:h-18 flex items-center justify-between gap-4">
        
        {/* Left Section: Brand Logo & Mobile/Tablet Menu Toggle */}
        <div className="flex items-center gap-3 shrink-0">
          {/* Mobile & Tablet Hamburger Button (Shows under 1280px) */}
          <button
            onClick={() => setMobileMenuOpen(true)}
            className="xl:hidden p-2 rounded-xl text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 border-0 bg-transparent cursor-pointer flex items-center justify-center"
            aria-label="Toggle Navigation"
          >
            <MenuOutlined className="text-xl" />
          </button>

          {/* Brand Logo */}
          <Link to="/" className="flex items-center gap-2.5 no-underline group shrink-0">
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-2xl bg-gradient-to-tr from-blue-600 via-indigo-600 to-purple-600 flex items-center justify-center text-white shadow-md shadow-blue-500/25 group-hover:scale-105 transition-transform duration-300 shrink-0">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z" />
              </svg>
            </div>
            <div>
              <div className="text-lg sm:text-xl font-black text-slate-900 dark:text-white tracking-tight leading-none group-hover:text-blue-600 transition-colors whitespace-nowrap">
                {settings?.siteName ? settings.siteName.split('-')[0].trim() : 'ShareStudy'}<span className="text-blue-600">.</span>
              </div>
              <div className="text-[10px] text-slate-400 dark:text-slate-400 font-semibold tracking-wider uppercase mt-0.5 hidden sm:block whitespace-nowrap">
                {settings?.siteSlogan || 'Học Tập & Chia Sẻ'}
              </div>
            </div>
          </Link>
        </div>

        {/* Desktop Navigation Links (Shows on XL screens >= 1280px) */}
        <nav className="hidden xl:flex items-center gap-1 bg-slate-100/70 dark:bg-slate-800/70 p-1.5 rounded-2xl border border-slate-200/60 dark:border-slate-700/60">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`no-underline px-4 py-2 rounded-xl text-xs font-bold transition-all duration-200 flex items-center gap-2 whitespace-nowrap shrink-0 ${
                  isActive 
                    ? 'bg-white dark:bg-slate-900 text-blue-600 dark:text-blue-400 shadow-sm' 
                    : 'text-slate-600 dark:text-slate-300 hover:text-slate-900 dark:hover:text-white hover:bg-white/60'
                }`}
              >
                {item.icon}
                {item.label}
              </Link>
            );
          })}
        </nav>

        {/* Right Action Tools & User Menu */}
        <div className="flex items-center gap-2 sm:gap-2.5 shrink-0">
          
          {/* Upload CTA Button */}
          <Link to="/upload" className="no-underline hidden md:inline-flex">
            <Button 
              type="primary" 
              icon={<CloudUploadOutlined />}
              className="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 font-bold h-10 px-4 rounded-xl flex items-center shadow-md shadow-blue-500/20 border-0 text-xs whitespace-nowrap"
            >
              Đăng tài liệu
            </Button>
          </Link>

          {/* Notification Bell */}
          <Popover 
            content={notificationContent} 
            trigger="click" 
            placement="bottomRight"
            overlayClassName="notification-popover"
          >
            <button className="relative p-2.5 rounded-xl text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors border-0 bg-transparent cursor-pointer shrink-0">
              <Badge count={unreadCount} overflowCount={99} size="small" offset={[2, -2]}>
                <BellOutlined className="text-lg text-slate-700 dark:text-slate-300" />
              </Badge>
            </button>
          </Popover>

          {/* Dark Mode Toggle Icon */}
          <button 
            onClick={toggleDarkMode}
            className="p-2.5 rounded-xl text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors border-0 bg-transparent cursor-pointer shrink-0"
            title={isDarkMode ? 'Chuyển giao diện Sáng' : 'Chuyển giao diện Tối'}
          >
            {isDarkMode ? <SunOutlined className="text-lg text-amber-400" /> : <MoonOutlined className="text-lg text-slate-700" />}
          </button>

          {/* User Account Dropdown or Login Buttons */}
          {user ? (
            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" trigger={['click']}>
              <div className="flex items-center gap-2 cursor-pointer p-1 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors border border-transparent hover:border-slate-200 shrink-0">
                <Avatar 
                  src={formatAvatarUrl(user.avatarUrl, user.fullName || user.username)} 
                  icon={!user.avatarUrl && <UserOutlined />} 
                  className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white font-bold"
                />
                <span className="text-xs font-bold text-slate-800 dark:text-white hidden xl:inline-block max-w-[110px] truncate">
                  {user.fullName || user.username}
                </span>
                <DownOutlined className="text-[10px] text-slate-400 dark:text-slate-400 hidden xl:inline-block" />
              </div>
            </Dropdown>
          ) : (
            <div className="flex items-center gap-1.5 shrink-0">
              <Button 
                type="text" 
                onClick={openLoginModal}
                className="font-bold text-xs text-slate-700 dark:text-slate-300 hover:text-blue-600"
              >
                Đăng nhập
              </Button>
              <Button 
                type="primary" 
                onClick={openRegisterModal}
                className="bg-blue-600 hover:bg-blue-500 font-bold rounded-xl text-xs h-9"
              >
                Đăng ký
              </Button>
            </div>
          )}

        </div>

      </div>

      {/* Mobile & iPad Navigation Drawer */}
      <Drawer
        title={
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-xl bg-blue-600 text-white font-black text-xs flex items-center justify-center shadow-md shadow-blue-500/20">
              SS
            </div>
            <span className="font-black text-slate-900 text-base">ShareStudy</span>
          </div>
        }
        placement="left"
        onClose={() => setMobileMenuOpen(false)}
        open={mobileMenuOpen}
        styles={{ wrapper: { width: 280 } }}
      >
        <div className="space-y-4">
          <div className="space-y-1">
            {navItems.map((item) => {
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  onClick={() => setMobileMenuOpen(false)}
                  className={`no-underline px-4 py-3 rounded-2xl text-xs font-bold flex items-center gap-3 transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white shadow-md shadow-blue-500/20'
                      : 'text-slate-700 hover:bg-slate-100'
                  }`}
                >
                  <span className="text-base">{item.icon}</span>
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </div>

          <div className="pt-4 border-t border-slate-100 space-y-2">
            <Link to="/upload" onClick={() => setMobileMenuOpen(false)} className="no-underline block">
              <Button 
                type="primary" 
                icon={<CloudUploadOutlined />} 
                className="w-full bg-blue-600 font-bold rounded-xl h-10"
              >
                Đăng tài liệu ngay
              </Button>
            </Link>

            {!user && (
              <div className="grid grid-cols-2 gap-2 pt-2">
                <Button 
                  onClick={() => { setMobileMenuOpen(false); openLoginModal(); }} 
                  className="w-full rounded-xl font-bold text-xs"
                >
                  Đăng nhập
                </Button>
                <Button 
                  type="primary" 
                  onClick={() => { setMobileMenuOpen(false); openRegisterModal(); }} 
                  className="w-full bg-blue-600 rounded-xl font-bold text-xs"
                >
                  Đăng ký
                </Button>
              </div>
            )}
          </div>
        </div>
      </Drawer>

    </header>
  );
};

export default Header;
