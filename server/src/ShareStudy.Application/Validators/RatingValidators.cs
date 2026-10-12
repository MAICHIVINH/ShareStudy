using FluentValidation;
using ShareStudy.Application.DTOs.Rating;

namespace ShareStudy.Application.Validators;

public class CreateRatingRequestValidator : AbstractValidator<CreateRatingRequest>
{
    public CreateRatingRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Điểm đánh giá phải từ 1 đến 5 sao.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Nội dung đánh giá không được vượt quá 1000 ký tự.");
    }
}

public class UpdateRatingRequestValidator : AbstractValidator<UpdateRatingRequest>
{
    public UpdateRatingRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Điểm đánh giá phải từ 1 đến 5 sao.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Nội dung đánh giá không được vượt quá 1000 ký tự.");
    }
}
