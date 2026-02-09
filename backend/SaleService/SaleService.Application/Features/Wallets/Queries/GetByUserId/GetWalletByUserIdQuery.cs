using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Wallets.Queries.GetByUserId;

public sealed record GetWalletByUserIdQuery(Guid UserId) :
    IRequest<Result<GetWalletByUserIdQueryResponse>>;

    public sealed class GetWalletByUserIdQueryResponse {      
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public byte[]? RowVersion { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }


