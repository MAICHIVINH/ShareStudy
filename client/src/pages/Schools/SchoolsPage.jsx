import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input, Skeleton, Empty, Tag, Button, Modal, Spin } from 'antd';
import { 
  SearchOutlined, 
  BankOutlined, 
  ArrowRightOutlined, 
  ApartmentOutlined,
  ReadOutlined,
  FilterOutlined,
  BookOutlined,
  GlobalOutlined,
  EnvironmentOutlined,
  FileTextOutlined
} from '@ant-design/icons';
import { educationService } from '../../services/educationService';

export const SchoolsPage = () => {
  const navigate = useNavigate();
  const [schools, setSchools] = useState([]);
  const [educationLevels, setEducationLevels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedLevelId, setSelectedLevelId] = useState(null);

  // School Detail Modal State
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [selectedSchoolDetail, setSelectedSchoolDetail] = useState(null);
  const [loadingDetail, setLoadingDetail] = useState(false);

  useEffect(() => {
    fetchInitialData();
  }, []);

  useEffect(() => {
    fetchSchools();
  }, [selectedLevelId]);

  const fetchInitialData = async () => {
    try {
      const levelsRes = await educationService.getEducationLevels();
      if (levelsRes.data) {
        setEducationLevels(levelsRes.data || []);
      }
    } catch (err) {
      console.error('Error fetching education levels:', err);
    }
  };

  const fetchSchools = async () => {
    setLoading(true);
    try {
      const res = await educationService.getAllSchools({
        educationLevelId: selectedLevelId || undefined,
      });
      if (res.data) {
        setSchools(res.data || []);
      }
    } catch (err) {
      console.error('Error fetching schools:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDetailModal = async (schoolId, e) => {
    if (e) e.stopPropagation();
    setDetailModalOpen(true);
    setLoadingDetail(true);
    setSelectedSchoolDetail(null);
    try {
      const res = await educationService.getSchoolDetail(schoolId);
      if (res.data) {
        setSelectedSchoolDetail(res.data);
      }
    } catch (err) {
      console.error('Error fetching school detail:', err);
    } finally {
      setLoadingDetail(false);
    }
  };

  const filteredSchools = schools.filter((s) => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return true;
    const nameMatch = (s.name || '').toLowerCase().includes(term);
    const codeMatch = (s.code || '').toLowerCase().includes(term);
    const levelMatch = (s.educationLevelName || '').toLowerCase().includes(term);
    return nameMatch || codeMatch || levelMatch;
  });

  return (
    <div className="space-y-8">
      
      {/* SECTION 1: HERO HEADER BANNER */}
      <section className="bg-gradient-to-r from-blue-700 via-indigo-600 to-purple-700 text-white rounded-3xl p-8 sm:p-10 shadow-lg relative overflow-hidden">
        {/* SVG Graphic Decoration */}
        <div className="absolute right-0 top-0 bottom-0 opacity-10 pointer-events-none hidden md:block">
          <svg className="w-96 h-96 transform translate-x-12 -translate-y-12" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z" />
          </svg>
        </div>

        <div className="relative z-10 max-w-3xl space-y-6">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 bg-white/15 backdrop-blur-md rounded-full text-xs font-semibold tracking-wide uppercase mb-3">
              <BankOutlined /> Mạng lưới cơ sở đào tạo
            </div>
            <h1 className="text-3xl sm:text-4xl font-black tracking-tight leading-tight m-0 text-white">
              Danh Sách Trường Học & Ngành Đào Tạo
            </h1>
            <p className="text-sm sm:text-base text-blue-100 mt-2 font-medium leading-relaxed">
              Khám phá hệ thống Khoa, Chuyên ngành và tài liệu bài giảng theo từng Trường Đại học, Cao đẳng trên toàn quốc.
            </p>
          </div>

          {/* Search Input Box */}
          <div className="flex gap-2 max-w-xl">
            <Input
              size="large"
              placeholder="Nhập tên trường, mã trường (VD: Bách Khoa, DTH, HCMUS)..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              prefix={<SearchOutlined className="text-slate-400 text-lg mr-1" />}
              className="rounded-2xl shadow-md text-sm border-0 h-12"
              allowClear
            />
          </div>

          {/* Stats Bar */}
          <div className="flex flex-wrap items-center gap-4 text-xs font-semibold pt-2">
            <div className="bg-white/15 backdrop-blur-md px-3.5 py-1.5 rounded-xl flex items-center gap-2">
              <BankOutlined />
              <span>{schools.length} Trường đào tạo</span>
            </div>
            <div className="bg-white/15 backdrop-blur-md px-3.5 py-1.5 rounded-xl flex items-center gap-2">
              <ApartmentOutlined />
              <span>Đa dạng Khoa & Chuyên ngành</span>
            </div>
          </div>
        </div>
      </section>

      {/* SECTION 2: FILTER BAR */}
      <section className="bg-white p-4 sm:p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between items-center gap-4">
        {/* Education Level Pills */}
        <div className="flex flex-wrap items-center gap-2 w-full sm:w-auto">
          <span className="text-xs font-bold text-slate-500 mr-1 flex items-center gap-1">
            <FilterOutlined /> Cấp học:
          </span>
          
          <button
            onClick={() => setSelectedLevelId(null)}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border ${
              selectedLevelId === null
                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
            }`}
          >
            Tất cả cấp học
          </button>

          {educationLevels.map((lvl) => {
            const isActive = selectedLevelId === lvl.id;
            return (
              <button
                key={lvl.id}
                onClick={() => setSelectedLevelId(lvl.id)}
                className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all cursor-pointer border ${
                  isActive
                    ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                    : 'bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100'
                }`}
              >
                {lvl.name}
              </button>
            );
          })}
        </div>

        {/* Counter Summary */}
        <div className="text-xs font-medium text-slate-500 self-end sm:self-auto">
          Hiển thị <span className="font-bold text-slate-900">{filteredSchools.length}</span> trường
        </div>
      </section>

      {/* SECTION 3: SCHOOL CARDS GRID */}
      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="bg-white p-6 rounded-2xl border border-slate-200 space-y-4">
              <div className="flex items-center gap-3">
                <Skeleton.Avatar active shape="square" className="w-14 h-14 rounded-2xl" />
                <Skeleton active paragraph={{ rows: 1 }} />
              </div>
            </div>
          ))}
        </div>
      ) : filteredSchools.length === 0 ? (
        <div className="bg-white p-12 rounded-3xl border border-slate-200 text-center space-y-4 shadow-sm">
          <Empty description={<span className="text-slate-500 font-medium text-sm">Không tìm thấy trường nào phù hợp</span>} />
          {(searchTerm || selectedLevelId) && (
            <Button 
              type="primary" 
              onClick={() => { setSearchTerm(''); setSelectedLevelId(null); }}
              className="bg-blue-600 font-bold rounded-xl"
            >
              Xóa bộ lọc tìm kiếm
            </Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredSchools.map((school) => (
            <div
              key={school.id}
              className="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm hover:shadow-md transition-all duration-300 group flex flex-col justify-between card-hover-effect"
            >
              <div>
                {/* Header Row: School Code Avatar & Level Tag */}
                <div className="flex items-start justify-between gap-3 mb-4">
                  <div className="w-14 h-14 rounded-2xl bg-gradient-to-tr from-blue-600 to-indigo-600 text-white font-black text-sm flex items-center justify-center shrink-0 shadow-md shadow-blue-500/20">
                    {school.code || 'UNI'}
                  </div>

                  <Tag color="blue" className="m-0 font-bold text-[10px] rounded-lg px-2 py-0.5 border-0">
                    {school.educationLevelName || 'Đại học'}
                  </Tag>
                </div>

                {/* School Name & Info */}
                <h3 className="text-base font-bold text-slate-900 line-clamp-2 leading-snug mb-2 group-hover:text-blue-600 transition-colors m-0">
                  {school.name}
                </h3>

                <div className="space-y-1 text-xs text-slate-500 mb-4">
                  <div className="font-semibold text-slate-600">
                    Mã trường: <span className="text-blue-600 font-bold">{school.code || 'N/A'}</span>
                  </div>
                  {school.address && (
                    <div className="text-[11px] text-slate-400 truncate flex items-center gap-1">
                      <EnvironmentOutlined /> {school.address}
                    </div>
                  )}
                </div>
              </div>

              {/* Card Action Buttons Row */}
              <div className="pt-3 border-t border-slate-100 flex items-center justify-between gap-2">
                <Button
                  size="small"
                  icon={<ApartmentOutlined />}
                  onClick={(e) => handleOpenDetailModal(school.id, e)}
                  className="rounded-xl text-[11px] font-semibold border-slate-200 hover:border-blue-500 hover:text-blue-600"
                >
                  Xem Khoa & Ngành
                </Button>

                <Button
                  type="primary"
                  size="small"
                  icon={<FileTextOutlined />}
                  onClick={() => navigate(`/documents?schoolId=${school.id}`)}
                  className="rounded-xl text-[11px] font-semibold bg-blue-600 hover:bg-blue-700"
                >
                  Xem Tài liệu
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* SCHOOL DETAIL MODAL (Khoa & Ngành) */}
      <Modal
        title={
          selectedSchoolDetail ? (
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-blue-600 text-white font-black text-xs flex items-center justify-center shrink-0">
                {selectedSchoolDetail.code || 'UNI'}
              </div>
              <div>
                <div className="font-bold text-slate-900 text-sm">{selectedSchoolDetail.name}</div>
                <div className="text-[11px] text-slate-400 font-normal">Cơ cấu Khoa & Chuyên ngành đào tạo</div>
              </div>
            </div>
          ) : 'Chi tiết Trường & Ngành'
        }
        open={detailModalOpen}
        onCancel={() => setDetailModalOpen(false)}
        footer={
          selectedSchoolDetail && (
            <div className="flex justify-between items-center pt-2">
              {selectedSchoolDetail.website ? (
                <a href={selectedSchoolDetail.website} target="_blank" rel="noreferrer" className="text-xs text-blue-600 font-semibold no-underline flex items-center gap-1">
                  <GlobalOutlined /> {selectedSchoolDetail.website}
                </a>
              ) : <div />}

              <Button
                type="primary"
                icon={<FileTextOutlined />}
                onClick={() => {
                  setDetailModalOpen(false);
                  navigate(`/documents?schoolId=${selectedSchoolDetail.id}`);
                }}
                className="bg-blue-600 rounded-xl font-semibold text-xs"
              >
                Xem tất cả tài liệu trường
              </Button>
            </div>
          )
        }
        width={650}
        className="rounded-3xl"
      >
        {loadingDetail ? (
          <div className="py-12 text-center">
            <Spin size="large" tip="Đang tải danh sách Khoa & Ngành..." />
          </div>
        ) : !selectedSchoolDetail ? (
          <Empty description="Không có thông tin chi tiết" />
        ) : (
          <div className="py-3 space-y-6">
            
            {/* School Description */}
            {selectedSchoolDetail.description && (
              <div className="bg-slate-50 p-3.5 rounded-2xl text-xs text-slate-600 leading-relaxed">
                {selectedSchoolDetail.description}
              </div>
            )}

            {/* List of Faculties & Majors */}
            <div className="space-y-4">
              <h4 className="text-xs font-bold text-slate-700 uppercase tracking-wider m-0 flex items-center gap-2">
                <ApartmentOutlined className="text-blue-600" />
                Danh sách Khoa & Ngành trực thuộc ({selectedSchoolDetail.faculties ? selectedSchoolDetail.faculties.length : 0} Khoa)
              </h4>

              {!selectedSchoolDetail.faculties || selectedSchoolDetail.faculties.length === 0 ? (
                <div className="text-xs text-slate-400 text-center py-4 bg-slate-50 rounded-2xl">
                  Chưa có dữ liệu Khoa & Ngành
                </div>
              ) : (
                <div className="space-y-3 max-h-80 overflow-y-auto pr-1">
                  {selectedSchoolDetail.faculties.map((fac) => (
                    <div key={fac.id} className="bg-white p-4 rounded-2xl border border-slate-200 space-y-2">
                      <div className="flex justify-between items-center">
                        <div className="font-bold text-xs text-slate-900">{fac.name}</div>
                        <Button
                          size="small"
                          type="text"
                          className="text-[11px] text-blue-600 font-semibold p-0 hover:bg-transparent"
                          onClick={() => {
                            setDetailModalOpen(false);
                            navigate(`/documents?schoolId=${selectedSchoolDetail.id}&facultyId=${fac.id}`);
                          }}
                        >
                          Lọc theo Khoa →
                        </Button>
                      </div>

                      {/* Majors Pills */}
                      {fac.majors && fac.majors.length > 0 ? (
                        <div className="flex flex-wrap gap-1.5 pt-1">
                          {fac.majors.map((maj) => (
                            <Tag
                              key={maj.id}
                              color="blue"
                              className="cursor-pointer hover:bg-blue-600 hover:text-white transition-colors m-0 text-[11px] py-0.5 px-2 rounded-lg border-0"
                              onClick={() => {
                                setDetailModalOpen(false);
                                navigate(`/documents?majorId=${maj.id}`);
                              }}
                            >
                              {maj.name}
                            </Tag>
                          ))}
                        </div>
                      ) : (
                        <div className="text-[11px] text-slate-400 italic">Chưa có ngành trực thuộc</div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>

          </div>
        )}
      </Modal>

    </div>
  );
};

export default SchoolsPage;
