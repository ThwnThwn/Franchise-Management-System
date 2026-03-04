using MediatR;

namespace SalesService.Application.Features.Orders.Queries.GetByIds
{
    public class GetByUserIdsQuery() : IRequest<IEnumerable<GetByUserIdsRes>>
    {
        public List<Guid> UserIds { get; set; } = new();
    }
    public class GetByUserIdsRes
    {
        public Guid UserId { get; set; }
        public int OrderCount { get; set; }
    }
}
