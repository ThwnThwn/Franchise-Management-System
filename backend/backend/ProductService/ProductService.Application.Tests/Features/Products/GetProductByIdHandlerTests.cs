using Ardalis.Result;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Dtos;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByIdHandler _handler;
    private readonly GetProductByIdValidator _validator;

    public GetProductByIdHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdHandler(_productRepositoryMock.Object);
        _validator = new GetProductByIdValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Product with id {productId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductIsFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        // Khởi tạo Entity thật để extension method MapToProductDto() có thể chạy trơn tru
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Sample Product",
            Category = new Category { Name = "Test Category" }, // Bắt buộc phải có để mapper không bị NullReference
            ProductVariants = []
        };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<ProductDto>();
        result.Value.Id.Should().Be(productId);

        _productRepositoryMock.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenProductIdIsEmpty()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        // Test kiểm tra xem cả 2 rule (NotEmpty và Must) có bắn ra đúng lỗi không
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
              .WithErrorMessage("Product Id must be provided.");

        result.ShouldHaveValidationErrorFor(x => x.ProductId)
              .WithErrorMessage("Product Id must be a valid non-empty GUID.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenProductIdIsValid()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    #endregion Validator Tests
}