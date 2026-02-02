using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient
{
    public record class DeleteIngredientCommand(Guid Request) : IRequest<Unit>
    {
    }
}
