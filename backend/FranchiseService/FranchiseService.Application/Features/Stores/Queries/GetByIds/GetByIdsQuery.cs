using FranchiseService.Application.Features.Stores.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetByIds
{
    public class GetByIdsQuery : IRequest<IEnumerable<StoreDto>>
    {
        public required IEnumerable<string> Ids { get; set; }
    }
}