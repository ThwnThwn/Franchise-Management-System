using MediatR;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.CartItem.Response;
using SalesService.Application.Features.CartItems.Queries.GetByCustomerId;
using SalesService.Application.Interfaces.CartItems;

public class GetCartItemsByCustomerIdHandler
    : IRequestHandler<GetCartItemsByCustomerIdQuery, List<CartItemRep>>
{
    private readonly ICartItemRepository _cartRepo;
    private readonly IProductServiceClient _productClient;

    public GetCartItemsByCustomerIdHandler(ICartItemRepository cartRepo,IProductServiceClient productClient)
    {
        _cartRepo = cartRepo;
        _productClient = productClient;
    }

    public async Task<List<CartItemRep>> Handle(GetCartItemsByCustomerIdQuery request,CancellationToken cancellationToken)
    {
        var cartItems = await _cartRepo.GetByCustomerIdAsync(request.CustomerId);

        var result = new List<CartItemRep>();

        foreach (var item in cartItems)
        {
            var product = await _productClient.GetProductVariantBySkuAsync(item.Sku);
            if(product == null)
            {
                continue; 
            }
            result.Add(new CartItemRep
            {
                Id = item.Id,
                CustomerId = item.CustomerId,
                Sku = item.Sku,
                ProductName = product.ProductName,
                Quantity = item.Quantity,
                BasePrice = item.BasePrice,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            });
        }

        return result;
    }
}
