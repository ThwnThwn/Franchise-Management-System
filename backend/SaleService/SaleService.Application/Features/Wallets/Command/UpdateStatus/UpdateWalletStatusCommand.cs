using Ardalis.Result;
using MediatR;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.Wallets.Commands.UpdateStatus.UpdateWalletStatusCommand;

namespace SalesService.Application.Features.Wallets.Commands.UpdateStatus;

public sealed record UpdateWalletStatusCommand(
    Guid WalletId,
    Guid AdminId,
    UpdateWalletStatusRequest Request
) : IRequest<Result<UpdateWalletStatusResponse>>
{
    public sealed class UpdateWalletStatusRequest
    {
        public WalletStatus NewStatus { get; init; }
        public string? Reason { get; init; } 
        public string? AdminNotes { get; init; } 
    }

    public sealed class UpdateWalletStatusResponse
    {
        public Guid WalletId { get; init; }
        public WalletStatus OldStatus { get; init; }
        public WalletStatus NewStatus { get; init; }
        public string? Reason { get; init; }
        public Guid UpdatedBy { get; init; } 
        public DateTimeOffset UpdatedAt { get; init; }
    }
}