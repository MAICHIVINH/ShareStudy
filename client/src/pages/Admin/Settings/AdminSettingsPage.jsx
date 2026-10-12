import React, { useState, useEffect } from 'react';
import { Form, Input, Switch, Button, Select, InputNumber, Tabs, message, Divider } from 'antd';
import { 
  SettingOutlined, 
  SaveOutlined, 
  SafetyOutlined, 
  CloudUploadOutlined, 
  MailOutlined, 
  GlobalOutlined,
  ToolOutlined,
  DatabaseOutlined,
  SearchOutlined,
  PlusOutlined
} from '@ant-design/icons';

import { adminService } from '../../../services/adminService';

export const AdminSettingsPage = () => {
  const [form] = Form.useForm();
  const [saving, setSaving] = useState(false);

  const initialSettings = {
    siteName: 'ShareStudy - Hệ thống Chia sẻ Tài liệu Học tập',
    siteSlogan: 'Tri thức mở, kết nối cộng đồng học thuật Việt Nam',
    contactEmail: 'support@sharestudy.vn',
    maintenanceMode: false,
    
    // Auth & Security
    allowRegistration: true,
    requireEmailVerification: true,
    maxUploadMB: 50,
    allowedFileExtensions: ['pdf', 'docx', 'pptx', 'xlsx', 'zip'],

    // Notifications
    notifyOnDocumentApproval: true,
    notifyOnNewReport: true,
    smtpHost: 'smtp.gmail.com',
    smtpPort: 587,
    smtpEmail: 'no-reply@sharestudy.vn',

    // Storage
    storageProvider: 'Local',
    storageBasePath: './wwwroot/uploads',
  };

  useEffect(() => {
    fetchSettings();
  }, []);

  const fetchSettings = async () => {
    try {
      const res = await adminService.getSettings();
      if (res.data) {
        form.setFieldsValue(res.data);
      }
    } catch (err) {
      console.log('Using default settings view');
    }
  };

  const handleSaveSettings = async (values) => {
    setSaving(true);
    try {
      await adminService.updateSettings(values);
      message.success({
        content: 'Đã lưu cấu hình hệ thống thành công!',
        className: 'font-semibold text-xs',
      });
      fetchSettings();
    } catch (err) {
      message.error(err.message || 'Lỗi khi lưu cài đặt hệ thống');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="w-full space-y-4">
      
      {/* Top Sub-Header Bar (Matching Screenshot Action Bar) */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <Input
          prefix={<SearchOutlined className="text-slate-400 text-xs" />}
          placeholder="Tìm theo mã cài đặt, từ khóa cấu hình..."
          className="w-full sm:w-80 rounded-md text-xs bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 h-9 shadow-2xs"
        />

        <Button
          type="primary"
          icon={<SaveOutlined />}
          onClick={() => form.submit()}
          loading={saving}
          className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-5 text-xs shadow-2xs border-0 ml-auto"
        >
          Lưu cài đặt hệ thống
        </Button>
      </div>

      {/* Main Content Card (Matching Screenshot Clean Card) */}
      <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800">
        <Form
          form={form}
          layout="vertical"
          initialValues={initialSettings}
          onFinish={handleSaveSettings}
          className="w-full"
        >
          <Tabs
            defaultActiveKey="general"
            items={[
              {
                key: 'general',
                label: (
                  <span className="font-semibold text-xs flex items-center gap-1.5 py-0.5">
                    <GlobalOutlined /> Cấu hình chung
                  </span>
                ),
                children: (
                  <div className="pt-4 space-y-5 w-full">
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 w-full">
                      <Form.Item
                        label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Tên hệ thống (Site Name) <span className="text-red-500">*</span></span>}
                        name="siteName"
                        rules={[{ required: true, message: 'Không được để trống tên hệ thống!' }]}
                        className="w-full"
                      >
                        <Input className="rounded-md h-9 text-xs w-full" placeholder="ShareStudy" />
                      </Form.Item>

                      <Form.Item 
                        label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Khẩu hiệu / Slogan</span>} 
                        name="siteSlogan"
                        className="w-full"
                      >
                        <Input className="rounded-md h-9 text-xs w-full" placeholder="Tri thức mở, kết nối cộng đồng học thuật Việt Nam" />
                      </Form.Item>
                    </div>

                    <Form.Item
                      label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Email quản trị liên hệ <span className="text-red-500">*</span></span>}
                      name="contactEmail"
                      rules={[{ required: true, type: 'email', message: 'Email không hợp lệ!' }]}
                      className="w-full"
                    >
                      <Input prefix={<MailOutlined className="text-slate-400 mr-1" />} className="rounded-md h-9 text-xs w-full" placeholder="support@sharestudy.vn" />
                    </Form.Item>

                    <Divider className="my-2" />

                    <div className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-md border border-slate-200 dark:border-slate-700 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 w-full">
                      <div>
                        <div className="font-bold text-xs text-slate-800 dark:text-slate-200 flex items-center gap-2">
                          <ToolOutlined /> Chế độ bảo trì hệ thống (Maintenance Mode)
                        </div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          Tạm thời dừng truy cập của người dùng để nâng cấp hạ tầng.
                        </div>
                      </div>

                      <Form.Item name="maintenanceMode" valuePropName="checked" className="m-0 shrink-0">
                        <Switch checkedChildren="BẬT" unCheckedChildren="TẮT" />
                      </Form.Item>
                    </div>

                    <div className="pt-2 flex justify-end w-full">
                      <Button 
                        type="primary" 
                        htmlType="submit" 
                        icon={<SaveOutlined />}
                        loading={saving}
                        className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-6 text-xs"
                      >
                        Lưu cài đặt
                      </Button>
                    </div>

                  </div>
                ),
              },
              {
                key: 'security',
                label: (
                  <span className="font-semibold text-xs flex items-center gap-1.5 py-0.5">
                    <SafetyOutlined /> Bảo mật & Đăng ký
                  </span>
                ),
                children: (
                  <div className="pt-4 space-y-5 w-full">
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 w-full">
                      <div className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-md border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-4 h-full">
                        <div>
                          <div className="font-semibold text-xs text-slate-800 dark:text-slate-200">Cho phép đăng ký tài khoản mới</div>
                          <div className="text-[11px] text-slate-400 mt-0.5">Mở cổng cho người dùng tự tạo tài khoản.</div>
                        </div>
                        <Form.Item name="allowRegistration" valuePropName="checked" className="m-0 shrink-0">
                          <Switch checkedChildren="Mở" unCheckedChildren="Khóa" />
                        </Form.Item>
                      </div>

                      <div className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-md border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-4 h-full">
                        <div>
                          <div className="font-semibold text-xs text-slate-800 dark:text-slate-200">Yêu cầu xác thực Email</div>
                          <div className="text-[11px] text-slate-400 mt-0.5">Xác thực email trước khi được phép tải file.</div>
                        </div>
                        <Form.Item name="requireEmailVerification" valuePropName="checked" className="m-0 shrink-0">
                          <Switch checkedChildren="Có" unCheckedChildren="Không" />
                        </Form.Item>
                      </div>
                    </div>

                    <Divider className="my-2" />

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 w-full">
                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Dung lượng tải lên tối đa (MB)</span>} name="maxUploadMB" className="w-full">
                        <InputNumber min={5} max={500} className="w-full rounded-md h-9 text-xs flex items-center" addonAfter="MB" />
                      </Form.Item>

                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Định dạng file cho phép</span>} name="allowedFileExtensions" className="w-full">
                        <Select
                          mode="tags"
                          placeholder="pdf, docx, pptx..."
                          className="w-full text-xs"
                          options={[
                            { value: 'pdf', label: 'PDF (.pdf)' },
                            { value: 'docx', label: 'Word (.docx)' },
                            { value: 'pptx', label: 'PowerPoint (.pptx)' },
                            { value: 'xlsx', label: 'Excel (.xlsx)' },
                            { value: 'zip', label: 'Nén (.zip)' },
                          ]}
                        />
                      </Form.Item>
                    </div>

                    <div className="pt-2 flex justify-end w-full">
                      <Button 
                        type="primary" 
                        htmlType="submit" 
                        icon={<SaveOutlined />}
                        loading={saving}
                        className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-6 text-xs"
                      >
                        Lưu cài đặt
                      </Button>
                    </div>

                  </div>
                ),
              },
              {
                key: 'notifications',
                label: (
                  <span className="font-semibold text-xs flex items-center gap-1.5 py-0.5">
                    <MailOutlined /> Thông báo & Email
                  </span>
                ),
                children: (
                  <div className="pt-4 space-y-5 w-full">
                    
                    <div className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-md border border-slate-200 dark:border-slate-700 flex items-center justify-between gap-4 w-full">
                      <div>
                        <div className="font-semibold text-xs text-slate-800 dark:text-slate-200">Gửi Email khi phê duyệt bài đăng</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">Tự động báo tin cho người đăng khi tài liệu được chấp nhận.</div>
                      </div>
                      <Form.Item name="notifyOnDocumentApproval" valuePropName="checked" className="m-0 shrink-0">
                        <Switch checkedChildren="BẬT" unCheckedChildren="TẮT" />
                      </Form.Item>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 w-full">
                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">SMTP Host Server</span>} name="smtpHost" className="w-full">
                        <Input className="rounded-md h-9 text-xs font-mono w-full" placeholder="smtp.gmail.com" />
                      </Form.Item>
                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">SMTP Port</span>} name="smtpPort" className="w-full">
                        <InputNumber className="w-full rounded-md h-9 text-xs font-mono flex items-center" placeholder="587" />
                      </Form.Item>
                    </div>

                    <div className="pt-2 flex justify-end w-full">
                      <Button 
                        type="primary" 
                        htmlType="submit" 
                        icon={<SaveOutlined />}
                        loading={saving}
                        className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-6 text-xs"
                      >
                        Lưu cài đặt
                      </Button>
                    </div>

                  </div>
                ),
              },
              {
                key: 'storage',
                label: (
                  <span className="font-semibold text-xs flex items-center gap-1.5 py-0.5">
                    <DatabaseOutlined /> Lưu trữ Storage
                  </span>
                ),
                children: (
                  <div className="pt-4 space-y-5 w-full">
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 w-full">
                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Phương thức lưu trữ tài liệu</span>} name="storageProvider" className="w-full">
                        <Select
                          className="h-9 text-xs w-full"
                          options={[
                            { value: 'Local', label: 'Lưu tại Server Cục bộ (Local Storage)' },
                            { value: 'MinIO', label: 'Lưu tại Cloud MinIO / AWS S3' },
                          ]}
                        />
                      </Form.Item>

                      <Form.Item label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Đường dẫn thư mục lưu file</span>} name="storageBasePath" className="w-full">
                        <Input className="rounded-md h-9 text-xs font-mono w-full" />
                      </Form.Item>
                    </div>

                    <div className="pt-2 flex justify-end w-full">
                      <Button 
                        type="primary" 
                        htmlType="submit" 
                        icon={<SaveOutlined />}
                        loading={saving}
                        className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-6 text-xs"
                      >
                        Lưu cài đặt
                      </Button>
                    </div>

                  </div>
                ),
              },
            ]}
          />
        </Form>
      </div>

    </div>
  );
};

export default AdminSettingsPage;
