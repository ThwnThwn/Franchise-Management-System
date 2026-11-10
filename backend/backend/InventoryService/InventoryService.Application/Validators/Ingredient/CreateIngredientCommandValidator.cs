using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient;
using InventoryService.Application.Helper;

namespace InventoryService.Application.Validators.Ingredient
{
    public class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
    {
        public CreateIngredientCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage(Message.Ingredient.NameRequired)
                .MaximumLength(100).WithMessage(Message.Ingredient.NameTooLong);
            RuleFor(x => x.Request.BaseUnit)
                .NotEmpty().WithMessage(Message.Ingredient.BaseUnitRequired)
                .Must(CheckValidBaseUnit.IsValid).WithMessage(Message.Ingredient.InvalidBaseUnit);
        }
    }
}
