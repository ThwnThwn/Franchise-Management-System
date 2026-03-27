using FluentAssertions;
using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Features.Category.Commands.CreateCategory;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using Moq;

namespace InventoryService.Application.Tests.Features.Category;

public class CreateCategoryHandlerTests
{
    private readonly Mock<IIngredientCategoryRepository> _categoryRepositoryMock;
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IIngredientCategoryRepository>();
        _handler = new CreateCategoryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync_WithCorrectMapping()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Topping",
            Description = "Các loại trân chu, thạch cho cà phê"
        };
        var command = new CreateCategoryCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 1. Kiểm tra kết quả trả về có phải là Unit.Value không
        result.Should().Be(Unit.Value);

        // 2. Kiểm tra xem Repository có được gọi đúng 1 lần với dữ liệu đã map đúng không
        _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.Is<IngredientCategory>(c =>
            c.Name == request.Name &&
            c.Description == request.Description
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnitValue_WhenExecutionIsSuccessful()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "Dụng cụ", Description = null };
        var command = new CreateCategoryCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }
}