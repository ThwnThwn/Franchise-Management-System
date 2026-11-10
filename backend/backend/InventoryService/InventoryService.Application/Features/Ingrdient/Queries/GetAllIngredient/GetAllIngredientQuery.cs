using AutoMapper;
using BuildingBlocks.Core.Pagination;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Ingrdient.Queries.GetAllIngredient
{
    
    public class GetAllIngredientQuery() : PaginationRequest, IRequest<PagedResult<IngredientViewRes>>
    {
        public Guid? CategoryId { get; set; }
    }
}
