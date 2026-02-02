using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Dtos.StoreIngredient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Queries.GetAllStock
{
    public class GetAllStockQuery : PaginationRequest, IRequest<PagedResult<GetAllStockViewRes>>
    {
        public IEnumerable<Guid>? StoreIds { get; set; }
        public IEnumerable<Guid>? CategoryIds { get; set; }
        public StockStatus? StockStatus { get; set; }
        public IEnumerable<Guid>? IngredientIds { get; set; }
    }
}
