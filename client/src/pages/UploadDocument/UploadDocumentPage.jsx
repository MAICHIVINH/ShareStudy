import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Select, Button, Upload, Progress, message, Card, Tag } from 'antd';
import { 
  InboxOutlined, 
  CloudUploadOutlined, 
  FilePdfOutlined, 
  FileWordOutlined, 
  FileExcelOutlined, 
  FilePptOutlined, 
  FileZipOutlined, 
  FileTextOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { educationService } from '../../services/educationService';
import { documentService } from '../../services/documentService';

const { TextArea } = Input;
const { Dragger } = Upload;

export const UploadDocumentPage = () => {
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [fileList, setFileList] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);

  // Dependent Options States
  const [educationLevels, setEducationLevels] = useState([]);
  const [schools, setSchools] = useState([]);
  const [faculties, setFaculties] = useState([]);
  const [majors, setMajors] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [documentTypes, setDocumentTypes] = useState([]);
  const [academicYears, setAcademicYears] = useState([]);

  // Selected values for dependent dropdowns
  const [selectedLevel, setSelectedLevel] = useState(null);
  const [selectedSchool, setSelectedSchool] = useState(null);
  const [selectedFaculty, setSelectedFaculty] = useState(null);
  const [selectedMajor, setSelectedMajor] = useState(null);

  useEffect(() => {
    fetchInitialOptions();
  }, []);

  const fetchInitialOptions = async () => {
    try {
      const [levelsRes, typesRes, yearsRes] = await Promise.allSettled([
        educationService.getEducationLevels(),
        educationService.getDocumentTypes(),
        educationService.getAcademicYears(),
      ]);

      if (levelsRes.status === 'fulfilled' && levelsRes.value?.data) {
        setEducationLevels(levelsRes.value.data || []);
      }
      if (typesRes.status === 'fulfilled' && typesRes.value?.data) {
        setDocumentTypes(typesRes.value.data || []);
      }
      if (yearsRes.status === 'fulfilled' && yearsRes.value?.data) {
        setAcademicYears(yearsRes.value.data || []);
      }
    } catch (err) {
      console.error('Error fetching options:', err);
    }
  };

  // Dependent dropdown handlers
  const handleLevelChange = (val) => {
    setSelectedLevel(val);
    setSelectedSchool(null);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    form.setFieldsValue({ schoolId: undefined, facultyId: undefined, majorId: undefined, subjectId: undefined });

    if (val) {
      const level = educationLevels.find((l) => l.id === val);
      if (level && (level.code === 'DH' || level.code === 'CD' || level.name.toLowerCase().includes('đại học') || level.name.toLowerCase().includes('cao đẳng'))) {
        educationService.getSchoolsByLevel(val).then((res) => setSchools(res.data || []));
      } else {
        setSchools([]);
      }
    } else {
      setSchools([]);
    }
  };

  const handleSchoolChange = (val) => {
    setSelectedSchool(val);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    form.setFieldsValue({ facultyId: undefined, majorId: undefined, subjectId: undefined });

    if (val) {
      educationService.getFacultiesBySchool(val).then((res) => setFaculties(res.data || []));
    } else {
      setFaculties([]);
    }
  };

  const handleFacultyChange = (val) => {
    setSelectedFaculty(val);
    setSelectedMajor(null);
    form.setFieldsValue({ majorId: undefined, subjectId: undefined });

    if (val) {
      educationService.getMajorsByFaculty(val).then((res) => setMajors(res.data || []));
    } else {
      setMajors([]);
    }
  };

  const handleMajorChange = (val) => {
    setSelectedMajor(val);
    form.setFieldsValue({ subjectId: undefined });

    if (val) {
      educationService.getSubjectsByMajor(val).then((res) => setSubjects(res.data || []));
    } else {
      setSubjects([]);
    }
  };

  // Format file size
  const formatFileSize = (bytes) => {
    if (!bytes) return '0 KB';
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  // Get file icon based on extension
  const getFileIcon = (fileName) => {
    const ext = (fileName || '').split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf': return <FilePdfOutlined className="text-red-500 text-3xl" />;
      case 'doc':
      case 'docx': return <FileWordOutlined className="text-blue-500 text-3xl" />;
      case 'xls':
      case 'xlsx': return <FileExcelOutlined className="text-emerald-500 text-3xl" />;
      case 'ppt':
      case 'pptx': return <FilePptOutlined className="text-orange-500 text-3xl" />;
      case 'zip':
      case 'rar': return <FileZipOutlined className="text-purple-500 text-3xl" />;
      default: return <FileTextOutlined className="text-slate-500 text-3xl" />;
    }
  };

  // File Upload Drag & Drop Props
  const draggerProps = {
    name: 'file',
    multiple: false,
    maxCount: 1,
    fileList,
    showUploadList: false,
    beforeUpload: (file) => {
      const allowedExts = ['pdf', 'doc', 'docx', 'ppt', 'pptx', 'xls', 'xlsx', 'zip', 'rar'];
      const fileExt = file.name.split('.').pop()?.toLowerCase();
      const isAllowed = allowedExts.includes(fileExt);

      if (!isAllowed) {
        message.error(`Chỉ hỗ trợ định dạng: ${allowedExts.join(', ')}`);
        return Upload.LIST_IGNORE;
      }

      const isLt50M = file.size / 1024 / 1024 < 50;
      if (!isLt50M) {
        message.error('Dung lượng file tối đa là 50MB!');
        return Upload.LIST_IGNORE;
      }

      setFileList([file]);
      form.setFieldsValue({ fileList: [file] });

      // Auto-fill title if title is empty
      if (!form.getFieldValue('title')) {
        const titleWithoutExt = file.name.substring(0, file.name.lastIndexOf('.')) || file.name;
        form.setFieldsValue({ title: titleWithoutExt });
      }

      return false; // Prevent automatic upload
    },
    onRemove: () => {
      setFileList([]);
      form.setFieldsValue({ fileList: [] });
    },
  };

  const handleRemoveFile = (e) => {
    e.stopPropagation();
    setFileList([]);
    form.setFieldsValue({ fileList: [] });
  };

  const onFinish = async (values) => {
    if (fileList.length === 0) {
      message.warning('Vui lòng chọn file tài liệu để tải lên!');
      return;
    }

    setUploading(true);
    setUploadProgress(30);

    try {
      const formData = new FormData();
      formData.append('file', fileList[0]);
      formData.append('title', values.title.trim());
      if (values.description) formData.append('description', values.description.trim());
      formData.append('educationLevelId', values.educationLevelId);
      if (values.schoolId) formData.append('schoolId', values.schoolId);
      if (values.facultyId) formData.append('facultyId', values.facultyId);
      if (values.majorId) formData.append('majorId', values.majorId);
      if (values.subjectId) formData.append('subjectId', values.subjectId);
      formData.append('documentTypeId', values.documentTypeId);
      if (values.academicYearId) formData.append('academicYearId', values.academicYearId);
      if (values.tags && values.tags.length > 0) {
        formData.append('tags', values.tags.join(','));
      }

      setUploadProgress(70);
      await documentService.uploadDocument(formData);
      setUploadProgress(100);

      message.success('Đăng tài liệu thành công! Trạng thái: Đang chờ duyệt.');
      setTimeout(() => {
        navigate('/profile?tab=uploads');
      }, 1000);
    } catch (err) {
      message.error(err.message || 'Tải lên tài liệu thất bại');
      setUploadProgress(0);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      
      {/* Page Header */}
      <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4 transition-colors">
        <div className="w-12 h-12 rounded-2xl bg-blue-600 flex items-center justify-center text-white text-xl shadow-md shadow-blue-500/20 shrink-0">
          <CloudUploadOutlined />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white m-0 tracking-tight">
            Đóng góp tài liệu học tập
          </h1>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
            Chia sẻ kiến thức đến hàng ngàn học sinh, sinh viên trên toàn quốc
          </p>
        </div>
      </div>

      {/* Main Upload Form Card */}
      <Card className="rounded-3xl border-slate-200 dark:border-slate-800 shadow-sm bg-white dark:bg-slate-900">
        <Form
          form={form}
          layout="vertical"
          onFinish={onFinish}
          requiredMark={false}
          className="space-y-6"
        >
          
          {/* Step 1: File Drag & Drop Box */}
          <div className="space-y-3">
            <div className="text-xs font-bold text-slate-800 dark:text-slate-200 uppercase tracking-wider block">
              1. CHỌN FILE TÀI LIỆU (TỐI ĐA 50MB) <span className="text-red-500">*</span>
            </div>

            <Form.Item
              name="fileList"
              rules={[{ required: true, message: 'Vui lòng chọn file tài liệu để đăng!' }]}
              className="m-0"
            >
              <Dragger {...draggerProps}>
                {fileList.length === 0 ? (
                  /* Initial Empty Drop State */
                  <div className="py-8 flex flex-col items-center justify-center space-y-3">
                    <div className="w-16 h-16 rounded-2xl bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 flex items-center justify-center text-3xl shadow-sm">
                      <InboxOutlined />
                    </div>
                    <div>
                      <p className="text-sm font-bold text-slate-800 dark:text-slate-200 m-0">
                        Kéo thả file vào đây hoặc <span className="text-blue-600 dark:text-blue-400 underline">bấm để duyệt file</span>
                      </p>
                      <p className="text-xs text-slate-400 mt-1.5 m-0">
                        Hỗ trợ định dạng: PDF, DOC, DOCX, PPT, PPTX, XLS, XLSX, ZIP, RAR (Dung lượng tối đa 50MB)
                      </p>
                    </div>
                  </div>
                ) : (
                  /* Selected File State INSIDE Dragger */
                  <div className="py-6 px-4 flex flex-col sm:flex-row items-center justify-between gap-4">
                    <div className="flex items-center gap-4 text-left">
                      {getFileIcon(fileList[0].name)}
                      <div>
                        <div className="text-sm font-bold text-slate-900 dark:text-white line-clamp-1">
                          {fileList[0].name}
                        </div>
                        <div className="text-xs text-slate-500 flex items-center gap-2 mt-1">
                          <span>Dung lượng: {formatFileSize(fileList[0].size)}</span>
                          <span>•</span>
                          <Tag color="success" icon={<CheckCircleOutlined />} className="m-0 text-[11px] font-semibold">
                            Đã chọn file
                          </Tag>
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-2">
                      <span className="text-xs text-blue-600 font-semibold flex items-center gap-1 hover:underline">
                        <ReloadOutlined /> Bấm để đổi file khác
                      </span>
                      <Button
                        type="text"
                        danger
                        icon={<DeleteOutlined />}
                        onClick={handleRemoveFile}
                        className="hover:bg-red-50"
                        title="Xóa file"
                      />
                    </div>
                  </div>
                )}
              </Dragger>
            </Form.Item>

            {uploading && (
              <Progress percent={uploadProgress} status="active" className="mt-3" />
            )}
          </div>

          {/* Step 2: Basic Info */}
          <div className="space-y-4 pt-4 border-t border-slate-100 dark:border-slate-800">
            <div className="text-xs font-bold text-slate-800 dark:text-slate-200 uppercase tracking-wider block">
              2. THÔNG TIN BẮT BUỘC
            </div>

            <Form.Item
              label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Tiêu đề tài liệu <span className="text-red-500">*</span></span>}
              name="title"
              rules={[{ required: true, message: 'Vui lòng nhập tiêu đề tài liệu!' }]}
            >
              <Input 
                placeholder="VD: Giáo trình Cơ sở dữ liệu - ĐH Bách Khoa 2024"
                className="rounded-xl h-11 text-xs"
              />
            </Form.Item>

            <Form.Item
              label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Mô tả tài liệu</span>}
              name="description"
            >
              <TextArea
                rows={4}
                placeholder="Mô tả tóm tắt nội dung tài liệu, môn học, giáo viên hướng dẫn..."
                className="rounded-xl text-xs"
              />
            </Form.Item>
          </div>

          {/* Step 3: Dependent Education Hierarchy */}
          <div className="space-y-4 pt-4 border-t border-slate-100 dark:border-slate-800">
            <div className="text-xs font-bold text-slate-800 dark:text-slate-200 uppercase tracking-wider block">
              3. PHÂN LOẠI GIÁO DỤC (BỘ LỌC PHỤ THUỘC)
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              
              {/* 1. Cấp học */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Cấp học <span className="text-red-500">*</span></span>}
                name="educationLevelId"
                rules={[{ required: true, message: 'Vui lòng chọn cấp học!' }]}
              >
                <Select
                  placeholder="Chọn cấp học"
                  onChange={handleLevelChange}
                  options={educationLevels.map((l) => ({ value: l.id, label: l.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* 2. Trường (Hiện khi Cấp học = Đại học/Cao đẳng) */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Trường</span>}
                name="schoolId"
              >
                <Select
                  placeholder="Chọn trường đại học / cao đẳng"
                  onChange={handleSchoolChange}
                  disabled={schools.length === 0}
                  options={schools.map((s) => ({ value: s.id, label: `${s.code ? s.code + ' - ' : ''}${s.name}` }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* 3. Khoa */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Khoa</span>}
                name="facultyId"
              >
                <Select
                  placeholder="Chọn khoa"
                  onChange={handleFacultyChange}
                  disabled={faculties.length === 0}
                  options={faculties.map((f) => ({ value: f.id, label: f.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* 4. Ngành học */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Ngành học</span>}
                name="majorId"
              >
                <Select
                  placeholder="Chọn ngành học"
                  onChange={handleMajorChange}
                  disabled={majors.length === 0}
                  options={majors.map((m) => ({ value: m.id, label: m.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* 5. Môn học */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Môn học</span>}
                name="subjectId"
              >
                <Select
                  placeholder="Chọn môn học"
                  disabled={subjects.length === 0}
                  options={subjects.map((sj) => ({ value: sj.id, label: sj.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* Loại tài liệu */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Loại tài liệu <span className="text-red-500">*</span></span>}
                name="documentTypeId"
                rules={[{ required: true, message: 'Vui lòng chọn loại tài liệu!' }]}
              >
                <Select
                  placeholder="Chọn loại tài liệu"
                  options={documentTypes.map((t) => ({ value: t.id, label: t.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* Năm học */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Khóa / Năm học</span>}
                name="academicYearId"
              >
                <Select
                  placeholder="Chọn năm học"
                  options={academicYears.map((y) => ({ value: y.id, label: y.name }))}
                  className="h-11 rounded-xl"
                />
              </Form.Item>

              {/* Tags */}
              <Form.Item
                label={<span className="text-xs font-bold text-slate-700 dark:text-slate-300">Thẻ / Tags (Nhấn Enter để thêm)</span>}
                name="tags"
              >
                <Select
                  mode="tags"
                  placeholder="VD: csharp, csdl, bachkhoa..."
                  className="rounded-xl"
                />
              </Form.Item>

            </div>
          </div>

          {/* Submit Button */}
          <div className="pt-4 border-t border-slate-100 dark:border-slate-800 flex justify-end">
            <Button
              type="primary"
              htmlType="submit"
              loading={uploading}
              icon={<CloudUploadOutlined />}
              className="bg-blue-600 hover:bg-blue-700 font-bold rounded-2xl h-12 px-8 text-sm shadow-md shadow-blue-500/20"
            >
              ĐĂNG TÀI LIỆU
            </Button>
          </div>

        </Form>
      </Card>

    </div>
  );
};

export default UploadDocumentPage;
