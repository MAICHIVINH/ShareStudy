using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateMajorRequestValidator : AbstractValidator<CreateMajorRequest>
{
    public CreateMajorRequestValidator()
    {
        RuleFor(x => x.FacultyId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Khoa hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên ngành không được để trống.")
            .MaximumLength(150).WithMessage("Tên ngành không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã ngành không được để trống.")
            .MaximumLength(50).WithMessage("Mã ngành không được vượt quá 50 ký tự.");
    }
}

public class UpdateMajorRequestValidator : AbstractValidator<UpdateMajorRequest>
{
    public UpdateMajorRequestValidator()
    {
        RuleFor(x => x.FacultyId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Khoa hợp lệ.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên ngành không được để trống.")
            .MaximumLength(150).WithMessage("Tên ngành không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã ngành không được để trống.")
            .MaximumLength(50).WithMessage("Mã ngành không được vượt quá 50 ký tự.");
    }
}
