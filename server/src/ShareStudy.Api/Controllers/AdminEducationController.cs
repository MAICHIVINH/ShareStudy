using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Education;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminEducationController : ControllerBase
{
    private readonly IAdminEducationService _adminService;
    private readonly IEducationService _educationService;

    public AdminEducationController(IAdminEducationService adminService, IEducationService educationService)
    {
        _adminService = adminService;
        _educationService = educationService;
    }

    #region Education Levels

    [HttpGet("education-levels")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EducationLevelDto>>>> GetEducationLevels(CancellationToken cancellationToken)
    {
        var result = await _educationService.GetEducationLevelsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EducationLevelDto>>.SuccessResponse(result));
    }

    [HttpPost("education-levels")]
    public async Task<ActionResult<ApiResponse<EducationLevelDto>>> CreateEducationLevel(
        [FromBody] CreateEducationLevelRequest request,
        [FromServices] IValidator<CreateEducationLevelRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateEducationLevelAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<EducationLevelDto>.SuccessResponse(result, "Tạo cấp học thành công", 201));
    }

    [HttpPut("education-levels/{id}")]
    public async Task<ActionResult<ApiResponse<EducationLevelDto>>> UpdateEducationLevel(
        long id,
        [FromBody] UpdateEducationLevelRequest request,
        [FromServices] IValidator<UpdateEducationLevelRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateEducationLevelAsync(id, request, cancellationToken);
        return Ok(ApiResponse<EducationLevelDto>.SuccessResponse(result, "Cập nhật cấp học thành công"));
    }

    [HttpDelete("education-levels/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEducationLevel(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteEducationLevelAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa cấp học thành công"));
    }

    [HttpPatch("education-levels/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<EducationLevelDto>>> ToggleEducationLevelStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleEducationLevelStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<EducationLevelDto>.SuccessResponse(result, "Đổi trạng thái cấp học thành công"));
    }

    #endregion

    #region Schools

    [HttpGet("schools")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SchoolDto>>>> GetSchools([FromQuery] long? educationLevelId, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSchoolsAsync(educationLevelId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolDto>>.SuccessResponse(result));
    }

    [HttpPost("schools")]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> CreateSchool(
        [FromBody] CreateSchoolRequest request,
        [FromServices] IValidator<CreateSchoolRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateSchoolAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<SchoolDto>.SuccessResponse(result, "Tạo trường học thành công", 201));
    }

    [HttpPut("schools/{id}")]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> UpdateSchool(
        long id,
        [FromBody] UpdateSchoolRequest request,
        [FromServices] IValidator<UpdateSchoolRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateSchoolAsync(id, request, cancellationToken);
        return Ok(ApiResponse<SchoolDto>.SuccessResponse(result, "Cập nhật trường học thành công"));
    }

    [HttpDelete("schools/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSchool(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteSchoolAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa trường học thành công"));
    }

    [HttpPatch("schools/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> ToggleSchoolStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleSchoolStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<SchoolDto>.SuccessResponse(result, "Đổi trạng thái trường học thành công"));
    }

    #endregion

    #region Faculties

    [HttpGet("faculties")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FacultyDto>>>> GetFaculties([FromQuery] long schoolId, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetFacultiesBySchoolAsync(schoolId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FacultyDto>>.SuccessResponse(result));
    }

    [HttpPost("faculties")]
    public async Task<ActionResult<ApiResponse<FacultyDto>>> CreateFaculty(
        [FromBody] CreateFacultyRequest request,
        [FromServices] IValidator<CreateFacultyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateFacultyAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<FacultyDto>.SuccessResponse(result, "Tạo khoa thành công", 201));
    }

    [HttpPut("faculties/{id}")]
    public async Task<ActionResult<ApiResponse<FacultyDto>>> UpdateFaculty(
        long id,
        [FromBody] UpdateFacultyRequest request,
        [FromServices] IValidator<UpdateFacultyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateFacultyAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FacultyDto>.SuccessResponse(result, "Cập nhật khoa thành công"));
    }

    [HttpDelete("faculties/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteFaculty(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteFacultyAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa khoa thành công"));
    }

    [HttpPatch("faculties/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<FacultyDto>>> ToggleFacultyStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleFacultyStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<FacultyDto>.SuccessResponse(result, "Đổi trạng thái khoa thành công"));
    }

    #endregion

    #region Majors

    [HttpGet("majors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MajorDto>>>> GetMajors([FromQuery] long facultyId, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetMajorsByFacultyAsync(facultyId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MajorDto>>.SuccessResponse(result));
    }

    [HttpPost("majors")]
    public async Task<ActionResult<ApiResponse<MajorDto>>> CreateMajor(
        [FromBody] CreateMajorRequest request,
        [FromServices] IValidator<CreateMajorRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateMajorAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<MajorDto>.SuccessResponse(result, "Tạo ngành thành công", 201));
    }

    [HttpPut("majors/{id}")]
    public async Task<ActionResult<ApiResponse<MajorDto>>> UpdateMajor(
        long id,
        [FromBody] UpdateMajorRequest request,
        [FromServices] IValidator<UpdateMajorRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateMajorAsync(id, request, cancellationToken);
        return Ok(ApiResponse<MajorDto>.SuccessResponse(result, "Cập nhật ngành thành công"));
    }

    [HttpDelete("majors/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteMajor(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteMajorAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa ngành thành công"));
    }

    [HttpPatch("majors/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<MajorDto>>> ToggleMajorStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleMajorStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<MajorDto>.SuccessResponse(result, "Đổi trạng thái ngành thành công"));
    }

    #endregion

    #region Subjects

    [HttpGet("subjects")]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectDto>>>> GetSubjects([FromQuery] SubjectQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _educationService.GetSubjectsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectDto>>.SuccessResponse(result));
    }

    [HttpPost("subjects")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> CreateSubject(
        [FromBody] CreateSubjectRequest request,
        [FromServices] IValidator<CreateSubjectRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateSubjectAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<SubjectDto>.SuccessResponse(result, "Tạo môn học thành công", 201));
    }

    [HttpPut("subjects/{id}")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> UpdateSubject(
        long id,
        [FromBody] UpdateSubjectRequest request,
        [FromServices] IValidator<UpdateSubjectRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateSubjectAsync(id, request, cancellationToken);
        return Ok(ApiResponse<SubjectDto>.SuccessResponse(result, "Cập nhật môn học thành công"));
    }

    [HttpDelete("subjects/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSubject(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteSubjectAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa môn học thành công"));
    }

    [HttpPatch("subjects/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> ToggleSubjectStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleSubjectStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<SubjectDto>.SuccessResponse(result, "Đổi trạng thái môn học thành công"));
    }

    #endregion

    #region Document Types

    [HttpGet("document-types")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentTypeDto>>>> GetDocumentTypes(CancellationToken cancellationToken)
    {
        var result = await _educationService.GetDocumentTypesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentTypeDto>>.SuccessResponse(result));
    }

    [HttpPost("document-types")]
    public async Task<ActionResult<ApiResponse<DocumentTypeDto>>> CreateDocumentType(
        [FromBody] CreateDocumentTypeRequest request,
        [FromServices] IValidator<CreateDocumentTypeRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.CreateDocumentTypeAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<DocumentTypeDto>.SuccessResponse(result, "Tạo loại tài liệu thành công", 201));
    }

    [HttpPut("document-types/{id}")]
    public async Task<ActionResult<ApiResponse<DocumentTypeDto>>> UpdateDocumentType(
        long id,
        [FromBody] UpdateDocumentTypeRequest request,
        [FromServices] IValidator<UpdateDocumentTypeRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var result = await _adminService.UpdateDocumentTypeAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DocumentTypeDto>.SuccessResponse(result, "Cập nhật loại tài liệu thành công"));
    }

    [HttpDelete("document-types/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteDocumentType(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteDocumentTypeAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa loại tài liệu thành công"));
    }

    [HttpPatch("document-types/{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<DocumentTypeDto>>> ToggleDocumentTypeStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleDocumentTypeStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<DocumentTypeDto>.SuccessResponse(result, "Đổi trạng thái loại tài liệu thành công"));
    }

    #endregion
}
