using FluentValidation;

namespace ProductService.Application.Features.Categories.Command.UpdateCategory
{
    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.id)
                .NotEmpty()
                .WithMessage("Category ID is required");

            RuleFor(x => x.Request.Name)
                .MaximumLength(255)
                .WithMessage("Category name must not exceed 255 characters");

            RuleFor(x => x.Request.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description))
                .WithMessage("Category description must not exceed 1000 characters");
        }
    }
}