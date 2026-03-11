using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetAllWithdrawals;

public record GetAllWithdrawalsQuery(
    Guid? UserId,
    WithdrawalStatus? Status,
    int PageIndex,
    int PageSize
) : IRequest<PagedResult<WithdrawalSummaryDto>>;