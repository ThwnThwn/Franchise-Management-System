using InventoryService.Application.Dtos.Category;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Queries.GetAllCategory
{
    public class GetAllCategoryQuery : IRequest<List<CategoryViewRes>>
    {
    }
}
