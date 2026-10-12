import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Input, Button, message } from 'antd';
import { 
  FacebookFilled, 
  YoutubeFilled, 
  ArrowUpOutlined,
  SendOutlined
} from '@ant-design/icons';

export const Footer = () => {
  const [email, setEmail] = useState('');

  const handleSubscribe = (e) => {
    e.preventDefault();
    if (!email || !email.includes('@')) {
      message.error('Vui lòng nhập email hợp lệ!');
      return;
    }
    message.success('Đăng ký nhận bản tin thành công!');
    setEmail('');
  };

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <footer className="bg-white border-t border-slate-200 mt-16 text-slate-600 relative">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-8">
          
          {/* Brand Info */}
          <div className="lg:col-span-1">
            <Link to="/" className="flex items-center gap-3 no-underline">
              <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center text-white shadow-md shadow-blue-500/20">
                <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z" />
                </svg>
              </div>
              <div>
                <div className="text-xl font-bold text-slate-900 tracking-tight leading-none">
                  ShareStudy
                </div>
                <div className="text-xs text-slate-500 font-normal mt-0.5">
                  Chia sẻ tài liệu học tập
                </div>
              </div>
            </Link>
            
            <p className="text-xs text-slate-500 mt-4 leading-relaxed">
              Nền tảng chia sẻ tài liệu học tập miễn phí cho học sinh, sinh viên trên toàn quốc.
            </p>

            {/* Social Links */}
            <div className="flex items-center gap-3 mt-4">
              <a href="#" className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-blue-600 hover:text-white transition-colors">
                <FacebookFilled />
              </a>
              <a href="#" className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-blue-600 hover:text-white transition-colors">
                <SendOutlined />
              </a>
              <a href="#" className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-blue-600 hover:text-white transition-colors">
                <YoutubeFilled />
              </a>
              <a href="#" className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-600 hover:bg-blue-600 hover:text-white transition-colors">
                <span className="font-bold text-xs">TT</span>
              </a>
            </div>
          </div>

          {/* Col 2: Về chúng tôi */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-4">Về chúng tôi</h4>
            <ul className="space-y-2.5 text-xs text-slate-500 p-0 list-none">
              <li><Link to="/about" className="hover:text-blue-600 transition-colors no-underline">Giới thiệu</Link></li>
              <li><Link to="/terms" className="hover:text-blue-600 transition-colors no-underline">Quy định sử dụng</Link></li>
              <li><Link to="/privacy" className="hover:text-blue-600 transition-colors no-underline">Chính sách bảo mật</Link></li>
              <li><Link to="/contact" className="hover:text-blue-600 transition-colors no-underline">Liên hệ</Link></li>
            </ul>
          </div>

          {/* Col 3: Hỗ trợ */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-4">Hỗ trợ</h4>
            <ul className="space-y-2.5 text-xs text-slate-500 p-0 list-none">
              <li><Link to="/help" className="hover:text-blue-600 transition-colors no-underline">Trung tâm hỗ trợ</Link></li>
              <li><Link to="/guide" className="hover:text-blue-600 transition-colors no-underline">Hướng dẫn sử dụng</Link></li>
              <li><Link to="/faq" className="hover:text-blue-600 transition-colors no-underline">Câu hỏi thường gặp</Link></li>
              <li><Link to="/feedback" className="hover:text-blue-600 transition-colors no-underline">Góp ý & Báo lỗi</Link></li>
            </ul>
          </div>

          {/* Col 4: Cộng đồng */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-4">Cộng đồng</h4>
            <ul className="space-y-2.5 text-xs text-slate-500 p-0 list-none">
              <li><Link to="/blog" className="hover:text-blue-600 transition-colors no-underline">Blog</Link></li>
              <li><Link to="/forum" className="hover:text-blue-600 transition-colors no-underline">Diễn đàn</Link></li>
              <li><Link to="/groups" className="hover:text-blue-600 transition-colors no-underline">Nhóm học tập</Link></li>
              <li><Link to="/events" className="hover:text-blue-600 transition-colors no-underline">Sự kiện</Link></li>
            </ul>
          </div>

          {/* Col 5: Đăng ký nhận bản tin */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-4">Đăng ký nhận bản tin</h4>
            <p className="text-xs text-slate-500 mb-3 leading-relaxed">
              Nhận thông báo về tài liệu mới và các cập nhật mới nhất.
            </p>
            <form onSubmit={handleSubscribe} className="flex gap-1.5">
              <Input
                placeholder="Nhập email của bạn"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="rounded-xl text-xs"
              />
              <Button 
                type="primary" 
                htmlType="submit"
                className="bg-blue-600 hover:bg-blue-700 rounded-xl text-xs font-semibold px-4"
              >
                Đăng ký
              </Button>
            </form>
          </div>

        </div>

        {/* Bottom Bar */}
        <div className="border-t border-slate-100 mt-10 pt-6 flex justify-between items-center text-xs text-slate-500">
          <div>
            © 2024 ShareStudy. All rights reserved.
          </div>
        </div>

      </div>

      {/* Floating Back to Top Button */}
      <button
        onClick={scrollToTop}
        className="fixed bottom-6 right-6 w-10 h-10 rounded-full bg-blue-600 text-white flex items-center justify-center shadow-lg hover:bg-blue-700 transition-all border-0 cursor-pointer z-40"
        title="Về đầu trang"
      >
        <ArrowUpOutlined className="text-sm" />
      </button>

    </footer>
  );
};
