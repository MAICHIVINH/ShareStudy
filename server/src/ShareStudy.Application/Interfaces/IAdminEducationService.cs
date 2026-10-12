using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Interfaces;

public interface IAdminEducationService
{
    // Education Level
    Task<EducationLevelDto> CreateEducationLevelAsync(CreateEducationLevelRequest request, CancellationToken cancellationToken = default);
    Task<EducationLevelDto> UpdateEducationLevelAsync(long id, UpdateEducationLevelRequest request, CancellationToken cancellationToken = default);
    Task DeleteEducationLevelAsync(long id, CancellationToken cancellationToken = default);
    Task<EducationLevelDto> ToggleEducationLevelStatusAsync(long id, CancellationToken cancellationToken = default);

    // School
    Task<SchoolDto> CreateSchoolAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default);
    Task<SchoolDto> UpdateSchoolAsync(long id, UpdateSchoolRequest request, CancellationToken cancellationToken = default);
    Task DeleteSchoolAsync(long id, CancellationToken cancellationToken = default);
    Task<SchoolDto> ToggleSchoolStatusAsync(long id, CancellationToken cancellationToken = default);

    // Faculty
    Task<FacultyDto> CreateFacultyAsync(CreateFacultyRequest request, CancellationToken cancellationToken = default);
    Task<FacultyDto> UpdateFacultyAsync(long id, UpdateFacultyRequest request, CancellationToken cancellationToken = default);
    Task DeleteFacultyAsync(long id, CancellationToken cancellationToken = default);
    Task<FacultyDto> ToggleFacultyStatusAsync(long id, CancellationToken cancellationToken = default);

    // Major
    Task<MajorDto> CreateMajorAsync(CreateMajorRequest request, CancellationToken cancellationToken = default);
    Task<MajorDto> UpdateMajorAsync(long id, UpdateMajorRequest request, CancellationToken cancellationToken = default);
    Task DeleteMajorAsync(long id, CancellationToken cancellationToken = default);
    Task<MajorDto> ToggleMajorStatusAsync(long id, CancellationToken cancellationToken = default);

    // Subject
    Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);
    Task<SubjectDto> UpdateSubjectAsync(long id, UpdateSubjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteSubjectAsync(long id, CancellationToken cancellationToken = default);
    Task<SubjectDto> ToggleSubjectStatusAsync(long id, CancellationToken cancellationToken = default);

    // DocumentType
    Task<DocumentTypeDto> CreateDocumentTypeAsync(CreateDocumentTypeRequest request, CancellationToken cancellationToken = default);
    Task<DocumentTypeDto> UpdateDocumentTypeAsync(long id, UpdateDocumentTypeRequest request, CancellationToken cancellationToken = default);
    Task DeleteDocumentTypeAsync(long id, CancellationToken cancellationToken = default);
    Task<DocumentTypeDto> ToggleDocumentTypeStatusAsync(long id, CancellationToken cancellationToken = default);
}
