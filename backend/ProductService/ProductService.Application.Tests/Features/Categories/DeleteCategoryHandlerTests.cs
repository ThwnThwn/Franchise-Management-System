using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Categories.Command.DeleteCategory;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Categories.Command.DeleteCategory.DeleteCategoryCommand;

namespace ProductService.Application.Tests.Features.Categories;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DeleteCategoryCommandHandler _handler;
    private readonly DeleteCategoryCommandValidator _validator;

    public DeleteCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
        _validator = new DeleteCategoryCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Category with ID '{categoryId}' not found.");

        // Ensure subsequent methods are NEVER called
        _categoryRepositoryMock.Verify(repo => repo.HasProductsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenCategoryHasAssociatedProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);
        var existingCategory = new Category { Id = categoryId, Name = "Electronics" };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(repo => repo.HasProductsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Mô phỏng category đang chứa product

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain("Cannot delete category with associated products.");

        // Ensure DeleteAsync is NEVER called
        _categoryRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCategoryCanBeDeleted()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);
        var existingCategory = new Category { Id = categoryId, Name = "Electronics" };
        var expectedResponse = new DeleteCategoryCommandResponse
        {
            Id = categoryId,
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow
        };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(repo => repo.HasProductsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Mô phỏng category rỗng, an toàn để xóa

        _mapperMock
            .Setup(m => m.Map<DeleteCategoryCommandResponse>(existingCategory))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(categoryId);
        result.Value.IsDeleted.Should().BeTrue();

        // Verify that DeleteAsync was called exactly ONCE with the correct ID
        _categoryRepositoryMock.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenCategoryIdIsEmpty()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
              .WithErrorMessage("Category ID is required");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenCategoryIdIsValid()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    #endregion Validator Tests
}