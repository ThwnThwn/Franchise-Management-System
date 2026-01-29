using FranchiseService.Application.Features.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Queries.GetStoreDetail
{
    public class GetStoreDetailQuery : IRequest<StoreDto?>
    {
        public string Id { get; set; } = string.Empty;
    }
}