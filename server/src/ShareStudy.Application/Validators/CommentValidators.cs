using FluentValidation;
using ShareStudy.Application.DTOs.Comment;

namespace ShareStudy.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung bình luận không được vượt quá 2000 ký tự.");
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung bình luận không được vượt quá 2000 ký tự.");
    }
}
