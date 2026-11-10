using Ardalis.Result;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Products.Command.DeleteProduct;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly DeleteProductCommandHandler _handler;
    private readonly DeleteProductCommandValidator _validator;

    public DeleteProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteProductCommandHandler(_productRepositoryMock.Object);
        _validator = new DeleteProductCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Không tìm thấy sản phẩm với mã: '{productId}'.");

        // Đảm bảo hàm DeleteAsync KHÔNG bao giờ được gọi
        _productRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductIsDeletedSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);
        var existingProduct = new Product { Id = productId, Name = "Existing Product" };

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(repo => repo.DeleteAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Đảm bảo hàm DeleteAsync được gọi đúng 1 lần với đúng ID
        _productRepositoryMock.Verify(repo => repo.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenExceptionIsThrownDuringDeletion()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);
        var existingProduct = new Product { Id = productId, Name = "Existing Product" };
        var exceptionMessage = "Database connection lost";

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Giả lập văng Exception khi gọi DeleteAsync
        _productRepositoryMock
            .Setup(repo => repo.DeleteAsync(productId))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain($"Đã xảy ra lỗi khi xóa sản phẩm: {exceptionMessage}");

        _productRepositoryMock.Verify(repo => repo.DeleteAsync(productId), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenProductIdIsEmpty()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
              .WithErrorMessage("Product ID must be provided.");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenProductIdIsValid()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    #endregion Validator Tests
}