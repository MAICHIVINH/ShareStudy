import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input, Button, Pagination, Skeleton, message } from 'antd';
import { 
  SearchOutlined, 
  FileTextOutlined, 
  BankOutlined, 
  BookOutlined, 
  TeamOutlined, 
  ArrowRightOutlined,
  StarFilled,
  ReadOutlined,
  FileDoneOutlined,
  CodeOutlined,
  DesktopOutlined,
  DeploymentUnitOutlined,
  BulbOutlined,
  PieChartOutlined,
  CheckCircleOutlined,
  FolderOpenOutlined,
  TrophyOutlined,
  PaperClipOutlined,
  FolderOutlined,
  FormOutlined,
  UserAddOutlined,
  CheckOutlined
} from '@ant-design/icons';
import { documentService } from '../../services/documentService';
import { educationService } from '../../services/educationService';
import { DocumentCard } from '../../components/documents/DocumentCard';
import { formatAvatarUrl } from '../../utils/avatarUtils';
import { useAuth } from '../../contexts/AuthContext';

export const HomePage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [searchKeyword, setSearchKeyword] = useState('');
  
  // Follow State (Persisted in localStorage per user account)
  const [followingMap, setFollowingMap] = useState(() => {
    try {
      const key = user?.id ? `sharestudy_following_${user.id}` : 'sharestudy_following_guest';
      const saved = localStorage.getItem(key);
      return saved ? JSON.parse(saved) : {};
    } catch {
      return {};
    }
  });

  useEffect(() => {
    if (user?.id) {
      try {
        const saved = localStorage.getItem(`sharestudy_following_${user.id}`);
        if (saved) setFollowingMap(JSON.parse(saved));
      } catch {}
    }
  }, [user?.id]);

  const handleToggleFollow = (userId, userName) => {
    if (!user) {
      message.info('Vui lòng đăng nhập để theo dõi thành viên!');
      navigate('/login');
      return;
    }

    const isFollowing = !!followingMap[userId];
    const nextState = {
      ...followingMap,
      [userId]: !isFollowing,
    };

    setFollowingMap(nextState);
    try {
      localStorage.setItem(`sharestudy_following_${user.id}`, JSON.stringify(nextState));
    } catch (err) {
      console.error('Lỗi lưu trạng thái theo dõi:', err);
    }

    if (!isFollowing) {
      message.success(`Đã theo dõi thành viên ${userName}`);
    } else {
      message.info(`Đã bỏ theo dõi thành viên ${userName}`);
    }
  };
  
  // Dynamic Data States
  const [latestDocuments, setLatestDocuments] = useState([]);
  const [popularDocuments, setPopularDocuments] = useState([]);
  const [topRatedDocuments, setTopRatedDocuments] = useState([]);
  const [docTypes, setDocTypes] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [loading, setLoading] = useState(true);

  // Pagination for Latest Documents
  const [page, setPage] = useState(1);
  const [pageSize] = useState(8);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    fetchHomeData();
  }, [page]);

  const fetchHomeData = async () => {
    setLoading(true);
    try {
      const [latestRes, popularRes, topRatedRes, subjectsRes, typesRes] = await Promise.all([
        documentService.getDocuments({ pageNumber: page, pageSize, sortBy: 'newest' }),
        documentService.getPopularDocuments(5),
        documentService.getTopRatedDocuments(5),
        educationService.getAllSubjects({ pageSize: 6 }),
        educationService.getDocumentTypes(),
      ]);

      if (latestRes.data) {
        setLatestDocuments(latestRes.data.items || []);
        setTotalCount(latestRes.data.totalCount || 0);
      }
      if (popularRes.data) setPopularDocuments(popularRes.data || []);
      if (topRatedRes.data) setTopRatedDocuments(topRatedRes.data || []);
      if (subjectsRes.data) setSubjects(subjectsRes.data.items || subjectsRes.data || []);
      if (typesRes.data) setDocTypes(typesRes.data || []);
    } catch (err) {
      console.error('Error fetching home data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    if (searchKeyword.trim()) {
      navigate(`/documents?keyword=${encodeURIComponent(searchKeyword.trim())}`);
    }
  };

  // Comprehensive Helper for Category Colors & Icons
  const getCategoryTheme = (name) => {
    const n = (name || '').toLowerCase();

    if (n.includes('bài giảng') || n.includes('giáo trình')) {
      return { bg: 'bg-red-50 text-red-600', icon: <ReadOutlined className="text-2xl" /> };
    }
    if (n.includes('bài tập')) {
      return { bg: 'bg-emerald-50 text-emerald-600', icon: <FileDoneOutlined className="text-2xl" /> };
    }
    if (n.includes('báo cáo')) {
      return { bg: 'bg-sky-50 text-sky-600', icon: <PieChartOutlined className="text-2xl" /> };
    }
    if (n.includes('đáp án')) {
      return { bg: 'bg-teal-50 text-teal-600', icon: <CheckCircleOutlined className="text-2xl" /> };
    }
    if (n.includes('đề cương')) {
      return { bg: 'bg-purple-50 text-purple-600', icon: <FileTextOutlined className="text-2xl" /> };
    }
    if (n.includes('đề thi')) {
      return { bg: 'bg-blue-50 text-blue-600', icon: <BookOutlined className="text-2xl" /> };
    }
    if (n.includes('đồ án')) {
      return { bg: 'bg-indigo-50 text-indigo-600', icon: <FolderOpenOutlined className="text-2xl" /> };
    }
    if (n.includes('luận văn')) {
      return { bg: 'bg-rose-50 text-rose-600', icon: <TrophyOutlined className="text-2xl" /> };
    }
    if (n.includes('slide')) {
      return { bg: 'bg-amber-50 text-amber-600', icon: <DesktopOutlined className="text-2xl" /> };
    }
    if (n.includes('tham khảo')) {
      return { bg: 'bg-cyan-50 text-cyan-600', icon: <PaperClipOutlined className="text-2xl" /> };
    }
    return { bg: 'bg-slate-100 text-slate-600', icon: <FolderOutlined className="text-2xl" /> };
  };

  // Compute top community uploaders dynamically from real document data
  const communityUsers = React.useMemo(() => {
    const allDocs = [...latestDocuments, ...popularDocuments, ...topRatedDocuments];
    const userMap = new Map();

    allDocs.forEach((doc) => {
      const name = doc.uploaderName || 'Thành viên';
      const userId = doc.uploaderId || name;
      const avatar = formatAvatarUrl(doc.uploaderAvatar, name);

      if (!userMap.has(userId)) {
        userMap.set(userId, {
          name,
          avatar,
          docCount: 1,
          downloadCount: doc.downloadCount || 0,
        });
      } else {
        const existing = userMap.get(userId);
        existing.docCount += 1;
        existing.downloadCount += doc.downloadCount || 0;
        if (doc.uploaderAvatar) {
          existing.avatar = formatAvatarUrl(doc.uploaderAvatar, name);
        }
      }
    });

    const list = Array.from(userMap.values())
      .sort((a, b) => b.downloadCount - a.downloadCount)
      .slice(0, 4);

    if (list.length === 0) {
      return [
        { userId: 'admin', name: 'Admin ShareStudy', avatar: formatAvatarUrl(null, 'Admin'), docs: '1 tài liệu', downloads: '10 lượt tải' }
      ];
    }

    return list.map((u, i) => ({
      userId: u.name || i,
      name: u.name,
      avatar: u.avatar,
      docs: `${u.docCount} tài liệu`,
      downloads: `${u.downloadCount} lượt tải`,
    }));
  }, [latestDocuments, popularDocuments, topRatedDocuments]);

  return (
    <div className="space-y-12">
      
      {/* SECTION 1: HERO SEARCH BANNER */}
      <section className="bg-gradient-to-r from-blue-50 via-indigo-50/60 to-blue-100 rounded-3xl p-8 sm:p-12 relative overflow-hidden border border-blue-100/80 shadow-sm">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-center relative z-10">
          
          {/* Left Content */}
          <div className="lg:col-span-7 space-y-6">
            <div>
              <h1 className="text-3xl sm:text-4xl lg:text-5xl font-black text-slate-900 tracking-tight leading-tight mb-2">
                Tìm kiếm tài liệu học tập
              </h1>
              <p className="text-sm sm:text-base text-slate-600 font-medium">
                Nhanh chóng - Miễn phí - Chất lượng
              </p>
            </div>

            {/* Search Box */}
            <form onSubmit={handleSearchSubmit} className="flex gap-2 max-w-xl">
              <Input
                size="large"
                placeholder="Tìm kiếm tài liệu, môn học, đề thi, giáo trình..."
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
                prefix={<SearchOutlined className="text-slate-400 text-lg mr-1" />}
                className="rounded-2xl shadow-sm text-sm border-slate-200 h-12"
              />
              <Button 
                type="primary" 
                htmlType="submit"
                className="bg-blue-600 hover:bg-blue-700 h-12 px-6 rounded-2xl font-bold flex items-center justify-center shadow-md shadow-blue-500/20"
              >
                <SearchOutlined className="text-lg" />
              </Button>
            </form>

            {/* Dynamic Stats Row */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 pt-4 border-t border-blue-200/50">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-white shadow-sm flex items-center justify-center text-blue-600">
                  <FileTextOutlined className="text-lg" />
                </div>
                <div>
                  <div className="text-sm font-bold text-slate-900">{totalCount || 0}+</div>
                  <div className="text-[11px] text-slate-500">Tài liệu</div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-white shadow-sm flex items-center justify-center text-blue-600">
                  <BankOutlined className="text-lg" />
                </div>
                <div>
                  <div className="text-sm font-bold text-slate-900">50+</div>
                  <div className="text-[11px] text-slate-500">Trường & Khoa</div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-white shadow-sm flex items-center justify-center text-blue-600">
                  <BookOutlined className="text-lg" />
                </div>
                <div>
                  <div className="text-sm font-bold text-slate-900">{subjects.length || 10}+</div>
                  <div className="text-[11px] text-slate-500">Môn học</div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-white shadow-sm flex items-center justify-center text-blue-600">
                  <TeamOutlined className="text-lg" />
                </div>
                <div>
                  <div className="text-sm font-bold text-slate-900">100+</div>
                  <div className="text-[11px] text-slate-500">Thành viên</div>
                </div>
              </div>
            </div>

          </div>

          {/* Right Graphic Illustration */}
          <div className="lg:col-span-5 hidden lg:flex justify-center items-center relative">
            <div className="w-72 h-72 rounded-3xl bg-blue-600/10 flex items-center justify-center relative">
              <svg className="w-56 h-56 text-blue-600 drop-shadow-xl" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 3L1 9l11 6 9-4.91V17h2V9L12 3zM5 13.18v4L12 21l7-3.82v-4L12 17l-7-3.82z" />
              </svg>
            </div>
          </div>

        </div>
      </section>

      {/* SECTION 2: DYNAMIC CATEGORY CARDS FROM DATABASE */}
      <section className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
        {loading ? (
          [1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="bg-white p-4 rounded-2xl border border-slate-200 h-20 flex items-center gap-3">
              <Skeleton.Avatar active shape="square" className="w-12 h-12 rounded-xl" />
              <Skeleton active paragraph={{ rows: 1 }} title={false} />
            </div>
          ))
        ) : (
          docTypes.map((type) => {
            const theme = getCategoryTheme(type.name);
            return (
              <Link
                key={type.id}
                to={`/documents?type=${encodeURIComponent(type.name)}`}
                className="bg-white rounded-xl border border-solid border-slate-200 shadow-sm p-4 text-slate-800 hover:!border-primary transition-all duration-300 flex items-center gap-3 group"
              >
                <div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 ${theme.bg}`}>
                  {theme.icon}
                </div>
                <div>
                  <div className="text-xs font-bold text-slate-900 group-hover:text-blue-600 transition-colors">
                    {type.name}
                  </div>
                  <div className="text-[11px] text-slate-500 mt-0.5 font-medium">
                    {type.documentCount || 0} tài liệu
                  </div>
                </div>
              </Link>
            );
          })
        )}
      </section>

      {/* SECTION 3: TÀI LIỆU MỚI NHẤT */}
      <section className="space-y-6">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-bold text-slate-900 tracking-tight m-0">
            Tài liệu mới nhất
          </h2>
          <Link to="/documents" className="text-xs font-semibold text-blue-600 hover:text-blue-700 no-underline flex items-center gap-1">
            Xem tất cả <ArrowRightOutlined />
          </Link>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="bg-white p-4 rounded-2xl border border-slate-200 space-y-3">
                <Skeleton.Avatar active shape="square" className="w-full h-32 rounded-xl" />
                <Skeleton active paragraph={{ rows: 2 }} />
              </div>
            ))}
          </div>
        ) : latestDocuments.length === 0 ? (
          <div className="bg-white p-8 rounded-2xl border border-slate-200 text-center text-xs text-slate-500">
            Chưa có tài liệu nào
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {latestDocuments.map((doc) => (
                <DocumentCard key={doc.id} document={doc} />
              ))}
            </div>

            {totalCount > pageSize && (
              <div className="flex justify-center pt-4">
                <Pagination
                  current={page}
                  pageSize={pageSize}
                  total={totalCount}
                  onChange={(p) => setPage(p)}
                  showSizeChanger={false}
                />
              </div>
            )}
          </>
        )}
      </section>

      {/* SECTION 4: KHÁM PHÁ THEO MÔN HỌC */}
      <section className="space-y-6">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-bold text-slate-900 tracking-tight m-0">
            Khám phá theo môn học
          </h2>
          <Link to="/subjects" className="text-xs font-semibold text-blue-600 hover:text-blue-700 no-underline flex items-center gap-1">
            Xem tất cả <ArrowRightOutlined />
          </Link>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
          {loading ? (
            [1, 2, 3, 4, 5, 6].map((i) => (
              <div key={i} className="bg-white p-4 rounded-2xl border border-slate-200 h-24 flex flex-col items-center justify-center">
                <Skeleton active paragraph={{ rows: 1 }} title={false} />
              </div>
            ))
          ) : subjects.length === 0 ? (
            <div className="col-span-full bg-white p-6 rounded-2xl border border-slate-200 text-center text-xs text-slate-400">
              Chưa có dữ liệu môn học
            </div>
          ) : (
            subjects.slice(0, 6).map((sb) => (
              <Link
                key={sb.id}
                to={`/documents?subjectId=${sb.id}`}
                className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm hover:shadow-md transition-all no-underline text-center flex flex-col items-center justify-between group card-hover-effect"
              >
                <div className="w-12 h-12 rounded-2xl flex items-center justify-center mb-2 bg-blue-50 text-blue-600">
                  <BookOutlined className="text-2xl" />
                </div>
                <div>
                  <h4 className="text-xs font-bold text-slate-900 line-clamp-2 mb-1 group-hover:text-blue-600 transition-colors m-0">
                    {sb.name}
                  </h4>
                  <span className="text-[11px] text-slate-400 font-medium">
                    {sb.documentCount || 0} tài liệu
                  </span>
                </div>
              </Link>
            ))
          )}
        </div>
      </section>

      {/* SECTION 5: 3 BOTTOM RANKING COLUMNS */}
      <section className="grid grid-cols-1 md:grid-cols-3 gap-8">
        
        {/* COL 1: Tài liệu được tải nhiều */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div className="flex justify-between items-center border-b border-slate-100 pb-3">
            <h3 className="text-sm font-bold text-slate-900 m-0">Tài liệu được tải nhiều</h3>
            <Link to="/documents?sort=most_downloaded" className="text-xs text-blue-600 font-semibold no-underline">Xem tất cả</Link>
          </div>
          <div className="space-y-3">
            {popularDocuments.length === 0 ? (
              <div className="text-xs text-slate-400 text-center py-4">Chưa có tài liệu</div>
            ) : (
              popularDocuments.map((doc, idx) => (
                <Link 
                  key={doc.id} 
                  to={`/documents/${doc.slug}`}
                  className="flex items-center justify-between no-underline p-2 rounded-xl hover:bg-slate-50 transition-colors group"
                >
                  <div className="flex items-center gap-3 truncate mr-2">
                    <span className="text-xs font-black text-slate-400 w-4">{idx + 1}</span>
                    <span className="text-xs font-semibold text-slate-800 group-hover:text-blue-600 truncate">
                      {doc.title}
                    </span>
                    <span className="text-[10px] font-bold px-1.5 py-0.5 rounded bg-red-100 text-red-600 uppercase">
                      {doc.fileType || 'PDF'}
                    </span>
                  </div>
                  <span className="text-[11px] text-slate-500 shrink-0 font-medium">
                    {doc.downloadCount || 0} lượt tải
                  </span>
                </Link>
              ))
            )}
          </div>
        </div>

        {/* COL 2: Tài liệu đánh giá cao */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div className="flex justify-between items-center border-b border-slate-100 pb-3">
            <h3 className="text-sm font-bold text-slate-900 m-0">Tài liệu đánh giá cao</h3>
            <Link to="/documents?sort=highest_rated" className="text-xs text-blue-600 font-semibold no-underline">Xem tất cả</Link>
          </div>
          <div className="space-y-3">
            {topRatedDocuments.length === 0 ? (
              <div className="text-xs text-slate-400 text-center py-4">Chưa có tài liệu</div>
            ) : (
              topRatedDocuments.map((doc, idx) => (
                <Link 
                  key={doc.id} 
                  to={`/documents/${doc.slug}`}
                  className="flex items-center justify-between no-underline p-2 rounded-xl hover:bg-slate-50 transition-colors group"
                >
                  <div className="flex items-center gap-3 truncate mr-2">
                    <span className="text-xs font-black text-slate-400 w-4">{idx + 1}</span>
                    <span className="text-xs font-semibold text-slate-800 group-hover:text-blue-600 truncate">
                      {doc.title}
                    </span>
                    <span className="text-[10px] font-bold px-1.5 py-0.5 rounded bg-blue-100 text-blue-600 uppercase">
                      {doc.fileType || 'PDF'}
                    </span>
                  </div>
                  <span className="text-[11px] font-bold text-amber-500 flex items-center gap-1 shrink-0">
                    <StarFilled className="text-xs" /> {doc.ratingCount > 0 && doc.averageRating ? Number(doc.averageRating).toFixed(1) : '0.0'}
                  </span>
                </Link>
              ))
            )}
          </div>
        </div>

        {/* COL 3: Cộng đồng nổi bật */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
          <div className="flex justify-between items-center border-b border-slate-100 pb-3">
            <h3 className="text-sm font-bold text-slate-900 m-0">Cộng đồng nổi bật</h3>
            <span className="text-[11px] text-slate-400 font-normal">Top đóng góp</span>
          </div>
          <div className="space-y-3">
            {communityUsers.map((usr, idx) => {
              const isFollowing = !!followingMap[usr.userId];
              return (
                <div key={idx} className="flex items-center justify-between p-2 rounded-xl hover:bg-slate-50 transition-colors">
                  <div className="flex items-center gap-2.5 truncate">
                    <img src={usr.avatar} alt={usr.name} className="w-8 h-8 rounded-full object-cover border border-slate-200" />
                    <div className="truncate">
                      <div className="text-xs font-bold text-slate-800 truncate">{usr.name}</div>
                      <div className="text-[10px] text-slate-400">{usr.docs} • {usr.downloads}</div>
                    </div>
                  </div>
                  <Button 
                    size="small" 
                    type={isFollowing ? "primary" : "default"}
                    icon={isFollowing ? <CheckOutlined /> : <UserAddOutlined />}
                    onClick={() => handleToggleFollow(usr.userId, usr.name)}
                    className={`rounded-lg text-[11px] font-semibold transition-all ${
                      isFollowing 
                        ? 'bg-blue-600 border-blue-600 hover:bg-blue-700' 
                        : 'border-slate-200 hover:text-blue-600 hover:border-blue-300'
                    }`}
                  >
                    {isFollowing ? 'Đã theo dõi' : 'Theo dõi'}
                  </Button>
                </div>
              );
            })}
          </div>
        </div>

      </section>

    </div>
  );
};

export default HomePage;
