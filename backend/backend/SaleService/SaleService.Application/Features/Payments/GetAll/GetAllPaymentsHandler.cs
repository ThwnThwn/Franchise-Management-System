using MediatR;
using BuildingBlocks.Core.Pagination;
using SalesService.Application.Interfaces.Payments;

namespace SalesService.Application.Features.Payments.GetAll;

public sealed class GetAllPaymentsHandler
    : IRequestHandler<GetAllPaymentsQuery, BuildingBlocks.Core.Pagination.PagedResult<PaymentListItemDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetAllPaymentsHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<BuildingBlocks.Core.Pagination.PagedResult<PaymentListItemDto>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var paged = await _paymentRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = paged.Items.Select(p => new PaymentListItemDto
        {
            Id = p.Id,
            OrderId = p.OrderId ?? Guid.Empty,
            Code = p.Code,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method,
            Status = p.Status,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt
        }).ToList();

        return BuildingBlocks.Core.Pagination.PagedResult<PaymentListItemDto>.Create(
            items,
            paged.TotalCount,
            request.Page,
            request.PageSize);
    }
}

