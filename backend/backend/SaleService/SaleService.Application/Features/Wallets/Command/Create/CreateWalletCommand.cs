using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Command.Create.CreateWalletCommand;

namespace SalesService.Application.Features.Wallets.Command.Create;

public sealed record CreateWalletCommand(Guid UserId)
    : IRequest<Result<CreateWalletResponse>>
{
    public sealed class CreateWalletResponse
    {
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public WalletStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}