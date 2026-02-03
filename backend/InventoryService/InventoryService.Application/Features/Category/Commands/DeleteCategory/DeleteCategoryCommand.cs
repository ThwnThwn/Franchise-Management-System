using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Commands.DeleteCategory
{
    public record class DeleteCategoryCommand(Guid Request) : IRequest<Unit>
    {
    }
}
