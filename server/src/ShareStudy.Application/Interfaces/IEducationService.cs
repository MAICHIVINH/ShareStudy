using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Interfaces;

public interface IEducationService
{
    Task<IReadOnlyList<EducationLevelDto>> GetEducationLevelsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchoolDto>> GetSchoolsByEducationLevelAsync(long levelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectDto>> GetSubjectsByEducationLevelAsync(long levelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchoolDto>> GetSchoolsAsync(long? educationLevelId = null, CancellationToken cancellationToken = default);
    Task<SchoolDetailDto> GetSchoolByIdAsync(long schoolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacultyDto>> GetFacultiesBySchoolAsync(long schoolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MajorDto>> GetMajorsByFacultyAsync(long facultyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectDto>> GetSubjectsByMajorAsync(long majorId, CancellationToken cancellationToken = default);
    Task<PagedResult<SubjectDto>> GetSubjectsAsync(SubjectQueryParameters query, CancellationToken cancellationToken = default);
    Task<SubjectDto> GetSubjectByIdAsync(long subjectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AcademicYearDto>> GetAcademicYearsAsync(CancellationToken cancellationToken = default);
    void InvalidateEducationCache();
}
