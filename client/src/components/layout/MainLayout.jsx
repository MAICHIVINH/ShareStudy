import React from 'react';
import { Header } from './Header';
import { Footer } from './Footer';
import { useSettings } from '../../contexts/SettingsContext';
import { useAuth } from '../../contexts/AuthContext';
import { Alert, Button } from 'antd';
import { ToolOutlined, LockOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';

export const MainLayout = ({ children }) => {
  const { settings } = useSettings();
  const { isAdmin } = useAuth();

  const isMaintenanceActive = settings?.maintenanceMode && !isAdmin;

  return (
    <div className="min-h-screen flex flex-col bg-[#F8FAFC]">
      <Header />

      {/* Maintenance Alert Notice Banner for Admin */}
      {settings?.maintenanceMode && isAdmin && (
        <div className="bg-amber-500 text-white text-xs py-2 px-4 text-center font-bold flex items-center justify-center gap-2 shadow-inner">
          <ToolOutlined /> Hệ thống đang bật Chế độ Bảo trì (Maintenance Mode). Bạn đang truy cập dưới quyền Quản trị viên Admin.
        </div>
      )}

      <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {isMaintenanceActive ? (
          <div className="max-w-xl mx-auto my-12 p-8 bg-white rounded-3xl border border-amber-200 shadow-xl text-center space-y-6">
            <div className="w-20 h-20 bg-amber-100 text-amber-600 rounded-3xl flex items-center justify-center mx-auto text-3xl shadow-inner">
              <ToolOutlined />
            </div>
            
            <div>
              <h2 className="text-2xl font-black text-slate-900 m-0">Hệ Thống Đang Bảo Trì</h2>
              <p className="text-xs text-slate-500 mt-2 leading-relaxed">
                Hệ thống <b>{settings?.siteName || 'ShareStudy'}</b> đang thực hiện nâng cấp hạ tầng định kỳ để nâng cao trải nghiệm. Vui lòng quay lại sau!
              </p>
            </div>

            <div className="p-4 bg-slate-50 rounded-2xl border border-slate-100 text-xs text-slate-600 font-medium">
              Ghi chú từ BQT: <b>{settings?.siteSlogan || 'Tri thức mở, kết nối cộng đồng học thuật'}</b>
            </div>

            <div className="pt-2">
              <Link to="/login">
                <Button icon={<LockOutlined />} className="rounded-xl text-xs font-bold">
                  Đăng nhập Quản trị viên
                </Button>
              </Link>
            </div>
          </div>
        ) : (
          children
        )}
      </main>
      <Footer />
    </div>
  );
};
