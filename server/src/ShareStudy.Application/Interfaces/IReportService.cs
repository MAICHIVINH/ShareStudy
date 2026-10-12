using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Report;

namespace ShareStudy.Application.Interfaces;

public interface IReportService
{
    Task<ReportDto> CreateReportAsync(long reporterId, long documentId, CreateReportRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ReportDto>> GetUserReportsAsync(long reporterId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}
