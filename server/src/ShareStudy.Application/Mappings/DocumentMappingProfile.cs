using AutoMapper;
using ShareStudy.Application.DTOs.Document;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Mappings;

public class DocumentMappingProfile : Profile
{
    public DocumentMappingProfile()
    {
        CreateMap<Document, DocumentListItemDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel.Name))
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.School != null ? src.School.Name : null))
            .ForMember(dest => dest.FacultyName, opt => opt.MapFrom(src => src.Faculty != null ? src.Faculty.Name : null))
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.Name : null))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : null))
            .ForMember(dest => dest.CategorySummary, opt => opt.MapFrom(src => BuildCategorySummary(src)))
            .ForMember(dest => dest.UploaderId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UploaderName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UploaderAvatar, opt => opt.MapFrom(src => src.User.AvatarUrl))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Document, DocumentDetailDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel.Name))
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.School != null ? src.School.Name : null))
            .ForMember(dest => dest.FacultyName, opt => opt.MapFrom(src => src.Faculty != null ? src.Faculty.Name : null))
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.Name : null))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : null))
            .ForMember(dest => dest.CategorySummary, opt => opt.MapFrom(src => BuildCategorySummary(src)))
            .ForMember(dest => dest.UploaderId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UploaderName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UploaderAvatar, opt => opt.MapFrom(src => src.User.AvatarUrl))
            .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src => src.DocumentType.Name))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.Name : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.DocumentTags.Select(dt => dt.Tag.Name).ToList()));
    }

    private static string BuildCategorySummary(Document src)
    {
        var parts = new List<string>();
        if (src.Major != null && !string.IsNullOrWhiteSpace(src.Major.Name))
        {
            parts.Add(src.Major.Name);
        }
        if (src.Subject != null && !string.IsNullOrWhiteSpace(src.Subject.Name))
        {
            parts.Add(src.Subject.Name);
        }

        if (parts.Count > 0)
        {
            return string.Join(" > ", parts);
        }

        return src.EducationLevel?.Name ?? string.Empty;
    }
}
