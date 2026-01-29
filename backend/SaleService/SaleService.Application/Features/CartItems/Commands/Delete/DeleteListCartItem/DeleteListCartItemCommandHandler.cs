using MediatR;
using SalesService.Application.Interfaces.CartItems;

namespace SalesService.Application.Features.CartItems.Commands.Delete.DeleteListCartItem
{
    public class DeleteListCartItemCommandHandler
        : IRequestHandler<DeleteListCartItemCommand, bool>
    {
        private readonly ICartItemRepository _cartItemRepository;

        public DeleteListCartItemCommandHandler(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<bool> Handle(DeleteListCartItemCommand request, CancellationToken cancellationToken)
        {
            await _cartItemRepository.RemoveItemsAsync(
                request.Request.CustomerId,
                request.Request.Skus,
                cancellationToken);

            return true;
        }
    }
}
