using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces.CartItems
{
    public interface ICartItemRepository 
    {
        Task<CartItem> CreateAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<CartItem>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<CartItem?> GetByCustomerAndSkuAsync(Guid customerId,string sku, CancellationToken cancellationToken = default);
        Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task RemoveItemsAsync(Guid? customerId,List<string> skus, CancellationToken cancellationToken = default);
        Task ClearCartAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
