using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.Interfaces.Payments;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Payments.GetMyPaymentHistory;

public sealed class GetMyPaymentHistoryHandler
    : IRequestHandler<GetMyPaymentHistoryQuery, PagedResult<PaymentHistoryItemDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetMyPaymentHistoryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PagedResult<PaymentHistoryItemDto>> Handle(
        GetMyPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var paged = await _paymentRepository.GetMyPaymentHistoryAsync(
            request.CustomerId,
            request.Page <= 0 ? 1 : request.Page,
            request.PageSize <= 0 ? 10 : request.PageSize,
            request.StartDate,
            request.EndDate,
            request.Status,
            cancellationToken);

        var items = paged.Items.Select(p => new PaymentHistoryItemDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            OrderCode = p.Order?.Code ?? string.Empty,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = ((PaymentMethod)p.Method).ToString(),
            Status = p.Status,
            PaymentDate = p.PaidAt ?? p.CreatedAt
        }).ToList();

        return PagedResult<PaymentHistoryItemDto>.Create(
            items,
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }
}
