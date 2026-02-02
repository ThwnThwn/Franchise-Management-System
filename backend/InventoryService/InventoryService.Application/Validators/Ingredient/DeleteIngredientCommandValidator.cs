using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Validators.Ingredient
{
    public class DeleteIngredientCommandValidator : AbstractValidator<DeleteIngredientCommand>
    {
        public DeleteIngredientCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotEmpty()
                .WithMessage(Message.Ingredient.Required);
        }
    }
}
