using FluentValidation;

namespace ProductService.Application.Features.Categories.Command.CreateCategory;

public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(255)
            .WithMessage("Category name must not exceed 255 characters");

        RuleFor(x => x.Request.Name)
        .NotEmpty()
        .MaximumLength(255)
        .Matches(@"^[a-zA-Z0-9\s\-_]+$")  //  Chỉ cho phép ký tự hợp lệ
        .WithMessage("Category name contains invalid characters");

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
    }
}