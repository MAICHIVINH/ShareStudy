using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên môn học không được để trống.")
            .MaximumLength(150).WithMessage("Tên môn học không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã môn học không được để trống.")
            .MaximumLength(50).WithMessage("Mã môn học không được vượt quá 50 ký tự.");

        RuleFor(x => x.Credits)
            .GreaterThanOrEqualTo(0).WithMessage("Số tín chỉ/tiết học phải lớn hơn hoặc bằng 0.");
    }
}

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên môn học không được để trống.")
            .MaximumLength(150).WithMessage("Tên môn học không được vượt quá 150 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã môn học không được để trống.")
            .MaximumLength(50).WithMessage("Mã môn học không được vượt quá 50 ký tự.");

        RuleFor(x => x.Credits)
            .GreaterThanOrEqualTo(0).WithMessage("Số tín chỉ/tiết học phải lớn hơn hoặc bằng 0.");
    }
}
