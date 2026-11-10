using MediatR;

namespace EngagementService.Application.Features.Voucher.Queries.GetByCode
{
    public record class GetVoucherByCodeQuery(string code) : IRequest<Domain.Entities.Voucher>
    {
    }
}
