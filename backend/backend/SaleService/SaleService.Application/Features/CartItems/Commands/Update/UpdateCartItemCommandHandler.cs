using MediatR;
using SalesService.Application.Dtos.CartItem.Response;
using SalesService.Application.Interfaces.CartItems;

namespace SalesService.Application.Features.CartItems.Commands.Update
{
    public class UpdateCartItemCommandHandler
        : IRequestHandler<UpdateCartItemCommand, CartItemRep>
    {
        private readonly ICartItemRepository _cartItemRepository;

        public UpdateCartItemCommandHandler(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<CartItemRep> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            var req = request.Request;

            // 1. Find cart item by CustomerId + SKU
            var cartItem = await _cartItemRepository.GetByCustomerAndSkuAsync(
                req.CustomerId,
                req.Sku,
                cancellationToken);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            // 2. Update quantity
            cartItem.Quantity = req.Quantity;

            // 3. Save
            await _cartItemRepository.UpdateAsync(cartItem, cancellationToken);

            // 4. Return response
            return new CartItemRep
            {
                Id = cartItem.Id,
                CustomerId = cartItem.CustomerId,
                Sku = cartItem.Sku,
                Quantity = cartItem.Quantity,
                BasePrice = cartItem.BasePrice,
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt
            };
        }
    }
}
