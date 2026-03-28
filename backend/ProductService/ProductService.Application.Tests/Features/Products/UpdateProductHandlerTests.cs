using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Products.Command.UpdateProduct;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateProductCommandHandler _handler;
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new UpdateProductCommandHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _mapperMock.Object);

        _validator = new UpdateProductCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenProductNameAlreadyExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var requestData = new UpdateProductCommandRequest(productId, "Existing Name", "Desc", true, Guid.NewGuid(), null);
        var command = new UpdateProductCommand(productId, requestData);

        // Mô phỏng DB trả về 1 Product (tức là đã tồn tại tên này ở ID khác)
        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(requestData.Name ?? "", productId))
            .ReturnsAsync(new Product { Id = Guid.NewGuid(), Name = requestData.Name! });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain($"Tên sản phẩm '{requestData.Name}' đã tồn tại trong hệ thống.");

        _categoryRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.UpdateWithVariantsAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var requestData = new UpdateProductCommandRequest(productId, "Valid Name", "Desc", true, categoryId, null);
        var command = new UpdateProductCommand(productId, requestData);

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(It.IsAny<string>(), productId))
            .ReturnsAsync((Product?)null);

        // Mô phỏng Category không tồn tại
        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Không tìm thấy danh mục với mã: '{categoryId}'.");

        _productRepositoryMock.Verify(repo => repo.UpdateWithVariantsAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdatingWithVariants()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var variants = new List<UpdateProductVariantRequest>
        {
            new("SKU1", "Var1", "Attr1", 100, 50, true),
            new(null, null, null, null, null, null) // Test fallback null values -> ?? "" and ?? 0
        };
        var requestData = new UpdateProductCommandRequest(productId, "Valid Name", "Desc", true, categoryId, variants);
        var command = new UpdateProductCommand(productId, requestData);

        var expectedResponse = new UpdateProductCommandResponse { ProductId = productId, Name = "Valid Name" };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(It.IsAny<string>(), productId))
            .ReturnsAsync((Product?)null);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId });

        _mapperMock
            .Setup(m => m.Map<UpdateProductCommandResponse>(It.IsAny<Product>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);

        // Verify that the mapped Product entity has variants
        _productRepositoryMock.Verify(repo => repo.UpdateWithVariantsAsync(It.Is<Product>(p =>
            p.Id == productId &&
            p.ProductVariants.Count == 2 &&
            p.ProductVariants.Any(v => v.Sku == "SKU1") &&
            p.ProductVariants.Any(v => v.Sku == "")
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenVariantsAndNameAreNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        // Test fallback request.Request.Name ?? "" và Variants ?? []
        var requestData = new UpdateProductCommandRequest(productId, null, "Desc", true, categoryId, null);
        var command = new UpdateProductCommand(productId, requestData);

        var expectedResponse = new UpdateProductCommandResponse { ProductId = productId, Name = "" };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync("", productId))
            .ReturnsAsync((Product?)null);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId });

        _mapperMock
            .Setup(m => m.Map<UpdateProductCommandResponse>(It.IsAny<Product>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _productRepositoryMock.Verify(repo => repo.UpdateWithVariantsAsync(It.Is<Product>(p =>
            p.Name == "" &&
            p.ProductVariants.Count == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var requestData = new UpdateProductCommandRequest(productId, "Valid Name", "Desc", true, categoryId, null);
        var command = new UpdateProductCommand(productId, requestData);

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(It.IsAny<string>(), productId))
            .ReturnsAsync((Product?)null);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId });

        var exceptionMessage = "Database lock error";
        _productRepositoryMock
            .Setup(repo => repo.UpdateWithVariantsAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain($"Lỗi khi cập nhật sản phẩm: {exceptionMessage}");
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenProductIdIsEmpty()
    {
        var requestData = new UpdateProductCommandRequest(Guid.Empty, "Name", null, true, Guid.NewGuid(), null);
        var command = new UpdateProductCommand(Guid.Empty, requestData);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.ProductId)
              .WithErrorMessage("Product ID must be provided.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenCategoryIdIsEmpty()
    {
        var requestData = new UpdateProductCommandRequest(Guid.NewGuid(), "Name", null, true, Guid.Empty, null);
        var command = new UpdateProductCommand(Guid.NewGuid(), requestData);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.CategoryId)
              .WithErrorMessage("Category ID must be provided.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var longName = new string('a', 201);
        var requestData = new UpdateProductCommandRequest(Guid.NewGuid(), longName, null, true, Guid.NewGuid(), null);
        var command = new UpdateProductCommand(Guid.NewGuid(), requestData);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var longDesc = new string('a', 201);
        var requestData = new UpdateProductCommandRequest(Guid.NewGuid(), "Name", longDesc, true, Guid.NewGuid(), null);
        var command = new UpdateProductCommand(Guid.NewGuid(), requestData);

        var result = _validator.TestValidate(command);

        // Lưu ý: validator rule cài Max(200) nhưng msg đang ghi là 300
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
              .WithErrorMessage("Product description must not exceed 300 characters.");
    }

    [Fact]
    public void Validator_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var requestData = new UpdateProductCommandRequest(Guid.NewGuid(), "Valid Name", "Valid Desc", true, Guid.NewGuid(), null);
        var command = new UpdateProductCommand(Guid.NewGuid(), requestData);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion Validator Tests
}