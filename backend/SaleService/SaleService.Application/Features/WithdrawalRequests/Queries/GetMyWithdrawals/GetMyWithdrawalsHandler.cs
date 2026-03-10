using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Application.Persisitences.WithdrawalRequest;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetMyWithdrawals;

public class GetMyWithdrawalsHandler(IWithdrawalRequestRepository repo) : IRequestHandler<GetMyWithdrawalsQuery, PagedResult<WithdrawalSummaryDto>>
{
    private readonly IWithdrawalRequestRepository _repo = repo;

    public async Task<PagedResult<WithdrawalSummaryDto>> Handle(GetMyWithdrawalsQuery request, CancellationToken ct)
    {
        var pagedEntities = await _repo.GetPagedWithdrawalsAsync(
            request.UserId, request.Status, request.PageIndex, request.PageSize, ct);

        var dtos = pagedEntities.Items.Select(x => new WithdrawalSummaryDto
        {
            Id = x.Id,
            RequestCode = x.RequestCode,
            UserId = x.UserId,
            Amount = x.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            FeeAmount = x.FeeAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            BankCode = x.BankCode,
            BankName = x.BankName,
            AccountName = x.AccountName,
            MaskedAccountNumber = x.MaskedAccountNumber,
            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();

        return PagedResult<WithdrawalSummaryDto>.Create(
            dtos,
            pagedEntities.TotalCount,
            pagedEntities.Page,
            pagedEntities.PageSize);
    }
}