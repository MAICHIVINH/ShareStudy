import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Tag, Tooltip, message } from 'antd';
import {
  FilePdfOutlined,
  FileWordOutlined,
  FileExcelOutlined,
  FilePptOutlined,
  FileZipOutlined,
  FileTextOutlined,
  EyeOutlined,
  DownloadOutlined,
  StarFilled,
  HeartOutlined,
  HeartFilled
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { favoriteService } from '../../services/favoriteService';
import { documentService } from '../../services/documentService';
import { useAuth } from '../../contexts/AuthContext';
import { formatAvatarUrl } from '../../utils/avatarUtils';

export const DocumentCard = ({ document, onFavoriteToggle }) => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [isFavorited, setIsFavorited] = useState(document?.isFavorite || false);
  const [loadingFav, setLoadingFav] = useState(false);

  useEffect(() => {
    setIsFavorited(document?.isFavorite || false);
  }, [document?.isFavorite]);

  if (!document) return null;

  // File Format Badge Color & Icon Helper
  const getFileBadge = (fileType) => {
    const type = (fileType || '').toLowerCase();
    switch (type) {
      case 'pdf':
        return { color: 'red', label: 'PDF', icon: <FilePdfOutlined /> };
      case 'doc':
      case 'docx':
        return { color: 'blue', label: 'WORD', icon: <FileWordOutlined /> };
      case 'xls':
      case 'xlsx':
        return { color: 'green', label: 'EXCEL', icon: <FileExcelOutlined /> };
      case 'ppt':
      case 'pptx':
        return { color: 'orange', label: 'POWERPOINT', icon: <FilePptOutlined /> };
      case 'zip':
      case 'rar':
        return { color: 'purple', label: 'ARCHIVE', icon: <FileZipOutlined /> };
      default:
        return { color: 'default', label: type.toUpperCase() || 'FILE', icon: <FileTextOutlined /> };
    }
  };

  const badge = getFileBadge(document.fileType);

  // Format file size
  const formatFileSize = (bytes) => {
    if (!bytes) return '0 KB';
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(0)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  // Toggle favorite handler
  const handleToggleFavorite = async (e) => {
    e.stopPropagation();
    if (!user) {
      message.info('Vui lòng đăng nhập để lưu tài liệu yêu thích!');
      navigate('/login');
      return;
    }

    if (loadingFav) return;
    setLoadingFav(true);

    try {
      if (isFavorited) {
        await favoriteService.removeFavorite(document.id);
        setIsFavorited(false);
        message.success('Đã xóa khỏi danh sách yêu thích!');
      } else {
        await favoriteService.addFavorite(document.id);
        setIsFavorited(true);
        message.success('Đã thêm vào danh sách yêu thích!');
      }

      if (onFavoriteToggle) {
        onFavoriteToggle(document.id, !isFavorited);
      }
    } catch (err) {
      message.error(err.message || 'Thao tác không thành công!');
    } finally {
      setLoadingFav(false);
    }
  };

  // Download handler
  const handleDownload = async (e) => {
    e.stopPropagation();
    try {
      const res = await documentService.downloadDocument(document.id);
      if (res.data?.downloadUrl) {
        window.open(res.data.downloadUrl, '_blank');
      }
    } catch (err) {
      message.error(err.message || 'Không thể tải tài liệu về lúc này.');
    }
  };

  const authorName = document.uploaderName || document.userName || document.user?.fullName || 'Thành viên';
  const authorAvatar = formatAvatarUrl(document.uploaderAvatar || document.userAvatar || document.user?.avatarUrl, authorName);

  return (
    <div className="bg-white rounded-xl border border-solid border-slate-200 shadow-sm hover:!border-primary transition-all duration-300 flex flex-col justify-between p-5 group">

      {/* Top Section */}
      <div>
        {/* Category Breadcrumbs & Format Badge */}
        <div className="flex items-center justify-between gap-2 mb-3">
          <div className="text-[11px] font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-lg truncate max-w-[70%]">
            {document.categorySummary || document.subjectName || document.educationLevelName || 'Tài liệu học tập'}
          </div>

          <Tag color={badge.color} className="m-0 font-bold text-[10px] rounded-md px-1.5 py-0.5 flex items-center gap-1 border-0">
            {badge.icon}
            <span>{badge.label}</span>
          </Tag>
        </div>

        {/* Title */}
        <Link
          to={`/documents/${document.slug || document.id}`}
          className="text-slate-900 font-bold text-sm line-clamp-2 leading-snug hover:text-blue-600 transition-colors no-underline block mb-2"
        >
          {document.title}
        </Link>

        {/* Metadata info */}
        <div className="flex items-center gap-3 text-xs text-slate-500 mb-4">
          <span className="bg-slate-100 px-2 py-0.5 rounded text-[11px] font-medium text-slate-600">
            {formatFileSize(document.fileSize)}
          </span>
          {document.schoolName && (
            <span className="truncate max-w-[150px] text-slate-400">
              {document.schoolName}
            </span>
          )}
        </div>
      </div>

      {/* Bottom Section */}
      <div>
        {/* Stats bar */}
        <div className="flex items-center justify-between text-xs text-slate-500 pt-3 border-t border-slate-100 mb-3">
          <div className="flex items-center gap-1">
            <EyeOutlined className="text-slate-400" />
            <span>{document.viewCount || 0}</span>
          </div>

          <div className="flex items-center gap-1">
            <DownloadOutlined className="text-slate-400" />
            <span>{document.downloadCount || 0}</span>
          </div>

          <div className="flex items-center gap-1 font-semibold text-amber-500">
            <StarFilled className="text-amber-400 text-xs" />
            <span>{document.ratingCount > 0 && document.averageRating ? Number(document.averageRating).toFixed(1) : '0.0'}</span>
          </div>

          <div>
            {dayjs(document.createdAt).format('DD/MM/YYYY')}
          </div>
        </div>

        {/* Author & Action Buttons */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 max-w-[55%]">
            <img
              src={authorAvatar}
              alt={authorName}
              className="w-6 h-6 rounded-full object-cover border border-slate-200 shrink-0"
            />
            <span className="text-xs text-slate-700 font-medium truncate">
              {authorName}
            </span>
          </div>

          <div className="flex items-center gap-1.5">
            <Tooltip title={isFavorited ? 'Bỏ yêu thích' : 'Lưu yêu thích'}>
              <button
                onClick={handleToggleFavorite}
                disabled={loadingFav}
                className={`bg-transparent border-none shadow-none p-1.5 rounded-lg text-base transition-colors cursor-pointer ${isFavorited
                    ? 'text-red-600'
                    : 'text-slate-500 hover:text-red-500'
                  }`}
              >
                {isFavorited ? <HeartFilled className="text-red-500" /> : <HeartOutlined />}
              </button>
            </Tooltip>

            <Tooltip title="Tải xuống tài liệu">
              <button
                onClick={handleDownload}
                className="bg-transparent border-none shadow-none p-1.5 rounded-lg text-slate-500 hover:text-blue-600 text-base transition-colors cursor-pointer"
              >
                <DownloadOutlined />
              </button>
            </Tooltip>
          </div>
        </div>

      </div>

    </div>
  );
};

export default DocumentCard;
