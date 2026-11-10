using InventoryService.Application.Dtos.Recipe;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Commands.MutationRecipe
{
    public record class MutationRecipeCommand(MutationRecipeReqDto Request) : IRequest<Unit>
    {
    }
}
