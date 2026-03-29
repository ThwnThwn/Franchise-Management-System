using Moq;
using FluentAssertions;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.Inventory;
using SalesService.Application.Features.CartItems.Queries.GetByCustomerId;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Domain.Entities;

namespace SalesService.Application.Tests.Features.CartItems;

public class GetCartItemsByCustomerIdHandlerTests
{
    private readonly Mock<ICartItemRepository> _cartRepoMock = new();
    private readonly Mock<IProductServiceClient> _productClientMock = new();
    private readonly GetCartItemsByCustomerIdHandler _handler;

    public GetCartItemsByCustomerIdHandlerTests()
    {
        _handler = new GetCartItemsByCustomerIdHandler(_cartRepoMock.Object, _productClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCartItems()
    {
        var customerId = Guid.NewGuid();
        var query = new GetCartItemsByCustomerIdQuery(customerId);
        _cartRepoMock.Setup(x => x.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CartItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedItems_WhenCartItemsExist()
    {
        var customerId = Guid.NewGuid();
        var query = new GetCartItemsByCustomerIdQuery(customerId);
        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), CustomerId = customerId, Sku = "SKU1", Quantity = 2, BasePrice = 100 },
            new() { Id = Guid.NewGuid(), CustomerId = customerId, Sku = "SKU2", Quantity = 1, BasePrice = 200 }
        };

        _cartRepoMock.Setup(x => x.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(cartItems);

        _productClientMock.Setup(x => x.GetProductVariantBySkuAsync("SKU1"))
            .ReturnsAsync(new ProductVariantInfo { Sku = "SKU1", ProductName = "Product 1", ImageUrl = "url1" });
        _productClientMock.Setup(x => x.GetProductVariantBySkuAsync("SKU2"))
            .ReturnsAsync((ProductVariantInfo?)null); // testing skip line 29

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Sku.Should().Be("SKU1");
        result.First().ProductName.Should().Be("Product 1");
    }
}
