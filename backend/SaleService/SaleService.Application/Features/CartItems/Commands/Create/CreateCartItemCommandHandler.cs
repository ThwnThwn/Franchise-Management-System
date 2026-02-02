using MediatR;
using SalesService.Application.Dtos.CartItem.Response;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Domain.Entities;

namespace SalesService.Application.Features.CartItems.Commands.Create
{
    public class CreateCartItemCommandHandler
        : IRequestHandler<CreateCartItemCommand, CartItemRep>
    {
        private readonly ICartItemRepository _cartItemRepository;

        public CreateCartItemCommandHandler(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<CartItemRep> Handle(
            CreateCartItemCommand request,
            CancellationToken cancellationToken)
        {
            var req = request.Request;

            // 1. Check if item already exists in cart (Customer + SKU)
            var existingItem =
                await _cartItemRepository.GetByCustomerAndSkuAsync(
                    req.CustomerId,
                    req.Sku,
                    cancellationToken);

            if (existingItem != null)
            {
                // If exists → increase quantity
                existingItem.Quantity += req.Quantity;

                await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);

                return new CartItemRep
                {
                    Id = existingItem.Id,
                    CustomerId = existingItem.CustomerId,
                    Sku = existingItem.Sku,
                    Quantity = existingItem.Quantity,
                    BasePrice = existingItem.BasePrice,
                    CreatedAt = existingItem.CreatedAt,
                    UpdatedAt = existingItem.UpdatedAt
                };
            }

            // 2. Create new cart item
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CustomerId = req.CustomerId,
                Sku = req.Sku,
                Quantity = req.Quantity,
                BasePrice = req.BasePrice,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _cartItemRepository.CreateAsync(cartItem, cancellationToken);

            // 3. Return response
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
