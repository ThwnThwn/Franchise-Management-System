using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.Ingrdient.Queries.GetAllIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Queries.GetAllStock
{
    public class GetAllStockHandler : IRequestHandler<GetAllStockQuery, PagedResult<GetAllStockViewRes>>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepository;
        public GetAllStockHandler(IStoreIngredientRepository storeIngredientRepository)
        {
            _storeIngredientRepository = storeIngredientRepository;
        }
        public async Task<PagedResult<GetAllStockViewRes>> Handle(GetAllStockQuery request, CancellationToken cancellationToken)
        {
            var pagedIngredients = await _storeIngredientRepository.GetPagedStockAsync(
                request.StoreIds,
                request.CategoryIds,
                request.StockStatus,
                request.IngredientIds,
                request.Page,
                request.PageSize,
                cancellationToken
            );
            var result = pagedIngredients.Items.Select(i => _mapper.Map<IngredientViewRes>(i)).ToList();
            return PagedResult<GetAllStockViewRes>.Create(result, pagedIngredients.TotalCount, request.Page, request.PageSize);
        }
    }
}
