using Ardalis.Result;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.ProductVariants.Queries.GetById;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.ProductVariants.Queries.GetById.GetProductVariantByIdQuery;

namespace ProductService.Application.Tests.Features.ProductVariants;

public class GetProductVariantByIdHandlerTests
{
    private readonly Mock<IProductVariantRepository> _repositoryMock;
    private readonly GetProductVariantByIdQueryHandler _handler;
    private readonly GetProductVariantByIdQueryValidator _validator;

    public GetProductVariantByIdHandlerTests()
    {
        _repositoryMock = new Mock<IProductVariantRepository>();
        _handler = new GetProductVariantByIdQueryHandler(_repositoryMock.Object);
        _validator = new GetProductVariantByIdQueryValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenVariantDoesNotExist()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var query = new GetProductVariantByIdQuery(variantId);

        _repositoryMock
            .Setup(repo => repo.GetProductVariantByIdAsync(variantId))
            .ReturnsAsync((ProductVariant?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Product Variant with id '{variantId}' not found");

        _repositoryMock.Verify(repo => repo.GetProductVariantByIdAsync(variantId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenVariantExists()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var query = new GetProductVariantByIdQuery(variantId);

        // Domain quán nước: Tạo dữ liệu cho 1 Option của đồ uống (VD: Cà phê đen Size L)
        var existingVariant = new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Sku = "CFD-SIZE-L",
            Name = "Size L",
            Attributes = "Size: L, Ice: 50%, Sugar: 50%",
            BasePrice = 35000,
            Cost = 15000,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetProductVariantByIdAsync(variantId))
            .ReturnsAsync(existingVariant);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<GetProductVariantByIdQueryResponse>();

        // Kiểm tra xem đoạn code gán tay (manual mapping) của bro có sót field nào không
        result.Value.Id.Should().Be(existingVariant.Id);
        result.Value.Sku.Should().Be(existingVariant.Sku);
        result.Value.Name.Should().Be(existingVariant.Name);
        result.Value.Attributes.Should().Be(existingVariant.Attributes);
        result.Value.BasePrice.Should().Be(existingVariant.BasePrice);
        result.Value.Cost.Should().Be(existingVariant.Cost);
        result.Value.IsActive.Should().Be(existingVariant.IsActive);
        result.Value.ProductId.Should().Be(existingVariant.ProductId);
        result.Value.CreatedAt.Should().Be(existingVariant.CreatedAt);
        result.Value.UpdatedAt.Should().Be(existingVariant.UpdatedAt);

        _repositoryMock.Verify(repo => repo.GetProductVariantByIdAsync(variantId), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenVariantIdIsEmpty()
    {
        // Arrange
        var query = new GetProductVariantByIdQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductVariantId)
              .WithErrorMessage("Product Variant Id is required");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenVariantIdIsValid()
    {
        // Arrange
        var query = new GetProductVariantByIdQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductVariantId);
    }

    #endregion Validator Tests
}