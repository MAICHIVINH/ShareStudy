import React, { useState, useEffect } from 'react';
import { Select, Checkbox, Radio, Button, Space } from 'antd';
import { FilterOutlined, ReloadOutlined } from '@ant-design/icons';
import { educationService } from '../../services/educationService';

export const FilterPanel = ({ filters = {}, onApplyFilter, onResetFilter }) => {
  // Cascading Dropdown Options
  const [educationLevels, setEducationLevels] = useState([]);
  const [schools, setSchools] = useState([]);
  const [faculties, setFaculties] = useState([]);
  const [majors, setMajors] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [documentTypes, setDocumentTypes] = useState([]);
  const [academicYears, setAcademicYears] = useState([]);

  // Local Filter State
  const [selectedLevel, setSelectedLevel] = useState(filters.educationLevelId || null);
  const [selectedSchool, setSelectedSchool] = useState(filters.schoolId || null);
  const [selectedFaculty, setSelectedFaculty] = useState(filters.facultyId || null);
  const [selectedMajor, setSelectedMajor] = useState(filters.majorId || null);
  const [selectedSubject, setSelectedSubject] = useState(filters.subjectId || null);
  const [selectedAcademicYear, setSelectedAcademicYear] = useState(filters.academicYearId || null);
  const [selectedDocTypes, setSelectedDocTypes] = useState(filters.documentTypeIds || []);
  const parseFileTypes = (val) => {
    if (!val) return [];
    if (Array.isArray(val)) return val;
    return typeof val === 'string' ? val.split(',').map((s) => s.trim()) : [];
  };

  const [selectedFormats, setSelectedFormats] = useState(parseFileTypes(filters.fileTypes));
  const [selectedMinRating, setSelectedMinRating] = useState(filters.minRating || null);
  const [selectedSortBy, setSelectedSortBy] = useState(filters.sortBy || 'newest');

  // Load initial Education Levels, Document Types, Academic Years
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
      console.error('Error fetching initial filter options:', err);
    }
  };

  // Sync state when filters prop changes (e.g. from URL search params)
  useEffect(() => {
    setSelectedLevel(filters.educationLevelId || null);
    setSelectedSchool(filters.schoolId || null);
    setSelectedFaculty(filters.facultyId || null);
    setSelectedMajor(filters.majorId || null);
    setSelectedSubject(filters.subjectId || null);
    setSelectedAcademicYear(filters.academicYearId || null);
    setSelectedDocTypes(filters.documentTypeIds || []);
    setSelectedFormats(parseFileTypes(filters.fileTypes));
    setSelectedMinRating(filters.minRating || null);
    setSelectedSortBy(filters.sortBy || 'newest');
  }, [filters]);

  // Handle Education Level Change
  useEffect(() => {
    if (selectedLevel) {
      const level = educationLevels.find((l) => l.id === Number(selectedLevel));
      // Show schools only if College/University ("DH" or "CD")
      if (level && (level.code === 'DH' || level.code === 'CD' || level.name.toLowerCase().includes('đại học') || level.name.toLowerCase().includes('cao đẳng'))) {
        educationService.getSchoolsByLevel(selectedLevel).then((res) => setSchools(res.data || []));
      } else {
        setSchools([]);
      }
    } else {
      setSchools([]);
    }
  }, [selectedLevel, educationLevels]);

  // Handle School Change -> Fetch Faculties
  useEffect(() => {
    if (selectedSchool) {
      educationService.getFacultiesBySchool(selectedSchool).then((res) => setFaculties(res.data || []));
    } else {
      setFaculties([]);
    }
  }, [selectedSchool]);

  // Handle Faculty Change -> Fetch Majors
  useEffect(() => {
    if (selectedFaculty) {
      educationService.getMajorsByFaculty(selectedFaculty).then((res) => setMajors(res.data || []));
    } else {
      setMajors([]);
    }
  }, [selectedFaculty]);

  // Handle Major Change -> Fetch Subjects
  useEffect(() => {
    if (selectedMajor) {
      educationService.getSubjectsByMajor(selectedMajor).then((res) => setSubjects(res.data || []));
    } else {
      setSubjects([]);
    }
  }, [selectedMajor]);

  const handleLevelSelect = (val) => {
    setSelectedLevel(val);
    setSelectedSchool(null);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    setSelectedSubject(null);
  };

  const handleSchoolSelect = (val) => {
    setSelectedSchool(val);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    setSelectedSubject(null);
  };

  const handleFacultySelect = (val) => {
    setSelectedFaculty(val);
    setSelectedMajor(null);
    setSelectedSubject(null);
  };

  const handleMajorSelect = (val) => {
    setSelectedMajor(val);
    setSelectedSubject(null);
  };

  const handleApply = () => {
    onApplyFilter({
      educationLevelId: selectedLevel,
      schoolId: selectedSchool,
      facultyId: selectedFaculty,
      majorId: selectedMajor,
      subjectId: selectedSubject,
      academicYearId: selectedAcademicYear,
      documentTypeIds: selectedDocTypes,
      fileTypes: Array.isArray(selectedFormats) ? selectedFormats.join(',') : selectedFormats,
      minRating: selectedMinRating,
      sortBy: selectedSortBy,
    });
  };

  const handleReset = () => {
    setSelectedLevel(null);
    setSelectedSchool(null);
    setSelectedFaculty(null);
    setSelectedMajor(null);
    setSelectedSubject(null);
    setSelectedAcademicYear(null);
    setSelectedDocTypes([]);
    setSelectedFormats([]);
    setSelectedMinRating(null);
    setSelectedSortBy('newest');
    if (onResetFilter) onResetFilter();
  };

  const isUniversityLevel = Boolean(schools.length > 0 || selectedLevel);

  return (
    <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm space-y-5 text-slate-800">
      
      {/* Title Header */}
      <div className="flex justify-between items-center pb-3 border-b border-slate-100">
        <h3 className="text-sm font-bold text-slate-900 flex items-center gap-2 m-0 uppercase tracking-wider">
          <FilterOutlined className="text-blue-600" />
          BỘ LỌC TÀI LIỆU
        </h3>
        <button 
          onClick={handleReset}
          className="text-xs text-blue-600 hover:text-blue-700 font-medium border-0 bg-transparent cursor-pointer"
        >
          Xóa bộ lọc
        </button>
      </div>

      {/* Dependent Hierarchical Selects */}
      <div className="space-y-3.5">
        
        {/* 1. Cấp học */}
        <div>
          <label className="text-xs font-bold text-slate-700 mb-1 block">Cấp học</label>
          <Select
            placeholder="Chọn cấp học"
            value={selectedLevel}
            onChange={handleLevelSelect}
            allowClear
            className="w-full"
            options={educationLevels.map((l) => ({ value: l.id, label: l.name }))}
          />
        </div>

        {/* 2. Trường (Hiện khi Cấp học = Đại học/Cao đẳng) */}
        {schools.length > 0 && (
          <div>
            <label className="text-xs font-bold text-slate-700 mb-1 block">Trường</label>
            <Select
              placeholder="Chọn trường đại học / cao đẳng"
              value={selectedSchool}
              onChange={handleSchoolSelect}
              allowClear
              className="w-full"
              options={schools.map((s) => ({ value: s.id, label: `${s.code ? s.code + ' - ' : ''}${s.name}` }))}
            />
          </div>
        )}

        {/* 3. Khoa */}
        {selectedSchool && (
          <div>
            <label className="text-xs font-bold text-slate-700 mb-1 block">Khoa</label>
            <Select
              placeholder="Chọn khoa"
              value={selectedFaculty}
              onChange={handleFacultySelect}
              allowClear
              className="w-full"
              options={faculties.map((f) => ({ value: f.id, label: f.name }))}
            />
          </div>
        )}

        {/* 4. Ngành học */}
        {selectedFaculty && (
          <div>
            <label className="text-xs font-bold text-slate-700 mb-1 block">Ngành học</label>
            <Select
              placeholder="Chọn ngành học"
              value={selectedMajor}
              onChange={handleMajorSelect}
              allowClear
              className="w-full"
              options={majors.map((m) => ({ value: m.id, label: m.name }))}
            />
          </div>
        )}

        {/* 5. Môn học */}
        {selectedMajor && (
          <div>
            <label className="text-xs font-bold text-slate-700 mb-1 block">Môn học</label>
            <Select
              placeholder="Chọn môn học"
              value={selectedSubject}
              onChange={(val) => setSelectedSubject(val)}
              allowClear
              className="w-full"
              options={subjects.map((sj) => ({ value: sj.id, label: sj.name }))}
            />
          </div>
        )}

        {/* Khóa / Năm học */}
        <div>
          <label className="text-xs font-bold text-slate-700 mb-1 block">Khóa / Năm học</label>
          <Select
            placeholder="Chọn năm học"
            value={selectedAcademicYear}
            onChange={(val) => setSelectedAcademicYear(val)}
            allowClear
            className="w-full"
            options={academicYears.map((y) => ({ value: y.id, label: y.name }))}
          />
        </div>

      </div>

      {/* Checkboxes: Loại tài liệu */}
      <div className="pt-3 border-t border-slate-100">
        <label className="text-xs font-bold text-slate-700 mb-2 block">Loại tài liệu</label>
        <Checkbox.Group
          value={selectedDocTypes}
          onChange={(vals) => setSelectedDocTypes(vals)}
          className="grid grid-cols-2 gap-2 text-xs"
        >
          {documentTypes.map((t) => (
            <Checkbox key={t.id} value={t.id} className="text-xs text-slate-600">
              {t.name}
            </Checkbox>
          ))}
        </Checkbox.Group>
      </div>

      {/* Checkboxes: Định dạng */}
      <div className="pt-3 border-t border-slate-100">
        <label className="text-xs font-bold text-slate-700 mb-2 block">Định dạng</label>
        <Checkbox.Group
          value={selectedFormats}
          onChange={(vals) => setSelectedFormats(vals)}
          className="grid grid-cols-2 gap-2 text-xs"
        >
          {['PDF', 'DOCX', 'PPTX', 'XLSX'].map((fmt) => (
            <Checkbox key={fmt} value={fmt} className="text-xs text-slate-600">
              {fmt}
            </Checkbox>
          ))}
        </Checkbox.Group>
      </div>

      {/* Radios: Đánh giá */}
      <div className="pt-3 border-t border-slate-100">
        <label className="text-xs font-bold text-slate-700 mb-2 block">Đánh giá</label>
        <Radio.Group
          value={selectedMinRating}
          onChange={(e) => setSelectedMinRating(e.target.value)}
          className="flex flex-col space-y-1.5 text-xs text-slate-600"
        >
          <Radio value={5}>5 sao</Radio>
          <Radio value={4}>4 sao trở lên</Radio>
          <Radio value={3}>3 sao trở lên</Radio>
          <Radio value={null}>Tất cả</Radio>
        </Radio.Group>
      </div>

      {/* Dropdown: Sắp xếp theo */}
      <div className="pt-3 border-t border-slate-100">
        <label className="text-xs font-bold text-slate-700 mb-1 block">Sắp xếp theo</label>
        <Select
          value={selectedSortBy}
          onChange={(val) => setSelectedSortBy(val)}
          className="w-full"
          options={[
            { value: 'newest', label: 'Mới nhất' },
            { value: 'oldest', label: 'Cũ nhất' },
            { value: 'most_downloaded', label: 'Tải nhiều nhất' },
            { value: 'highest_rated', label: 'Đánh giá cao nhất' },
            { value: 'most_viewed', label: 'Xem nhiều nhất' },
            { value: 'title_asc', label: 'Tên A - Z' },
            { value: 'title_desc', label: 'Tên Z - A' },
          ]}
        />
      </div>

      {/* Action Buttons */}
      <div className="pt-3 border-t border-slate-100 flex gap-2">
        <Button
          type="primary"
          onClick={handleApply}
          className="w-full bg-blue-600 hover:bg-blue-700 rounded-xl font-semibold text-xs h-10"
        >
          Áp dụng bộ lọc
        </Button>
        <Button
          onClick={handleReset}
          className="w-full rounded-xl text-xs h-10 border-slate-200"
        >
          Xóa bộ lọc
        </Button>
      </div>

    </div>
  );
};

export default FilterPanel;
