using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Services;

namespace ShareStudy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<IAdminEducationService, AdminEducationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminManagementService, AdminManagementService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
