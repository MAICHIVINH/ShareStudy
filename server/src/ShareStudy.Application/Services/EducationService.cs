using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Education;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class EducationService : IEducationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private const string EducationCachePrefix = "EducationCache_";

    public EducationService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IReadOnlyList<EducationLevelDto>> GetEducationLevelsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Levels";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var levels = await _unitOfWork.Repository<EducationLevel>()
                .Query()
                .Where(el => el.Status == EntityStatus.Active)
                .Include(el => el.Schools)
                .OrderBy(el => el.DisplayOrder)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<EducationLevelDto>>(levels);
        }) ?? new List<EducationLevelDto>();
    }

    public async Task<IReadOnlyList<SchoolDto>> GetSchoolsByEducationLevelAsync(long levelId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Schools_Level_{levelId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var schools = await _unitOfWork.Repository<School>()
                .Query()
                .Where(s => s.EducationLevelId == levelId && s.Status == EntityStatus.Active)
                .Include(s => s.EducationLevel)
                .Include(s => s.Faculties)
                    .ThenInclude(f => f.Majors)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<SchoolDto>>(schools);
        }) ?? new List<SchoolDto>();
    }

    public async Task<IReadOnlyList<SubjectDto>> GetSubjectsByEducationLevelAsync(long levelId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Subjects_Level_{levelId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            // Find all subjects belonging directly or indirectly to education level
            var subjects = await _unitOfWork.Repository<Subject>()
                .Query()
                .Where(s => s.Status == EntityStatus.Active &&
                            ((s.MajorId == null && s.Documents.Any(d => d.EducationLevelId == levelId)) ||
                             (s.Major != null && s.Major.Faculty.School.EducationLevelId == levelId)))
                .Include(s => s.Major)
                    .ThenInclude(m => m!.Faculty)
                        .ThenInclude(f => f.School)
                .Include(s => s.Documents)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<SubjectDto>>(subjects);
        }) ?? new List<SubjectDto>();
    }

    public async Task<IReadOnlyList<SchoolDto>> GetSchoolsAsync(long? educationLevelId = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Schools_All_{(educationLevelId.HasValue ? educationLevelId.Value : "All")}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var query = _unitOfWork.Repository<School>()
                .Query()
                .Where(s => s.Status == EntityStatus.Active);

            if (educationLevelId.HasValue)
            {
                query = query.Where(s => s.EducationLevelId == educationLevelId.Value);
            }

            var schools = await query
                .Include(s => s.EducationLevel)
                .Include(s => s.Faculties)
                    .ThenInclude(f => f.Majors)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<SchoolDto>>(schools);
        }) ?? new List<SchoolDto>();
    }

    public async Task<SchoolDetailDto> GetSchoolByIdAsync(long schoolId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}SchoolDetail_{schoolId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var school = await _unitOfWork.Repository<School>()
                .Query()
                .Include(s => s.EducationLevel)
                .Include(s => s.Faculties.Where(f => f.Status == EntityStatus.Active))
                    .ThenInclude(f => f.Majors.Where(m => m.Status == EntityStatus.Active))
                .FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);

            if (school == null)
            {
                throw new NotFoundException("Trường học", schoolId);
            }

            return _mapper.Map<SchoolDetailDto>(school);
        }) ?? throw new NotFoundException("Trường học", schoolId);
    }

    public async Task<IReadOnlyList<FacultyDto>> GetFacultiesBySchoolAsync(long schoolId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Faculties_School_{schoolId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var faculties = await _unitOfWork.Repository<Faculty>()
                .Query()
                .Where(f => f.SchoolId == schoolId && f.Status == EntityStatus.Active)
                .Include(f => f.School)
                .Include(f => f.Majors)
                .OrderBy(f => f.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<FacultyDto>>(faculties);
        }) ?? new List<FacultyDto>();
    }

    public async Task<IReadOnlyList<MajorDto>> GetMajorsByFacultyAsync(long facultyId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Majors_Faculty_{facultyId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var majors = await _unitOfWork.Repository<Major>()
                .Query()
                .Where(m => m.FacultyId == facultyId && m.Status == EntityStatus.Active)
                .Include(m => m.Faculty)
                    .ThenInclude(f => f.School)
                .Include(m => m.Subjects)
                .OrderBy(m => m.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<MajorDto>>(majors);
        }) ?? new List<MajorDto>();
    }

    public async Task<IReadOnlyList<SubjectDto>> GetSubjectsByMajorAsync(long majorId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}Subjects_Major_{majorId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var subjects = await _unitOfWork.Repository<Subject>()
                .Query()
                .Where(s => s.MajorId == majorId && s.Status == EntityStatus.Active)
                .Include(s => s.Major)
                    .ThenInclude(m => m!.Faculty)
                        .ThenInclude(f => f.School)
                .Include(s => s.Documents)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<SubjectDto>>(subjects);
        }) ?? new List<SubjectDto>();
    }

    public async Task<PagedResult<SubjectDto>> GetSubjectsAsync(SubjectQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Subject>()
            .Query()
            .Where(s => s.Status == EntityStatus.Active);

        if (query.MajorId.HasValue)
        {
            queryable = queryable.Where(s => s.MajorId == query.MajorId.Value);
        }

        if (query.EducationLevelId.HasValue)
        {
            queryable = queryable.Where(s => s.EducationLevelId == query.EducationLevelId.Value ||
                                             (s.Major != null && s.Major.Faculty.School.EducationLevelId == query.EducationLevelId.Value) ||
                                             s.Documents.Any(d => d.EducationLevelId == query.EducationLevelId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(s => s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var subjects = await queryable
            .Include(s => s.EducationLevel)
            .Include(s => s.Major)
                .ThenInclude(m => m!.Faculty)
                    .ThenInclude(f => f.School)
            .Include(s => s.Documents)
            .OrderBy(s => s.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<SubjectDto>>(subjects);
        return new PagedResult<SubjectDto>(dtos, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<SubjectDto> GetSubjectByIdAsync(long subjectId, CancellationToken cancellationToken = default)
    {
        var subject = await _unitOfWork.Repository<Subject>()
            .Query()
            .Include(s => s.Major)
                .ThenInclude(m => m!.Faculty)
                    .ThenInclude(f => f.School)
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);

        if (subject == null)
        {
            throw new NotFoundException("Môn học", subjectId);
        }

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}DocumentTypes";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var docTypes = await _unitOfWork.Repository<DocumentType>()
                .Query()
                .Where(dt => dt.Status == EntityStatus.Active)
                .Include(dt => dt.Documents)
                .OrderBy(dt => dt.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<DocumentTypeDto>>(docTypes);
        }) ?? new List<DocumentTypeDto>();
    }

    public async Task<IReadOnlyList<AcademicYearDto>> GetAcademicYearsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EducationCachePrefix}AcademicYears";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var academicYears = await _unitOfWork.Repository<AcademicYear>()
                .Query()
                .Where(ay => ay.Status == EntityStatus.Active)
                .OrderBy(ay => ay.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<AcademicYearDto>>(academicYears);
        }) ?? new List<AcademicYearDto>();
    }

    public void InvalidateEducationCache()
    {
        // Simple cache clear by removing known keys or clearing memory cache
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Clear();
        }
    }
}
