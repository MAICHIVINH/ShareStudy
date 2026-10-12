using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateEducationLevelRequestValidator : AbstractValidator<CreateEducationLevelRequest>
{
    public CreateEducationLevelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cấp học không được để trống.")
            .MaximumLength(100).WithMessage("Tên cấp học không được vượt quá 100 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã cấp học không được để trống.")
            .MaximumLength(50).WithMessage("Mã cấp học không được vượt quá 50 ký tự.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }
}

public class UpdateEducationLevelRequestValidator : AbstractValidator<UpdateEducationLevelRequest>
{
    public UpdateEducationLevelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cấp học không được để trống.")
            .MaximumLength(100).WithMessage("Tên cấp học không được vượt quá 100 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã cấp học không được để trống.")
            .MaximumLength(50).WithMessage("Mã cấp học không được vượt quá 50 ký tự.");
    }
}
