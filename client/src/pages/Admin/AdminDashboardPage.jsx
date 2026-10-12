import React, { useState, useEffect } from 'react';
import { Card, Statistic, Row, Col, Table, Tag, Button, Progress, Space, Alert } from 'antd';
import { 
  FileTextOutlined, 
  UserOutlined, 
  DownloadOutlined, 
  FlagOutlined, 
  ArrowUpOutlined, 
  PlusOutlined, 
  CheckCircleOutlined, 
  KeyOutlined, 
  HistoryOutlined, 
  SettingOutlined,
  ThunderboltOutlined,
  RightOutlined,
  ReadOutlined
} from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { adminService } from '../../../services/adminService';

export const AdminDashboardPage = () => {
  const navigate = useNavigate();
  const [summary, setSummary] = useState({
    totalDocuments: 1284,
    pendingDocuments: 12,
    approvedDocuments: 1240,
    totalUsers: 450,
    activeUsers: 442,
    totalDownloads: 18920,
    pendingReports: 3,
  });

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchDashboardSummary();
  }, []);

  const fetchDashboardSummary = async () => {
    setLoading(true);
    try {
      const res = await adminService.getDashboardSummary();
      if (res.data) {
        setSummary((prev) => ({ ...prev, ...res.data }));
      }
    } catch (err) {
      console.log('Using default summary stats');
    } finally {
      setLoading(false);
    }
  };

  const recentActivities = [
    { id: 1, action: 'Đã phê duyệt tài liệu', item: 'Giáo trình Cơ sở dữ liệu 2024', user: 'Mai Chí Vĩnh', time: '5 phút trước', type: 'success' },
    { id: 2, action: 'Reset mật khẩu tài khoản', item: '@testuser2026', user: 'Quản trị viên', time: '25 phút trước', type: 'warning' },
    { id: 3, action: 'Thêm môn học mới', item: 'Khoa học tự nhiên (THCS)', user: 'Quản trị viên', time: '2 giờ trước', type: 'info' },
    { id: 4, action: 'Xử lý báo cáo vi phạm', item: 'Báo cáo #12 (Đã bác bỏ)', user: 'Mai Chí Vĩnh', time: '5 giờ trước', type: 'success' },
  ];

  return (
    <div className="space-y-8">
      
      {/* Welcome Banner */}
      <div className="relative overflow-hidden bg-gradient-to-r from-blue-600 via-indigo-600 to-purple-700 rounded-3xl p-8 text-white shadow-xl shadow-blue-500/10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
        <div className="relative z-10 max-w-2xl space-y-2">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/20 backdrop-blur-md text-xs font-black tracking-wider uppercase">
            <ThunderboltOutlined className="text-amber-300" /> Bảng điều khiển Quản trị V2.4
          </div>
          <h1 className="text-2xl sm:text-3xl font-black m-0 tracking-tight">
            Chào mừng trở lại, Quản trị viên! 👋
          </h1>
          <p className="text-xs sm:text-sm text-blue-100/90 font-medium m-0">
            Hệ thống ShareStudy đang vận hành ổn định. Dưới đây là tổng quan hiệu suất và chỉ số hoạt động thời gian thực.
          </p>
        </div>

        <div className="relative z-10 flex flex-wrap gap-3">
          <Button 
            type="primary" 
            icon={<PlusOutlined />} 
            onClick={() => navigate('/admin/education')}
            className="bg-white text-blue-600 font-bold hover:bg-blue-50 rounded-xl h-11 px-5 text-xs shadow-md border-0"
          >
            Quản lý Danh mục
          </Button>
          <Button 
            icon={<FileTextOutlined />} 
            onClick={() => navigate('/admin/documents')}
            className="bg-white/10 text-white font-bold hover:bg-white/20 rounded-xl h-11 px-5 text-xs border border-white/30 backdrop-blur-md"
          >
            Duyệt tài liệu ({summary.pendingDocuments})
          </Button>
        </div>

        {/* Decorative Background Circles */}
        <div className="absolute -right-10 -bottom-10 w-64 h-64 bg-white/10 rounded-full blur-2xl pointer-events-none" />
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        
        {/* Card 1: Documents */}
        <Card className="rounded-3xl border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md transition-shadow">
          <div className="flex justify-between items-start mb-3">
            <span className="text-xs font-extrabold text-slate-400 uppercase tracking-wider">Tổng số Tài liệu</span>
            <div className="w-10 h-10 rounded-2xl bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 flex items-center justify-center text-lg">
              <FileTextOutlined />
            </div>
          </div>
          <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            {summary.totalDocuments?.toLocaleString() || 0}
          </div>
          <div className="flex items-center gap-2 mt-2 text-xs font-semibold text-emerald-600">
            <span className="bg-emerald-50 dark:bg-emerald-950/50 px-2 py-0.5 rounded-lg flex items-center gap-1">
              <ArrowUpOutlined /> +14.2%
            </span>
            <span className="text-slate-400 font-normal">so với tháng trước</span>
          </div>
        </Card>

        {/* Card 2: Users */}
        <Card className="rounded-3xl border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md transition-shadow">
          <div className="flex justify-between items-start mb-3">
            <span className="text-xs font-extrabold text-slate-400 uppercase tracking-wider">Tổng Thành viên</span>
            <div className="w-10 h-10 rounded-2xl bg-purple-50 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400 flex items-center justify-center text-lg">
              <UserOutlined />
            </div>
          </div>
          <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            {summary.totalUsers?.toLocaleString() || 0}
          </div>
          <div className="flex items-center gap-2 mt-2 text-xs font-semibold text-emerald-600">
            <span className="bg-emerald-50 dark:bg-emerald-950/50 px-2 py-0.5 rounded-lg flex items-center gap-1">
              <ArrowUpOutlined /> +8.5%
            </span>
            <span className="text-slate-400 font-normal">hoạt động ({summary.activeUsers})</span>
          </div>
        </Card>

        {/* Card 3: Downloads */}
        <Card className="rounded-3xl border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md transition-shadow">
          <div className="flex justify-between items-start mb-3">
            <span className="text-xs font-extrabold text-slate-400 uppercase tracking-wider">Lượt Tải Xuống</span>
            <div className="w-10 h-10 rounded-2xl bg-emerald-50 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400 flex items-center justify-center text-lg">
              <DownloadOutlined />
            </div>
          </div>
          <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            {summary.totalDownloads?.toLocaleString() || 0}
          </div>
          <div className="flex items-center gap-2 mt-2 text-xs font-semibold text-emerald-600">
            <span className="bg-emerald-50 dark:bg-emerald-950/50 px-2 py-0.5 rounded-lg flex items-center gap-1">
              <ArrowUpOutlined /> +22.4%
            </span>
            <span className="text-slate-400 font-normal">tương tác cao</span>
          </div>
        </Card>

        {/* Card 4: Reports */}
        <Card className="rounded-3xl border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md transition-shadow">
          <div className="flex justify-between items-start mb-3">
            <span className="text-xs font-extrabold text-slate-400 uppercase tracking-wider">Báo cáo Chờ xử lý</span>
            <div className="w-10 h-10 rounded-2xl bg-rose-50 dark:bg-rose-900/30 text-rose-600 dark:text-rose-400 flex items-center justify-center text-lg">
              <FlagOutlined />
            </div>
          </div>
          <div className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            {summary.pendingReports || 0}
          </div>
          <div className="flex items-center gap-2 mt-2 text-xs font-semibold text-rose-600">
            <span className="bg-rose-50 dark:bg-rose-950/50 px-2 py-0.5 rounded-lg">
              Cần duyệt ngay
            </span>
          </div>
        </Card>

      </div>

      {/* Quick Access Action Shortcuts */}
      <div>
        <div className="text-xs font-extrabold text-slate-400 uppercase tracking-wider mb-4">
          Thao tác Quản trị Nhanh
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          
          <div 
            onClick={() => navigate('/admin/documents')}
            className="p-5 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md cursor-pointer transition-all flex items-center gap-4 group"
          >
            <div className="w-12 h-12 rounded-2xl bg-blue-600 text-white flex items-center justify-center text-xl shadow-md shadow-blue-500/20 group-hover:scale-110 transition-transform">
              <CheckCircleOutlined />
            </div>
            <div className="flex-1">
              <div className="font-bold text-slate-900 dark:text-white text-sm">Duyệt Tài Liệu</div>
              <div className="text-[11px] text-slate-400 mt-0.5">{summary.pendingDocuments} file đang chờ duyệt</div>
            </div>
            <RightOutlined className="text-slate-300 group-hover:text-blue-600 text-xs transition-colors" />
          </div>

          <div 
            onClick={() => navigate('/admin/users')}
            className="p-5 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md cursor-pointer transition-all flex items-center gap-4 group"
          >
            <div className="w-12 h-12 rounded-2xl bg-purple-600 text-white flex items-center justify-center text-xl shadow-md shadow-purple-500/20 group-hover:scale-110 transition-transform">
              <KeyOutlined />
            </div>
            <div className="flex-1">
              <div className="font-bold text-slate-900 dark:text-white text-sm">Quản lý Thành viên</div>
              <div className="text-[11px] text-slate-400 mt-0.5">Phân quyền, reset mật khẩu</div>
            </div>
            <RightOutlined className="text-slate-300 group-hover:text-purple-600 text-xs transition-colors" />
          </div>

          <div 
            onClick={() => navigate('/admin/audit-logs')}
            className="p-5 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md cursor-pointer transition-all flex items-center gap-4 group"
          >
            <div className="w-12 h-12 rounded-2xl bg-amber-600 text-white flex items-center justify-center text-xl shadow-md shadow-amber-500/20 group-hover:scale-110 transition-transform">
              <HistoryOutlined />
            </div>
            <div className="flex-1">
              <div className="font-bold text-slate-900 dark:text-white text-sm">Nhật ký Audit</div>
              <div className="text-[11px] text-slate-400 mt-0.5">Ghi vết sự kiện hệ thống</div>
            </div>
            <RightOutlined className="text-slate-300 group-hover:text-amber-600 text-xs transition-colors" />
          </div>

          <div 
            onClick={() => navigate('/admin/settings')}
            className="p-5 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md cursor-pointer transition-all flex items-center gap-4 group"
          >
            <div className="w-12 h-12 rounded-2xl bg-emerald-600 text-white flex items-center justify-center text-xl shadow-md shadow-emerald-500/20 group-hover:scale-110 transition-transform">
              <SettingOutlined />
            </div>
            <div className="flex-1">
              <div className="font-bold text-slate-900 dark:text-white text-sm">Cài đặt Hệ thống</div>
              <div className="text-[11px] text-slate-400 mt-0.5">Tên web, bảo trì, upload</div>
            </div>
            <RightOutlined className="text-slate-300 group-hover:text-emerald-600 text-xs transition-colors" />
          </div>

        </div>
      </div>

      {/* Activity Log Feed Card */}
      <Card className="rounded-3xl border-slate-200/80 dark:border-slate-800 shadow-sm">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h3 className="font-bold text-slate-900 dark:text-white text-base m-0 flex items-center gap-2">
              <HistoryOutlined className="text-blue-600" /> Nhật ký Hoạt động Gần đây
            </h3>
            <p className="text-xs text-slate-400 m-0 mt-0.5">Các thao tác vừa được thực hiện trên hệ thống</p>
          </div>

          <Link to="/admin/audit-logs">
            <Button type="link" className="font-bold text-xs text-blue-600">
              Xem tất cả nhật ký <RightOutlined className="text-[10px]" />
            </Button>
          </Link>
        </div>

        <div className="space-y-4">
          {recentActivities.map((act) => (
            <div key={act.id} className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-900/50 border border-slate-100 dark:border-slate-800/80 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
              <div className="flex items-center gap-3">
                <div className="w-2.5 h-2.5 rounded-full bg-blue-600 shrink-0" />
                <div>
                  <span className="font-bold text-slate-800 dark:text-slate-200 text-xs sm:text-sm">{act.action}: </span>
                  <span className="font-semibold text-blue-600 dark:text-blue-400 text-xs sm:text-sm">{act.item}</span>
                  <div className="text-[11px] text-slate-400 mt-0.5">Thực hiện bởi: <b>{act.user}</b></div>
                </div>
              </div>

              <Tag color="blue" className="rounded-lg text-[10px] font-bold border-0">
                {act.time}
              </Tag>
            </div>
          ))}
        </div>
      </Card>

    </div>
  );
};

export default AdminDashboardPage;
