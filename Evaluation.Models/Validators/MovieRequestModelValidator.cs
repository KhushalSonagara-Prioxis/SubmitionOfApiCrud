using Evaluation.Models.RequestModel;
using FluentValidation;


namespace Evaluation.Models.Validators;

public class MovieRequestModelValidator : AbstractValidator<MovieRequestModel>
{
    public MovieRequestModelValidator()
    {
        RuleFor(m => m.MovieSid)
            .NotEmpty().WithMessage("Movie SID is required.")
            .MaximumLength(10).WithMessage("Movie SID must be exactly 10 characters.");

        RuleFor(m => m.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(1, 255).WithMessage("Title must be between 1 and 255 characters.");

        RuleFor(m => m.Genre)
            .NotEmpty().WithMessage("Genre is required.")
            .MaximumLength(100);

        RuleFor(m => m.Director)
            .NotEmpty().WithMessage("Director is required.")
            .MaximumLength(150);

        RuleFor(m => m.ReleaseDate)
            .NotEmpty().WithMessage("Release date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Release date cannot be in the future.");

        RuleFor(m => m.Rating)
            .InclusiveBetween(0.0m, 10.0m).When(m => m.Rating.HasValue)
            .WithMessage("Rating must be between 0.0 and 10.0.");

        RuleFor(m => m.Review)
            .MaximumLength(1000).WithMessage("Review cannot exceed 1000 characters.");
    }
}
