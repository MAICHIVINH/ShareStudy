using FluentValidation;
using ShareStudy.Application.DTOs.Report;

namespace ShareStudy.Application.Validators;

public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Lý do báo cáo không được để trống.");
    }
}
