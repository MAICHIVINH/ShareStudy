using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateFacultyRequestValidator : AbstractValidator<CreateFacultyRequest>
{
    public CreateFacultyRequestValidator()
    {
        RuleFor(x => x.SchoolId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Trường học hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên khoa không được để trống.")
            .MaximumLength(150).WithMessage("Tên khoa không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã khoa không được để trống.")
            .MaximumLength(50).WithMessage("Mã khoa không được vượt quá 50 ký tự.");
    }
}

public class UpdateFacultyRequestValidator : AbstractValidator<UpdateFacultyRequest>
{
    public UpdateFacultyRequestValidator()
    {
        RuleFor(x => x.SchoolId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Trường học hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên khoa không được để trống.")
            .MaximumLength(150).WithMessage("Tên khoa không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã khoa không được để trống.")
            .MaximumLength(50).WithMessage("Mã khoa không được vượt quá 50 ký tự.");
    }
}
