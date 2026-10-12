using AutoMapper;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
