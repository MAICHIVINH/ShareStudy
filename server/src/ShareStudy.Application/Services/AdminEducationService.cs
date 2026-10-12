using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Education;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class AdminEducationService : IAdminEducationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEducationService _educationService;

    public AdminEducationService(IUnitOfWork unitOfWork, IMapper mapper, IEducationService educationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _educationService = educationService;
    }

    #region Education Level

    public async Task<EducationLevelDto> CreateEducationLevelAsync(CreateEducationLevelRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EducationLevel>();
        if (await repo.AnyAsync(el => el.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã cấp học '{request.Code}' đã tồn tại.");
        }

        var entity = new EducationLevel
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder,
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<EducationLevelDto>(entity);
    }

    public async Task<EducationLevelDto> UpdateEducationLevelAsync(long id, UpdateEducationLevelRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EducationLevel>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Cấp học", id);

        if (await repo.AnyAsync(el => el.Id != id && el.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã cấp học '{request.Code}' đã tồn tại.");
        }

        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Description = request.Description?.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<EducationLevelDto>(entity);
    }

    public async Task DeleteEducationLevelAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EducationLevel>();
        var entity = await repo.Query()
            .Include(el => el.Schools)
            .Include(el => el.Documents)
            .FirstOrDefaultAsync(el => el.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Cấp học", id);

        if (entity.Schools.Any())
        {
            throw new BadRequestException($"Không thể xóa cấp học '{entity.Name}' vì đang có {entity.Schools.Count} trường trực thuộc.");
        }

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa cấp học '{entity.Name}' vì đang chứa {entity.Documents.Count} tài liệu.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<EducationLevelDto> ToggleEducationLevelStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<EducationLevel>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Cấp học", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<EducationLevelDto>(entity);
    }

    #endregion

    #region School

    public async Task<SchoolDto> CreateSchoolAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var eduRepo = _unitOfWork.Repository<EducationLevel>();
        if (!await eduRepo.AnyAsync(el => el.Id == request.EducationLevelId, cancellationToken))
        {
            throw new NotFoundException("Cấp học", request.EducationLevelId);
        }

        var repo = _unitOfWork.Repository<School>();
        if (await repo.AnyAsync(s => s.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã trường '{request.Code}' đã tồn tại.");
        }

        var entity = new School
        {
            EducationLevelId = request.EducationLevelId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Address = request.Address?.Trim(),
            Website = request.Website?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            Description = request.Description?.Trim(),
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var school = await repo.Query()
            .Include(s => s.EducationLevel)
            .Include(s => s.Faculties)
                .ThenInclude(f => f.Majors)
            .FirstAsync(s => s.Id == entity.Id, cancellationToken);

        return _mapper.Map<SchoolDto>(school);
    }

    public async Task<SchoolDto> UpdateSchoolAsync(long id, UpdateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<School>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Trường học", id);

        var eduRepo = _unitOfWork.Repository<EducationLevel>();
        if (!await eduRepo.AnyAsync(el => el.Id == request.EducationLevelId, cancellationToken))
        {
            throw new NotFoundException("Cấp học", request.EducationLevelId);
        }

        if (await repo.AnyAsync(s => s.Id != id && s.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã trường '{request.Code}' đã tồn tại.");
        }

        entity.EducationLevelId = request.EducationLevelId;
        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Address = request.Address?.Trim();
        entity.Website = request.Website?.Trim();
        entity.LogoUrl = request.LogoUrl?.Trim();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var school = await repo.Query()
            .Include(s => s.EducationLevel)
            .Include(s => s.Faculties)
                .ThenInclude(f => f.Majors)
            .FirstAsync(s => s.Id == entity.Id, cancellationToken);

        return _mapper.Map<SchoolDto>(school);
    }

    public async Task DeleteSchoolAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<School>();
        var entity = await repo.Query()
            .Include(s => s.Faculties)
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Trường học", id);

        if (entity.Faculties.Any())
        {
            throw new BadRequestException($"Không thể xóa trường '{entity.Name}' vì đang có {entity.Faculties.Count} khoa trực thuộc.");
        }

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa trường '{entity.Name}' vì đang có {entity.Documents.Count} tài liệu liên quan.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<SchoolDto> ToggleSchoolStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<School>();
        var entity = await repo.Query()
            .Include(s => s.EducationLevel)
            .Include(s => s.Faculties)
                .ThenInclude(f => f.Majors)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Trường học", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<SchoolDto>(entity);
    }

    #endregion

    #region Faculty

    public async Task<FacultyDto> CreateFacultyAsync(CreateFacultyRequest request, CancellationToken cancellationToken = default)
    {
        var schoolRepo = _unitOfWork.Repository<School>();
        if (!await schoolRepo.AnyAsync(s => s.Id == request.SchoolId, cancellationToken))
        {
            throw new NotFoundException("Trường học", request.SchoolId);
        }

        var repo = _unitOfWork.Repository<Faculty>();
        if (await repo.AnyAsync(f => f.SchoolId == request.SchoolId && f.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã khoa '{request.Code}' đã tồn tại trong trường này.");
        }

        var entity = new Faculty
        {
            SchoolId = request.SchoolId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Description = request.Description?.Trim(),
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var faculty = await repo.Query()
            .Include(f => f.School)
            .Include(f => f.Majors)
            .FirstAsync(f => f.Id == entity.Id, cancellationToken);

        return _mapper.Map<FacultyDto>(faculty);
    }

    public async Task<FacultyDto> UpdateFacultyAsync(long id, UpdateFacultyRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Faculty>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Khoa", id);

        var schoolRepo = _unitOfWork.Repository<School>();
        if (!await schoolRepo.AnyAsync(s => s.Id == request.SchoolId, cancellationToken))
        {
            throw new NotFoundException("Trường học", request.SchoolId);
        }

        if (await repo.AnyAsync(f => f.Id != id && f.SchoolId == request.SchoolId && f.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã khoa '{request.Code}' đã tồn tại trong trường này.");
        }

        entity.SchoolId = request.SchoolId;
        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var faculty = await repo.Query()
            .Include(f => f.School)
            .Include(f => f.Majors)
            .FirstAsync(f => f.Id == entity.Id, cancellationToken);

        return _mapper.Map<FacultyDto>(faculty);
    }

    public async Task DeleteFacultyAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Faculty>();
        var entity = await repo.Query()
            .Include(f => f.Majors)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Khoa", id);

        if (entity.Majors.Any())
        {
            throw new BadRequestException($"Không thể xóa khoa '{entity.Name}' vì đang có {entity.Majors.Count} ngành trực thuộc.");
        }

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa khoa '{entity.Name}' vì đang có {entity.Documents.Count} tài liệu liên quan.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<FacultyDto> ToggleFacultyStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Faculty>();
        var entity = await repo.Query()
            .Include(f => f.School)
            .Include(f => f.Majors)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Khoa", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<FacultyDto>(entity);
    }

    #endregion

    #region Major

    public async Task<MajorDto> CreateMajorAsync(CreateMajorRequest request, CancellationToken cancellationToken = default)
    {
        var facultyRepo = _unitOfWork.Repository<Faculty>();
        if (!await facultyRepo.AnyAsync(f => f.Id == request.FacultyId, cancellationToken))
        {
            throw new NotFoundException("Khoa", request.FacultyId);
        }

        var repo = _unitOfWork.Repository<Major>();
        if (await repo.AnyAsync(m => m.FacultyId == request.FacultyId && m.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã ngành '{request.Code}' đã tồn tại trong khoa này.");
        }

        var entity = new Major
        {
            FacultyId = request.FacultyId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Description = request.Description?.Trim(),
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var major = await repo.Query()
            .Include(m => m.Faculty)
                .ThenInclude(f => f.School)
            .Include(m => m.Subjects)
            .FirstAsync(m => m.Id == entity.Id, cancellationToken);

        return _mapper.Map<MajorDto>(major);
    }

    public async Task<MajorDto> UpdateMajorAsync(long id, UpdateMajorRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Major>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Ngành", id);

        var facultyRepo = _unitOfWork.Repository<Faculty>();
        if (!await facultyRepo.AnyAsync(f => f.Id == request.FacultyId, cancellationToken))
        {
            throw new NotFoundException("Khoa", request.FacultyId);
        }

        if (await repo.AnyAsync(m => m.Id != id && m.FacultyId == request.FacultyId && m.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã ngành '{request.Code}' đã tồn tại trong khoa này.");
        }

        entity.FacultyId = request.FacultyId;
        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var major = await repo.Query()
            .Include(m => m.Faculty)
                .ThenInclude(f => f.School)
            .Include(m => m.Subjects)
            .FirstAsync(m => m.Id == entity.Id, cancellationToken);

        return _mapper.Map<MajorDto>(major);
    }

    public async Task DeleteMajorAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Major>();
        var entity = await repo.Query()
            .Include(m => m.Subjects)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Ngành", id);

        if (entity.Subjects.Any())
        {
            throw new BadRequestException($"Không thể xóa ngành '{entity.Name}' vì đang có {entity.Subjects.Count} môn học trực thuộc.");
        }

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa ngành '{entity.Name}' vì đang có {entity.Documents.Count} tài liệu liên quan.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<MajorDto> ToggleMajorStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Major>();
        var entity = await repo.Query()
            .Include(m => m.Faculty)
                .ThenInclude(f => f.School)
            .Include(m => m.Subjects)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Ngành", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<MajorDto>(entity);
    }

    #endregion

    #region Subject

    public async Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MajorId.HasValue)
        {
            var majorRepo = _unitOfWork.Repository<Major>();
            if (!await majorRepo.AnyAsync(m => m.Id == request.MajorId.Value, cancellationToken))
            {
                throw new NotFoundException("Ngành", request.MajorId.Value);
            }
        }

        var repo = _unitOfWork.Repository<Subject>();
        if (await repo.AnyAsync(s => s.MajorId == request.MajorId && s.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã môn học '{request.Code}' đã tồn tại.");
        }

        var entity = new Subject
        {
            EducationLevelId = request.EducationLevelId,
            MajorId = request.MajorId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Credits = request.Credits,
            Description = request.Description?.Trim(),
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var subject = await repo.Query()
            .Include(s => s.EducationLevel)
            .Include(s => s.Major)
                .ThenInclude(m => m!.Faculty)
                    .ThenInclude(f => f.School)
            .Include(s => s.Documents)
            .FirstAsync(s => s.Id == entity.Id, cancellationToken);

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> UpdateSubjectAsync(long id, UpdateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Subject>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Môn học", id);

        if (request.MajorId.HasValue)
        {
            var majorRepo = _unitOfWork.Repository<Major>();
            if (!await majorRepo.AnyAsync(m => m.Id == request.MajorId.Value, cancellationToken))
            {
                throw new NotFoundException("Ngành", request.MajorId.Value);
            }
        }

        if (await repo.AnyAsync(s => s.Id != id && s.MajorId == request.MajorId && s.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã môn học '{request.Code}' đã tồn tại.");
        }

        entity.EducationLevelId = request.EducationLevelId;
        entity.MajorId = request.MajorId;
        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Credits = request.Credits;
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();

        var subject = await repo.Query()
            .Include(s => s.EducationLevel)
            .Include(s => s.Major)
                .ThenInclude(m => m!.Faculty)
                    .ThenInclude(f => f.School)
            .Include(s => s.Documents)
            .FirstAsync(s => s.Id == entity.Id, cancellationToken);

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task DeleteSubjectAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Subject>();
        var entity = await repo.Query()
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Môn học", id);

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa môn học '{entity.Name}' vì đang có {entity.Documents.Count} tài liệu liên quan.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<SubjectDto> ToggleSubjectStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Subject>();
        var entity = await repo.Query()
            .Include(s => s.Major)
                .ThenInclude(m => m!.Faculty)
                    .ThenInclude(f => f.School)
            .Include(s => s.Documents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Môn học", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<SubjectDto>(entity);
    }

    #endregion

    #region DocumentType

    public async Task<DocumentTypeDto> CreateDocumentTypeAsync(CreateDocumentTypeRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<DocumentType>();
        if (await repo.AnyAsync(dt => dt.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã loại tài liệu '{request.Code}' đã tồn tại.");
        }

        var entity = new DocumentType
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Icon = request.Icon?.Trim(),
            Status = EntityStatus.Active
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<DocumentTypeDto>(entity);
    }

    public async Task<DocumentTypeDto> UpdateDocumentTypeAsync(long id, UpdateDocumentTypeRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<DocumentType>();
        var entity = await repo.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new NotFoundException("Loại tài liệu", id);

        if (await repo.AnyAsync(dt => dt.Id != id && dt.Code.ToLower() == request.Code.Trim().ToLower(), cancellationToken))
        {
            throw new ValidationException($"Mã loại tài liệu '{request.Code}' đã tồn tại.");
        }

        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim().ToUpper();
        entity.Icon = request.Icon?.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<DocumentTypeDto>(entity);
    }

    public async Task DeleteDocumentTypeAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<DocumentType>();
        var entity = await repo.Query()
            .Include(dt => dt.Documents)
            .FirstOrDefaultAsync(dt => dt.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Loại tài liệu", id);

        if (entity.Documents.Any())
        {
            throw new BadRequestException($"Không thể xóa loại tài liệu '{entity.Name}' vì đang chứa {entity.Documents.Count} tài liệu.");
        }

        repo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
    }

    public async Task<DocumentTypeDto> ToggleDocumentTypeStatusAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<DocumentType>();
        var entity = await repo.Query()
            .Include(dt => dt.Documents)
            .FirstOrDefaultAsync(dt => dt.Id == id, cancellationToken);

        if (entity == null) throw new NotFoundException("Loại tài liệu", id);

        entity.Status = entity.Status == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _educationService.InvalidateEducationCache();
        return _mapper.Map<DocumentTypeDto>(entity);
    }

    #endregion
}
