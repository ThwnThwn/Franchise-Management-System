using FluentAssertions;
using InventoryService.Application.EventHandlers;
using InventoryService.Application.Persistences.Repositories;
using Moq;
using SharedKernel.Events;
using Xunit;
using System;
using System.Threading.Tasks;

// Tuân thủ đúng Namespace Duy yêu cầu
namespace InventoryService.Application.Tests.Features.EventTests;

public class OrderCancelledHandlerTests
{
    private readonly Mock<IStoreIngredientRepository> _storeIngredientRepoMock;
    private readonly OrderCancelledHandler _handler;

    public OrderCancelledHandlerTests()
    {
        _storeIngredientRepoMock = new Mock<IStoreIngredientRepository>();
        _handler = new OrderCancelledHandler(_storeIngredientRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallRemoveQueueByOrderIdAsync_WithCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var @event = new OrderCancelledEvent
        {
            OrderId = orderId,
            CancelledAt = DateTimeOffset.Now,
            Reason = "User cancelled"
        };

        // Act
        await _handler.Handle(@event);

        // Assert
        // Xác nhận Repository được gọi đúng 1 lần với đúng OrderId từ Event truyền xuống
        _storeIngredientRepoMock.Verify(x => x.RemoveQueueByOrderIdAsync(orderId), Times.Once);
    }
}