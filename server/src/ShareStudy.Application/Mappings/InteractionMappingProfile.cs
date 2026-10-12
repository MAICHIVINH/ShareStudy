using AutoMapper;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.DTOs.Comment;
using ShareStudy.Application.DTOs.Favorite;
using ShareStudy.Application.DTOs.Notification;
using ShareStudy.Application.DTOs.Rating;
using ShareStudy.Application.DTOs.Report;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Mappings;

public class InteractionMappingProfile : Profile
{
    public InteractionMappingProfile()
    {
        // Favorite
        CreateMap<Favorite, FavoriteDto>();

        // Rating
        CreateMap<Rating, RatingDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.AvatarUrl));

        // Comment
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.AvatarUrl))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Report
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReporterId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.Document.Title))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.ResolvedById, opt => opt.MapFrom(src => src.ResolvedBy))
            .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => src.ResolverUser != null ? src.ResolverUser.FullName : null))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Notification
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.ReferenceId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        // User (Admin View)
        CreateMap<User, AdminUserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
    }
}
