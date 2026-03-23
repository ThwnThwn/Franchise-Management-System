using MediatR;
using SalesService.Application.Dtos.WithdrawalRequests;

namespace SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;

public record GetWithdrawalByIdQuery(Guid Id) : IRequest<WithdrawalDetailDto?>;