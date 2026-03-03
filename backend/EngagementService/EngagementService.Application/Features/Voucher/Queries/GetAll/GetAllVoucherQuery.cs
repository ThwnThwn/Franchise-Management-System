using MediatR;

namespace EngagementService.Application.Features.Voucher.Queries.GetAll
{
    public record class GetAllVoucherQuery() : IRequest<IEnumerable<Domain.Entities.Voucher>>
    {

    }
}
