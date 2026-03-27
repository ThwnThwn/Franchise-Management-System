using AutoMapper;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Features.StoreIngredient.Queries.GetAllStock;
using InventoryService.Application.Persistences.Repositories;
using Moq;

namespace InventoryService.Application.Tests.Features.StoreIngredient;

public class GetAllStockHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllStockHandler _handler;

    public GetAllStockHandlerTests()
    {
        _storeRepoMock = new Mock<IStoreIngredientRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllStockHandler(_storeRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenDataExists()
    {
        // Arrange
        var query = new GetAllStockQuery
        {
            Page = 1,
            PageSize = 10,
            StoreIds = new List<Guid> { Guid.NewGuid() }
        };

        // Tạo dữ liệu mẫu từ Repository (Giả sử Entity trả về là StoreIngredient)
        var storeIngredients = new List<Domain.Entities.StoreIngredient>
        {
            new() { StoreId = query.StoreIds.First(), Quantity = 100 },
            new() { StoreId = query.StoreIds.First(), Quantity = 50 }
        };

        // Mock kết quả phân trang từ Repository
        // Lưu ý: Duy cần đảm bảo GetPagedStockAsync trả về PagedResult hoặc cấu trúc tương đương
        var pagedData = PagedResult<Domain.Entities.StoreIngredient>.Create(storeIngredients, 2, query.Page, query.PageSize);

        _storeRepoMock.Setup(x => x.GetPagedStockAsync(
                query.StoreIds,
                query.CategoryIds,
                query.StockStatus,
                query.IngredientIds,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        // Mock AutoMapper: Khi map StoreIngredient -> StockViewRes
        _mapperMock.Setup(m => m.Map<StockViewRes>(It.IsAny<Domain.Entities.StoreIngredient>()))
            .Returns((Domain.Entities.StoreIngredient src) => new StockViewRes { StoreId = src.StoreId, Quantity = (decimal)src.Quantity! });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Count.Should().Be(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);

        // Xác nhận mapper được gọi đúng số lần bằng số lượng Items
        _mapperMock.Verify(m => m.Map<StockViewRes>(It.IsAny<Domain.Entities.StoreIngredient>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoDataFound()
    {
        // Arrange
        var query = new GetAllStockQuery { Page = 1, PageSize = 10 };
        var emptyPagedData = PagedResult<Domain.Entities.StoreIngredient>.Create(new List<Domain.Entities.StoreIngredient>(), 0, query.Page, query.PageSize);

        _storeRepoMock.Setup(x => x.GetPagedStockAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<StockStatus?>(),
                It.IsAny<IEnumerable<Guid>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _mapperMock.Verify(m => m.Map<StockViewRes>(It.IsAny<Domain.Entities.StoreIngredient>()), Times.Never);
    }
}