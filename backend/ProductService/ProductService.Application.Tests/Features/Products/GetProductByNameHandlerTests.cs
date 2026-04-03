using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class GetProductByNameHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetProductByNameQueryHandler _handler;
    private readonly GetProductByNameQueryValidator _validator;

    public GetProductByNameHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetProductByNameQueryHandler(
            _productRepositoryMock.Object,
            _mapperMock.Object);

        _validator = new GetProductByNameQueryValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenProductNameIsNull()
    {
        // Arrange
        var query = new GetProductByNameQuery(null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().ContainSingle(e => e.ErrorMessage == "Product name cannot be empty.");

        // Đảm bảo không gọi xuống DB
        _productRepositoryMock.Verify(repo => repo.GetProductByNameContainingAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductsListIsEmpty()
    {
        // Arrange
        var query = new GetProductByNameQuery("Trà vảy rồng");

        // Giả lập DB trả về List rỗng để cover nhánh products.Count == 0
        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameContainingAsync(query.ProductName))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"No product found with name '{query.ProductName}'");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductsAreFound()
    {
        // Arrange
        var query = new GetProductByNameQuery("Cà phê");

        var productList = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Cà phê sữa đá" },
            new() { Id = Guid.NewGuid(), Name = "Bạc xỉu" }
        };

        var dtoList = new List<GetProductByNameQueryResponse>
        {
            new() { ProductId = productList[0].Id, Name = "Cà phê sữa đá" },
            new() { ProductId = productList[1].Id, Name = "Bạc xỉu" }
        };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameContainingAsync(query.ProductName))
            .ReturnsAsync(productList);

        _mapperMock
            .Setup(m => m.Map<List<GetProductByNameQueryResponse>>(productList))
            .Returns(dtoList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.First().Name.Should().Be("Cà phê sữa đá");

        _productRepositoryMock.Verify(repo => repo.GetProductByNameContainingAsync(query.ProductName), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GetProductByNameQueryResponse>>(productList), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenProductNameIsNull()
    {
        // Arrange
        var query = new GetProductByNameQuery(null!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
              .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenProductNameIsValid()
    {
        // Arrange
        var query = new GetProductByNameQuery("Espresso");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductName);
    }

    #endregion Validator Tests
}