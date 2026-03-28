using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetProductsHandler(_productRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoProductsExist()
    {
        // Arrange
        // Thử kịch bản không truyền CategoryId (lấy toàn bộ menu)
        var query = new GetProductsQuery(null);
        var emptyProductList = new List<Product>();
        var emptyDtoList = new List<GetProductListItemDto>();

        _productRepositoryMock
            .Setup(repo => repo.GetAllProductsAsync(query.CategoryId))
            .ReturnsAsync(emptyProductList);

        _mapperMock
            .Setup(m => m.Map<List<GetProductListItemDto>>(emptyProductList))
            .Returns(emptyDtoList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Products.Should().NotBeNull();
        result.Value.Products.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        _productRepositoryMock.Verify(repo => repo.GetAllProductsAsync(query.CategoryId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnPopulatedList_WhenProductsExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        // Thử kịch bản có truyền CategoryId (ví dụ lấy nhóm Cà Phê)
        var query = new GetProductsQuery(categoryId);

        var productsFromDb = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Cà phê đen đá", CategoryId = categoryId },
            new() { Id = Guid.NewGuid(), Name = "Bạc xỉu", CategoryId = categoryId }
        };

        var mappedDtos = new List<GetProductListItemDto>
        {
            new() { Id = productsFromDb[0].Id, Name = "Cà phê đen đá", CategoryId = categoryId },
            new() { Id = productsFromDb[1].Id, Name = "Bạc xỉu", CategoryId = categoryId }
        };

        _productRepositoryMock
            .Setup(repo => repo.GetAllProductsAsync(query.CategoryId))
            .ReturnsAsync(productsFromDb);

        _mapperMock
            .Setup(m => m.Map<List<GetProductListItemDto>>(productsFromDb))
            .Returns(mappedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Products.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);

        // Kiểm tra nhẹ phần tử đầu tiên xem có khớp không
        result.Value.Products.First().Name.Should().Be("Cà phê đen đá");

        _productRepositoryMock.Verify(repo => repo.GetAllProductsAsync(query.CategoryId), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GetProductListItemDto>>(productsFromDb), Times.Once);
    }
}