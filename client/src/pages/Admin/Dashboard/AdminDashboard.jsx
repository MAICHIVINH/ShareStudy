import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Skeleton, Tag, Button } from 'antd';
import { 
  FileTextOutlined, 
  UserOutlined, 
  DownloadOutlined, 
  EyeOutlined, 
  ClockCircleOutlined, 
  FlagOutlined,
  RightOutlined
} from '@ant-design/icons';
import { 
  ResponsiveContainer, 
  AreaChart, 
  Area, 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip
} from 'recharts';
import { Link, useNavigate } from 'react-router-dom';
import { adminService } from '../../../services/adminService';

export const AdminDashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState({
    totalDocuments: 1284,
    pendingDocuments: 12,
    totalUsers: 450,
    totalDownloads: 18920,
    totalViews: 45210,
    pendingReports: 3
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    setLoading(true);
    try {
      const res = await adminService.getDashboardStats();
      if (res.data) {
        setStats((prev) => ({ ...prev, ...res.data }));
      }
    } catch (err) {
      console.log('Using default summary stats');
    } finally {
      setLoading(false);
    }
  };

  const uploadsData = [
    { date: 'T2', uploads: 12 },
    { date: 'T3', uploads: 19 },
    { date: 'T4', uploads: 15 },
    { date: 'T5', uploads: 25 },
    { date: 'T6', uploads: 22 },
    { date: 'T7', uploads: 30 },
    { date: 'CN', uploads: 28 },
  ];

  const downloadsData = [
    { date: 'T2', downloads: 140 },
    { date: 'T3', downloads: 210 },
    { date: 'T4', downloads: 180 },
    { date: 'T5', downloads: 290 },
    { date: 'T6', downloads: 350 },
    { date: 'T7', downloads: 420 },
    { date: 'CN', downloads: 380 },
  ];

  const statCards = [
    { title: 'Tài liệu', value: stats?.totalDocuments || 0, icon: <FileTextOutlined /> },
    { title: 'Thành viên', value: stats?.totalUsers || 0, icon: <UserOutlined /> },
    { title: 'Lượt tải', value: stats?.totalDownloads || 0, icon: <DownloadOutlined /> },
    { title: 'Lượt xem', value: stats?.totalViews || 0, icon: <EyeOutlined /> },
    { title: 'Chờ duyệt', value: stats?.pendingDocuments || 0, icon: <ClockCircleOutlined /> },
    { title: 'Báo cáo vi phạm', value: stats?.pendingReports || 0, icon: <FlagOutlined /> },
  ];

  if (loading) {
    return <Skeleton active paragraph={{ rows: 8 }} />;
  }

  return (
    <div className="space-y-6">
      
      {/* Simple Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white dark:bg-slate-900 p-6 rounded-xl border border-slate-200 dark:border-slate-800">
        <div>
          <h1 className="text-xl font-bold text-slate-900 dark:text-white m-0">
            Tổng quan Hệ thống
          </h1>
          <p className="text-xs text-slate-500 m-0 mt-1">
            Bảng theo dõi các chỉ số hoạt động của ứng dụng
          </p>
        </div>

        <div className="flex gap-2">
          <Button 
            type="primary" 
            onClick={() => navigate('/admin/documents')}
            className="bg-blue-600 font-medium rounded-lg h-9 px-4 text-xs"
          >
            Duyệt tài liệu ({stats.pendingDocuments})
          </Button>
        </div>
      </div>

      {/* 6 Minimal Stat Cards */}
      <Row gutter={[16, 16]}>
        {statCards.map((card, idx) => (
          <Col xs={24} sm={12} lg={4} key={idx}>
            <div className="bg-white dark:bg-slate-900 p-4 rounded-xl border border-slate-200 dark:border-slate-800 flex items-center justify-between">
              <div>
                <div className="text-[11px] font-medium text-slate-400 mb-1">
                  {card.title}
                </div>
                <div className="text-xl font-bold text-slate-900 dark:text-white">
                  {card.value.toLocaleString()}
                </div>
              </div>
              <div className="w-9 h-9 rounded-lg bg-blue-50 dark:bg-slate-800 text-blue-600 dark:text-blue-400 flex items-center justify-center text-base">
                {card.icon}
              </div>
            </div>
          </Col>
        ))}
      </Row>

      {/* Charts */}
      <Row gutter={[16, 16]}>
        <Col xs={24} lg={12}>
          <div className="bg-white dark:bg-slate-900 p-5 rounded-xl border border-slate-200 dark:border-slate-800 space-y-4">
            <div className="font-bold text-sm text-slate-800 dark:text-slate-200">Tài liệu tải lên theo ngày</div>
            <div className="h-56">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={uploadsData}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Area type="monotone" dataKey="uploads" stroke="#2563eb" fill="#3b82f6" fillOpacity={0.15} />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </div>
        </Col>

        <Col xs={24} lg={12}>
          <div className="bg-white dark:bg-slate-900 p-5 rounded-xl border border-slate-200 dark:border-slate-800 space-y-4">
            <div className="font-bold text-sm text-slate-800 dark:text-slate-200">Lượt tải tài liệu theo ngày</div>
            <div className="h-56">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={downloadsData}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Area type="monotone" dataKey="downloads" stroke="#2563eb" fill="#3b82f6" fillOpacity={0.15} />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </div>
        </Col>
      </Row>

    </div>
  );
};

export default AdminDashboard;
