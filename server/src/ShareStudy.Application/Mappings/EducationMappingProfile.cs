using AutoMapper;
using ShareStudy.Application.DTOs.Education;
using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Mappings;

public class EducationMappingProfile : Profile
{
    public EducationMappingProfile()
    {
        // EducationLevel
        CreateMap<EducationLevel, EducationLevelDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SchoolCount, opt => opt.MapFrom(src => src.Schools.Count));

        // School
        CreateMap<School, SchoolDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.FacultyCount, opt => opt.MapFrom(src => src.Faculties.Count))
            .ForMember(dest => dest.MajorCount, opt => opt.MapFrom(src => src.Faculties.Sum(f => f.Majors.Count)));

        CreateMap<School, SchoolDetailDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.FacultyCount, opt => opt.MapFrom(src => src.Faculties.Count))
            .ForMember(dest => dest.MajorCount, opt => opt.MapFrom(src => src.Faculties.Sum(f => f.Majors.Count)))
            .ForMember(dest => dest.Faculties, opt => opt.MapFrom(src => src.Faculties));

        // Faculty
        CreateMap<Faculty, FacultyDto>()
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.School.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MajorCount, opt => opt.MapFrom(src => src.Majors.Count));

        CreateMap<Faculty, FacultyDetailDto>()
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.School.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MajorCount, opt => opt.MapFrom(src => src.Majors.Count))
            .ForMember(dest => dest.Majors, opt => opt.MapFrom(src => src.Majors));

        // Major
        CreateMap<Major, MajorDto>()
            .ForMember(dest => dest.FacultyName, opt => opt.MapFrom(src => src.Faculty.Name))
            .ForMember(dest => dest.SchoolId, opt => opt.MapFrom(src => src.Faculty.SchoolId))
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.Faculty.School.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SubjectCount, opt => opt.MapFrom(src => src.Subjects.Count));

        CreateMap<Major, MajorDetailDto>()
            .ForMember(dest => dest.FacultyName, opt => opt.MapFrom(src => src.Faculty.Name))
            .ForMember(dest => dest.SchoolId, opt => opt.MapFrom(src => src.Faculty.SchoolId))
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.Faculty.School.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SubjectCount, opt => opt.MapFrom(src => src.Subjects.Count))
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));

        // Subject
        CreateMap<Subject, SubjectDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel != null ? src.EducationLevel.Name : (src.Major != null ? src.Major.Faculty.School.EducationLevel.Name : null)))
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.Name : null))
            .ForMember(dest => dest.FacultyId, opt => opt.MapFrom(src => src.Major != null ? (long?)src.Major.FacultyId : null))
            .ForMember(dest => dest.FacultyName, opt => opt.MapFrom(src => src.Major != null ? src.Major.Faculty.Name : null))
            .ForMember(dest => dest.SchoolId, opt => opt.MapFrom(src => src.Major != null ? (long?)src.Major.Faculty.SchoolId : null))
            .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.Major != null ? src.Major.Faculty.School.Name : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents.Count));

        // DocumentType
        CreateMap<DocumentType, DocumentTypeDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents.Count));

        // AcademicYear
        CreateMap<AcademicYear, AcademicYearDto>();
    }
}
