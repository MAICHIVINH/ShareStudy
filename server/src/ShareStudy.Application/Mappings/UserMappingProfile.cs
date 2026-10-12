using AutoMapper;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
}
