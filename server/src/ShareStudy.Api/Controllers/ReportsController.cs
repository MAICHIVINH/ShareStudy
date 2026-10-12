using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Report;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUserService;

    public ReportsController(IReportService reportService, ICurrentUserService currentUserService)
    {
        _reportService = reportService;
        _currentUserService = currentUserService;
    }

    [HttpPost("api/documents/{id}/reports")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReportDto>>> CreateReport(
        long id,
        [FromBody] CreateReportRequest request,
        [FromServices] IValidator<CreateReportRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _reportService.CreateReportAsync(userId, id, request, cancellationToken);
        return StatusCode(201, ApiResponse<ReportDto>.SuccessResponse(result, "Gửi báo cáo vi phạm thành công", 201));
    }

    [HttpGet("api/users/me/reports")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportDto>>>> GetMyReports(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _reportService.GetUserReportsAsync(userId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReportDto>>.SuccessResponse(result));
    }
}
