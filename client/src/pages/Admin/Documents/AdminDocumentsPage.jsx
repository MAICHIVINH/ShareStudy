import React, { useState, useEffect } from 'react';
import { Table, Tag, Button, Input, Select, Modal, Popconfirm, message, Space, Card, Tooltip, Dropdown } from 'antd';
import { 
  SearchOutlined, 
  CheckOutlined, 
  CloseOutlined, 
  EyeOutlined, 
  EyeInvisibleOutlined, 
  DeleteOutlined, 
  FileTextOutlined,
  ExclamationCircleOutlined,
  MoreOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { adminService } from '../../../services/adminService';

const { TextArea } = Input;

export const AdminDocumentsPage = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  const [statusFilter, setStatusFilter] = useState('');
  const [keyword, setKeyword] = useState('');

  // Reject Modal State
  const [rejectModalVisible, setRejectModalVisible] = useState(false);
  const [selectedDocId, setSelectedDocId] = useState(null);
  const [rejectReason, setRejectReason] = useState('');
  const [rejecting, setRejecting] = useState(false);

  useEffect(() => {
    fetchDocuments();
  }, [pageNumber, pageSize, statusFilter]);

  const fetchDocuments = async () => {
    setLoading(true);
    try {
      const res = await adminService.getDocuments({
        pageNumber,
        pageSize,
        status: statusFilter || undefined,
        keyword: keyword || undefined,
      });
      if (res.data) {
        setDocuments(res.data.items || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (err) {
      message.error(err.message || 'Lỗi khi tải danh sách tài liệu');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    setPageNumber(1);
    fetchDocuments();
  };

  const handleApprove = async (id) => {
    try {
      await adminService.approveDocument(id);
      message.success('Đã phê duyệt tài liệu thành công!');
      fetchDocuments();
    } catch (err) {
      message.error(err.message || 'Thao tác phê duyệt thất bại');
    }
  };

  const handleOpenRejectModal = (id) => {
    setSelectedDocId(id);
    setRejectReason('');
    setRejectModalVisible(true);
  };

  const handleRejectSubmit = async () => {
    if (!rejectReason.trim()) {
      message.warning('Vui lòng nhập lý do từ chối!');
      return;
    }
    setRejecting(true);
    try {
      await adminService.rejectDocument(selectedDocId, rejectReason.trim());
      message.success('Đã từ chối tài liệu');
      setRejectModalVisible(false);
      fetchDocuments();
    } catch (err) {
      message.error(err.message || 'Thao tác từ chối thất bại');
    } finally {
      setRejecting(false);
    }
  };

  const handleToggleHide = async (id) => {
    try {
      await adminService.hideDocument(id);
      message.success('Đã cập nhật trạng thái hiển thị tài liệu!');
      fetchDocuments();
    } catch (err) {
      message.error(err.message || 'Thao tác thất bại');
    }
  };

  const handleDelete = async (id) => {
    try {
      await adminService.deleteDocument(id);
      message.success('Đã xóa vĩnh viễn tài liệu!');
      fetchDocuments();
    } catch (err) {
      message.error(err.message || 'Không thể xóa tài liệu này');
    }
  };

  const getNormalizedStatus = (status) => {
    const s = String(status || '').toLowerCase();
    if (s === 'approved' || s === '2') return 'approved';
    if (s === 'pending' || s === '1') return 'pending';
    if (s === 'rejected' || s === '3') return 'rejected';
    if (s === 'hidden' || s === '4') return 'hidden';
    return s;
  };

  const renderStatusTag = (status) => {
    const norm = getNormalizedStatus(status);
    if (norm === 'approved') return <span className="border border-emerald-400 bg-emerald-50 text-emerald-600 text-xs px-2 py-0.5 rounded font-medium">Đã phê duyệt</span>;
    if (norm === 'pending') return <span className="border border-amber-400 bg-amber-50 text-amber-600 text-xs px-2 py-0.5 rounded font-medium">Chờ duyệt</span>;
    if (norm === 'rejected') return <span className="border border-rose-400 bg-rose-50 text-rose-600 text-xs px-2 py-0.5 rounded font-medium">Bị từ chối</span>;
    if (norm === 'hidden') return <span className="border border-slate-300 bg-slate-50 text-slate-500 text-xs px-2 py-0.5 rounded font-medium">Đang ẩn</span>;
    return <Tag color="blue">{status}</Tag>;
  };

  const columns = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 70,
      sorter: (a, b) => a.id - b.id,
    },
    {
      title: 'Tên tài liệu',
      dataIndex: 'title',
      key: 'title',
      sorter: (a, b) => (a.title || '').localeCompare(b.title || ''),
      render: (text, record) => (
        <div>
          <a href={`/documents/${record.slug || record.id}`} target="_blank" rel="noreferrer" className="font-bold text-slate-800 hover:text-blue-600">
            {text}
          </a>
          <div className="text-[11px] text-slate-400">{record.categorySummary || record.subjectName || 'Tài liệu'}</div>
        </div>
      ),
    },
    {
      title: 'Người đăng',
      dataIndex: 'uploaderName',
      key: 'uploaderName',
      sorter: (a, b) => (a.uploaderName || '').localeCompare(b.uploaderName || ''),
      render: (text) => <span className="text-xs font-semibold text-slate-700">{text || 'Thành viên'}</span>,
    },
    {
      title: 'Định dạng',
      dataIndex: 'fileType',
      key: 'fileType',
      width: 110,
      sorter: (a, b) => (a.fileType || '').localeCompare(b.fileType || ''),
      render: (text) => <Tag color="blue" className="uppercase font-bold text-[10px]">{text}</Tag>,
    },
    {
      title: 'Xem / Tải',
      key: 'stats',
      width: 120,
      sorter: (a, b) => (a.downloadCount || 0) - (b.downloadCount || 0),
      render: (_, r) => <span className="text-xs text-slate-500 font-medium">{r.viewCount || 0} / {r.downloadCount || 0}</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      sorter: (a, b) => String(a.status || '').localeCompare(String(b.status || '')),
      render: (status) => renderStatusTag(status),
    },
    {
      title: 'Ngày đăng',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 130,
      sorter: (a, b) => new Date(a.createdAt || 0) - new Date(b.createdAt || 0),
      render: (date) => <span className="text-xs text-slate-400">{dayjs(date).format('DD/MM/YYYY')}</span>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 100,
      render: (_, record) => {
        const norm = getNormalizedStatus(record.status);
        
        const menuItems = [
          {
            key: 'view',
            icon: <EyeOutlined className="text-blue-600" />,
            label: <a href={`/documents/${record.slug || record.id}`} target="_blank" rel="noreferrer">Xem trang chi tiết</a>
          },
          norm !== 'approved' && {
            key: 'approve',
            icon: <CheckOutlined className="text-emerald-600" />,
            label: 'Phê duyệt tài liệu',
            onClick: () => handleApprove(record.id)
          },
          norm !== 'rejected' && {
            key: 'reject',
            icon: <CloseOutlined className="text-rose-600" />,
            label: 'Từ chối tài liệu',
            onClick: () => handleOpenRejectModal(record.id)
          },
          norm === 'approved' && {
            key: 'hide',
            icon: <EyeInvisibleOutlined className="text-slate-500" />,
            label: 'Ẩn tài liệu',
            onClick: () => handleToggleHide(record.id)
          },
          norm === 'hidden' && {
            key: 'show',
            icon: <EyeOutlined className="text-blue-600" />,
            label: 'Hiển thị lại tài liệu',
            onClick: () => handleToggleHide(record.id)
          },
          { type: 'divider' },
          {
            key: 'delete',
            danger: true,
            icon: <DeleteOutlined />,
            label: (
              <Popconfirm
                title="Xóa vĩnh viễn tài liệu?"
                description="Bạn có chắc chắn muốn xóa không?"
                okText="Xóa"
                cancelText="Hủy"
                okButtonProps={{ danger: true }}
                onConfirm={() => handleDelete(record.id)}
              >
                <span>Xóa vĩnh viễn</span>
              </Popconfirm>
            )
          }
        ].filter(Boolean);

        return (
          <Dropdown menu={{ items: menuItems }} placement="bottomRight" trigger={['click']}>
            <Tooltip title="Menu Thao tác">
              <Button size="small" icon={<MoreOutlined />} className="rounded-lg font-bold" />
            </Tooltip>
          </Dropdown>
        );
      },
    },
  ];

  return (
    <div className="w-full space-y-4">
      
      {/* Top Sub-Header Bar with Left Search & Filter (Matching AdminSettingsPage layout) */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
          <Input
            placeholder="Tìm theo tên tài liệu, mã..."
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            onPressEnter={handleSearch}
            prefix={<SearchOutlined className="text-slate-400 text-xs" />}
            className="w-full sm:w-72 rounded-md text-xs bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 h-9"
            allowClear
          />

          <Select
            placeholder="Lọc trạng thái"
            value={statusFilter || undefined}
            onChange={(val) => { setStatusFilter(val || ''); setPageNumber(1); }}
            className="w-40 text-xs rounded-md h-9"
            allowClear
            options={[
              { value: 'Pending', label: 'Chờ duyệt' },
              { value: 'Approved', label: 'Đã duyệt' },
              { value: 'Rejected', label: 'Bị từ chối' },
              { value: 'Hidden', label: 'Đang ẩn' },
            ]}
          />
        </div>
      </div>

      {/* Main Table Container */}
      <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800">
        <Table
          dataSource={documents}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{
            current: pageNumber,
            pageSize: pageSize,
            total: totalCount,
            onChange: (p, ps) => { setPageNumber(p); setPageSize(ps); },
            showSizeChanger: true,
          }}
        />
      </div>

      {/* Reject Modal */}
      <Modal
        title={
          <span className="font-bold flex items-center gap-2 text-red-600">
            <ExclamationCircleOutlined /> Từ chối tài liệu
          </span>
        }
        open={rejectModalVisible}
        onOk={handleRejectSubmit}
        onCancel={() => setRejectModalVisible(false)}
        confirmLoading={rejecting}
        okText="Xác nhận từ chối"
        cancelText="Hủy"
        okButtonProps={{ danger: true }}
      >
        <div className="py-2 space-y-2">
          <p className="text-xs text-slate-600">
            Vui lòng nhập lý do từ chối tài liệu này (Lý do sẽ được gửi thông báo đến người đăng):
          </p>
          <TextArea
            rows={4}
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
            placeholder="VD: Tài liệu vi phạm bản quyền, nội dung không rõ ràng hoặc hình ảnh mờ..."
            className="rounded-xl"
          />
        </div>
      </Modal>

    </div>
  );
};

export default AdminDocumentsPage;
