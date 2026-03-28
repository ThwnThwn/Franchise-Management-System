using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using ProductService.Application.Dtos;
using ProductService.Application.Features.Products.Command.CreateProduct;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;

namespace ProductService.Application.Tests.Features.Products;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly CreateProductCommandHandler _handler;
    private readonly CreateProductCommandValidator _validator;

    public CreateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();
        _cloudinaryServiceMock = new Mock<ICloudinaryService>();

        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _mapperMock.Object,
            _cloudinaryServiceMock.Object);

        _validator = new CreateProductCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenProductNameAlreadyExists()
    {
        // Arrange
        var request = new CreateProductCommandRequest { Name = "Existing Product" };
        var command = new CreateProductCommand(request);

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(request.Name, null))
            .ReturnsAsync(new Product { Id = Guid.NewGuid(), Name = request.Name });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain($"Đã tồn tại tên sản phẩm: '{request.Name}'.");

        // Đảm bảo không gọi save hay upload ảnh
        _cloudinaryServiceMock.Verify(c => c.UploadImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndSkipUpload_WhenImageFileIsNull()
    {
        // Arrange
        var request = new CreateProductCommandRequest { Name = "New Product", ImageFile = null };
        var command = new CreateProductCommand(request);
        var productEntity = new Product { Name = request.Name };
        var expectedResponse = new CreateProductCommandResponse { Name = request.Name };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(request.Name, null))
            .ReturnsAsync((Product?)null);

        _mapperMock.Setup(m => m.Map<Product>(request)).Returns(productEntity);
        _mapperMock.Setup(m => m.Map<CreateProductCommandResponse>(productEntity)).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Đảm bảo AddAsync được gọi nhưng UploadImageAsync thì KHÔNG
        _cloudinaryServiceMock.Verify(c => c.UploadImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.AddAsync(productEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndSetImageUrl_WhenImageUploadIsSuccessful()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>(); // Mock file ảnh
        var request = new CreateProductCommandRequest { Name = "New Product", ImageFile = fileMock.Object };
        var command = new CreateProductCommand(request);
        var productEntity = new Product { Name = request.Name };

        var uploadResult = new CloudinaryUploadResult { Url = "http://image.url", PublicId = "pub-id-123" };
        var expectedResponse = new CreateProductCommandResponse { Name = request.Name, ImageUrl = uploadResult.Url };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(request.Name, null))
            .ReturnsAsync((Product?)null);

        _mapperMock.Setup(m => m.Map<Product>(request)).Returns(productEntity);

        // Setup mock trả về file upload thành công
        _cloudinaryServiceMock
            .Setup(c => c.UploadImageAsync(request.ImageFile))
            .ReturnsAsync(uploadResult);

        _mapperMock.Setup(m => m.Map<CreateProductCommandResponse>(productEntity)).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        productEntity.ImageUrl.Should().Be(uploadResult.Url);
        productEntity.ImagePublicId.Should().Be(uploadResult.PublicId);

        _cloudinaryServiceMock.Verify(c => c.UploadImageAsync(request.ImageFile), Times.Once);
        _productRepositoryMock.Verify(repo => repo.AddAsync(productEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_ButNotSetImageUrl_WhenCloudinaryReturnsNull()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var request = new CreateProductCommandRequest { Name = "New Product", ImageFile = fileMock.Object };
        var command = new CreateProductCommand(request);
        var productEntity = new Product { Name = request.Name };
        var expectedResponse = new CreateProductCommandResponse { Name = request.Name };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByNameAsync(request.Name, null))
            .ReturnsAsync((Product?)null);

        _mapperMock.Setup(m => m.Map<Product>(request)).Returns(productEntity);

        // Mô phỏng trường hợp Cloudinary bị lỗi/trả về null
        _cloudinaryServiceMock
            .Setup(c => c.UploadImageAsync(request.ImageFile))
            .ReturnsAsync((CloudinaryUploadResult?)null);

        _mapperMock.Setup(m => m.Map<CreateProductCommandResponse>(productEntity)).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        productEntity.ImageUrl.Should().BeNull(); // URL không được set
        productEntity.ImagePublicId.Should().BeNull();

        _cloudinaryServiceMock.Verify(c => c.UploadImageAsync(request.ImageFile), Times.Once);
        _productRepositoryMock.Verify(repo => repo.AddAsync(productEntity), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_ShouldHaveError_WhenNameIsEmpty(string invalidName)
    {
        // Arrange
        var command = new CreateProductCommand(new CreateProductCommandRequest { Name = invalidName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Product name must be provided.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 201);
        var command = new CreateProductCommand(new CreateProductCommandRequest { Name = longName });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var longDescription = new string('a', 201);
        var command = new CreateProductCommand(new CreateProductCommandRequest
        {
            Name = "Valid Name",
            Description = longDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
              .WithErrorMessage("Product description must not exceed 300 characters.");
    }

    [Fact]
    public void Validator_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateProductCommand(new CreateProductCommandRequest
        {
            Name = "Valid Product Name",
            Description = "A valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion Validator Tests
}