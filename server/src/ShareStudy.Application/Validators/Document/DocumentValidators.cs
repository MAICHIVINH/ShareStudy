using FluentValidation;
using ShareStudy.Application.DTOs.Document;

namespace ShareStudy.Application.Validators.Document;

public class CreateDocumentRequestValidator : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề tài liệu không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề tài liệu không được vượt quá 200 ký tự.");

        RuleFor(x => x.EducationLevelId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Cấp học hợp lệ.");

        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Loại tài liệu hợp lệ.");

        RuleFor(x => x.File)
            .NotNull().WithMessage("Vui lòng đính kèm file tài liệu.");
    }
}

public class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
{
    public UpdateDocumentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề tài liệu không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề tài liệu không được vượt quá 200 ký tự.");

        RuleFor(x => x.EducationLevelId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Cấp học hợp lệ.");

        RuleFor(x => x.DocumentTypeId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Loại tài liệu hợp lệ.");
    }
}
