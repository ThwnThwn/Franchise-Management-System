using AutoMapper;
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
    public class GetAllStockHandler : IRequestHandler<GetAllStockQuery, PagedResult<StockViewRes>>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepository;
        private readonly IMapper _mapper;
        public GetAllStockHandler(IStoreIngredientRepository storeIngredientRepository, IMapper mapper)
        {
            _storeIngredientRepository = storeIngredientRepository;
            _mapper = mapper;
        }
        public async Task<PagedResult<StockViewRes>> Handle(GetAllStockQuery request, CancellationToken cancellationToken)
        {
            var pagedStock = await _storeIngredientRepository.GetPagedStockAsync(
                request.StoreIds,
                request.CategoryIds,
                request.StockStatus,
                request.IngredientIds,
                request.Page,
                request.PageSize,
                cancellationToken
            );
            var result = pagedStock.Items.Select(i => _mapper.Map<StockViewRes>(i)).ToList();
            return PagedResult<StockViewRes>.Create(result, pagedStock.TotalCount, request.Page, request.PageSize);
        }
    }
}
