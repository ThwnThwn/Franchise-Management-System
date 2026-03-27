using FluentAssertions;
using InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient;
using InventoryService.Application.Persistences.Repositories;
using Moq;
using Xunit;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Application.Tests.Features.IngredientTests;

public class DeleteIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _ingredientRepoMock;
    private readonly DeleteIngredientHandler _handler;

    public DeleteIngredientHandlerTests()
    {
        _ingredientRepoMock = new Mock<IIngredientRepository>();
        _handler = new DeleteIngredientHandler(_ingredientRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsync_WithCorrectId()
    {
        // Arrange
        var ingredientId = Guid.NewGuid();
        var command = new DeleteIngredientCommand(ingredientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        // Quan trọng nhất: Xác nhận repo đã gọi hàm Delete đúng với ID gửi lên
        _ingredientRepoMock.Verify(x => x.DeleteAsync(ingredientId), Times.Once);
    }
}