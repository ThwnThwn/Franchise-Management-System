using MediatR;


namespace EngagementService.Application.Features.Voucher.Queries.GetMyVoucher
{
    public record class GetMyVoucherQuery(Guid userID) : IRequest<List<Domain.Entities.Voucher>>
    {
    }
}
