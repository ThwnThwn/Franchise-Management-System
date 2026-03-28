using FluentAssertions;
using MockQueryable;
using Moq;
using SupplierService.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Tests.Features.PurchaseOrders;

public class GetPurchaseOrdersHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepoMock;
    private readonly GetPurchaseOrdersQueryHandler _handler;

    private readonly Guid _supplierTrungNguyenId = Guid.NewGuid();
    private readonly Guid _supplierVinamilkId = Guid.NewGuid();
    private readonly Guid _storeQ1Id = Guid.NewGuid();
    private readonly Guid _storeGoVapId = Guid.NewGuid();

    public GetPurchaseOrdersHandlerTests()
    {
        _purchaseOrderRepoMock = new Mock<IPurchaseOrderRepository>();
        _handler = new GetPurchaseOrdersQueryHandler(_purchaseOrderRepoMock.Object);
    }

    private void SetupMockData(List<PurchaseOrder> data)
    {
        var mockQueryable = data.BuildMock();
        _purchaseOrderRepoMock.Setup(repo => repo.GetQueryable()).Returns(mockQueryable);
    }

    private List<PurchaseOrder> GetTestPurchaseOrders()
    {
        return
        [
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-001",
                SupplierId = _supplierTrungNguyenId,
                Supplier = new Supplier { Id = _supplierTrungNguyenId, Name = "Cà phê Trung Nguyên" },
                StoreId = _storeQ1Id,
                Status = PurchaseOrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                TotalAmount = 1500000m,
                NetAmount = 1500000m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-5),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-002",
                SupplierId = _supplierVinamilkId,
                Supplier = new Supplier { Id = _supplierVinamilkId, Name = "Sữa Vinamilk" },
                StoreId = _storeGoVapId,
                Status = PurchaseOrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                TotalAmount = 500000m,
                NetAmount = 480000m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-4),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-4)
            },
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-003",
                SupplierId = _supplierTrungNguyenId,
                Supplier = new Supplier { Id = _supplierTrungNguyenId, Name = "Cà phê Trung Nguyên" },
                StoreId = _storeGoVapId,
                Status = PurchaseOrderStatus.Confirmed,
                PaymentStatus = PaymentStatus.Pending,
                TotalAmount = 2000000m,
                NetAmount = 2000000m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-3),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            },
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-004",
                SupplierId = _supplierVinamilkId,
                Supplier = new Supplier { Id = _supplierVinamilkId, Name = "Sữa Vinamilk" },
                StoreId = _storeQ1Id,
                Status = PurchaseOrderStatus.Cancelled,
                PaymentStatus = PaymentStatus.Cancelled,
                TotalAmount = 300000m,
                NetAmount = 300000m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-2),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-005",
                SupplierId = _supplierTrungNguyenId,
                Supplier = new Supplier { Id = _supplierTrungNguyenId, Name = "Cà phê Trung Nguyên" },
                StoreId = _storeQ1Id,
                Status = PurchaseOrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                TotalAmount = 4000000m,
                NetAmount = 3800000m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-1),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        ];
    }

    [Fact]
    public async Task Handle_ShouldReturnAllOrders_WithDefaultPagination_WhenNoFiltersApplied()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(null, null, null, 0, 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.PurchaseOrders.Should().HaveCount(5);

        var firstOrder = result.PurchaseOrders.First();
        firstOrder.Code.Should().Be("PO-005");
    }

    [Fact]
    public async Task Handle_ShouldFilterBySupplierId_WhenValidSupplierIdProvided()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(_supplierTrungNguyenId.ToString(), null, null, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.PurchaseOrders.Should().HaveCount(3);
        result.PurchaseOrders.All(po => po.SupplierName == "Cà phê Trung Nguyên").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByStoreId_WhenValidStoreIdProvided()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(null, _storeQ1Id.ToString(), null, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.PurchaseOrders.Should().HaveCount(3);
        result.PurchaseOrders.Select(po => po.Code).Should().ContainInOrder("PO-005", "PO-004", "PO-001");
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusHasValue()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(null, null, (int)PurchaseOrderStatus.Completed, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.PurchaseOrders.Should().HaveCount(2);
        result.PurchaseOrders.All(po => po.Status == (int)PurchaseOrderStatus.Completed).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldApplyMultipleFiltersCorrectly()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(
            _supplierTrungNguyenId.ToString(),
            _storeQ1Id.ToString(),
            (int)PurchaseOrderStatus.Completed,
            1,
            10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.PurchaseOrders.Should().ContainSingle();
        result.PurchaseOrders.First().Code.Should().Be("PO-005");
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationCorrectly()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(null, null, null, 2, 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(5);
        result.PurchaseOrders.Should().HaveCount(2);

        result.PurchaseOrders.ElementAt(0).Code.Should().Be("PO-003");
        result.PurchaseOrders.ElementAt(1).Code.Should().Be("PO-002");
    }

    [Fact]
    public async Task Handle_ShouldIgnoreFilters_WhenIdsAreInvalidGuids()
    {
        // Arrange
        var data = GetTestPurchaseOrders();
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery("khong-phai-guid", "cung-khong-phai-guid", null, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(5);
        result.PurchaseOrders.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ShouldMapNullSupplierNameToEmptyString()
    {
        // Arrange
        var data = new List<PurchaseOrder>
        {
            new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                Code = "PO-006",
                SupplierId = Guid.NewGuid(),
                Supplier = null,
                Status = PurchaseOrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                OrderDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        SetupMockData(data);

        var query = new GetPurchaseOrdersQuery(null, null, null, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PurchaseOrders.Should().ContainSingle();
        result.PurchaseOrders.First().SupplierName.Should().BeEmpty();
    }
}