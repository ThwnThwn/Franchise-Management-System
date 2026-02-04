using AutoMapper;
using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Queries.GetAllIngredient
{
    public class GetAllIngredientHandler : IRequestHandler<GetAllIngredientQuery, PagedResult<IngredientViewRes>>
    {
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IMapper _mapper;

        public GetAllIngredientHandler(IIngredientRepository ingredientRepository, IMapper mapper)
        {
            _ingredientRepository = ingredientRepository;
            _mapper = mapper;
        }
        public async Task<PagedResult<IngredientViewRes>> Handle(GetAllIngredientQuery request, CancellationToken cancellationToken)
        {
            var pagedIngredients =  await _ingredientRepository.GetPagedIngredientsAsync(
                request.CategoryId,
                request.Page,
                request.PageSize,
                cancellationToken
            );
           var result = pagedIngredients.Items.Select(i => _mapper.Map<IngredientViewRes>(i)).ToList();
           return PagedResult<IngredientViewRes>.Create(result, pagedIngredients.TotalCount, request.Page, request.PageSize);
        }
    }
}
