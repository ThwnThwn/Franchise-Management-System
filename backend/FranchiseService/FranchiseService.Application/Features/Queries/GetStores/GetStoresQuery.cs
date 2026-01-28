using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Queries.GetStores
{
    public class GetStoresQuery : PaginationRequest, IRequest<PagedResult<StoreDto>>
    {
        public string? Keyword { get; set; }
    }
}