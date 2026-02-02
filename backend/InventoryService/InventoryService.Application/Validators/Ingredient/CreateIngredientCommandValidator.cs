using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .Must(IsMassOrVolume).WithMessage(Message.Ingredient.InvalidBaseUnit);
            RuleFor(x => x.Request.CategoryId)
                .NotEmpty().WithMessage(Message.Ingredient.CategoryRequired);
        }
        private bool IsMassOrVolume(string baseUnit)
        {
            if (string.IsNullOrWhiteSpace(baseUnit)) return false;
            var unit = baseUnit.ToLower().Trim();
            // Kiểm tra các biến thể của gam và mililit
            return unit == "g" || unit == "gam" || unit == "gram" ||
                   unit == "ml" || unit == "mililit" || unit == "millilitre";
        }
    }
}
