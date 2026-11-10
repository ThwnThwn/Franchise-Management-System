using FluentAssertions;
using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Features.Category.Commands.UpdateCategory;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using Moq;

namespace InventoryService.Application.Tests.Features.Category;

public class UpdateCategoryHandlerTests
{
    private readonly Mock<IIngredientCategoryRepository> _categoryRepositoryMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IIngredientCategoryRepository>();
        _handler = new UpdateCategoryHandler(_categoryRepositoryMock.Object);
    }

    // --- TRƯỜNG HỢP 1: CẬP NHẬT THÀNH CÔNG (FULL) ---
    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new IngredientCategory { Id = categoryId, Name = "Old Name", Description = "Old Desc" };
        var request = new UpdateCategoryRequest { Id = categoryId, Name = "New Name", Description = "New Desc" };
        var command = new UpdateCategoryCommand(request);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        existingCategory.Name.Should().Be("New Name");
        existingCategory.Description.Should().Be("New Desc");
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory), Times.Once);
    }

    // --- TRƯỜNG HỢP 2: LỖI KHÔNG TÌM THẤY ---
    [Fact]
    public async Task Handle_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var request = new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "Test" };
        var command = new UpdateCategoryCommand(request);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((IngredientCategory?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DirectoryNotFoundException>();
        // Lưu ý: Duy có thể check Message.Category.NotFound nếu muốn kỹ hơn
    }

    // --- TRƯỜNG HỢP 3: CẬP NHẬT MỘT PHẦN (CHỈ UPDATE NAME) ---
    [Fact]
    public async Task Handle_ShouldOnlyUpdateName_WhenDescriptionIsEmpty()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new IngredientCategory { Id = categoryId, Name = "Old Name", Description = "Keep Me" };
        var request = new UpdateCategoryRequest { Id = categoryId, Name = "Updated Name", Description = "" };
        var command = new UpdateCategoryCommand(request);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(existingCategory);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCategory.Name.Should().Be("Updated Name");
        existingCategory.Description.Should().Be("Keep Me"); // Không bị ghi đè thành rỗng
    }
}