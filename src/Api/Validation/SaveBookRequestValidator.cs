using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveBookRequestValidator : AbstractValidator<SaveBookRequest>
{
    public SaveBookRequestValidator()
    {
        RuleFor(request => request.Isbn)
            .Must(value => !string.IsNullOrWhiteSpace(value) && value.Length <= 32)
            .WithMessage("ISBN обов'язковий, до 32 символів.");

        RuleFor(request => request.Title)
            .Must(value => !string.IsNullOrWhiteSpace(value) && value.Length <= 200)
            .WithMessage("Назва обов'язкова, до 200 символів.");

        RuleFor(request => request.Author)
            .Must(value => !string.IsNullOrWhiteSpace(value) && value.Length <= 200)
            .WithMessage("Автор обов'язковий, до 200 символів.");

        RuleFor(request => request.Year)
            .Must(year => year >= 1450 && year <= DateTime.Now.Year)
            .WithMessage("Рік видання має бути від 1450 до поточного року.");

        RuleFor(request => request.Copies)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Кількість примірників не може бути від'ємною.");
    }
}
