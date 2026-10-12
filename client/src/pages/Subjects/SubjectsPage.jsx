import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input, Skeleton, Empty, Pagination, Tag, Button } from 'antd';
import { 
  SearchOutlined, 
  BookOutlined, 
  CodeOutlined, 
  DatabaseOutlined, 
  DesktopOutlined, 
  CalculatorOutlined, 
  GlobalOutlined, 
  BulbOutlined, 
  BankOutlined, 
  ArrowRightOutlined,
  FileTextOutlined,
  FilterOutlined,
  ReadOutlined
} from '@ant-design/icons';
import { educationService } from '../../services/educationService';

export const SubjectsPage = () => {
  const navigate = useNavigate();
  const [subjects, setSubjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Pagination & Filtering
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(12);
  const [totalCount, setTotalCount] = useState(0);
  const [filterType, setFilterType] = useState('all'); // 'all', 'hasDocs', 'highCredits'

  useEffect(() => {
    fetchSubjects();
  }, [page, pageSize, searchTerm]);

  const fetchSubjects = async () => {
    setLoading(true);
    try {
      const res = await educationService.getAllSubjects({
        pageNumber: page,
        pageSize,
        searchTerm: searchTerm.trim() || undefined,
      });
      if (res.data) {
        setSubjects(res.data.items || res.data || []);
        setTotalCount(res.data.totalCount || (res.data.items ? res.data.items.length : 0));
      }
    } catch (err) {
      console.error('Error fetching subjects:', err);
    } finally {
      setLoading(false);
    }
  };

  // Helper for dynamic Subject Theme (Icon & Colors)
  const getSubjectTheme = (name) => {
    const n = (name || '').toLowerCase();
    if (n.includes('lập trình') || n.includes('cấu trúc') || n.includes('code') || n.includes('phần mềm')) {
      return { bg: 'bg-emerald-50 text-emerald-600', icon: <CodeOutlined className="text-2xl" /> };
    }
    if (n.includes('cơ sở dữ liệu') || n.includes('sql') || n.includes('dữ liệu')) {
      return { bg: 'bg-blue-50 text-blue-600', icon: <DatabaseOutlined className="text-2xl" /> };
    }
    if (n.includes('mạng') || n.includes('web') || n.includes('hệ điều hành')) {
      return { bg: 'bg-purple-50 text-purple-600', icon: <DesktopOutlined className="text-2xl" /> };
    }
    if (n.includes('toán') || n.includes('giải tích') || n.includes('đại số') || n.includes('xác suất')) {
      return { bg: 'bg-amber-50 text-amber-600', icon: <CalculatorOutlined className="text-2xl" /> };
    }
    if (n.includes('anh') || n.includes('ngoại ngữ') || n.includes('tiếng')) {
      return { bg: 'bg-sky-50 text-sky-600', icon: <GlobalOutlined className="text-2xl" /> };
    }
    if (n.includes('trí tuệ') || n.includes('ai') || n.includes('học máy')) {
      return { bg: 'bg-indigo-50 text-indigo-600', icon: <BulbOutlined className="text-2xl" /> };
    }
    if (n.includes('kinh tế') || n.includes('quản trị') || n.includes('tài chính')) {
      return { bg: 'bg-rose-50 text-rose-600', icon: <BankOutlined className="text-2xl" /> };
    }
    return { bg: 'bg-teal-50 text-teal-600', icon: <BookOutlined className="text-2xl" /> };
  };

  // Client-side quick filter
  const displayedSubjects = subjects.filter((s) => {
    if (filterType === 'hasDocs') return (s.documentCount || 0) > 0;
    if (filterType === 'highCredits') return (s.credits || 0) >= 3;
    return true;
  });

  const totalDocuments = subjects.reduce((sum, s) => sum + (s.documentCount || 0), 0);

  return (
    <div className="space-y-8">
      
      {/* SECTION 1: HERO HEADER BANNER */}
      <section className="bg-gradient-to-r from-blue-700 via-indigo-600 to-purple-700 text-white rounded-3xl p-8 sm:p-10 shadow-lg relative overflow-hidden">
        {/* Background Graphic SVG Overlay */}
        <div className="absolute right-0 top-0 bottom-0 opacity-10 pointer-events-none hidden md:block">
          <svg className="w-96 h-96 transform translate-x-12 -translate-y-12" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z" />
          </svg>
        </div>

        <div className="relative z-10 max-w-3xl space-y-6">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 bg-white/15 backdrop-blur-md rounded-full text-xs font-semibold tracking-wide uppercase mb-3">
              <ReadOutlined /> Danh mục môn học chính thức
            </div>
            <h1 className="text-3xl sm:text-4xl font-black tracking-tight leading-tight m-0 text-white">
              Khám Phá Môn Học & Học Phần
            </h1>
            <p className="text-sm sm:text-base text-blue-100 mt-2 font-medium leading-relaxed">
              Tra cứu nhanh bài giảng, giáo trình, đề thi và slide tài liệu học tập theo từng môn học trong chương trình đào tạo.
            </p>
          </div>

          {/* Search Box */}
          <div className="flex gap-2 max-w-xl">
            <Input
              size="large"
              placeholder="Nhập tên môn học, mã môn (VD: Cấu trúc dữ liệu, INT2204)..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setPage(1);
              }}
              prefix={<SearchOutlined className="text-slate-400 text-lg mr-1" />}
              className="rounded-2xl shadow-md text-sm border-0 h-12"
              allowClear
            />
          </div>

          {/* Stats Badges Row */}
          <div className="flex flex-wrap items-center gap-4 text-xs font-semibold pt-2">
            <div className="bg-white/15 backdrop-blur-md px-3.5 py-1.5 rounded-xl flex items-center gap-2">
              <BookOutlined />
              <span>{totalCount} Môn học</span>
            </div>
            <div className="bg-white/15 backdrop-blur-md px-3.5 py-1.5 rounded-xl flex items-center gap-2">
              <FileTextOutlined />
              <span>{totalDocuments}+ Tài liệu đã đăng</span>
            </div>
          </div>
        </div>
      </section>

      {/* SECTION 2: FILTER BAR & CONTROLS */}
      <section className="bg-white p-4 sm:p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between items-center gap-4">
        
        {/* Quick Filter Pills */}
        <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
          <span className="text-xs font-bold text-slate-500 mr-1 flex items-center gap-1">
            <FilterOutlined /> Bộ lọc:
          </span>
          <button
            onClick={() => setFilterType('all')}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border ${
              filterType === 'all'
                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
            }`}
          >
            Tất cả môn học
          </button>

          <button
            onClick={() => setFilterType('hasDocs')}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border ${
              filterType === 'hasDocs'
                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
            }`}
          >
            Có tài liệu bài giảng
          </button>

          <button
            onClick={() => setFilterType('highCredits')}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border ${
              filterType === 'highCredits'
                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
            }`}
          >
            Môn từ 3 tín chỉ
          </button>
        </div>

        {/* Counter Summary */}
        <div className="text-xs font-medium text-slate-500 self-end sm:self-auto">
          Hiển thị <span className="font-bold text-slate-900">{displayedSubjects.length}</span> môn học
        </div>
      </section>

      {/* SECTION 3: SUBJECT CARDS GRID */}
      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
            <div key={i} className="bg-white p-5 rounded-2xl border border-slate-200 space-y-3">
              <div className="flex justify-between items-center">
                <Skeleton.Avatar active shape="square" className="w-12 h-12 rounded-2xl" />
                <Skeleton active paragraph={{ rows: 0 }} className="w-16" />
              </div>
              <Skeleton active paragraph={{ rows: 2 }} />
            </div>
          ))}
        </div>
      ) : displayedSubjects.length === 0 ? (
        <div className="bg-white p-12 rounded-3xl border border-slate-200 text-center space-y-4 shadow-sm">
          <Empty description={<span className="text-slate-500 font-medium text-sm">Không tìm thấy môn học nào phù hợp</span>} />
          {searchTerm && (
            <Button 
              type="primary" 
              onClick={() => { setSearchTerm(''); setFilterType('all'); setPage(1); }}
              className="bg-blue-600 font-bold rounded-xl"
            >
              Xóa từ khóa tìm kiếm
            </Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {displayedSubjects.map((subject) => {
            const theme = getSubjectTheme(subject.name);
            return (
              <div
                key={subject.id}
                onClick={() => navigate(`/documents?subjectId=${subject.id}`)}
                className="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm hover:shadow-md transition-all duration-300 cursor-pointer group flex flex-col justify-between card-hover-effect"
              >
                <div>
                  {/* Top Bar: Icon + Credits Badge */}
                  <div className="flex items-center justify-between gap-2 mb-4">
                    <div className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 shadow-sm transition-transform group-hover:scale-110 ${theme.bg}`}>
                      {theme.icon}
                    </div>

                    <div className="flex items-center gap-1.5">
                      {subject.credits > 0 && (
                        <Tag color="blue" className="m-0 font-bold text-[10px] rounded-lg px-2 py-0.5 border-0">
                          {subject.credits} TC
                        </Tag>
                      )}
                      <Tag color="cyan" className="m-0 font-semibold text-[10px] rounded-lg px-2 py-0.5 border-0">
                        {subject.documentCount || 0} tài liệu
                      </Tag>
                    </div>
                  </div>

                  {/* Subject Name */}
                  <h3 className="text-base font-bold text-slate-900 line-clamp-2 leading-snug mb-2 group-hover:text-blue-600 transition-colors m-0">
                    {subject.name}
                  </h3>

                  {/* Code & Major info */}
                  <div className="space-y-1 text-xs text-slate-500 mb-4">
                    <div className="font-semibold text-slate-600">
                      Mã môn: <span className="text-blue-600 font-bold">{subject.code || 'N/A'}</span>
                    </div>
                    {subject.majorName && (
                      <div className="truncate text-slate-400 text-[11px]">
                        Ngành: {subject.majorName}
                      </div>
                    )}
                  </div>
                </div>

                {/* Card Action Footer */}
                <div className="pt-3 border-t border-slate-100 flex justify-between items-center text-xs font-semibold text-blue-600 group-hover:text-blue-700">
                  <span>Xem tài liệu môn học</span>
                  <ArrowRightOutlined className="text-blue-500 group-hover:translate-x-1.5 transition-transform" />
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* SECTION 4: PAGINATION */}
      {totalCount > pageSize && (
        <div className="flex justify-center pt-4">
          <Pagination
            current={page}
            pageSize={pageSize}
            total={totalCount}
            onChange={(p, ps) => { setPage(p); setPageSize(ps); }}
            showSizeChanger
            pageSizeOptions={['12', '24', '48']}
            showTotal={(total) => `Tổng số ${total} môn học`}
            className="bg-white px-6 py-3 rounded-2xl border border-slate-200 shadow-sm"
          />
        </div>
      )}

    </div>
  );
};

export default SubjectsPage;
