using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
{
    public CreateSchoolRequestValidator()
    {
        RuleFor(x => x.EducationLevelId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Cấp học hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên trường không được để trống.")
            .MaximumLength(150).WithMessage("Tên trường không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã trường không được để trống.")
            .MaximumLength(50).WithMessage("Mã trường không được vượt quá 50 ký tự.");
    }
}

public class UpdateSchoolRequestValidator : AbstractValidator<UpdateSchoolRequest>
{
    public UpdateSchoolRequestValidator()
    {
        RuleFor(x => x.EducationLevelId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Cấp học hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên trường không được để trống.")
            .MaximumLength(150).WithMessage("Tên trường không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã trường không được để trống.")
            .MaximumLength(50).WithMessage("Mã trường không được vượt quá 50 ký tự.");
    }
}
