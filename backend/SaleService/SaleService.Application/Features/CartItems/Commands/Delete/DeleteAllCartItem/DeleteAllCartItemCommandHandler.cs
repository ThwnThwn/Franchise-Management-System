using MediatR;
using SalesService.Application.Interfaces.CartItems;

namespace SalesService.Application.Features.CartItems.Commands.Delete.DeleteAllCartItem
{
    public class DeleteAllCartItemCommandHandler
        : IRequestHandler<DeleteAllCartItemCommand, bool>
    {
        private readonly ICartItemRepository _cartItemRepository;

        public DeleteAllCartItemCommandHandler(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<bool> Handle(DeleteAllCartItemCommand command,CancellationToken cancellationToken)
        {
            var request = command.Request;
            await _cartItemRepository.ClearCartAsync( request.CustomerId,cancellationToken);
            return true;
        }
    }
}
