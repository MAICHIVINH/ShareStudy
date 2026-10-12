import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Tabs, Avatar, Tag, Table, Button, Form, Input, Upload, Modal, Slider, message, Skeleton, Empty } from 'antd';
import { 
  UserOutlined, 
  CloudUploadOutlined, 
  HeartOutlined, 
  FlagOutlined, 
  EditOutlined,
  EyeOutlined,
  DownloadOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  EyeInvisibleOutlined,
  MailOutlined,
  IdcardOutlined,
  PictureOutlined,
  SaveOutlined,
  UploadOutlined,
  CameraOutlined,
  ScissorOutlined,
  ZoomInOutlined,
  ZoomOutOutlined,
  DragOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { useAuth } from '../../contexts/AuthContext';
import { documentService } from '../../services/documentService';
import { favoriteService } from '../../services/favoriteService';
import { reportService } from '../../services/reportService';
import { userService } from '../../services/userService';
import { DocumentCard } from '../../components/documents/DocumentCard';
import { formatAvatarUrl } from '../../utils/avatarUtils';

export const ProfilePage = () => {
  const { user, setUser } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const activeTab = searchParams.get('tab') || 'uploads';

  // Tab Data States
  const [myUploads, setMyUploads] = useState([]);
  const [favorites, setFavorites] = useState([]);
  const [reports, setReports] = useState([]);

  const [loadingUploads, setLoadingUploads] = useState(false);
  const [loadingFavorites, setLoadingFavorites] = useState(false);
  const [loadingReports, setLoadingReports] = useState(false);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  // Avatar & Cropper States
  const [previewAvatar, setPreviewAvatar] = useState(user?.avatarUrl);
  const [cropModalVisible, setCropModalVisible] = useState(false);
  const [rawImageSrc, setRawImageSrc] = useState(null);
  const [zoomScale, setZoomScale] = useState(1);
  const [cropPos, setCropPos] = useState({ x: 52, y: 52 }); // Centered in 280x280 box (280 - 176)/2 = 52

  const isDragging = useRef(false);
  const dragStart = useRef({ x: 0, y: 0 });
  const initialPos = useRef({ x: 52, y: 52 });

  const [form] = Form.useForm();

  useEffect(() => {
    if (user?.id) {
      fetchMyUploads();
      fetchMyFavorites();
      fetchMyReports();
    }
  }, [user?.id]);

  useEffect(() => {
    setPreviewAvatar(user?.avatarUrl);
    form.setFieldsValue({
      fullName: user?.fullName,
      bio: user?.bio,
      avatarUrl: user?.avatarUrl,
    });
  }, [user]);

  const fetchMyUploads = async () => {
    if (!user?.id) return;
    setLoadingUploads(true);
    try {
      const res = await documentService.getDocuments({ userId: user.id, pageSize: 50 });
      if (res.data?.items) setMyUploads(res.data.items);
      else setMyUploads([]);
    } catch (err) {
      console.error(err);
      setMyUploads([]);
    } finally {
      setLoadingUploads(false);
    }
  };

  const fetchMyFavorites = async () => {
    setLoadingFavorites(true);
    try {
      const res = await favoriteService.getMyFavorites({ pageSize: 50 });
      if (res.data?.items) setFavorites(res.data.items);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingFavorites(false);
    }
  };

  const fetchMyReports = async () => {
    setLoadingReports(true);
    try {
      const res = await reportService.getMyReports({ pageSize: 50 });
      if (res.data?.items) setReports(res.data.items);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingReports(false);
    }
  };

  // Avatar Image File Selector Handler -> Opens Cropper Modal
  const handleAvatarFileUpload = (file) => {
    const isImage = file.type.startsWith('image/');
    if (!isImage) {
      message.error('Vui lòng chọn file hình ảnh (JPG, PNG, WEBP, GIF)!');
      return Upload.LIST_IGNORE;
    }
    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      message.error('Dung lượng ảnh đại diện tối đa là 10MB!');
      return Upload.LIST_IGNORE;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      setRawImageSrc(e.target.result);
      setZoomScale(1);
      setCropPos({ x: 52, y: 52 }); // Reset position to center
      setCropModalVisible(true);
    };
    reader.readAsDataURL(file);
    return false; // Prevent automatic upload
  };

  // Dragging crop box mouse/touch handlers
  const handleMouseDown = (e) => {
    isDragging.current = true;
    const clientX = e.touches ? e.touches[0].clientX : e.clientX;
    const clientY = e.touches ? e.touches[0].clientY : e.clientY;
    dragStart.current = { x: clientX, y: clientY };
    initialPos.current = { ...cropPos };
  };

  const handleMouseMove = (e) => {
    if (!isDragging.current) return;
    const clientX = e.touches ? e.touches[0].clientX : e.clientX;
    const clientY = e.touches ? e.touches[0].clientY : e.clientY;
    const dx = clientX - dragStart.current.x;
    const dy = clientY - dragStart.current.y;

    // Boundaries for 280px container and 176px crop box (280 - 176 = 104px max)
    const newX = Math.max(0, Math.min(104, initialPos.current.x + dx));
    const newY = Math.max(0, Math.min(104, initialPos.current.y + dy));

    setCropPos({ x: newX, y: newY });
  };

  const handleMouseUp = () => {
    isDragging.current = false;
  };

  // Apply Crop using HTML5 Canvas & Upload to MinIO
  const handleApplyCrop = () => {
    if (!rawImageSrc) return;

    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.src = rawImageSrc;
    img.onload = () => {
      const canvas = document.createElement('canvas');
      const outputSize = 300; // 300x300px avatar output
      canvas.width = outputSize;
      canvas.height = outputSize;
      const ctx = canvas.getContext('2d');

      const containerSize = 280;
      const cropBoxSize = 176;

      // Crop coordinates calculation
      const naturalCropW = (cropBoxSize / zoomScale) * (img.width / containerSize);
      const naturalCropH = (cropBoxSize / zoomScale) * (img.height / containerSize);

      const srcX = (cropPos.x / containerSize) * img.width;
      const srcY = (cropPos.y / containerSize) * img.height;

      ctx.drawImage(img, srcX, srcY, naturalCropW, naturalCropH, 0, 0, outputSize, outputSize);

      canvas.toBlob(async (blob) => {
        if (!blob) return;
        setUploadingAvatar(true);
        message.loading({ content: 'Đang lưu ảnh đại diện lên MinIO...', key: 'avatarMinio' });
        try {
          const res = await userService.uploadAvatar(blob);
          if (res.data?.avatarUrl) {
            const minioUrl = res.data.avatarUrl;
            setPreviewAvatar(minioUrl);
            form.setFieldsValue({ avatarUrl: minioUrl });
            message.success({ content: 'Đã lưu ảnh đại diện lên MinIO thành công!', key: 'avatarMinio' });
          }
        } catch (err) {
          console.error('Lỗi tải ảnh lên MinIO:', err);
          message.error({ content: 'Tải ảnh lên MinIO thất bại: ' + (err.response?.data?.message || err.message), key: 'avatarMinio' });
        } finally {
          setUploadingAvatar(false);
          setCropModalVisible(false);
        }
      }, 'image/jpeg', 0.90);
    };
  };

  const renderStatusTag = (status) => {
    const s = String(status).toLowerCase();
    if (s === 'approved' || s === '2') {
      return <Tag icon={<CheckCircleOutlined />} color="success">Đã duyệt</Tag>;
    }
    if (s === 'pending' || s === '1') {
      return <Tag icon={<ClockCircleOutlined />} color="warning">Đang chờ duyệt</Tag>;
    }
    if (s === 'rejected' || s === '3') {
      return <Tag icon={<CloseCircleOutlined />} color="error">Từ chối</Tag>;
    }
    if (s === 'hidden' || s === '4') {
      return <Tag icon={<EyeInvisibleOutlined />} color="default">Đang ẩn</Tag>;
    }
    return <Tag color="blue">{status}</Tag>;
  };

  const handleTabChange = (key) => {
    setSearchParams({ tab: key });
  };

  const handleUpdateProfile = async (values) => {
    try {
      message.loading({ content: 'Đang cập nhật thông tin tài khoản...', key: 'updateProfile' });
      const res = await userService.updateProfile({
        fullName: values.fullName,
        bio: values.bio,
        avatarUrl: values.avatarUrl,
      });

      if (res.data) {
        setUser(res.data);
        localStorage.setItem('user', JSON.stringify(res.data));
        message.success({ content: 'Cập nhật thông tin tài khoản và Avatar thành công!', key: 'updateProfile' });
      }
    } catch (err) {
      console.error('Cập nhật thông tin thất bại:', err);
      message.error({ content: 'Cập nhật thất bại: ' + (err.response?.data?.message || err.message), key: 'updateProfile' });
    }
  };

  const uploadColumns = [
    {
      title: 'Tài liệu',
      dataIndex: 'title',
      key: 'title',
      render: (text, record) => (
        <Link to={`/documents/${record.slug}`} className="font-bold text-slate-800 hover:text-blue-600 no-underline text-xs">
          {text}
        </Link>
      ),
    },
    {
      title: 'Loại',
      dataIndex: 'documentTypeName',
      key: 'documentTypeName',
      render: (text) => <span className="text-xs text-slate-500">{text || 'Tài liệu'}</span>,
    },
    {
      title: 'Ngày đăng',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => <span className="text-xs text-slate-400">{dayjs(date).format('DD/MM/YYYY')}</span>,
    },
    {
      title: 'Lượt xem / Tải',
      key: 'stats',
      render: (_, r) => (
        <span className="text-xs text-slate-500 font-medium">
          <EyeOutlined className="mr-1" />{r.viewCount || 0} • <DownloadOutlined className="mr-1" />{r.downloadCount || 0}
        </span>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status) => renderStatusTag(status),
    },
  ];

  const reportColumns = [
    {
      title: 'Tài liệu báo cáo',
      dataIndex: 'documentTitle',
      key: 'documentTitle',
      render: (text) => <span className="font-bold text-xs text-slate-800">{text}</span>,
    },
    {
      title: 'Lý do',
      dataIndex: 'reason',
      key: 'reason',
      render: (text) => <span className="text-xs font-medium text-red-500">{text}</span>,
    },
    {
      title: 'Ngày gửi',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => <span className="text-xs text-slate-400">{dayjs(date).format('DD/MM/YYYY')}</span>,
    },
    {
      title: 'Trạng thái xử lý',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        const s = String(status).toLowerCase();
        if (s === 'resolved') return <Tag color="success">Đã xử lý</Tag>;
        if (s === 'dismissed') return <Tag color="default">Bác bỏ</Tag>;
        return <Tag color="processing">Đang chờ</Tag>;
      },
    },
  ];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      
      {/* Profile User Banner Header */}
      <div className="bg-white dark:bg-slate-900 p-6 sm:p-8 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex flex-col sm:flex-row items-center sm:items-start gap-6 transition-colors">
        <Avatar 
          src={formatAvatarUrl(user?.avatarUrl, user?.fullName || user?.username)} 
          icon={<UserOutlined />} 
          size={80} 
          className="bg-blue-600 text-white shadow-md border-2 border-white dark:border-slate-800 shrink-0"
        />
        <div className="text-center sm:text-left space-y-2 flex-1">
          <div className="flex flex-col sm:flex-row items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900 dark:text-white m-0 tracking-tight">
              {user?.fullName || user?.username}
            </h1>
            <div className="flex gap-1.5">
              {user?.roles?.map((r) => (
                <Tag key={r} color={r === 'Admin' ? 'red' : r === 'Moderator' ? 'purple' : 'blue'}>
                  {r}
                </Tag>
              ))}
            </div>
          </div>
          <p className="text-xs text-slate-500 dark:text-slate-400 m-0">@{user?.username} • {user?.email}</p>
          {user?.bio && <p className="text-xs text-slate-600 dark:text-slate-300 italic m-0">{user.bio}</p>}
        </div>

        <Link to="/upload">
          <Button type="primary" icon={<CloudUploadOutlined />} className="bg-blue-600 hover:bg-blue-700 font-bold rounded-2xl h-11 px-6 shadow-md shadow-blue-500/20">
            Đăng tài liệu
          </Button>
        </Link>
      </div>

      {/* Main Tabs */}
      <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm">
        <Tabs
          activeKey={activeTab}
          onChange={handleTabChange}
          items={[
            {
              key: 'uploads',
              label: (
                <span className="font-semibold text-xs flex items-center gap-1">
                  <CloudUploadOutlined /> Tài liệu đã đăng ({myUploads.length})
                </span>
              ),
              children: loadingUploads ? (
                <Skeleton active paragraph={{ rows: 5 }} />
              ) : (
                <Table
                  dataSource={myUploads}
                  columns={uploadColumns}
                  rowKey="id"
                  pagination={{ pageSize: 10 }}
                  locale={{ emptyText: <Empty description="Bạn chưa đăng tài liệu nào" /> }}
                />
              ),
            },
            {
              key: 'favorites',
              label: (
                <span className="font-semibold text-xs flex items-center gap-1">
                  <HeartOutlined /> Tài liệu yêu thích ({favorites.length})
                </span>
              ),
              children: loadingFavorites ? (
                <Skeleton active paragraph={{ rows: 5 }} />
              ) : favorites.length === 0 ? (
                <Empty description="Chưa có tài liệu yêu thích" />
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 pt-4">
                  {favorites.map((item) => {
                    const docData = item.document 
                      ? { ...item.document, isFavorite: true } 
                      : { ...item, isFavorite: true };
                    return (
                      <DocumentCard 
                        key={item.id || item.documentId} 
                        document={docData} 
                        onFavoriteToggle={() => fetchMyFavorites()}
                      />
                    );
                  })}
                </div>
              ),
            },
            {
              key: 'reports',
              label: (
                <span className="font-semibold text-xs flex items-center gap-1">
                  <FlagOutlined /> Báo cáo của tôi ({reports.length})
                </span>
              ),
              children: loadingReports ? (
                <Skeleton active paragraph={{ rows: 5 }} />
              ) : (
                <Table
                  dataSource={reports}
                  columns={reportColumns}
                  rowKey="id"
                  pagination={{ pageSize: 10 }}
                  locale={{ emptyText: <Empty description="Bạn chưa gửi báo cáo nào" /> }}
                />
              ),
            },
            {
              key: 'settings',
              label: (
                <span className="font-semibold text-xs flex items-center gap-1">
                  <EditOutlined /> Cập nhật thông tin
                </span>
              ),
              children: (
                <div className="pt-4">
                  <Form
                    form={form}
                    layout="vertical"
                    initialValues={{
                      fullName: user?.fullName,
                      bio: user?.bio,
                      avatarUrl: user?.avatarUrl,
                    }}
                    onFinish={handleUpdateProfile}
                  >
                    <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
                      
                      {/* Left Column: Live Avatar Preview Card */}
                      <div className="lg:col-span-4 bg-slate-50 dark:bg-slate-800/50 p-6 rounded-2xl border border-slate-200 dark:border-slate-700 flex flex-col items-center text-center space-y-4">
                        <div className="text-xs font-bold text-slate-500 uppercase tracking-wider self-start">
                          Ảnh đại diện
                        </div>

                        <div className="relative group my-2">
                          <Avatar 
                            src={formatAvatarUrl(previewAvatar || user?.avatarUrl, user?.fullName || user?.username)} 
                            icon={<UserOutlined />} 
                            size={110} 
                            className="bg-blue-600 text-white shadow-lg border-4 border-white dark:border-slate-700"
                          />
                          <Upload showUploadList={false} beforeUpload={handleAvatarFileUpload}>
                            <div className="absolute inset-0 bg-slate-900/60 rounded-full flex flex-col items-center justify-center text-white opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer text-xs font-bold gap-1">
                              <CameraOutlined className="text-lg" />
                              <span>Cắt & Đổi ảnh</span>
                            </div>
                          </Upload>
                        </div>

                        <Upload showUploadList={false} beforeUpload={handleAvatarFileUpload}>
                          <Button 
                            icon={<UploadOutlined />} 
                            loading={uploadingAvatar}
                            className="rounded-xl text-xs font-bold border-blue-200 text-blue-600 hover:bg-blue-50"
                          >
                            Tải ảnh từ máy tính
                          </Button>
                        </Upload>

                        <div>
                          <div className="text-sm font-bold text-slate-900 dark:text-white">
                            {user?.fullName || user?.username}
                          </div>
                          <div className="text-xs text-slate-500 mt-0.5">
                            @{user?.username}
                          </div>
                        </div>

                        <div className="w-full pt-4 border-t border-slate-200 dark:border-slate-700 space-y-2.5 text-left">
                          <div className="flex items-center justify-between text-xs text-slate-600 dark:text-slate-400">
                            <span>Vai trò tài khoản:</span>
                            <span className="font-semibold text-blue-600">
                              {user?.roles?.join(', ') || 'Member'}
                            </span>
                          </div>
                          <div className="flex items-center justify-between text-xs text-slate-600 dark:text-slate-400">
                            <span>Trạng thái:</span>
                            <Tag color="success" className="m-0 text-[10px] font-bold">Hoạt động</Tag>
                          </div>
                        </div>
                      </div>

                      {/* Right Column: Information Form */}
                      <div className="lg:col-span-8 space-y-4">
                        <div className="text-xs font-bold text-slate-800 dark:text-slate-200 uppercase tracking-wider block">
                          Thông tin cá nhân
                        </div>

                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                          <Form.Item label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Tên tài khoản (Username)</span>}>
                            <Input 
                              prefix={<IdcardOutlined className="text-slate-400" />} 
                              value={user?.username} 
                              disabled 
                              className="rounded-xl h-11 text-xs bg-slate-100 dark:bg-slate-800" 
                            />
                          </Form.Item>

                          <Form.Item label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Email liên hệ</span>}>
                            <Input 
                              prefix={<MailOutlined className="text-slate-400" />} 
                              value={user?.email} 
                              disabled 
                              className="rounded-xl h-11 text-xs bg-slate-100 dark:bg-slate-800" 
                            />
                          </Form.Item>
                        </div>

                        <Form.Item 
                          label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Họ và tên <span className="text-red-500">*</span></span>} 
                          name="fullName"
                          rules={[{ required: true, message: 'Vui lòng nhập họ và tên!' }]}
                        >
                          <Input 
                            prefix={<UserOutlined className="text-slate-400" />} 
                            placeholder="Nhập họ và tên đầy đủ" 
                            className="rounded-xl h-11 text-xs" 
                          />
                        </Form.Item>

                        <Form.Item 
                          label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Ảnh đại diện (Avatar)</span>} 
                          name="avatarUrl"
                        >
                          <div className="flex gap-2">
                            <Input 
                              prefix={<PictureOutlined className="text-slate-400" />} 
                              placeholder="Hoặc dán URL ảnh đại diện" 
                              onChange={(e) => setPreviewAvatar(e.target.value)}
                              className="rounded-xl h-11 text-xs flex-1" 
                            />
                            <Upload showUploadList={false} beforeUpload={handleAvatarFileUpload}>
                              <Button 
                                icon={<UploadOutlined />} 
                                loading={uploadingAvatar}
                                className="rounded-xl h-11 text-xs font-bold border-slate-200 hover:text-blue-600"
                              >
                                Chọn ảnh từ máy
                              </Button>
                            </Upload>
                          </div>
                        </Form.Item>

                        <Form.Item 
                          label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Giới thiệu bản thân (Bio)</span>} 
                          name="bio"
                        >
                          <Input.TextArea 
                            rows={3} 
                            maxLength={200}
                            showCount
                            placeholder="VD: Sinh viên ngành Công nghệ thông tin - ĐH Bách Khoa..." 
                            className="rounded-xl text-xs" 
                          />
                        </Form.Item>

                        <div className="pt-4 border-t border-slate-100 dark:border-slate-800 flex justify-end">
                          <Button 
                            type="primary" 
                            htmlType="submit" 
                            icon={<SaveOutlined />}
                            className="bg-blue-600 hover:bg-blue-700 font-bold rounded-2xl h-11 px-8 text-xs shadow-md shadow-blue-500/20"
                          >
                            LƯU THAY ĐỔI
                          </Button>
                        </div>
                      </div>

                    </div>
                  </Form>
                </div>
              ),
            },
          ]}
        />
      </div>

      {/* Interactive Draggable Square Image Cropper Modal */}
      <Modal
        title={
          <span className="font-bold flex items-center gap-2 text-slate-800">
            <ScissorOutlined className="text-blue-600" /> Cắt & Di chuyển vùng ảnh đại diện
          </span>
        }
        open={cropModalVisible}
        onOk={handleApplyCrop}
        onCancel={() => setCropModalVisible(false)}
        okText="Lưu & Tải lên MinIO"
        cancelText="Hủy"
        width={480}
        confirmLoading={uploadingAvatar}
        okButtonProps={{ className: 'bg-blue-600 hover:bg-blue-700 rounded-xl font-bold' }}
        cancelButtonProps={{ className: 'rounded-xl' }}
      >
        <div className="py-4 flex flex-col items-center space-y-4 select-none">
          <p className="text-xs text-slate-500 text-center m-0 flex items-center gap-1">
            <DragOutlined className="text-blue-600" /> Nhấn giữ và kéo khung hình vuông để di chuyển vùng ảnh mong muốn
          </p>

          {/* Interactive Draggable Container (280x280px) */}
          <div 
            className="relative w-[280px] h-[280px] bg-slate-950 rounded-2xl overflow-hidden shadow-inner flex items-center justify-center cursor-crosshair touch-none"
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
            onMouseLeave={handleMouseUp}
            onTouchMove={handleMouseMove}
            onTouchEnd={handleMouseUp}
          >
            {/* Background Full Image */}
            {rawImageSrc && (
              <img 
                src={rawImageSrc} 
                alt="Original"
                style={{
                  transform: `scale(${zoomScale})`,
                  maxWidth: '100%',
                  maxHeight: '100%',
                  objectFit: 'contain',
                  pointerEvents: 'none'
                }}
              />
            )}

            {/* Movable & Draggable Square Crop Box (176x176px) */}
            <div 
              style={{
                position: 'absolute',
                left: `${cropPos.x}px`,
                top: `${cropPos.y}px`,
                width: '176px',
                height: '176px',
                boxShadow: '0 0 0 9999px rgba(15, 23, 42, 0.65)'
              }}
              onMouseDown={handleMouseDown}
              onTouchStart={handleMouseDown}
              className="border-2 border-blue-500 rounded-2xl cursor-grab active:cursor-grabbing flex items-center justify-center group"
            >
              {/* Corner Grips */}
              <div className="absolute -top-1 -left-1 w-3.5 h-3.5 bg-white border-2 border-blue-600 rounded-full shadow" />
              <div className="absolute -top-1 -right-1 w-3.5 h-3.5 bg-white border-2 border-blue-600 rounded-full shadow" />
              <div className="absolute -bottom-1 -left-1 w-3.5 h-3.5 bg-white border-2 border-blue-600 rounded-full shadow" />
              <div className="absolute -bottom-1 -right-1 w-3.5 h-3.5 bg-white border-2 border-blue-600 rounded-full shadow" />

              {/* Drag Hint Icon */}
              <div className="bg-blue-600/80 text-white rounded-full p-2 text-sm shadow-md group-hover:scale-110 transition-transform">
                <DragOutlined />
              </div>
            </div>

          </div>

          {/* Zoom Control Slider */}
          <div className="w-full px-6 flex items-center gap-3">
            <ZoomOutOutlined className="text-slate-400 text-sm" />
            <Slider
              min={1}
              max={3}
              step={0.1}
              value={zoomScale}
              onChange={(val) => setZoomScale(val)}
              className="flex-1"
            />
            <ZoomInOutlined className="text-blue-600 text-sm" />
          </div>
        </div>
      </Modal>

    </div>
  );
};

export default ProfilePage;
