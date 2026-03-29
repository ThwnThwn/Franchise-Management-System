using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Category.Commands.CreateCategory;

namespace InventoryService.Application.Validators.Category
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage(Message.Category.NameRequired)
                .MaximumLength(100).WithMessage(Message.Category.NameTooLong);
        }
    }
}
