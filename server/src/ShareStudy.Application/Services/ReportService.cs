using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Report;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReportDto> CreateReportAsync(long reporterId, long documentId, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        if (!Enum.TryParse<ReportReason>(request.Reason, true, out var reason))
        {
            throw new ValidationException($"Lý do báo cáo '{request.Reason}' không hợp lệ.");
        }

        var report = new Report
        {
            DocumentId = documentId,
            UserId = reporterId,
            Reason = reason,
            Description = request.Description?.Trim(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var reportRepo = _unitOfWork.Repository<Report>();
        await reportRepo.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await reportRepo.Query()
            .Include(r => r.Document)
            .Include(r => r.User)
            .FirstAsync(r => r.Id == report.Id, cancellationToken);

        return _mapper.Map<ReportDto>(created);
    }

    public async Task<PagedResult<ReportDto>> GetUserReportsAsync(long reporterId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Report>()
            .Query()
            .AsNoTracking()
            .Where(r => r.UserId == reporterId)
            .Include(r => r.Document)
            .Include(r => r.User)
            .Include(r => r.ResolverUser)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ReportDto>>(items);
        return new PagedResult<ReportDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
