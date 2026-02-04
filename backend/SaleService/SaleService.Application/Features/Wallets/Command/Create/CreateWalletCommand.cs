using MediatR;

namespace SalesService.Application.Features.Wallets.Command.Create
{
    public sealed class CreateWalletCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
    }
}
