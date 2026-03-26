using FluentAssertions;
using InventoryService.Application.Features.Category.Commands.DeleteCategory;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Moq;

namespace InventoryService.Application.Tests.Features.Category;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<IIngredientCategoryRepository> _categoryRepositoryMock;
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IIngredientCategoryRepository>();
        _handler = new DeleteCategoryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryDeleteAsync_WithCorrectId()
    {
        // Arrange: Tạo một ID mẫu
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        // Act: Chạy Handler
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 1. Kiểm tra kết quả trả về là Unit.Value (tín hiệu thành công của MediatR)
        result.Should().Be(Unit.Value);

        // 2. Quan trọng nhất: Xác nhận Repository thực sự được gọi với đúng cái ID đó
        _categoryRepositoryMock.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteSuccessfully_WhenIdIsValid()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        // Act & Assert: Đảm bảo không có Exception nào bị ném ra
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().NotThrowAsync();
    }
}