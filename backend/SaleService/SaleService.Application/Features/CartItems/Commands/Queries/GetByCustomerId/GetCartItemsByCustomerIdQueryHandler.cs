using MediatR;
using SalesService.Application.Dtos.CartItem.Response;
using SalesService.Application.Interfaces.CartItems;

namespace SalesService.Application.Features.CartItems.Queries.GetByCustomerId
{
    public class GetCartItemsByCustomerIdQueryHandler
        : IRequestHandler<GetCartItemsByCustomerIdQuery, List<CartItemResponse>>
    {
        private readonly ICartItemRepository _cartItemRepository;

        public GetCartItemsByCustomerIdQueryHandler(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }

        public async Task<List<CartItemResponse>> Handle(GetCartItemsByCustomerIdQuery request,CancellationToken cancellationToken)
        {
            var items = await _cartItemRepository.GetByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            // Map Entity → Response DTO
            return items.Select(x => new CartItemResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Sku = x.Sku,
                Quantity = x.Quantity,
                BasePrice = x.BasePrice,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }
    }
}
