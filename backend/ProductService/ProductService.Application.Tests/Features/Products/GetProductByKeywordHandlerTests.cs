using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using ProductService.Application.Dtos;
using ProductService.Application.Features.Products.Queries.GetByKeyword;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class GetProductByKeywordHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByKeywordQueryHandler _handler;

    public GetProductByKeywordHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByKeywordQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoProductsMatch()
    {
        // Arrange
        var query = new GetProductByKeywordQuery("Trà vảy rồng", null, 1, 10); // Đổi keyword không tồn tại cho chuẩn domain :))

        // Giả lập DB trả về PagedResult rỗng (0 items)
        var emptyPagedResult = new PagedResult<Product>(new List<Product>(), 0, query.Page, query.PageSize);

        _productRepositoryMock
            .Setup(repo => repo.GetAllProductWithPagingByAsync(
                query.Keyword, query.CategoryId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(query.Page);
        result.PageSize.Should().Be(query.PageSize);

        // Đảm bảo Repo được gọi đúng tham số truyền vào
        _productRepositoryMock.Verify(repo => repo.GetAllProductWithPagingByAsync(
            query.Keyword, query.CategoryId, query.Page, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedProducts_WhenProductsExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductByKeywordQuery("Cà phê", categoryId, 1, 10);

        // Khởi tạo List Product thật cẩn thận để Extension Method MapToProductDto không bị NullReferenceException
        var productList = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Cà phê sữa đá",
                CategoryId = categoryId,
                Category = new Category { Name = "Đồ uống" }, // Đổi thành danh mục Đồ uống
                ProductVariants = [] // Bắt buộc khởi tạo list rỗng
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bạc xỉu",
                CategoryId = categoryId,
                Category = new Category { Name = "Đồ uống" },
                ProductVariants = []
            }
        };

        var pagedResult = new PagedResult<Product>(productList, 2, query.Page, query.PageSize);

        _productRepositoryMock
            .Setup(repo => repo.GetAllProductWithPagingByAsync(
                query.Keyword, query.CategoryId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().Name.Should().Be("Cà phê sữa đá");
        result.Items.Last().Name.Should().Be("Bạc xỉu");
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(query.Page);
        result.PageSize.Should().Be(query.PageSize);

        // Đảm bảo kiểu dữ liệu trả về đúng là Dto
        result.Items.Should().AllBeOfType<ProductDto>();

        _productRepositoryMock.Verify(repo => repo.GetAllProductWithPagingByAsync(
            query.Keyword, query.CategoryId, query.Page, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
}