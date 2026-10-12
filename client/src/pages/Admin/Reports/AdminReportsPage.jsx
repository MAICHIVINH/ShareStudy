import React, { useState, useEffect } from 'react';
import { Table, Tag, Button, Modal, Select, Input, message, Card, Space, Dropdown, Tooltip } from 'antd';
import { FlagOutlined, CheckCircleOutlined, CloseCircleOutlined, EyeOutlined, MoreOutlined, EditOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { adminService } from '../../../services/adminService';

const { TextArea } = Input;

export const AdminReportsPage = () => {
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Resolve Modal State
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedReport, setSelectedReport] = useState(null);
  const [resolveStatus, setResolveStatus] = useState('Resolved');
  const [notes, setNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetchReports();
  }, [pageNumber, pageSize]);

  const fetchReports = async () => {
    setLoading(true);
    try {
      const res = await adminService.getReports({ pageNumber, pageSize });
      if (res.data) {
        setReports(res.data.items || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (err) {
      message.error(err.message || 'Lỗi khi tải danh sách báo cáo');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenResolveModal = (report) => {
    setSelectedReport(report);
    setResolveStatus('Resolved');
    setNotes('');
    setModalVisible(true);
  };

  const handleResolveSubmit = async () => {
    setSubmitting(true);
    try {
      await adminService.resolveReport(selectedReport.id, resolveStatus, notes.trim());
      message.success('Đã xử lý báo cáo vi phạm!');
      setModalVisible(false);
      fetchReports();
    } catch (err) {
      message.error(err.message || 'Thao tác xử lý thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const renderStatusTag = (status) => {
    const s = String(status).toLowerCase();
    if (s === 'resolved') return <Tag icon={<CheckCircleOutlined />} color="success">Đã xử lý</Tag>;
    if (s === 'dismissed') return <Tag icon={<CloseCircleOutlined />} color="default">Bác bỏ</Tag>;
    return <Tag color="warning">Đang chờ</Tag>;
  };

  const columns = [
    { title: 'ID', dataIndex: 'id', key: 'id', width: 70, sorter: (a, b) => a.id - b.id },
    {
      title: 'Tài liệu bị báo cáo',
      dataIndex: 'documentTitle',
      key: 'documentTitle',
      sorter: (a, b) => (a.documentTitle || '').localeCompare(b.documentTitle || ''),
      render: (title, record) => (
        <div>
          <a href={`/documents/${record.documentSlug || record.documentId}`} target="_blank" rel="noreferrer" className="font-bold text-slate-800 hover:text-blue-600">
            {title || `Tài liệu #${record.documentId}`}
          </a>
        </div>
      ),
    },
    {
      title: 'Người báo cáo',
      dataIndex: 'reporterName',
      key: 'reporterName',
      sorter: (a, b) => (a.reporterName || '').localeCompare(b.reporterName || ''),
      render: (name) => <span className="text-xs font-semibold text-slate-700">{name || 'Thành viên'}</span>,
    },
    {
      title: 'Lý do vi phạm',
      dataIndex: 'reason',
      key: 'reason',
      sorter: (a, b) => (a.reason || '').localeCompare(b.reason || ''),
      render: (reason) => <span className="text-xs font-medium text-red-600 bg-red-50 px-2 py-1 rounded-md">{reason}</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      sorter: (a, b) => String(a.status || '').localeCompare(String(b.status || '')),
      render: (s) => renderStatusTag(s),
    },
    {
      title: 'Ngày báo cáo',
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
        const isPending = String(record.status).toLowerCase() === 'pending';
        const menuItems = [
          {
            key: 'view_doc',
            icon: <EyeOutlined className="text-blue-600" />,
            label: <a href={`/documents/${record.documentSlug || record.documentId}`} target="_blank" rel="noreferrer">Xem tài liệu vi phạm</a>
          },
          isPending && {
            key: 'resolve',
            icon: <EditOutlined className="text-emerald-600" />,
            label: 'Xử lý báo cáo vi phạm',
            onClick: () => handleOpenResolveModal(record)
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
      
      {/* Table Container */}
      <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800">
        <Table
          dataSource={reports}
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

      {/* Modal Xử lý Báo cáo */}
      <Modal
        title={<span className="font-bold">Xử lý báo cáo vi phạm #{selectedReport?.id}</span>}
        open={modalVisible}
        onOk={handleResolveSubmit}
        onCancel={() => setModalVisible(false)}
        confirmLoading={submitting}
        okText="Xác nhận xử lý"
        cancelText="Hủy"
      >
        <div className="py-3 space-y-4">
          <div>
            <label className="text-xs font-bold text-slate-700 block mb-1">Kết quả xử lý:</label>
            <Select
              value={resolveStatus}
              onChange={setResolveStatus}
              className="w-full h-10 rounded-xl"
              options={[
                { value: 'Resolved', label: 'Đã xử lý (Duyệt báo cáo & xử lý tài liệu)' },
                { value: 'Dismissed', label: 'Bác bỏ (Báo cáo không chính xác)' },
              ]}
            />
          </div>

          <div>
            <label className="text-xs font-bold text-slate-700 block mb-1">Ghi chú xử lý:</label>
            <TextArea
              rows={4}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Nhập ghi chú phản hồi cho người báo cáo..."
              className="rounded-xl"
            />
          </div>
        </div>
      </Modal>

    </div>
  );
};

export default AdminReportsPage;
