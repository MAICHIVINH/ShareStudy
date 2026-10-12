using FluentValidation;
using ShareStudy.Application.DTOs.Education;

namespace ShareStudy.Application.Validators.Education;

public class CreateDocumentTypeRequestValidator : AbstractValidator<CreateDocumentTypeRequest>
{
    public CreateDocumentTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên loại tài liệu không được để trống.")
            .MaximumLength(100).WithMessage("Tên loại tài liệu không được vượt quá 100 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã loại tài liệu không được để trống.")
            .MaximumLength(50).WithMessage("Mã loại tài liệu không được vượt quá 50 ký tự.");
    }
}

public class UpdateDocumentTypeRequestValidator : AbstractValidator<UpdateDocumentTypeRequest>
{
    public UpdateDocumentTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên loại tài liệu không được để trống.")
            .MaximumLength(100).WithMessage("Tên loại tài liệu không được vượt quá 100 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã loại tài liệu không được để trống.")
            .MaximumLength(50).WithMessage("Mã loại tài liệu không được vượt quá 50 ký tự.");
    }
}
