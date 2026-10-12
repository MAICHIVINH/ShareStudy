import React, { useState } from 'react';
import { Modal, Form, Input, Button, message } from 'antd';
import { 
  UserOutlined, 
  LockOutlined, 
  MailOutlined, 
  IdcardOutlined,
  BookOutlined,
  ArrowRightOutlined
} from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';

export const AuthModal = () => {
  const { authModalOpen, authModalMode, closeAuthModal, setAuthModalMode, login, register } = useAuth();
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  if (!authModalOpen) return null;

  const isLogin = authModalMode === 'login';

  const handleFinish = async (values) => {
    setSubmitting(true);
    try {
      if (isLogin) {
        await login(values.usernameOrEmail, values.password);
        message.success('Đăng nhập thành công!');
        closeAuthModal();
        form.resetFields();
      } else {
        if (values.password !== values.confirmPassword) {
          message.error('Mật khẩu xác nhận không trùng khớp!');
          setSubmitting(false);
          return;
        }
        await register({
          fullName: values.fullName?.trim(),
          username: values.username?.trim(),
          email: values.email?.trim(),
          password: values.password,
        });
        message.success('Đăng ký tài khoản thành công!');
        closeAuthModal();
        form.resetFields();
      }
    } catch (err) {
      console.error(err);
      message.error(err.message || (isLogin ? 'Đăng nhập thất bại!' : 'Đăng ký thất bại!'));
    } finally {
      setSubmitting(false);
    }
  };

  const switchMode = (mode) => {
    form.resetFields();
    setAuthModalMode(mode);
  };

  return (
    <Modal
      open={authModalOpen}
      onCancel={() => { closeAuthModal(); form.resetFields(); }}
      footer={null}
      destroyOnClose
      centered
      width={isLogin ? 420 : 540}
      className="auth-modal font-sans"
      modalRender={(modal) => (
        <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl overflow-hidden border border-slate-200 dark:border-slate-800">
          {modal}
        </div>
      )}
    >
      <div className="p-5 sm:p-6 space-y-4">
        
        {/* Header Branding */}
        <div className="text-center space-y-1">
          <div className="w-10 h-10 rounded-xl bg-blue-600 text-white flex items-center justify-center text-lg font-bold mx-auto shadow-md shadow-blue-500/20">
            <BookOutlined />
          </div>

          <h2 className="text-lg font-bold text-slate-900 dark:text-white m-0 tracking-tight">
            {isLogin ? 'Chào mừng quay trở lại!' : 'Tạo tài khoản ShareStudy'}
          </h2>
          <p className="text-[11px] text-slate-500 dark:text-slate-400 m-0">
            {isLogin 
              ? 'Đăng nhập để xem, tải xuống và đóng góp tài liệu' 
              : 'Đăng ký ngay để mở khóa toàn bộ tính năng học tập'}
          </p>
        </div>

        {/* Form */}
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
          requiredMark={false}
          className="space-y-3"
        >
          {isLogin ? (
            <div className="space-y-3">
              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Tên đăng nhập hoặc Email</span>}
                name="usernameOrEmail"
                rules={[{ required: true, message: 'Nhập Username hoặc Email!' }]}
                className="mb-0"
              >
                <Input 
                  prefix={<UserOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="admin / student@sharestudy.vn"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>

              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Mật khẩu</span>}
                name="password"
                rules={[{ required: true, message: 'Nhập mật khẩu!' }]}
                className="mb-0"
              >
                <Input.Password 
                  prefix={<LockOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="••••••••"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-3 gap-y-2">
              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Họ và tên</span>}
                name="fullName"
                rules={[{ required: true, message: 'Nhập họ tên!' }]}
                className="mb-0"
              >
                <Input 
                  prefix={<IdcardOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="Nguyễn Văn A"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>

              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Tên tài khoản (Username)</span>}
                name="username"
                rules={[{ required: true, message: 'Nhập Username!' }]}
                className="mb-0"
              >
                <Input 
                  prefix={<UserOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="nguyenvana"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>

              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Email</span>}
                name="email"
                rules={[
                  { required: true, message: 'Nhập email!' },
                  { type: 'email', message: 'Email không hợp lệ!' }
                ]}
                className="mb-0 sm:col-span-2"
              >
                <Input 
                  prefix={<MailOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="nguyenvana@gmail.com"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>

              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Mật khẩu</span>}
                name="password"
                rules={[
                  { required: true, message: 'Nhập mật khẩu!' },
                  { min: 6, message: 'Từ 6 ký tự trở lên!' }
                ]}
                className="mb-0"
              >
                <Input.Password 
                  prefix={<LockOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="••••••••"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>

              <Form.Item
                label={<span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Xác nhận Mật khẩu</span>}
                name="confirmPassword"
                rules={[{ required: true, message: 'Xác nhận mật khẩu!' }]}
                className="mb-0"
              >
                <Input.Password 
                  prefix={<LockOutlined className="text-slate-400 text-xs mr-1" />}
                  placeholder="••••••••"
                  className="rounded-lg h-9 text-xs"
                />
              </Form.Item>
            </div>
          )}

          <Button
            type="primary"
            htmlType="submit"
            loading={submitting}
            icon={<ArrowRightOutlined />}
            className="w-full bg-blue-600 hover:bg-blue-500 font-bold rounded-lg h-9 text-xs border-0 mt-3 shadow-md shadow-blue-500/20"
          >
            {isLogin ? 'ĐĂNG NHẬP' : 'ĐĂNG KÝ TÀI KHOẢN'}
          </Button>
        </Form>

        {/* Bottom Mode Switcher */}
        <div className="text-center border-t border-slate-100 dark:border-slate-800 pt-3 text-xs text-slate-500">
          {isLogin ? (
            <span>
              Chưa có tài khoản?{' '}
              <button
                type="button"
                onClick={() => switchMode('register')}
                className="text-blue-600 hover:underline font-bold bg-transparent border-0 cursor-pointer p-0"
              >
                Đăng ký ngay
              </button>
            </span>
          ) : (
            <span>
              Đã có tài khoản?{' '}
              <button
                type="button"
                onClick={() => switchMode('login')}
                className="text-blue-600 hover:underline font-bold bg-transparent border-0 cursor-pointer p-0"
              >
                Đăng nhập ngay
              </button>
            </span>
          )}
        </div>

      </div>
    </Modal>
  );
};

export default AuthModal;
