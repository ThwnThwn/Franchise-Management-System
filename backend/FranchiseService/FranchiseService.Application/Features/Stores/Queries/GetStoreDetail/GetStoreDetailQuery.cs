using FranchiseService.Application.Features.Stores.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetStoreDetail
{
    public class GetStoreDetailQuery : IRequest<StoreDto?>
    {
        public string Id { get; set; } = string.Empty;
    }
}