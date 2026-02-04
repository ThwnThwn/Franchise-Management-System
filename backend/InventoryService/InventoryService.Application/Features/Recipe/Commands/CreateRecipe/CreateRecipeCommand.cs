using InventoryService.Application.Dtos.Recipe;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Commands.CreateRecipe
{
    public record class CreateRecipeCommand(MutationRecipeReqDto Request) : IRequest<Unit>
    {
    }
}
