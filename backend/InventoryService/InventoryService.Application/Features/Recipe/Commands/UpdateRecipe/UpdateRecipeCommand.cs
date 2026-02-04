using InventoryService.Application.Dtos.Recipe;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Commands.UpdateRecipe
{
    public record class UpdateRecipeCommand(MutationRecipeReqDto Request) : IRequest<Unit>
    {
    }
}
