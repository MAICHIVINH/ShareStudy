import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { 
  Button, 
  Rate, 
  Modal, 
  Input, 
  Select, 
  message, 
  Spin, 
  Avatar, 
  Divider, 
  Tooltip,
  Popconfirm
} from 'antd';
import { 
  DownloadOutlined, 
  HeartOutlined, 
  HeartFilled, 
  StarOutlined, 
  FlagOutlined, 
  UserOutlined, 
  EyeOutlined,
  FilePdfOutlined,
  ShareAltOutlined,
  MessageOutlined,
  HomeOutlined,
  BookOutlined,
  SendOutlined,
  RightOutlined,
  EditOutlined,
  DeleteOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { documentService } from '../../services/documentService';
import { favoriteService } from '../../services/favoriteService';
import { ratingService } from '../../services/ratingService';
import { commentService } from '../../services/commentService';
import { reportService } from '../../services/reportService';
import { useAuth } from '../../contexts/AuthContext';
import { formatAvatarUrl } from '../../utils/avatarUtils';

const { TextArea } = Input;

// Recursive Tree-Style Comment Item Component
const CommentItem = ({ comment, user, document, onReply, onUpdate, onDelete }) => {
  const [isReplying, setIsReplying] = useState(false);
  const [replyText, setReplyText] = useState('');

  const [isEditing, setIsEditing] = useState(false);
  const [editText, setEditText] = useState(comment.content);

  const isOwner = user && (user.id === comment.userId || user.userId === comment.userId);
  const isAdmin = user && user.roles?.includes('Admin');

  const handleSaveEdit = () => {
    if (!editText.trim()) return;
    onUpdate(comment.id, editText);
    setIsEditing(false);
  };

  const handleSendReply = () => {
    if (!replyText.trim()) return;
    onReply(comment.id, replyText);
    setReplyText('');
    setIsReplying(false);
  };

  const renderCommentContent = (text) => {
    if (!text) return null;
    if (text.includes('\u200B')) {
      const parts = text.split(/(@.*?\u200B)/g);
      return parts.map((part, index) => {
        if (part.startsWith('@') && part.endsWith('\u200B')) {
          return <span key={index} className="text-blue-600 font-semibold">{part.replace('\u200B', '')}</span>;
        }
        return <React.Fragment key={index}>{part}</React.Fragment>;
      });
    }
    const parts = text.split(/(@\S+)/g);
    return parts.map((part, index) => {
      if (part.startsWith('@')) {
        return <span key={index} className="text-blue-600 font-semibold">{part}</span>;
      }
      return <React.Fragment key={index}>{part}</React.Fragment>;
    });
  };

  return (
    <div className="space-y-3">
      <div className="p-4 bg-slate-50 dark:bg-slate-800/40 rounded-xl border border-slate-100 dark:border-slate-800 space-y-2">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <Avatar 
              src={formatAvatarUrl(comment.userAvatar, comment.userName)} 
              icon={<UserOutlined />} 
              className="bg-blue-600 shrink-0 text-xs font-bold" 
              size={30}
            />
            <div>
              <div className="flex items-center gap-2">
                <span className="font-bold text-xs text-slate-900 dark:text-white">
                  {comment.userName || 'Thành viên'}
                </span>
                {document && document.userId === comment.userId && (
                  <span className="bg-blue-100 dark:bg-blue-900/50 text-blue-600 dark:text-blue-300 font-semibold text-[10px] px-1.5 py-0.2 rounded">
                    Tác giả
                  </span>
                )}
              </div>
              <div className="text-[10px] text-slate-400">
                {dayjs(comment.createdAt).format('DD/MM/YYYY HH:mm')}
              </div>
            </div>
          </div>
        </div>

        {/* Content or Edit Form */}
        {isEditing ? (
          <div className="space-y-2 pt-1 pl-9">
            <TextArea
              rows={2}
              value={editText}
              onChange={(e) => setEditText(e.target.value)}
              className="rounded-lg text-xs"
            />
            <div className="flex gap-2 justify-end">
              <Button size="small" onClick={() => setIsEditing(false)} className="rounded-md text-xs">
                Hủy
              </Button>
              <Button size="small" type="primary" onClick={handleSaveEdit} className="bg-blue-600 rounded-md text-xs">
                Lưu chỉnh sửa
              </Button>
            </div>
          </div>
        ) : (
          <p className="text-xs text-slate-700 dark:text-slate-300 m-0 pl-9 leading-relaxed whitespace-pre-wrap">
            {renderCommentContent(comment.content)}
          </p>
        )}

        {/* Action Buttons Row */}
        {!isEditing && (
          <div className="flex items-center gap-4 pl-9 pt-1 text-[11px] font-semibold text-slate-500">
            <button
              onClick={() => {
                const nextState = !isReplying;
                setIsReplying(nextState);
                if (nextState) setReplyText(`@${comment.userName || 'Thành viên'}\u200B `);
              }}
              className="hover:text-blue-600 flex items-center gap-1 bg-transparent border-0 cursor-pointer p-0"
            >
              <MessageOutlined /> Trả lời
            </button>

            {isOwner && (
              <button
                onClick={() => { setIsEditing(true); setEditText(comment.content); }}
                className="hover:text-blue-600 flex items-center gap-1 bg-transparent border-0 cursor-pointer p-0"
              >
                <EditOutlined /> Sửa
              </button>
            )}

            {(isOwner || isAdmin) && (
              <Popconfirm
                title="Xóa bình luận này?"
                description="Bạn có chắc chắn muốn xóa không?"
                okText="Xóa"
                cancelText="Hủy"
                okButtonProps={{ danger: true }}
                onConfirm={() => onDelete(comment.id)}
              >
                <button className="hover:text-red-600 flex items-center gap-1 bg-transparent border-0 cursor-pointer p-0 text-slate-400">
                  <DeleteOutlined /> Xóa
                </button>
              </Popconfirm>
            )}
          </div>
        )}

        {/* Reply Input Box */}
        {isReplying && (
          <div className="mt-3 pl-9 space-y-2">
            <TextArea
              rows={2}
              placeholder={`Phản hồi @${comment.userName}...`}
              value={replyText}
              onChange={(e) => setReplyText(e.target.value)}
              className="rounded-lg text-xs"
            />
            <div className="flex justify-end gap-2">
              <Button size="small" onClick={() => setIsReplying(false)} className="rounded-md text-xs">
                Hủy
              </Button>
              <Button size="small" type="primary" onClick={handleSendReply} className="bg-blue-600 rounded-md text-xs">
                Gửi phản hồi
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* Nested Child Replies Tree */}
      {comment.replies && comment.replies.length > 0 && (
        <div className="pl-6 sm:pl-8 border-l-2 border-slate-200 dark:border-slate-800 space-y-3">
          {comment.replies.map((reply) => (
            <CommentItem
              key={reply.id}
              comment={reply}
              user={user}
              document={document}
              onReply={onReply}
              onUpdate={onUpdate}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export const DocumentDetailPage = () => {
  const { slug } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const [document, setDocument] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isFavorited, setIsFavorited] = useState(false);
  const [relatedDocuments, setRelatedDocuments] = useState([]);

  // Ratings & Comments State
  const [ratings, setRatings] = useState([]);
  const [comments, setComments] = useState([]);
  const [commentInput, setCommentInput] = useState('');

  // Modals
  const [rateModalOpen, setRateModalOpen] = useState(false);
  const [rateScore, setRateScore] = useState(5);
  const [rateComment, setRateComment] = useState('');
  const [submittingRating, setSubmittingRating] = useState(false);

  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [reportReason, setReportReason] = useState('Copyright');
  const [reportDescription, setReportDescription] = useState('');
  const [submittingReport, setSubmittingReport] = useState(false);

  useEffect(() => {
    if (slug) {
      fetchDocumentDetail();
    }
  }, [slug]);

  const fetchDocumentDetail = async () => {
    setLoading(true);
    try {
      const res = await documentService.getDocumentBySlug(slug);
      if (res.data) {
        const doc = res.data;
        setDocument(doc);
        setIsFavorited(doc.isFavorite || false);

        // Record View
        documentService.recordView(doc.id).catch(() => {});

        // Fetch Ratings, Comments, Related
        fetchRatingsAndComments(doc.id);
        fetchRelated(doc.subjectId);
      }
    } catch (err) {
      console.error('Error fetching document detail:', err);
      message.error(err.message || 'Không tìm thấy tài liệu');
    } finally {
      setLoading(false);
    }
  };

  const fetchRatingsAndComments = async (docId) => {
    try {
      const [rateRes, commentRes] = await Promise.all([
        ratingService.getDocumentRatings(docId, { pageSize: 10 }),
        commentService.getDocumentComments(docId, { pageSize: 20 }),
      ]);
      if (rateRes.data?.items) setRatings(rateRes.data.items);
      if (commentRes.data?.items) setComments(commentRes.data.items);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchRelated = async (subjectId) => {
    if (!subjectId) return;
    try {
      const res = await documentService.getDocuments({ subjectId, pageSize: 4 });
      if (res.data?.items) {
        setRelatedDocuments(res.data.items.filter((d) => d.slug !== slug));
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleDownload = async () => {
    if (!document) return;
    try {
      const res = await documentService.downloadDocument(document.id);
      if (res.data?.downloadUrl) {
        window.open(res.data.downloadUrl, '_blank');
        message.success('Đang tải tài liệu...');
      }
    } catch (err) {
      message.error(err.message || 'Không thể tải tài liệu lúc này');
    }
  };

  const handleToggleFavorite = async () => {
    if (!user) {
      message.warning('Vui lòng đăng nhập để lưu tài liệu!');
      navigate('/login');
      return;
    }
    try {
      if (isFavorited) {
        await favoriteService.removeFavorite(document.id);
        setIsFavorited(false);
        message.success('Đã bỏ yêu thích tài liệu');
      } else {
        await favoriteService.addFavorite(document.id);
        setIsFavorited(true);
        message.success('Đã lưu tài liệu vào danh sách yêu thích');
      }
    } catch (err) {
      message.error(err.message || 'Lỗi thao tác');
    }
  };

  const handleShare = () => {
    navigator.clipboard.writeText(window.location.href);
    message.success('Đã sao chép liên kết tài liệu!');
  };

  const handleSubmitRating = async () => {
    if (!user) {
      message.warning('Vui lòng đăng nhập để đánh giá!');
      navigate('/login');
      return;
    }
    setSubmittingRating(true);
    try {
      await ratingService.rateDocument({
        documentId: document.id,
        score: rateScore,
        comment: rateComment.trim() || undefined,
      });
      message.success('Cảm ơn bạn đã đánh giá tài liệu!');
      setRateModalOpen(false);
      setRateComment('');
      fetchDocumentDetail();
    } catch (err) {
      message.error(err.message || 'Đánh giá thất bại');
    } finally {
      setSubmittingRating(false);
    }
  };

  const handleSubmitComment = async () => {
    if (!user) {
      message.warning('Vui lòng đăng nhập để bình luận!');
      navigate('/login');
      return;
    }
    if (!commentInput.trim()) return;

    try {
      await commentService.addComment({
        documentId: document.id,
        content: commentInput.trim(),
      });
      message.success('Đã gửi bình luận');
      setCommentInput('');
      fetchRatingsAndComments(document.id);
    } catch (err) {
      message.error(err.message || 'Gửi bình luận thất bại');
    }
  };

  const handleReplyComment = async (parentId, content) => {
    if (!user) {
      message.warning('Vui lòng đăng nhập để phản hồi!');
      navigate('/login');
      return;
    }
    try {
      await commentService.addComment({
        documentId: document.id,
        parentId: parentId,
        content: content.trim(),
      });
      message.success('Đã gửi phản hồi');
      fetchRatingsAndComments(document.id);
    } catch (err) {
      message.error(err.message || 'Gửi phản hồi thất bại');
    }
  };

  const handleUpdateComment = async (commentId, newContent) => {
    try {
      await commentService.updateComment(commentId, { content: newContent });
      message.success('Đã cập nhật bình luận');
      fetchRatingsAndComments(document.id);
    } catch (err) {
      message.error(err.message || 'Cập nhật thất bại');
    }
  };

  const handleDeleteComment = async (commentId) => {
    try {
      await commentService.deleteComment(commentId);
      message.success('Đã xóa bình luận');
      fetchRatingsAndComments(document.id);
    } catch (err) {
      message.error(err.message || 'Xóa bình luận thất bại');
    }
  };

  const handleSubmitReport = async () => {
    if (!user) {
      message.warning('Vui lòng đăng nhập để báo cáo!');
      navigate('/login');
      return;
    }
    setSubmittingReport(true);
    try {
      await reportService.createReport({
        documentId: document.id,
        reason: reportReason,
        description: reportDescription.trim() || undefined,
      });
      message.success('Báo cáo của bạn đã được gửi tới Ban Quản Trị!');
      setReportModalOpen(false);
      setReportDescription('');
    } catch (err) {
      message.error(err.message || 'Gửi báo cáo thất bại');
    } finally {
      setSubmittingReport(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center py-20 bg-slate-50 dark:bg-slate-950">
        <Spin size="large" />
        <p className="text-xs font-semibold text-slate-500 mt-4">Đang tải chi tiết tài liệu...</p>
      </div>
    );
  }

  if (!document) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center py-20 bg-slate-50 dark:bg-slate-950">
        <h2 className="text-xl font-bold text-slate-800 dark:text-white">Không tìm thấy tài liệu</h2>
        <p className="text-xs text-slate-500 mt-1 mb-4">Tài liệu có thể đã bị xóa hoặc không tồn tại</p>
        <Button type="primary" onClick={() => navigate('/documents')} className="bg-blue-600 rounded-lg text-xs">
          Về danh sách tài liệu
        </Button>
      </div>
    );
  }

  return (
    <div className="w-full font-sans">
      <div className="space-y-2 w-full">
        
        {/* Breadcrumb Navigation Trail */}
        <div className="flex items-center gap-2 text-xs text-slate-500 bg-white dark:bg-slate-900 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 shadow-2xs">
          <Link to="/" className="hover:text-blue-600 no-underline text-slate-500 flex items-center gap-1">
            <HomeOutlined /> Trang chủ
          </Link>
          <RightOutlined className="text-[10px] text-slate-400" />
          <Link to="/documents" className="hover:text-blue-600 no-underline text-slate-500">
            Tài liệu
          </Link>
          {document.subjectName && (
            <>
              <RightOutlined className="text-[10px] text-slate-400" />
              <span className="text-slate-600 dark:text-slate-300 font-medium">{document.subjectName}</span>
            </>
          )}
          <RightOutlined className="text-[10px] text-slate-400" />
          <span className="font-bold text-slate-900 dark:text-white truncate max-w-xs sm:max-w-md">
            {document.title}
          </span>
        </div>

        {/* Main 2-Column Grid Layout */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
          
          {/* Left Column: Viewer & Full Details (8 cols) */}
          <div className="lg:col-span-8 space-y-6">
            
            {/* Header Title Card */}
            <div className="bg-white dark:bg-slate-900 p-6 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-3">
              <div className="flex flex-wrap items-center gap-2">
                <span className="px-2.5 py-0.5 bg-blue-600 text-white font-bold rounded-md text-[11px] uppercase tracking-wide">
                  {document.fileType || 'PDF'}
                </span>
                {document.documentTypeName && (
                  <span className="px-2.5 py-0.5 bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 font-semibold rounded-md text-[11px]">
                    {document.documentTypeName}
                  </span>
                )}
                {document.academicYearName && (
                  <span className="px-2.5 py-0.5 bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 font-semibold rounded-md text-[11px]">
                    Năm học {document.academicYearName}
                  </span>
                )}
              </div>

              <h1 className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white leading-snug m-0">
                {document.title}
              </h1>

              <div className="flex flex-wrap items-center gap-4 text-xs text-slate-500 border-t border-slate-100 dark:border-slate-800 pt-3">
                <span className="flex items-center gap-1.5">
                  <EyeOutlined /> {document.viewCount || 0} lượt xem
                </span>
                <span className="flex items-center gap-1.5">
                  <DownloadOutlined /> {document.downloadCount || 0} lượt tải
                </span>
                <span className="flex items-center gap-1 text-amber-500 font-bold">
                  ⭐ {document.ratingCount > 0 && document.averageRating ? Number(document.averageRating).toFixed(1) : '0.0'} ({document.ratingCount || 0} đánh giá)
                </span>
                <span className="text-slate-400 ml-auto">
                  Đăng {dayjs(document.createdAt).format('DD/MM/YYYY')}
                </span>
              </div>
            </div>

            {/* Document Viewer Frame */}
            <div className="bg-white dark:bg-slate-900 p-4 sm:p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-4">
              <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-3">
                <div className="font-bold text-xs text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  <FilePdfOutlined className="text-red-500 text-base" /> Xem trước tài liệu
                </div>

                <div className="flex items-center gap-2">
                  <Button size="small" icon={<ShareAltOutlined />} onClick={handleShare} className="rounded-md text-xs">
                    Chia sẻ
                  </Button>
                  <Button size="small" type="primary" icon={<DownloadOutlined />} onClick={handleDownload} className="bg-blue-600 rounded-md text-xs">
                    Tải ngay
                  </Button>
                </div>
              </div>

              <div className="w-full min-h-[550px] bg-slate-100 dark:bg-slate-950 rounded-xl flex flex-col items-center justify-center border border-slate-200 dark:border-slate-800 relative overflow-hidden">
                {document.presignedPreviewUrl ? (
                  <iframe
                    src={`${document.presignedPreviewUrl}#toolbar=0`}
                    title={document.title}
                    className="w-full h-[600px] rounded-lg border-0"
                  />
                ) : (
                  <div className="text-center space-y-4 p-8">
                    <div className="w-20 h-24 bg-white dark:bg-slate-900 rounded-xl shadow-md mx-auto flex items-center justify-center text-blue-600 text-4xl border border-slate-200 dark:border-slate-800">
                      <FilePdfOutlined />
                    </div>
                    <div>
                      <h4 className="text-base font-bold text-slate-800 dark:text-white m-0">{document.fileName || document.title}</h4>
                      <p className="text-xs text-slate-500 mt-1">
                        Nhấn nút bên dưới để tải xuống và xem toàn bộ nội dung tài liệu
                      </p>
                    </div>
                    <Button type="primary" icon={<DownloadOutlined />} onClick={handleDownload} className="bg-blue-600 font-semibold rounded-lg h-10 px-6 text-xs">
                      Tải xuống để xem đầy đủ
                    </Button>
                  </div>
                )}
              </div>
            </div>

            {/* Description Section */}
            <div className="bg-white dark:bg-slate-900 p-6 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-3">
              <h3 className="text-sm font-bold text-slate-900 dark:text-white m-0">Mô tả nội dung tài liệu</h3>
              <p className="text-xs text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-line m-0">
                {document.description || 'Tài liệu không có mô tả bổ sung.'}
              </p>
            </div>

            {/* Discussion & Tree Comment Thread */}
            <div className="bg-white dark:bg-slate-900 p-6 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-6">
              
              <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-4">
                <div>
                  <h3 className="text-sm font-bold text-slate-900 dark:text-white m-0 flex items-center gap-2">
                    <MessageOutlined className="text-blue-600" /> Thảo luận & Đánh giá
                  </h3>
                  <p className="text-[11px] text-slate-400 m-0 mt-0.5">Ý kiến và phản hồi từ cộng đồng người học</p>
                </div>

                <Button 
                  onClick={() => setRateModalOpen(true)} 
                  icon={<StarOutlined className="text-amber-500" />}
                  className="rounded-lg text-xs font-semibold border-amber-300 bg-amber-50 text-amber-700 dark:bg-amber-950/30"
                >
                  Viết đánh giá
                </Button>
              </div>

              {/* Main Comment Input Box */}
              <div className="flex gap-3">
                <Avatar 
                  src={user?.avatarUrl ? formatAvatarUrl(user.avatarUrl, user.fullName || user.username) : undefined} 
                  icon={!user?.avatarUrl && <UserOutlined />} 
                  className="bg-blue-600 shrink-0 font-bold"
                />
                <div className="flex-1 space-y-2">
                  <TextArea
                    rows={3}
                    placeholder={user ? "Viết bình luận hoặc đặt câu hỏi về tài liệu này..." : "Đăng nhập để tham gia thảo luận..."}
                    value={commentInput}
                    onChange={(e) => setCommentInput(e.target.value)}
                    disabled={!user}
                    className="rounded-xl text-xs"
                  />
                  <div className="flex justify-end">
                    <Button 
                      type="primary" 
                      onClick={handleSubmitComment}
                      disabled={!user || !commentInput.trim()}
                      icon={<SendOutlined />}
                      className="bg-blue-600 font-semibold rounded-lg text-xs"
                    >
                      Gửi bình luận
                    </Button>
                  </div>
                </div>
              </div>

              <Divider className="my-2" />

              {/* Tree Structured Comments List */}
              <div className="space-y-4">
                {comments.length === 0 ? (
                  <div className="text-center py-6 text-xs text-slate-400">
                    Chưa có bình luận nào. Hãy là người đầu tiên thảo luận về tài liệu này!
                  </div>
                ) : (
                  comments.map((comment) => (
                    <CommentItem
                      key={comment.id}
                      comment={comment}
                      user={user}
                      document={document}
                      onReply={handleReplyComment}
                      onUpdate={handleUpdateComment}
                      onDelete={handleDeleteComment}
                    />
                  ))
                )}
              </div>

            </div>

          </div>

          {/* Right Column: Sticky Sidebar & Academic Info (4 cols) */}
          <div className="lg:col-span-4 space-y-6 sticky top-20">
            
            {/* Primary Actions Card */}
            <div className="bg-white dark:bg-slate-900 p-6 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-5">
              
              <Button
                type="primary"
                size="large"
                icon={<DownloadOutlined />}
                onClick={handleDownload}
                className="w-full bg-blue-600 hover:bg-blue-500 font-bold rounded-xl h-12 text-sm shadow-md shadow-blue-500/20 border-0"
              >
                TẢI XUỐNG TÀI LIỆU
              </Button>

              <div className="grid grid-cols-3 gap-2">
                <Button
                  onClick={handleToggleFavorite}
                  icon={isFavorited ? <HeartFilled className="text-red-500" /> : <HeartOutlined />}
                  className={`rounded-lg h-9 text-xs font-semibold ${isFavorited ? 'border-red-200 bg-red-50 text-red-600' : ''}`}
                >
                  {isFavorited ? 'Đã lưu' : 'Lưu'}
                </Button>

                <Button
                  onClick={() => setRateModalOpen(true)}
                  icon={<StarOutlined className="text-amber-500" />}
                  className="rounded-lg h-9 text-xs font-semibold"
                >
                  Đánh giá
                </Button>

                <Button
                  onClick={() => setReportModalOpen(true)}
                  icon={<FlagOutlined className="text-slate-500" />}
                  className="rounded-lg h-9 text-xs font-semibold"
                >
                  Báo cáo
                </Button>
              </div>

              <Divider className="my-3" />

              {/* Uploader Profile Banner */}
              <div className="p-3.5 bg-slate-50 dark:bg-slate-800/50 rounded-xl border border-slate-100 dark:border-slate-800 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Avatar 
                    src={formatAvatarUrl(document.uploaderAvatar || document.userAvatar, document.uploaderName || document.userName)} 
                    icon={<UserOutlined />} 
                    className="bg-blue-600 shrink-0 font-bold" 
                    size={36}
                  />
                  <div>
                    <div className="text-xs font-bold text-slate-900 dark:text-white">
                      {document.uploaderName || document.userName || 'Thành viên'}
                    </div>
                    <div className="text-[10px] text-slate-400">
                      Đã đóng góp tài liệu
                    </div>
                  </div>
                </div>
              </div>

              <Divider className="my-3" />

              {/* Academic Hierarchy Metadata Table */}
              <div className="space-y-3 text-xs">
                <h4 className="font-bold text-xs text-slate-900 dark:text-white m-0 uppercase tracking-wider text-slate-400">
                  Thông tin học thuật
                </h4>

                <div className="space-y-2.5 text-slate-600 dark:text-slate-400">
                  <div className="flex justify-between items-center">
                    <span>Trường học:</span>
                    <span className="font-semibold text-slate-900 dark:text-white text-right max-w-[180px] truncate">
                      {document.schoolName || 'N/A'}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span>Khoa / Viện:</span>
                    <span className="font-semibold text-slate-900 dark:text-white text-right max-w-[180px] truncate">
                      {document.facultyName || 'N/A'}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span>Môn học:</span>
                    <span className="font-bold text-blue-600 text-right max-w-[180px] truncate">
                      {document.subjectName || 'N/A'}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span>Loại tài liệu:</span>
                    <span className="font-semibold text-slate-900 dark:text-white text-right">
                      {document.documentTypeName || 'Giáo trình'}
                    </span>
                  </div>

                  <div className="flex justify-between items-center">
                    <span>Năm học:</span>
                    <span className="font-semibold text-slate-900 dark:text-white text-right">
                      {document.academicYearName || '2023-2024'}
                    </span>
                  </div>
                </div>
              </div>

            </div>

            {/* Related Documents Grid Widget */}
            {relatedDocuments.length > 0 && (
              <div className="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-4">
                <h3 className="text-xs font-bold text-slate-900 dark:text-white m-0 uppercase tracking-wider flex items-center gap-2">
                  <BookOutlined className="text-blue-600" /> Tài liệu cùng môn học
                </h3>

                <div className="space-y-3">
                  {relatedDocuments.map((doc) => (
                    <Link
                      key={doc.id}
                      to={`/documents/${doc.slug || doc.id}`}
                      className="no-underline block p-3 rounded-xl border border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-800/30 hover:border-blue-300 transition-all"
                    >
                      <div className="font-bold text-xs text-slate-900 dark:text-white truncate">
                        {doc.title}
                      </div>
                      <div className="flex items-center justify-between text-[10px] text-slate-400 mt-1">
                        <span>{doc.fileType || 'PDF'} • {doc.downloadCount || 0} lượt tải</span>
                        <span className="text-amber-500 font-bold">⭐ {doc.averageRating ? Number(doc.averageRating).toFixed(1) : '5.0'}</span>
                      </div>
                    </Link>
                  ))}
                </div>
              </div>
            )}

          </div>

        </div>

      </div>

      {/* Rating Modal */}
      <Modal
        title={<span className="font-bold text-slate-900">Đánh giá tài liệu: {document.title}</span>}
        open={rateModalOpen}
        onOk={handleSubmitRating}
        onCancel={() => setRateModalOpen(false)}
        confirmLoading={submittingRating}
        okText="Gửi đánh giá"
        cancelText="Hủy"
        className="rounded-2xl"
      >
        <div className="py-4 space-y-4 text-center">
          <div>
            <div className="text-xs font-semibold text-slate-500 mb-2">Chọn số sao đánh giá:</div>
            <Rate value={rateScore} onChange={setRateScore} className="text-2xl text-amber-500" />
          </div>

          <TextArea
            rows={3}
            placeholder="Nhận xét chi tiết về chất lượng tài liệu (không bắt buộc)..."
            value={rateComment}
            onChange={(e) => setRateComment(e.target.value)}
            className="rounded-xl text-xs text-left"
          />
        </div>
      </Modal>

      {/* Report Modal */}
      <Modal
        title={<span className="font-bold text-red-600 flex items-center gap-2"><FlagOutlined /> Báo cáo tài liệu vi phạm</span>}
        open={reportModalOpen}
        onOk={handleSubmitReport}
        onCancel={() => setReportModalOpen(false)}
        confirmLoading={submittingReport}
        okText="Gửi báo cáo"
        cancelText="Hủy"
        okButtonProps={{ danger: true }}
        className="rounded-2xl"
      >
        <div className="py-4 space-y-4 text-xs">
          <div>
            <label className="font-bold text-slate-700 dark:text-slate-300 block mb-1">Lý do báo cáo vi phạm:</label>
            <Select
              value={reportReason}
              onChange={setReportReason}
              className="w-full text-xs"
              options={[
                { value: 'Copyright', label: 'Vi phạm bản quyền tác giả' },
                { value: 'InappropriateContent', label: 'Nội dung không phù hợp / phản cảm' },
                { value: 'LowQuality', label: 'Tài liệu lỗi, mờ, không đọc được' },
                { value: 'WrongCategory', label: 'Sai môn học / sai phân loại' },
                { value: 'Spam', label: 'Nội dung rác / Spam / Quảng cáo' },
              ]}
            />
          </div>

          <div>
            <label className="font-bold text-slate-700 dark:text-slate-300 block mb-1">Mô tả chi tiết vi phạm:</label>
            <TextArea
              rows={3}
              placeholder="Vui lòng cung cấp chi tiết vi phạm để Ban Quản Trị xem xét xử lý..."
              value={reportDescription}
              onChange={(e) => setReportDescription(e.target.value)}
              className="rounded-xl text-xs"
            />
          </div>
        </div>
      </Modal>

    </div>
  );
};

export default DocumentDetailPage;
