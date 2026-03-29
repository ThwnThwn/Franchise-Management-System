using Ardalis.Result;
using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using ProductService.Application.Features.Categories.Command.UpdateCategory;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Tests.Features.Categories;

public class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateCategoryCommandHandler _handler;
    private readonly UpdateCategoryCommandValidator _validator;

    public UpdateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new UpdateCategoryCommandHandler(_categoryRepositoryMock.Object, _mapperMock.Object);
        _validator = new UpdateCategoryCommandValidator();
    }

    #region Handler Tests

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryCommandRequest { Id = categoryId, Name = "New Name" };
        var command = new UpdateCategoryCommand(categoryId, request);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.Id))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($" Category with Id '{request.Id}' not found. ");

        _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenNameIsAlreadyInUseByAnotherCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryCommandRequest { Id = categoryId, Name = "Existing Name" };
        var command = new UpdateCategoryCommand(categoryId, request);

        var categoryToUpdate = new Category { Id = categoryId, Name = "Old Name" };
        var existingCategoryWithSameName = new Category { Id = Guid.NewGuid(), Name = "Existing Name" }; // Different ID

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.Id))
            .ReturnsAsync(categoryToUpdate);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByCategoryNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategoryWithSameName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().Contain($" Category name '{request.Name}' is already in use. ");

        _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdatingWithValidData()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryCommandRequest { Id = categoryId, Name = "Valid Name", Description = "New Desc" };
        var command = new UpdateCategoryCommand(categoryId, request);

        var categoryToUpdate = new Category { Id = categoryId, Name = "Old Name" };
        var expectedResponse = new UpdateCategoryCommandResponse { Id = categoryId, Name = "Valid Name" };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.Id))
            .ReturnsAsync(categoryToUpdate);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByCategoryNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null); // Name is not in use

        _mapperMock
            .Setup(m => m.Map(request, categoryToUpdate))
            .Returns((UpdateCategoryCommandRequest req, Category dest) =>
            {
                dest.Name = req.Name;
                dest.Description = req.Description;
                return dest;
            });

        _mapperMock
            .Setup(m => m.Map<UpdateCategoryCommandResponse>(It.IsAny<Category>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);

        _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(categoryToUpdate), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Handle_ShouldRetainOldName_WhenRequestNameIsNullOrEmpty(string? emptyName)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        // Giả lập case truyền tên trống để trigger nhánh if (string.IsNullOrEmpty(request.Request.Name))
        var request = new UpdateCategoryCommandRequest { Id = categoryId, Name = emptyName! };
        var command = new UpdateCategoryCommand(categoryId, request);

        var oldName = "Original Name";
        var categoryToUpdate = new Category { Id = categoryId, Name = oldName };
        var expectedResponse = new UpdateCategoryCommandResponse { Id = categoryId, Name = oldName };

        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(request.Id))
            .ReturnsAsync(categoryToUpdate);

        _categoryRepositoryMock
            .Setup(repo => repo.GetByCategoryNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Mô phỏng hành vi của AutoMapper
        _mapperMock
            .Setup(m => m.Map(request, categoryToUpdate))
            .Returns((UpdateCategoryCommandRequest req, Category dest) =>
            {
                // Lưu ý: Code gốc của bạn map thẳng vào categoryToUpdate,
                // sau đó gán lại Name nếu string.IsNullOrEmpty
                return dest;
            });

        _mapperMock
            .Setup(m => m.Map<UpdateCategoryCommandResponse>(categoryToUpdate))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);

        // Xác nhận hàm UpdateAsync được gọi với object đã qua xử lý
        _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(categoryToUpdate), Times.Once);
    }

    #endregion Handler Tests

    #region Validator Tests

    [Fact]
    public void Validator_ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var request = new UpdateCategoryCommandRequest { Id = Guid.Empty, Name = "Name" };
        var command = new UpdateCategoryCommand(Guid.Empty, request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Category ID is required");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenIdIsValid()
    {
        // Arrange
        var request = new UpdateCategoryCommandRequest { Id = Guid.NewGuid(), Name = "Name" };
        var command = new UpdateCategoryCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 256);
        var request = new UpdateCategoryCommandRequest { Id = Guid.NewGuid(), Name = longName };
        var command = new UpdateCategoryCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
              .WithErrorMessage("Category name must not exceed 255 characters");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var longDescription = new string('a', 1001);
        var request = new UpdateCategoryCommandRequest { Id = Guid.NewGuid(), Name = "Valid Name", Description = longDescription };
        var command = new UpdateCategoryCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
              .WithErrorMessage("Category description must not exceed 1000 characters");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenDescriptionIsNull()
    {
        // Arrange
        var request = new UpdateCategoryCommandRequest { Id = Guid.NewGuid(), Name = "Valid Name", Description = null };
        var command = new UpdateCategoryCommand(Guid.NewGuid(), request);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    #endregion Validator Tests
}