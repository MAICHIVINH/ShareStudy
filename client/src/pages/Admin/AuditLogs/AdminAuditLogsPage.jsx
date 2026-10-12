import React, { useState, useEffect } from 'react';
import { Table, Tag, Input, Select, Card, Button, Modal, Tooltip, Statistic, DatePicker, Space } from 'antd';
import { 
  HistoryOutlined, 
  SearchOutlined, 
  EyeOutlined, 
  SafetyCertificateOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  UserOutlined,
  SettingOutlined,
  FileTextOutlined,
  FilterOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';

// Mock/Initial Audit Logs Data Generator
const initialAuditLogs = [
  {
    id: 1001,
    timestamp: dayjs().subtract(5, 'minute').toISOString(),
    performer: 'Quản trị viên Hệ thống (admin)',
    ipAddress: '192.168.1.1',
    actionType: 'UPDATE_ROLE',
    actionName: 'Cập nhật vai trò người dùng',
    module: 'Users',
    severity: 'INFO',
    details: 'Đã gán vai trò Kiểm duyệt viên (Moderator) cho tài khoản @mcv',
    payload: { userId: 4, newRoles: ['Moderator', 'Member'], performedBy: 'admin' }
  },
  {
    id: 1002,
    timestamp: dayjs().subtract(25, 'minute').toISOString(),
    performer: 'Mai Chí Vĩnh (mcv)',
    ipAddress: '118.69.182.45',
    actionType: 'APPROVE_DOCUMENT',
    actionName: 'Phê duyệt tài liệu',
    module: 'Documents',
    severity: 'SUCCESS',
    details: 'Đã phê duyệt tài liệu #45: "Giáo trình Lập trình C# Nâng cao"',
    payload: { documentId: 45, title: 'Giáo trình Lập trình C# Nâng cao', status: 'Approved' }
  },
  {
    id: 1003,
    timestamp: dayjs().subtract(1, 'hour').toISOString(),
    performer: 'Quản trị viên Hệ thống (admin)',
    ipAddress: '192.168.1.1',
    actionType: 'RESET_PASSWORD',
    actionName: 'Reset mật khẩu người dùng',
    module: 'Users',
    severity: 'WARNING',
    details: 'Đã reset mật khẩu tạm thời cho tài khoản @testuser2026',
    payload: { userId: 2, username: 'testuser2026', method: 'Admin_Force_Reset' }
  },
  {
    id: 1004,
    timestamp: dayjs().subtract(3, 'hour').toISOString(),
    performer: 'Sinh Viên Test (student_140626)',
    ipAddress: '27.72.102.12',
    actionType: 'LOGIN',
    actionName: 'Đăng nhập hệ thống',
    module: 'Auth',
    severity: 'INFO',
    details: 'Đăng nhập thành công từ trình duyệt Chrome / Windows 11',
    payload: { userId: 3, loginMethod: 'Password', userAgent: 'Mozilla/5.0...' }
  },
  {
    id: 1005,
    timestamp: dayjs().subtract(6, 'hour').toISOString(),
    performer: 'Quản trị viên Hệ thống (admin)',
    ipAddress: '192.168.1.1',
    actionType: 'CREATE_SUBJECT',
    actionName: 'Thêm mới môn học',
    module: 'Education',
    severity: 'SUCCESS',
    details: 'Đã thêm mới môn học "Khoa học tự nhiên" (Mã: KHTN_THCS) thuộc Cấp THCS',
    payload: { subjectCode: 'KHTN_THCS', name: 'Khoa học tự nhiên', levelId: 1 }
  },
  {
    id: 1006,
    timestamp: dayjs().subtract(12, 'hour').toISOString(),
    performer: 'Hệ thống tự động (System Cron)',
    ipAddress: '127.0.0.1',
    actionType: 'SYSTEM_BACKUP',
    actionName: 'Sao lưu CSDL tự động',
    module: 'System',
    severity: 'INFO',
    details: 'Đã hoàn tất bản sao lưu định kỳ database_backup_20260812.sql (42.5 MB)',
    payload: { backupFile: 'database_backup_20260812.sql', sizeBytes: 44564480 }
  },
  {
    id: 1007,
    timestamp: dayjs().subtract(1, 'day').toISOString(),
    performer: 'Mai Chí Vĩnh (mcv)',
    ipAddress: '118.69.182.45',
    actionType: 'RESOLVE_REPORT',
    actionName: 'Xử lý báo cáo vi phạm',
    module: 'Reports',
    severity: 'SUCCESS',
    details: 'Đã bác bỏ báo cáo #12 về tài liệu #30 (Nội dung hợp lệ)',
    payload: { reportId: 12, resultStatus: 'Dismissed', adminNotes: 'Tài liệu đáp ứng quy định' }
  }
];

import { adminService } from '../../../services/adminService';

export const AdminAuditLogsPage = () => {
  const [logs, setLogs] = useState(initialAuditLogs);
  const [filteredLogs, setFilteredLogs] = useState(initialAuditLogs);
  const [loading, setLoading] = useState(false);
  
  // Filter States
  const [keyword, setKeyword] = useState('');
  const [moduleFilter, setModuleFilter] = useState(undefined);
  const [severityFilter, setSeverityFilter] = useState(undefined);

  // Detail Modal State
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedLog, setSelectedLog] = useState(null);

  useEffect(() => {
    fetchLogs();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [keyword, moduleFilter, severityFilter, logs]);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const res = await adminService.getAuditLogs({
        keyword: keyword || undefined,
        module: moduleFilter || undefined,
        severity: severityFilter || undefined
      });
      const items = res.data?.items || (Array.isArray(res.data) ? res.data : null);
      if (items && items.length > 0) {
        const mapped = items.map(item => ({
          ...item,
          payload: typeof item.payload === 'string' ? JSON.parse(item.payload || '{}') : (item.payload || {})
        }));
        setLogs(mapped);
        setFilteredLogs(mapped);
      } else if (items) {
        setLogs([]);
        setFilteredLogs([]);
      } else {
        setLogs(initialAuditLogs);
        setFilteredLogs(initialAuditLogs);
      }
    } catch (err) {
      setLogs(initialAuditLogs);
      setFilteredLogs(initialAuditLogs);
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let result = [...logs];

    if (keyword.trim()) {
      const q = keyword.toLowerCase().trim();
      result = result.filter(
        (item) =>
          item.performer.toLowerCase().includes(q) ||
          item.actionName.toLowerCase().includes(q) ||
          item.details.toLowerCase().includes(q) ||
          item.ipAddress.includes(q)
      );
    }

    if (moduleFilter) {
      result = result.filter((item) => item.module === moduleFilter);
    }

    if (severityFilter) {
      result = result.filter((item) => item.severity === severityFilter);
    }

    setFilteredLogs(result);
  };

  const handleRefresh = () => {
    setLoading(true);
    setTimeout(() => {
      setLogs([...initialAuditLogs]);
      setLoading(false);
    }, 400);
  };

  const handleOpenDetail = (record) => {
    setSelectedLog(record);
    setDetailModalVisible(true);
  };

  const renderSeverityTag = (severity) => {
    if (severity === 'SUCCESS') return <span className="border border-emerald-400 bg-emerald-50 text-emerald-600 text-xs px-2 py-0.5 rounded font-medium">Thành công</span>;
    if (severity === 'WARNING') return <span className="border border-amber-400 bg-amber-50 text-amber-600 text-xs px-2 py-0.5 rounded font-medium">Cảnh báo</span>;
    if (severity === 'ERROR') return <span className="border border-rose-400 bg-rose-50 text-rose-600 text-xs px-2 py-0.5 rounded font-medium">Lỗi</span>;
    return <span className="border border-blue-400 bg-blue-50 text-blue-600 text-xs px-2 py-0.5 rounded font-medium">Thông tin</span>;
  };

  const renderModuleTag = (mod) => {
    return <span className="text-xs font-semibold text-slate-700 bg-slate-100 dark:bg-slate-800 dark:text-slate-300 px-2 py-0.5 rounded">{mod}</span>;
  };

  const columns = [
    {
      title: 'Thời gian',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 150,
      sorter: (a, b) => new Date(a.timestamp || 0) - new Date(b.timestamp || 0),
      render: (t) => (
        <div>
          <div className="font-bold text-slate-800 text-xs">{dayjs(t).format('DD/MM/YYYY')}</div>
          <div className="text-[11px] text-slate-400 font-mono">{dayjs(t).format('HH:mm:ss')}</div>
        </div>
      ),
    },
    {
      title: 'Người thực hiện',
      dataIndex: 'performer',
      key: 'performer',
      sorter: (a, b) => (a.performer || '').localeCompare(b.performer || ''),
      render: (text, record) => (
        <div>
          <div className="font-bold text-slate-900 text-xs">{text}</div>
          <div className="text-[11px] text-slate-400 font-mono">IP: {record.ipAddress}</div>
        </div>
      ),
    },
    {
      title: 'Hành động',
      dataIndex: 'actionName',
      key: 'actionName',
      sorter: (a, b) => (a.actionName || '').localeCompare(b.actionName || ''),
      render: (text, record) => (
        <div>
          <span className="font-bold text-slate-800 text-xs">{text}</span>
          <div className="text-[10px] font-mono text-blue-600 mt-0.5">{record.actionType}</div>
        </div>
      ),
    },
    {
      title: 'Phân hệ',
      dataIndex: 'module',
      key: 'module',
      width: 120,
      sorter: (a, b) => (a.module || '').localeCompare(b.module || ''),
      render: (mod) => renderModuleTag(mod),
    },
    {
      title: 'Mức độ',
      dataIndex: 'severity',
      key: 'severity',
      width: 130,
      sorter: (a, b) => (a.severity || '').localeCompare(b.severity || ''),
      render: (sev) => renderSeverityTag(sev),
    },
    {
      title: 'Chi tiết thao tác',
      dataIndex: 'details',
      key: 'details',
      render: (text) => <span className="text-xs text-slate-600 truncate max-w-xs block">{text}</span>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 80,
      render: (_, record) => (
        <Tooltip title="Xem chi tiết dữ liệu Payload">
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => handleOpenDetail(record)}
            className="rounded-md text-xs"
          />
        </Tooltip>
      ),
    },
  ];

  return (
    <div className="w-full space-y-4">
      
      {/* Top Sub-Header Bar with Left Search & Filters */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
          <Input
            placeholder="Tìm theo người thực hiện, hành động, IP..."
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            prefix={<SearchOutlined className="text-slate-400 text-xs" />}
            className="w-full sm:w-72 rounded-md text-xs bg-white border-slate-200 h-9"
            allowClear
          />

          <Select
            placeholder="Lọc Phân hệ"
            value={moduleFilter}
            onChange={setModuleFilter}
            allowClear
            className="w-36 text-xs rounded-md h-9"
            options={[
              { value: 'Users', label: 'Người dùng' },
              { value: 'Documents', label: 'Tài liệu' },
              { value: 'Education', label: 'Giáo dục' },
              { value: 'Reports', label: 'Báo cáo' },
              { value: 'Auth', label: 'Xác thực' },
              { value: 'System', label: 'Hệ thống' },
            ]}
          />

          <Select
            placeholder="Mức độ"
            value={severityFilter}
            onChange={setSeverityFilter}
            allowClear
            className="w-36 text-xs rounded-md h-9"
            options={[
              { value: 'INFO', label: 'Thông tin' },
              { value: 'SUCCESS', label: 'Thành công' },
              { value: 'WARNING', label: 'Cảnh báo' },
              { value: 'ERROR', label: 'Lỗi' },
            ]}
          />
        </div>

        <Button
          icon={<ReloadOutlined />}
          onClick={handleRefresh}
          className="rounded-md font-semibold text-xs h-9 px-4 ml-auto"
        >
          Làm mới
        </Button>
      </div>

      {/* Data Table Container */}
      <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800">
        <Table
          dataSource={filteredLogs}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 8 }}
        />
      </div>

      {/* Log Detail Modal */}
      <Modal
        title={
          <span className="font-bold flex items-center gap-2 text-slate-900">
            <EyeOutlined className="text-blue-600" /> Chi tiết Nhật ký Audit #{selectedLog?.id}
          </span>
        }
        open={detailModalVisible}
        onOk={() => setDetailModalVisible(false)}
        onCancel={() => setDetailModalVisible(false)}
        okText="Đóng"
        cancelButtonProps={{ style: { display: 'none' } }}
        className="rounded-3xl"
        width={540}
      >
        {selectedLog && (
          <div className="py-2 space-y-4 text-xs">
            <div className="bg-slate-50 p-4 rounded-2xl border border-slate-100 space-y-2">
              <div className="flex justify-between">
                <span className="text-slate-500 font-medium">Thời gian:</span>
                <span className="font-bold text-slate-900">{dayjs(selectedLog.timestamp).format('DD/MM/YYYY HH:mm:ss')}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500 font-medium">Người thực hiện:</span>
                <span className="font-bold text-slate-900">{selectedLog.performer}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500 font-medium">Địa chỉ IP:</span>
                <span className="font-mono font-bold text-blue-600">{selectedLog.ipAddress}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500 font-medium">Phân hệ:</span>
                {renderModuleTag(selectedLog.module)}
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500 font-medium">Mức độ:</span>
                {renderSeverityTag(selectedLog.severity)}
              </div>
            </div>

            <div>
              <label className="font-bold text-slate-700 block mb-1">Mô tả hành động:</label>
              <div className="p-3 bg-white border border-slate-200 rounded-xl font-medium text-slate-800">
                {selectedLog.details}
              </div>
            </div>

            <div>
              <label className="font-bold text-slate-700 block mb-1">Dữ liệu chi tiết (Payload JSON):</label>
              <pre className="bg-slate-900 text-emerald-400 p-4 rounded-2xl text-[11px] font-mono overflow-x-auto">
                {JSON.stringify(selectedLog.payload, null, 2)}
              </pre>
            </div>
          </div>
        )}
      </Modal>

    </div>
  );
};

export default AdminAuditLogsPage;
