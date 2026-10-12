import React, { useState, useEffect } from 'react';
import { Card, Table, Button, Modal, Form, Input, Select, Popconfirm, message, Space, Tag, Alert, Dropdown, Tooltip } from 'antd';
import { 
  BankOutlined, 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  ClusterOutlined,
  ReadOutlined,
  ApartmentOutlined,
  FolderOpenOutlined,
  BookOutlined,
  MoreOutlined
} from '@ant-design/icons';
import { educationService } from '../../../services/educationService';
import { adminService } from '../../../services/adminService';

export const AdminEducationPage = () => {
  const [levels, setLevels] = useState([]);
  const [schools, setSchools] = useState([]);
  const [faculties, setFaculties] = useState([]);
  const [majors, setMajors] = useState([]);
  const [subjects, setSubjects] = useState([]);
  
  const [selectedLevel, setSelectedLevel] = useState(null);
  const [selectedSchool, setSelectedSchool] = useState(null);
  const [selectedFaculty, setSelectedFaculty] = useState(null);
  const [selectedMajor, setSelectedMajor] = useState(null);

  // Active view step:
  // Higher Ed: 'school', 'faculty', 'major', 'subject'
  // K12 (THCS/THPT): 'k12_subject'
  const [activeStep, setActiveStep] = useState('school'); 
  const [loading, setLoading] = useState(false);

  // Modal State for Add / Edit
  const [modalVisible, setModalVisible] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchInitialData();
  }, []);

  const isK12 = (level) => level && (level.code === 'THCS' || level.code === 'THPT');

  const fetchInitialData = async () => {
    setLoading(true);
    try {
      const levelRes = await educationService.getEducationLevels();
      if (levelRes.data && Array.isArray(levelRes.data) && levelRes.data.length > 0) {
        setLevels(levelRes.data);
        handleSelectLevel(levelRes.data[0]);
      }
    } catch (err) {
      console.error(err);
      message.error('Lỗi khi tải danh sách cấp học');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectLevel = async (level) => {
    if (!level) return;
    setSelectedLevel(level);
    setSelectedSchool(null);
    setSelectedFaculty(null);
    setSelectedMajor(null);

    if (isK12(level)) {
      setActiveStep('k12_subject');
      fetchSubjectsByLevel(level.id);
    } else {
      setActiveStep('school');
      fetchSchoolsByLevel(level.id);
    }
  };

  const fetchSchoolsByLevel = async (levelId) => {
    setLoading(true);
    try {
      const res = await educationService.getSchoolsByLevel(levelId);
      setSchools(res.data || []);
    } catch (err) {
      console.error(err);
      setSchools([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchFacultiesBySchool = async (school) => {
    if (!school) return;
    setSelectedSchool(school);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    setActiveStep('faculty');
    setLoading(true);
    try {
      const res = await educationService.getFacultiesBySchool(school.id);
      setFaculties(res.data || []);
    } catch (err) {
      console.error(err);
      setFaculties([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchMajorsByFaculty = async (faculty) => {
    if (!faculty) return;
    setSelectedFaculty(faculty);
    setSelectedMajor(null);
    setActiveStep('major');
    setLoading(true);
    try {
      const res = await educationService.getMajorsByFaculty(faculty.id);
      setMajors(res.data || []);
    } catch (err) {
      console.error(err);
      setMajors([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchSubjectsByMajor = async (major) => {
    if (!major) return;
    setSelectedMajor(major);
    setActiveStep('subject');
    setLoading(true);
    try {
      const res = await educationService.getSubjectsByMajor(major.id);
      setSubjects(res.data || []);
    } catch (err) {
      console.error(err);
      setSubjects([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchSubjectsByLevel = async (levelId) => {
    setLoading(true);
    try {
      const res = await educationService.getAllSubjects({ educationLevelId: levelId, pageSize: 100 });
      if (res.data) {
        setSubjects(res.data.items || res.data || []);
      } else {
        setSubjects([]);
      }
    } catch (err) {
      console.error(err);
      setSubjects([]);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenAddModal = () => {
    setEditingItem(null);
    form.resetFields();
    if (activeStep === 'subject' || activeStep === 'k12_subject') {
      form.setFieldsValue({ credits: 3 });
    }
    setModalVisible(true);
  };

  const handleOpenEditModal = (item) => {
    setEditingItem(item);
    form.setFieldsValue({
      name: item.name,
      code: item.code,
      description: item.description,
      credits: item.credits,
    });
    setModalVisible(true);
  };

  const handleSubmit = async (values) => {
    setSubmitting(true);
    try {
      const payload = {
        name: values.name.trim(),
        code: values.code.trim().toUpperCase(),
        description: values.description?.trim() || null,
      };

      if (activeStep === 'school') {
        if (editingItem) {
          await adminService.updateSchool(editingItem.id, { ...payload, educationLevelId: selectedLevel.id });
        } else {
          await adminService.createSchool({ ...payload, educationLevelId: selectedLevel.id });
        }
        fetchSchoolsByLevel(selectedLevel.id);
      } else if (activeStep === 'faculty') {
        if (editingItem) {
          await adminService.updateFaculty(editingItem.id, { ...payload, schoolId: selectedSchool.id });
        } else {
          await adminService.createFaculty({ ...payload, schoolId: selectedSchool.id });
        }
        fetchFacultiesBySchool(selectedSchool);
      } else if (activeStep === 'major') {
        if (editingItem) {
          await adminService.updateMajor(editingItem.id, { ...payload, facultyId: selectedFaculty.id });
        } else {
          await adminService.createMajor({ ...payload, facultyId: selectedFaculty.id });
        }
        fetchMajorsByFaculty(selectedFaculty);
      } else if (activeStep === 'subject' || activeStep === 'k12_subject') {
        const subjectPayload = {
          ...payload,
          credits: Number(values.credits) || 0,
          educationLevelId: selectedLevel?.id || null,
          majorId: isK12(selectedLevel) ? null : (selectedMajor?.id || null),
        };
        if (editingItem) {
          await adminService.updateSubject(editingItem.id, subjectPayload);
        } else {
          await adminService.createSubject(subjectPayload);
        }
        
        if (isK12(selectedLevel)) {
          fetchSubjectsByLevel(selectedLevel.id);
        } else if (selectedMajor) {
          fetchSubjectsByMajor(selectedMajor);
        }
      }

      message.success(`${editingItem ? 'Cập nhật' : 'Thêm mới'} thành công!`);
      setModalVisible(false);
    } catch (err) {
      message.error(err.message || 'Thao tác thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    try {
      if (activeStep === 'school') await adminService.deleteSchool(id);
      if (activeStep === 'faculty') await adminService.deleteFaculty(id);
      if (activeStep === 'major') await adminService.deleteMajor(id);
      if (activeStep === 'subject' || activeStep === 'k12_subject') await adminService.deleteSubject(id);

      message.success('Xóa thành công!');
      if (activeStep === 'school') fetchSchoolsByLevel(selectedLevel.id);
      if (activeStep === 'faculty') fetchFacultiesBySchool(selectedSchool);
      if (activeStep === 'major') fetchMajorsByFaculty(selectedFaculty);
      if (activeStep === 'subject') fetchSubjectsByMajor(selectedMajor);
      if (activeStep === 'k12_subject') fetchSubjectsByLevel(selectedLevel.id);
    } catch (err) {
      message.error(err.message || 'Xóa thất bại');
    }
  };

  // Table Columns Setup
  const columns = [
    { title: 'ID', dataIndex: 'id', key: 'id', width: 70, sorter: (a, b) => a.id - b.id },
    { title: 'Mã', dataIndex: 'code', key: 'code', width: 130, sorter: (a, b) => (a.code || '').localeCompare(b.code || ''), render: (c) => <Tag color="blue" className="font-bold">{c || 'N/A'}</Tag> },
    { title: 'Tên danh mục', dataIndex: 'name', key: 'name', sorter: (a, b) => (a.name || '').localeCompare(b.name || ''), render: (n) => <span className="font-bold text-slate-800 text-xs sm:text-sm">{n}</span> },
    ...((activeStep === 'subject' || activeStep === 'k12_subject') ? [
      { title: 'Số tiết / Tín chỉ', dataIndex: 'credits', key: 'credits', width: 140, sorter: (a, b) => (a.credits || 0) - (b.credits || 0) },
      { 
        title: 'Cấp áp dụng', 
        key: 'levelInfo',
        render: (_, r) => (
          <Tag color={isK12(selectedLevel) ? 'green' : 'purple'} className="font-bold text-[10px]">
            {isK12(selectedLevel) ? `Môn ${selectedLevel?.code}` : `Ngành: ${selectedMajor?.name || 'Đại học'}`}
          </Tag>
        ) 
      }
    ] : []),
    {
      title: 'Thao tác',
      key: 'actions',
      width: 100,
      render: (_, record) => {
        const menuItems = [
          activeStep === 'school' && {
            key: 'faculty',
            icon: <FolderOpenOutlined className="text-blue-600" />,
            label: 'Xem Danh sách Khoa',
            onClick: () => fetchFacultiesBySchool(record)
          },
          activeStep === 'faculty' && {
            key: 'major',
            icon: <ApartmentOutlined className="text-purple-600" />,
            label: 'Xem Danh sách Ngành',
            onClick: () => fetchMajorsByFaculty(record)
          },
          activeStep === 'major' && {
            key: 'subject',
            icon: <BookOutlined className="text-emerald-600" />,
            label: 'Xem Danh sách Môn học',
            onClick: () => fetchSubjectsByMajor(record)
          },
          {
            key: 'edit',
            icon: <EditOutlined className="text-amber-600" />,
            label: 'Chỉnh sửa danh mục',
            onClick: () => handleOpenEditModal(record)
          },
          { type: 'divider' },
          {
            key: 'delete',
            danger: true,
            icon: <DeleteOutlined />,
            label: (
              <Popconfirm
                title="Xác nhận xóa danh mục?"
                description="Bạn có chắc muốn xóa không?"
                okText="Xóa"
                cancelText="Hủy"
                okButtonProps={{ danger: true }}
                onConfirm={() => handleDelete(record.id)}
              >
                <span>Xóa danh mục</span>
              </Popconfirm>
            )
          }
        ].filter(Boolean);

        return (
          <Dropdown menu={{ items: menuItems }} placement="bottomRight" trigger={['click']}>
            <Tooltip title="Menu Thao tác">
              <Button size="small" icon={<MoreOutlined />} className="rounded-lg font-bold" />
            </Tooltip>
          </Dropdown>
        );
      },
    },
  ];

  const getCurrentTableData = () => {
    if (activeStep === 'school') return schools || [];
    if (activeStep === 'faculty') return faculties || [];
    if (activeStep === 'major') return majors || [];
    if (activeStep === 'subject' || activeStep === 'k12_subject') return subjects || [];
    return [];
  };

  const getStepTitle = () => {
    if (activeStep === 'k12_subject') return `Môn học ${selectedLevel?.name || ''} (${selectedLevel?.code || ''})`;
    if (activeStep === 'school') return `Trường thuộc ${selectedLevel?.name || ''}`;
    if (activeStep === 'faculty') return `Khoa thuộc ${selectedSchool?.name || ''}`;
    if (activeStep === 'major') return `Ngành thuộc Khoa ${selectedFaculty?.name || ''}`;
    if (activeStep === 'subject') return `Môn học thuộc Ngành ${selectedMajor?.name || ''}`;
    return 'Danh mục';
  };

  const getItemLabel = () => {
    if (activeStep === 'school') return 'Trường';
    if (activeStep === 'faculty') return 'Khoa';
    if (activeStep === 'major') return 'Ngành';
    return 'Môn học';
  };

  return (
    <div className="w-full space-y-4">
      
      {/* Top Sub-Header Bar with Right Action Button (Matching AdminSettingsPage layout & screenshot) */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleOpenAddModal}
          className="bg-blue-600 hover:bg-blue-500 font-semibold rounded-md h-9 px-4 text-xs border-0 ml-auto"
        >
          Thêm {getItemLabel()} mới
        </Button>
      </div>

      {/* Main Content Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        
        {/* Left Sidebar: Select Education Level */}
        <div className="bg-white dark:bg-slate-900 p-4 rounded-md border border-slate-200 dark:border-slate-800 lg:col-span-1">
          <h3 className="text-xs font-bold text-slate-400 mb-3 uppercase tracking-wider flex items-center gap-2">
            <BankOutlined className="text-blue-600" /> 1. Chọn Cấp Học
          </h3>

          <div className="space-y-2">
            {levels.map((level) => {
              const isSelected = selectedLevel?.id === level.id;
              const isLevelK12 = isK12(level);
              return (
                <div
                  key={level.id}
                  onClick={() => handleSelectLevel(level)}
                  className={`p-3 rounded-md cursor-pointer transition-all border ${
                    isSelected 
                      ? 'bg-blue-600 text-white border-blue-600' 
                      : 'bg-slate-50 dark:bg-slate-800/50 text-slate-700 dark:text-slate-300 border-slate-200 dark:border-slate-700 hover:bg-slate-100'
                  }`}
                >
                  <div className="flex justify-between items-center font-bold text-xs">
                    <span>{level.name}</span>
                    <span
                      className={`px-2 py-0.5 rounded text-[10px] font-black tracking-wider ${
                        isSelected
                          ? 'bg-white/20 text-white'
                          : 'bg-blue-50 text-blue-600 border border-blue-100'
                      }`}
                    >
                      {level.code}
                    </span>
                  </div>
                  <div className={`text-[11px] mt-1 font-medium ${isSelected ? 'text-blue-100' : 'text-slate-500'}`}>
                    {isLevelK12 
                      ? 'Cấp học → Môn học' 
                      : 'Trường → Khoa → Ngành → Môn'}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Right Workspace: Context Hierarchy & Data Table */}
        <div className="bg-white dark:bg-slate-900 p-5 rounded-md border border-slate-200 dark:border-slate-800 lg:col-span-3">
          
          {/* Breadcrumb Navigation Trail for Higher Ed */}
          {!isK12(selectedLevel) && (
            <div className="mb-4 bg-slate-50 dark:bg-slate-800/50 p-2.5 rounded-md border border-slate-200 dark:border-slate-700 flex items-center flex-wrap gap-2 text-xs font-semibold">
              <span 
                className={`cursor-pointer hover:text-blue-600 ${activeStep === 'school' ? 'text-blue-600' : 'text-slate-500'}`}
                onClick={() => { setActiveStep('school'); fetchSchoolsByLevel(selectedLevel?.id); }}
              >
                🏫 Trường ({(schools || []).length})
              </span>

              {selectedSchool && (
                <>
                  <span className="text-slate-300">/</span>
                  <span 
                    className={`cursor-pointer hover:text-blue-600 ${activeStep === 'faculty' ? 'text-blue-600' : 'text-slate-500'}`}
                    onClick={() => { setActiveStep('faculty'); fetchFacultiesBySchool(selectedSchool); }}
                  >
                    🏢 {selectedSchool.name} ({(faculties || []).length} Khoa)
                  </span>
                </>
              )}

              {selectedFaculty && (
                <>
                  <span className="text-slate-300">/</span>
                  <span 
                    className={`cursor-pointer hover:text-blue-600 ${activeStep === 'major' ? 'text-blue-600' : 'text-slate-500'}`}
                    onClick={() => { setActiveStep('major'); fetchMajorsByFaculty(selectedFaculty); }}
                  >
                    🎓 {selectedFaculty.name} ({(majors || []).length} Ngành)
                  </span>
                </>
              )}

              {selectedMajor && (
                <>
                  <span className="text-slate-300">/</span>
                  <span className="text-blue-600 font-extrabold">
                    📘 {selectedMajor.name} ({(subjects || []).length} Môn)
                  </span>
                </>
              )}
            </div>
          )}

          {/* K12 Specific Info Header */}
          {isK12(selectedLevel) && (
            <Alert
              type="info"
              showIcon
              message={<span className="font-bold text-xs">Cấu trúc Môn học Cấp {selectedLevel?.name} ({selectedLevel?.code})</span>}
              description={
                <span className="text-xs">
                  Danh mục môn học được phân loại chuẩn hóa trực tiếp theo cấp học <b>{selectedLevel?.name}</b> (Không chia theo Trường/Khoa/Ngành).
                </span>
              }
              className="mb-4 rounded-2xl border-blue-100 bg-blue-50/60"
            />
          )}

          {/* Table Header */}
          <div className="flex justify-between items-center mb-4">
            <div className="text-sm font-bold text-slate-900">
              {getStepTitle()}
            </div>
            <Tag color="blue" className="font-bold text-xs px-2.5 py-0.5 rounded-lg">
              Tổng cộng: {(getCurrentTableData() || []).length} mục
            </Tag>
          </div>

          {/* Data Table */}
          <Table
            dataSource={getCurrentTableData()}
            columns={columns}
            rowKey="id"
            loading={loading}
            pagination={{ pageSize: 8 }}
          />
        </div>

      </div>

      {/* Form Modal */}
      <Modal
        title={
          <span className="font-bold flex items-center gap-2 text-slate-900">
            <PlusOutlined className="text-blue-600" />
            {editingItem ? 'Chỉnh sửa' : 'Thêm mới'} {getItemLabel()} {selectedLevel?.code ? `(${selectedLevel.code})` : ''}
          </span>
        }
        open={modalVisible}
        onOk={() => form.submit()}
        onCancel={() => setModalVisible(false)}
        confirmLoading={submitting}
        okText="Lưu thông tin"
        cancelText="Hủy"
        className="rounded-3xl"
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="pt-2">
          
          <Form.Item
            label={<span className="text-xs font-bold">Tên {getItemLabel()}</span>}
            name="name"
            rules={[{ required: true, message: 'Vui lòng nhập tên!' }]}
          >
            <Input 
              placeholder={
                isK12(selectedLevel)
                  ? 'VD: Ngữ văn, Toán, Vật lí, Hóa học, Tin học...' 
                  : 'VD: Lập trình C#, Cơ sở dữ liệu, Công nghệ phần mềm...'
              } 
              className="rounded-xl h-10 text-xs" 
            />
          </Form.Item>

          <Form.Item
            label={<span className="text-xs font-bold">Mã (Code)</span>}
            name="code"
            rules={[{ required: true, message: 'Vui lòng nhập mã!' }]}
          >
            <Input placeholder="VD: TOAN_THPT, VAN_THCS, CSHARP..." className="rounded-xl h-10 text-xs" />
          </Form.Item>

          {(activeStep === 'subject' || activeStep === 'k12_subject') && (
            <Form.Item label={<span className="text-xs font-bold">Số tiết / Số tín chỉ</span>} name="credits">
              <Input type="number" placeholder="3" className="rounded-xl h-10 text-xs" />
            </Form.Item>
          )}

          <Form.Item label={<span className="text-xs font-bold">Mô tả</span>} name="description">
            <Input.TextArea rows={3} placeholder="Nhập mô tả..." className="rounded-xl text-xs" />
          </Form.Item>

        </Form>
      </Modal>

    </div>
  );
};

export default AdminEducationPage;
