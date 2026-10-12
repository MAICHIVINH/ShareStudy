import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Input, Button, Drawer, Pagination, Skeleton, Empty } from 'antd';
import { FilterOutlined, SearchOutlined } from '@ant-design/icons';
import { documentService } from '../../services/documentService';
import { educationService } from '../../services/educationService';
import { DocumentCard } from '../../components/documents/DocumentCard';
import { FilterPanel } from '../../components/documents/FilterPanel';

export const DocumentsPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  // State derived from URL search params
  const keyword = searchParams.get('keyword') || '';
  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '12', 10);
  const type = searchParams.get('type') || searchParams.get('documentTypeName') || '';
  const documentTypeId = searchParams.get('documentTypeId') ? Number(searchParams.get('documentTypeId')) : null;
  const sortBy = searchParams.get('sortBy') || searchParams.get('sort') || 'newest';

  const educationLevelId = searchParams.get('educationLevelId') ? Number(searchParams.get('educationLevelId')) : null;
  const schoolId = searchParams.get('schoolId') ? Number(searchParams.get('schoolId')) : null;
  const facultyId = searchParams.get('facultyId') ? Number(searchParams.get('facultyId')) : null;
  const majorId = searchParams.get('majorId') ? Number(searchParams.get('majorId')) : null;
  const subjectId = searchParams.get('subjectId') ? Number(searchParams.get('subjectId')) : null;
  const academicYearId = searchParams.get('academicYearId') ? Number(searchParams.get('academicYearId')) : null;
  const minRating = searchParams.get('minRating') ? Number(searchParams.get('minRating')) : null;
  const fileTypes = searchParams.get('fileTypes') || searchParams.get('fileType') || '';

  // Local state
  const [searchInputValue, setSearchInputValue] = useState(keyword);
  const [documents, setDocuments] = useState([]);
  const [documentTypes, setDocumentTypes] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [drawerOpen, setDrawerOpen] = useState(false);

  // Fetch document types from database on mount
  useEffect(() => {
    fetchDocumentTypes();
  }, []);

  const fetchDocumentTypes = async () => {
    try {
      const res = await educationService.getDocumentTypes();
      if (res.data) {
        setDocumentTypes(res.data || []);
      }
    } catch (err) {
      console.error('Error fetching document types:', err);
    }
  };

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchInputValue !== keyword) {
        updateQueryParams({ keyword: searchInputValue, page: 1 });
      }
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInputValue]);

  useEffect(() => {
    fetchDocuments();
  }, [searchParams]);

  const fetchDocuments = async () => {
    setLoading(true);
    try {
      const params = {
        keyword,
        pageNumber: page,
        pageSize,
        sort: sortBy,
        sortBy,
        educationLevelId,
        schoolId,
        facultyId,
        majorId,
        subjectId,
        academicYearId,
        minRating,
        fileType: fileTypes,
      };

      if (documentTypeId) {
        params.documentTypeId = documentTypeId;
      }
      if (type) {
        params.documentTypeName = type;
      }

      const res = await documentService.getDocuments(params);
      if (res.data) {
        setDocuments(res.data.items || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (err) {
      console.error('Error fetching documents:', err);
    } finally {
      setLoading(false);
    }
  };

  const updateQueryParams = (newParams) => {
    const params = new URLSearchParams(searchParams);
    Object.entries(newParams).forEach(([key, val]) => {
      if (val !== null && val !== undefined && val !== '') {
        params.set(key, val);
      } else {
        params.delete(key);
      }
    });
    setSearchParams(params);
  };

  const handleApplyFilter = (filterValues) => {
    // If user chose documentTypeIds array from FilterPanel
    let docTypeId = null;
    if (filterValues.documentTypeIds && filterValues.documentTypeIds.length > 0) {
      docTypeId = filterValues.documentTypeIds[0];
    } else if (filterValues.documentTypeId) {
      docTypeId = filterValues.documentTypeId;
    }

    updateQueryParams({
      ...filterValues,
      documentTypeId: docTypeId,
      type: '', // Reset text category when filter panel is applied
      page: 1,
    });
    setDrawerOpen(false);
  };

  const handleResetFilter = () => {
    setSearchParams(new URLSearchParams());
    setSearchInputValue('');
    setDrawerOpen(false);
  };

  // Build categories array combining 'Tất cả' + DB Document Types
  const categories = [
    { label: 'Tất cả', value: '' },
    ...documentTypes.map((t) => ({ label: t.name, value: t.name })),
  ];

  const currentFilters = {
    educationLevelId,
    schoolId,
    facultyId,
    majorId,
    subjectId,
    academicYearId,
    documentTypeIds: documentTypeId ? [documentTypeId] : [],
    minRating,
    fileTypes,
    sortBy,
  };

  return (
    <div className="space-y-6">
      
      {/* Top Header & Search Bar */}
      <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 m-0 tracking-tight">
            Danh sách tài liệu học tập
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            Hiển thị {totalCount} tài liệu phù hợp
          </p>
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          {/* Search Box */}
          <Input
            placeholder="Tìm kiếm tài liệu..."
            value={searchInputValue}
            onChange={(e) => setSearchInputValue(e.target.value)}
            prefix={<SearchOutlined className="text-slate-400" />}
            allowClear
            className="w-full sm:w-64 rounded-xl"
          />

          {/* Mobile Filter Button */}
          <Button
            icon={<FilterOutlined />}
            onClick={() => setDrawerOpen(true)}
            className="lg:hidden rounded-xl border-slate-200 font-semibold"
          >
            Bộ lọc
          </Button>
        </div>
      </div>

      {/* Main 2-Column Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        
        {/* Left Sidebar Filter (Desktop) */}
        <div className="hidden lg:block lg:col-span-3 sticky top-24">
          <FilterPanel
            filters={currentFilters}
            onApplyFilter={handleApplyFilter}
            onResetFilter={handleResetFilter}
          />
        </div>

        {/* Mobile Filter Drawer */}
        <Drawer
          title="Bộ lọc tài liệu"
          placement="left"
          onClose={() => setDrawerOpen(false)}
          open={drawerOpen}
          width={320}
        >
          <FilterPanel
            filters={currentFilters}
            onApplyFilter={handleApplyFilter}
            onResetFilter={handleResetFilter}
          />
        </Drawer>

        {/* Right Content Area */}
        <div className="lg:col-span-9 space-y-6">
          
          {loading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <div key={i} className="bg-white p-4 rounded-2xl border border-slate-200 space-y-3">
                  <Skeleton.Avatar active shape="square" className="w-full h-32 rounded-xl" />
                  <Skeleton active paragraph={{ rows: 2 }} />
                </div>
              ))}
            </div>
          ) : documents.length === 0 ? (
            <div className="bg-white p-12 rounded-2xl border border-slate-200 text-center">
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={<span className="text-slate-500 text-sm">Không tìm thấy tài liệu phù hợp với bộ lọc</span>}
              >
                <Button type="primary" onClick={handleResetFilter} className="bg-blue-600 rounded-xl mt-2">
                  Xóa bộ lọc
                </Button>
              </Empty>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {documents.map((doc) => (
                  <DocumentCard key={doc.id} document={doc} />
                ))}
              </div>

              {totalCount > pageSize && (
                <div className="flex justify-center pt-6">
                  <Pagination
                    current={page}
                    pageSize={pageSize}
                    total={totalCount}
                    onChange={(p) => updateQueryParams({ page: p })}
                    showSizeChanger={false}
                  />
                </div>
              )}
            </>
          )}

        </div>

      </div>

    </div>
  );
};

export default DocumentsPage;
