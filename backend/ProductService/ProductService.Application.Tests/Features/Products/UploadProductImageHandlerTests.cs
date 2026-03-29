using Ardalis.Result;
using FluentAssertions;
using Moq;
using ProductService.Application.Dtos;
using ProductService.Application.Features.Products.Command.UploadProductImage;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Products;

public class UploadProductImageHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly UploadProductImageHandler _handler;

    public UploadProductImageHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _cloudinaryServiceMock = new Mock<ICloudinaryService>();

        _handler = new UploadProductImageHandler(
            _productRepositoryMock.Object,
            _cloudinaryServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new UploadProductImageCommand(Guid.NewGuid(), [0x01, 0x02], "image.png", "image/png");

        // Mô phỏng repository trả về null
        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(command.ProductId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain("Không tìm thấy sản phẩm để cập nhật ảnh.");

        // Đảm bảo không có thao tác upload hay update nào được gọi
        _cloudinaryServiceMock.Verify(c => c.UploadImageFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndSkipDelete_WhenOldPublicIdIsEmpty()
    {
        // Arrange
        var command = new UploadProductImageCommand(Guid.NewGuid(), [0x01, 0x02], "image.png", "image/png");

        // Mô phỏng sản phẩm KHÔNG có ảnh cũ (ImagePublicId = null hoặc "")
        var existingProduct = new Product { Id = command.ProductId, ImagePublicId = "" };
        var uploadResult = new CloudinaryUploadResult { Url = "http://new.url", PublicId = "new-pub-id" };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(command.ProductId))
            .ReturnsAsync(existingProduct);

        _cloudinaryServiceMock
            .Setup(c => c.UploadImageFromBytesAsync(command.ImageData, command.FileName, command.ContentType))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ProductId.Should().Be(command.ProductId);
        result.Value.ImageUrl.Should().Be(uploadResult.Url);
        result.Value.ImagePublicId.Should().Be(uploadResult.PublicId);

        // Đảm bảo Product được cập nhật thông tin mới
        existingProduct.ImageUrl.Should().Be(uploadResult.Url);
        existingProduct.ImagePublicId.Should().Be(uploadResult.PublicId);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(existingProduct), Times.Once);

        // CỰC KỲ QUAN TRỌNG: Đảm bảo KHÔNG gọi hàm DeleteImageAsync vì ảnh cũ rỗng
        _cloudinaryServiceMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndDeleteOldImage_WhenOldPublicIdExists()
    {
        // Arrange
        var command = new UploadProductImageCommand(Guid.NewGuid(), [0x01, 0x02], "image.png", "image/png");
        var oldPublicId = "old-pub-id-123";

        // Mô phỏng sản phẩm ĐÃ CÓ ảnh cũ
        var existingProduct = new Product { Id = command.ProductId, ImagePublicId = oldPublicId, ImageUrl = "http://old.url" };
        var uploadResult = new CloudinaryUploadResult { Url = "http://new.url", PublicId = "new-pub-id" };

        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(command.ProductId))
            .ReturnsAsync(existingProduct);

        _cloudinaryServiceMock
            .Setup(c => c.UploadImageFromBytesAsync(command.ImageData, command.FileName, command.ContentType))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImageUrl.Should().Be(uploadResult.Url);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(existingProduct), Times.Once);

        // CỰC KỲ QUAN TRỌNG: Đảm bảo ĐÃ gọi hàm DeleteImageAsync với ĐÚNG ID ảnh cũ
        _cloudinaryServiceMock.Verify(c => c.DeleteImageAsync(oldPublicId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var command = new UploadProductImageCommand(Guid.NewGuid(), [0x01, 0x02], "image.png", "image/png");
        var existingProduct = new Product { Id = command.ProductId };
        var exceptionMessage = "Cloudinary API timeout";

        _productRepositoryMock
            .Setup(repo => repo.GetProductByIdAsync(command.ProductId))
            .ReturnsAsync(existingProduct);

        // Mô phỏng văng lỗi khi đang upload ảnh
        _cloudinaryServiceMock
            .Setup(c => c.UploadImageFromBytesAsync(command.ImageData, command.FileName, command.ContentType))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain($"Lỗi khi xử lý hình ảnh: {exceptionMessage}");

        // Đảm bảo không gọi Update vào DB nếu Upload lỗi
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}