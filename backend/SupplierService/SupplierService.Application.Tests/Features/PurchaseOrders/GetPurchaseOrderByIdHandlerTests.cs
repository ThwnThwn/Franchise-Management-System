using FluentAssertions;
using Moq;
using SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class GetPurchaseOrderByIdHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly GetPurchaseOrderByIdQueryHandler _handler;

    public GetPurchaseOrderByIdHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _handler = new GetPurchaseOrderByIdQueryHandler(_purchaseOrderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenIdIsNotValidGuid()
    {
        // Arrange
        var query = new GetPurchaseOrderByIdQuery("chuoi-nay-khong-phai-guid");

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid PurchaseOrder Id");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenOrderNotFound()
    {
        // Arrange
        var validGuidId = Guid.NewGuid();
        var query = new GetPurchaseOrderByIdQuery(validGuidId.ToString());

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(validGuidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Purchase order {validGuidId} not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedResult_WhenOrderExists_WithFullData()
    {
        // Arrange
        var poId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var orderDate = DateTimeOffset.UtcNow.AddDays(-2);
        var completedAt = DateTimeOffset.UtcNow;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-2);
        var updatedAt = DateTimeOffset.UtcNow;

        var po = new PurchaseOrder
        {
            Id = poId,
            Code = "PO-CF-001",
            SupplierId = supplierId,
            StoreId = storeId,
            Supplier = new Supplier { Id = supplierId, Name = "Nhà cung cấp Nguyên liệu Cầu Đất" },
            Status = PurchaseOrderStatus.Completed,
            PaymentStatus = PaymentStatus.Paid,
            TotalAmount = 2500000m,
            DiscountAmount = 100000m,
            NetAmount = 2400000m,
            PaidAmount = 2400000m,
            OrderDate = orderDate,
            CompletedAt = completedAt,
            Notes = "Giao hạt Arabica vào sáng sớm để rang",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items =
            [
                new PurchaseOrderItem
                {
                    Id = Guid.NewGuid(),
                    IngredientId = Guid.NewGuid(),
                    Quantity = 10m,
                    UnitPrice = 250000m,
                    TotalPrice = 2500000m
                }
            ]
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        var query = new GetPurchaseOrderByIdQuery(poId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(poId.ToString());
        result.Code.Should().Be("PO-CF-001");
        result.SupplierId.Should().Be(supplierId.ToString());
        result.SupplierName.Should().Be("Nhà cung cấp Nguyên liệu Cầu Đất");
        result.StoreId.Should().Be(storeId.ToString());
        result.Status.Should().Be((int)PurchaseOrderStatus.Completed);
        result.PaymentStatus.Should().Be((int)PaymentStatus.Paid);
        result.TotalAmount.Should().Be(2500000);
        result.DiscountAmount.Should().Be(100000);
        result.NetAmount.Should().Be(2400000);
        result.PaidAmount.Should().Be(2400000);
        result.OrderDate.Should().Be(orderDate.ToString("O"));
        result.CompletedAt.Should().Be(completedAt.ToString("O"));
        result.Notes.Should().Be("Giao hạt Arabica vào sáng sớm để rang");
        result.CreatedAt.Should().Be(createdAt.ToString("O"));
        result.UpdatedAt.Should().Be(updatedAt.ToString("O"));

        result.Items.Should().HaveCount(1);
        var resultItem = result.Items.First();
        var sourceItem = po.Items.First();
        resultItem.Id.Should().Be(sourceItem.Id.ToString());
        resultItem.IngredientId.Should().Be(sourceItem.IngredientId.ToString());
        resultItem.Quantity.Should().Be((double)sourceItem.Quantity);
        resultItem.UnitPrice.Should().Be((double)sourceItem.UnitPrice);
        resultItem.TotalPrice.Should().Be((double)sourceItem.TotalPrice);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedResult_WithFallbackValues_WhenNullablePropertiesAreNull()
    {
        // Arrange
        var poId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        var po = new PurchaseOrder
        {
            Id = poId,
            Code = "PO-CF-002",
            SupplierId = supplierId,
            StoreId = null,
            Supplier = null,
            CompletedAt = null,
            Notes = null,
            Items = []
        };

        _purchaseOrderRepoMock
            .Setup(x => x.GetByIdWithItemsAsync(poId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(po);

        var query = new GetPurchaseOrderByIdQuery(poId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SupplierName.Should().Be("");
        result.StoreId.Should().Be("");
        result.CompletedAt.Should().Be("");
        result.Notes.Should().Be("");
        result.Items.Should().BeEmpty();
    }
}