using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Commands.UpdateStatus.UpdateWalletStatusCommand;

namespace SalesService.Application.Features.Wallets.Commands.UpdateStatus;

public sealed record UpdateWalletStatusCommand(Guid WalletId,
    UpdateWalletStatusRequest Request
) : IRequest<Result<UpdateWalletStatusResponse>>
{
    public sealed class UpdateWalletStatusRequest
    {
        public WalletStatus NewStatus { get; init; }
        public string? Reason { get; init; }  
    }

    public sealed class UpdateWalletStatusResponse
    {
        public Guid WalletId { get; init; }
        public WalletStatus OldStatus { get; init; }
        public WalletStatus NewStatus { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
