using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.ApplicationDbContext;

namespace SalesService.Infrastructure.Repositories.CartItems
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly ISalesDbContext _context;
        public CartItemRepository(ISalesDbContext context)
        {
            _context = context;
        }

        public async Task<CartItem> CreateAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
            return cartItem;
        }
        //Delete
        public async Task DeleteAsync(CartItem cartItem,CancellationToken cancellationToken = default)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
        }

        //Update
        public async Task UpdateAsync(CartItem cartItem,CancellationToken cancellationToken = default)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
        }

        //Get by Id
        public async Task<CartItem?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default)
        {
            return await _context.CartItems
                    .Include(o => o.Id)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync(
                        o => o.Id == id,
                        cancellationToken);
        }

        //Get all items of customer
        public async Task<List<CartItem>> GetByCustomerIdAsync(Guid customerId,CancellationToken cancellationToken = default)
        {
            return await _context.CartItems
                .Where(x => x.CustomerId == customerId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        //Get item by Customer + SKU
        public async Task<CartItem?> GetByCustomerAndSkuAsync(Guid customerId,string sku,CancellationToken cancellationToken = default)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId && x.Sku == sku,
                    cancellationToken);
        }

        //Clear cart
        public async Task ClearCartAsync(Guid customerId,CancellationToken cancellationToken = default)
        {
            var items = await _context.CartItems
                .Where(x => x.CustomerId == customerId)
                .ToListAsync(cancellationToken);

            _context.CartItems.RemoveRange(items);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveItemsAsync(Guid customerId,List<string> skus,CancellationToken cancellationToken = default)
        {
            var items = await _context.CartItems
                .Where(x => x.CustomerId == customerId && skus.Contains(x.Sku))
                .ToListAsync(cancellationToken);

            _context.CartItems.RemoveRange(items);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
