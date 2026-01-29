using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.Stores.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetStores
{
    public class GetStoresQuery : PaginationRequest, IRequest<PagedResult<StoreDto>>
    {
        public string? Keyword { get; set; }
    }
}