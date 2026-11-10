using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient;

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
