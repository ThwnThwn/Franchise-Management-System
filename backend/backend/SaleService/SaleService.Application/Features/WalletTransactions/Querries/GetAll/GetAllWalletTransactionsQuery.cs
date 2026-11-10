using Ardalis.Result;
using MediatR;
using SalesService.Application.Features.WalletTransactions.Dtos;

namespace SalesService.Application.Features.WalletTransactions.Queries.GetAll;

public sealed record GetAllWalletTransactionsQuery(Guid LoggedUserId,
    int PageIndex = 1,
    int PageSize = 20)
    : IRequest<Result<BuildingBlocks.Core.Pagination.PagedResult<WalletTransactionResponse>>>;