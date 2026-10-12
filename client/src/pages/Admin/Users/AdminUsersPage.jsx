import React, { useState, useEffect } from 'react';
import { 
  Table, Tag, Switch, Button, Input, Modal, Select, Avatar, message, Card, Space, Popconfirm, Drawer, Tooltip, Statistic, Dropdown 
} from 'antd';
import { 
  UserOutlined, 
  SearchOutlined, 
  EditOutlined, 
  SafetyOutlined, 
  KeyOutlined, 
  DeleteOutlined, 
  EyeOutlined,
  CheckCircleOutlined,
  LockOutlined,
  TeamOutlined,
  CrownOutlined,
  MoreOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { adminService } from '../../../services/adminService';
import { formatAvatarUrl } from '../../../utils/avatarUtils';

export const AdminUsersPage = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  // Filter States
  const [keyword, setKeyword] = useState('');
  const [roleFilter, setRoleFilter] = useState(undefined);
  const [statusFilter, setStatusFilter] = useState(undefined);

  // Role Edit Modal State
  const [roleModalVisible, setRoleModalVisible] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const [selectedRoles, setSelectedRoles] = useState([]);
  const [savingRole, setSavingRole] = useState(false);

  // User Detail Drawer State
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [detailUser, setDetailUser] = useState(null);

  useEffect(() => {
    fetchUsers();
  }, [pageNumber, pageSize, roleFilter, statusFilter]);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const res = await adminService.getUsers({ 
        pageNumber, 
        pageSize, 
        searchTerm: keyword || undefined,
        role: roleFilter || undefined,
        status: statusFilter || undefined
      });
      if (res.data) {
        setUsers(res.data.items || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (err) {
      message.error(err.message || 'Lỗi khi tải danh sách người dùng');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    setPageNumber(1);
    fetchUsers();
  };

  const handleToggleStatus = async (userRecord, checked) => {
    try {
      await adminService.updateUserStatus(userRecord.id);
      message.success(`Đã ${checked ? 'bật hoạt động (mở khóa)' : 'tắt hoạt động (khóa)'} tài khoản ${userRecord.username}`);
      fetchUsers();
    } catch (err) {
      message.error(err.message || 'Thao tác cập nhật trạng thái thất bại');
    }
  };

  const handleOpenRoleModal = (userRecord) => {
    setSelectedUser(userRecord);
    setSelectedRoles(userRecord.roles || ['Member']);
    setRoleModalVisible(true);
  };

  const handleSaveRole = async () => {
    if (!selectedRoles || selectedRoles.length === 0) {
      message.warning('Người dùng phải có ít nhất 1 vai trò!');
      return;
    }
    setSavingRole(true);
    try {
      await adminService.updateUserRoles(selectedUser.id, selectedRoles);
      message.success(`Đã cập nhật vai trò cho người dùng ${selectedUser.username}!`);
      setRoleModalVisible(false);
      fetchUsers();
    } catch (err) {
      message.error(err.message || 'Thao tác cập nhật vai trò thất bại');
    } finally {
      setSavingRole(false);
    }
  };

  const handleResetPassword = async (userRecord) => {
    try {
      const res = await adminService.resetUserPassword(userRecord.id);
      const newPass = res.data || res.message;
      Modal.success({
        title: 'Reset Mật Khẩu Thành Công',
        content: (
          <div className="space-y-2 py-2">
            <p className="text-xs text-slate-600">Đã tạo mật khẩu tạm thời cho tài khoản <b>@{userRecord.username}</b>:</p>
            <div className="bg-slate-100 p-3 rounded-xl font-mono text-center font-bold text-base text-blue-600 border border-slate-200 select-all">
              {newPass}
            </div>
            <p className="text-[11px] text-slate-400">Vui lòng cung cấp mật khẩu này cho người dùng để đăng nhập lại.</p>
          </div>
        ),
      });
    } catch (err) {
      message.error(err.message || 'Reset mật khẩu thất bại');
    }
  };

  const handleDeleteUser = async (userRecord) => {
    try {
      await adminService.deleteUser(userRecord.id);
      message.success(`Đã xử lý xóa/khóa tài khoản ${userRecord.username}`);
      fetchUsers();
    } catch (err) {
      message.error(err.message || 'Xóa tài khoản thất bại');
    }
  };

  const handleOpenDetailDrawer = (userRecord) => {
    setDetailUser(userRecord);
    setDetailDrawerVisible(true);
  };

  // Quick stats calculation
  const userList = Array.isArray(users) ? users : [];
  const activeCount = userList.filter((u) => u && (String(u.status || '').toLowerCase() === 'active' || String(u.status) === '1')).length;
  const lockedCount = userList.filter((u) => u && String(u.status || '').toLowerCase() !== 'active' && String(u.status) !== '1').length;
  const staffCount = userList.filter((u) => u && (u.roles?.includes('Admin') || u.roles?.includes('Moderator'))).length;

  const columns = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 70,
      sorter: (a, b) => a.id - b.id,
    },
    {
      title: 'Người dùng',
      key: 'user',
      sorter: (a, b) => (a.fullName || a.username || '').localeCompare(b.fullName || b.username || ''),
      render: (_, r) => (
        <div className="flex items-center gap-3">
          <Avatar 
            src={formatAvatarUrl(r?.avatarUrl, r?.fullName || r?.username)} 
            icon={<UserOutlined />} 
            className="bg-blue-600 shrink-0 font-bold" 
          />
          <div>
            <div className="font-bold text-slate-900 text-xs hover:text-blue-600 cursor-pointer" onClick={() => handleOpenDetailDrawer(r)}>
              {r?.fullName || r?.username}
            </div>
            <div className="text-[11px] text-slate-400">@{r?.username} • {r?.email}</div>
          </div>
        </div>
      ),
    },
    {
      title: 'Vai trò',
      dataIndex: 'roles',
      key: 'roles',
      sorter: (a, b) => (a.roles?.[0] || '').localeCompare(b.roles?.[0] || ''),
      render: (roles) => (
        <Space size="small">
          {(roles || [])?.map((role) => (
            <Tag key={role} color={role === 'Admin' ? 'red' : role === 'Moderator' ? 'purple' : 'blue'} className="font-bold text-[10px] rounded-lg border-0 px-2 py-0.5">
              {role === 'Admin' ? 'Quản trị viên' : role === 'Moderator' ? 'Kiểm duyệt viên' : 'Thành viên'}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: 'Trạng thái',
      key: 'status',
      width: 140,
      sorter: (a, b) => String(a.status || '').localeCompare(String(b.status || '')),
      render: (_, r) => {
        const statusStr = String(r.status || '').toLowerCase();
        const isActive = statusStr === 'active' || statusStr === '1';
        return (
          <Switch
            checked={isActive}
            onChange={(checked) => handleToggleStatus(r, checked)}
            checkedChildren="Hoạt động"
            unCheckedChildren="Đã khóa"
          />
        );
      },
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      sorter: (a, b) => new Date(a.createdAt || 0) - new Date(b.createdAt || 0),
      render: (date) => <span className="text-xs text-slate-400">{dayjs(date).format('DD/MM/YYYY')}</span>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 100,
      render: (_, record) => {
        const menuItems = [
          {
            key: 'detail',
            icon: <EyeOutlined className="text-blue-600" />,
            label: 'Xem chi tiết hồ sơ',
            onClick: () => handleOpenDetailDrawer(record)
          },
          {
            key: 'role',
            icon: <EditOutlined className="text-purple-600" />,
            label: 'Đổi vai trò phân quyền',
            onClick: () => handleOpenRoleModal(record)
          },
          {
            key: 'reset',
            icon: <KeyOutlined className="text-amber-600" />,
            label: (
              <Popconfirm
                title="Reset mật khẩu?"
                description={`Tạo mật khẩu ngẫu nhiên mới cho @${record.username}?`}
                okText="Reset Mật Khẩu"
                cancelText="Hủy"
                onConfirm={() => handleResetPassword(record)}
              >
                <span>Reset mật khẩu</span>
              </Popconfirm>
            )
          },
          { type: 'divider' },
          {
            key: 'delete',
            danger: true,
            icon: <DeleteOutlined />,
            label: (
              <Popconfirm
                title="Xóa / Khóa vĩnh viễn?"
                description="Bạn có chắc chắn muốn xóa không?"
                okText="Xóa"
                cancelText="Hủy"
                okButtonProps={{ danger: true }}
                onConfirm={() => handleDeleteUser(record)}
              >
                <span>Xóa / Khóa tài khoản</span>
              </Popconfirm>
            )
          }
        ];

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
      
      {/* Top Sub-Header Bar with Left Search & Filters */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
          <Input
            placeholder="Tìm theo tên, email, username..."
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            onPressEnter={handleSearch}
            prefix={<SearchOutlined className="text-slate-400 text-xs" />}
            className="w-full sm:w-72 rounded-md text-xs bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 h-9"
            allowClear
          />

          <Select
            placeholder="Lọc theo vai trò"
            value={roleFilter}
            onChange={setRoleFilter}
            allowClear
            className="w-40 text-xs rounded-md h-9"
            options={[
              { value: 'Admin', label: 'Quản trị viên (Admin)' },
              { value: 'Moderator', label: 'Kiểm duyệt viên' },
              { value: 'Member', label: 'Thành viên' },
            ]}
          />

          <Select
            placeholder="Lọc theo trạng thái"
            value={statusFilter}
            onChange={setStatusFilter}
            allowClear
            className="w-40 text-xs rounded-md h-9"
            options={[
              { value: 'Active', label: 'Hoạt động' },
              { value: 'Inactive', label: 'Bị khóa' },
            ]}
          />
        </div>
      </div>

      {/* Main Table Container */}
      <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800">
        <Table
          dataSource={users}
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

      {/* Role Edit Modal */}
      <Modal
        title={
          <span className="font-bold flex items-center gap-2">
            <SafetyOutlined className="text-blue-600" /> Phân quyền vai trò: @{selectedUser?.username}
          </span>
        }
        open={roleModalVisible}
        onOk={handleSaveRole}
        onCancel={() => setRoleModalVisible(false)}
        confirmLoading={savingRole}
        okText="Lưu thay đổi"
        cancelText="Hủy"
        className="rounded-3xl"
      >
        <div className="py-4 space-y-3">
          <label className="text-xs font-bold text-slate-700 block">Chọn các vai trò cho tài khoản này:</label>
          <Select
            mode="multiple"
            placeholder="Chọn vai trò"
            value={selectedRoles}
            onChange={setSelectedRoles}
            className="w-full"
            options={[
              { value: 'Member', label: 'Member (Thành viên phổ thông)' },
              { value: 'Moderator', label: 'Moderator (Kiểm duyệt viên nội dung)' },
              { value: 'Admin', label: 'Admin (Quản trị viên toàn quyền)' },
            ]}
          />
        </div>
      </Modal>

      {/* User Detail Drawer */}
      <Drawer
        title={
          <div className="flex items-center gap-2">
            <UserOutlined className="text-blue-600" />
            <span className="font-bold text-sm">Hồ sơ Chi tiết Người dùng</span>
          </div>
        }
        placement="right"
        onClose={() => setDetailDrawerVisible(false)}
        open={detailDrawerVisible}
        width={380}
      >
        {detailUser && (
          <div className="space-y-6">
            <div className="text-center p-4 bg-slate-50 rounded-2xl border border-slate-100">
              <Avatar 
                size={72}
                src={formatAvatarUrl(detailUser.avatarUrl, detailUser.fullName || detailUser.username)} 
                icon={<UserOutlined />} 
                className="bg-blue-600 font-bold mb-2 shadow-md" 
              />
              <h3 className="font-bold text-slate-900 text-base m-0">{detailUser.fullName || detailUser.username}</h3>
              <p className="text-xs text-slate-400 m-0">@{detailUser.username}</p>
              
              <div className="flex justify-center gap-1.5 mt-2">
                {detailUser.roles?.map((r) => (
                  <Tag key={r} color={r === 'Admin' ? 'red' : r === 'Moderator' ? 'purple' : 'blue'} className="font-bold text-[10px] rounded-lg">
                    {r}
                  </Tag>
                ))}
              </div>
            </div>

            <div className="space-y-3 text-xs">
              <div className="flex justify-between py-2 border-b border-slate-100">
                <span className="text-slate-500 font-medium">Email:</span>
                <span className="font-bold text-slate-800">{detailUser.email}</span>
              </div>
              <div className="flex justify-between py-2 border-b border-slate-100">
                <span className="text-slate-500 font-medium">Trạng thái:</span>
                <Tag color={String(detailUser.status).toLowerCase() === 'active' ? 'green' : 'red'} className="font-bold text-[10px]">
                  {String(detailUser.status).toLowerCase() === 'active' ? 'Đang hoạt động' : 'Đã khóa'}
                </Tag>
              </div>
              <div className="flex justify-between py-2 border-b border-slate-100">
                <span className="text-slate-500 font-medium">Ngày tham gia:</span>
                <span className="font-bold text-slate-800">{dayjs(detailUser.createdAt).format('DD/MM/YYYY HH:mm')}</span>
              </div>
            </div>

            <div className="pt-2 flex flex-col gap-2">
              <Button 
                icon={<KeyOutlined />} 
                onClick={() => { setDetailDrawerVisible(false); handleResetPassword(detailUser); }}
                className="rounded-xl font-bold text-xs text-amber-600 border-amber-200"
              >
                Reset Mật Khẩu
              </Button>
              <Button 
                danger 
                icon={<DeleteOutlined />} 
                onClick={() => { setDetailDrawerVisible(false); handleDeleteUser(detailUser); }}
                className="rounded-xl font-bold text-xs"
              >
                Xóa / Khóa vĩnh viễn
              </Button>
            </div>
          </div>
        )}
      </Drawer>

    </div>
  );
};

export default AdminUsersPage;
