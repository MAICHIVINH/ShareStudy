using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Favorite;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<FavoriteDto>> GetUserFavoritesAsync(long userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Favorite>()
            .Query()
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.Document.Status == DocumentStatus.Approved)
            .Include(f => f.Document)
                .ThenInclude(d => d.EducationLevel)
            .Include(f => f.Document)
                .ThenInclude(d => d.School)
            .Include(f => f.Document)
                .ThenInclude(d => d.Faculty)
            .Include(f => f.Document)
                .ThenInclude(d => d.Major)
            .Include(f => f.Document)
                .ThenInclude(d => d.Subject)
            .Include(f => f.Document)
                .ThenInclude(d => d.User)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<FavoriteDto>>(items);
        foreach (var dto in dtos)
        {
            if (dto.Document != null)
            {
                dto.Document.IsFavorite = true;
            }
        }
        return new PagedResult<FavoriteDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task AddFavoriteAsync(long userId, long documentId, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        var favRepo = _unitOfWork.Repository<Favorite>();
        var existing = await favRepo.Query().FirstOrDefaultAsync(f => f.UserId == userId && f.DocumentId == documentId, cancellationToken);
        if (existing != null) return; // Idempotent

        await favRepo.AddAsync(new Favorite
        {
            UserId = userId,
            DocumentId = documentId,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        doc.FavoriteCount += 1;
        docRepo.Update(doc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFavoriteAsync(long userId, long documentId, CancellationToken cancellationToken = default)
    {
        var favRepo = _unitOfWork.Repository<Favorite>();
        var existing = await favRepo.Query().FirstOrDefaultAsync(f => f.UserId == userId && f.DocumentId == documentId, cancellationToken);
        if (existing == null) return;

        favRepo.Remove(existing);

        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc != null && doc.FavoriteCount > 0)
        {
            doc.FavoriteCount -= 1;
            docRepo.Update(doc);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
