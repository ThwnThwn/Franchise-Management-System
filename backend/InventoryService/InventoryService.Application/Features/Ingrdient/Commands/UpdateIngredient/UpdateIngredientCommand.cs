using InventoryService.Application.Dtos.Ingredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Commands.UpdateIngredient
{
    public record class UpdateIngredientCommand(UpdateIngredientReq Request) : IRequest<Unit>
    {
    }
}
