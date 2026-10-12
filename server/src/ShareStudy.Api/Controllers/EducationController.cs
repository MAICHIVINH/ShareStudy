using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Education;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService)
    {
        _educationService = educationService;
    }

    [HttpGet("education-levels")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EducationLevelDto>>>> GetEducationLevels(CancellationToken cancellationToken)
    {
        var result = await _educationService.GetEducationLevelsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EducationLevelDto>>.SuccessResponse(result, "Lấy danh sách cấp học thành công"));
    }

    [HttpGet("education-levels/{id}/schools")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SchoolDto>>>> GetSchoolsByEducationLevel(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSchoolsByEducationLevelAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolDto>>.SuccessResponse(result, "Lấy danh sách trường học theo cấp học thành công"));
    }

    [HttpGet("education-levels/{id}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectDto>>>> GetSubjectsByEducationLevel(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSubjectsByEducationLevelAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SubjectDto>>.SuccessResponse(result, "Lấy danh sách môn học theo cấp học thành công"));
    }

    [HttpGet("schools")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SchoolDto>>>> GetSchools([FromQuery] long? educationLevelId, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSchoolsAsync(educationLevelId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolDto>>.SuccessResponse(result, "Lấy danh sách trường học thành công"));
    }

    [HttpGet("schools/{id}")]
    public async Task<ActionResult<ApiResponse<SchoolDetailDto>>> GetSchoolById(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSchoolByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SchoolDetailDto>.SuccessResponse(result, "Lấy chi tiết trường học thành công"));
    }

    [HttpGet("schools/{id}/faculties")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FacultyDto>>>> GetFacultiesBySchool(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetFacultiesBySchoolAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FacultyDto>>.SuccessResponse(result, "Lấy danh sách khoa theo trường học thành công"));
    }

    [HttpGet("faculties/{id}/majors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MajorDto>>>> GetMajorsByFaculty(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetMajorsByFacultyAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MajorDto>>.SuccessResponse(result, "Lấy danh sách ngành theo khoa thành công"));
    }

    [HttpGet("majors/{id}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectDto>>>> GetSubjectsByMajor(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSubjectsByMajorAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SubjectDto>>.SuccessResponse(result, "Lấy danh sách môn học theo ngành thành công"));
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectDto>>>> GetSubjects([FromQuery] SubjectQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSubjectsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectDto>>.SuccessResponse(result, "Lấy danh sách môn học thành công"));
    }

    [HttpGet("subjects/{id}")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> GetSubjectById(long id, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSubjectByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SubjectDto>.SuccessResponse(result, "Lấy chi tiết môn học thành công"));
    }

    [HttpGet("document-types")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentTypeDto>>>> GetDocumentTypes(CancellationToken cancellationToken)
    {
        var result = await _educationService.GetDocumentTypesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentTypeDto>>.SuccessResponse(result, "Lấy danh sách loại tài liệu thành công"));
    }

    [HttpGet("academic-years")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AcademicYearDto>>>> GetAcademicYears(CancellationToken cancellationToken)
    {
        var result = await _educationService.GetAcademicYearsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AcademicYearDto>>.SuccessResponse(result, "Lấy danh sách năm học thành công"));
    }
}
