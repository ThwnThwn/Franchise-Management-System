using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Dtos.Ingredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Commands.CreateCategory
{
    public record class CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<Unit>
    {
    }
}
