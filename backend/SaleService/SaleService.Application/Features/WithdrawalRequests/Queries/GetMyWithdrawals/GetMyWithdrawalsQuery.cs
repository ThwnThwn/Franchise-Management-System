using BuildingBlocks.Core.Pagination;
using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetMyWithdrawals;

public record GetMyWithdrawalsQuery(
     Guid UserId,
     WithdrawalStatus? Status,
     int PageIndex,
     int PageSize
) : IRequest<PagedResult<WithdrawalSummaryDto>>;