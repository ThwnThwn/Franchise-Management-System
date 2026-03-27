using FluentAssertions;
using InventoryService.Application.EventHandlers;
using InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation;
using MediatR;
using Moq;
using SharedKernel.Events;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Tuân thủ đúng Namespace Duy yêu cầu
namespace InventoryService.Application.Tests.Features.EventTests;

public class OrderPaidHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrderPaidHandler _handler;

    public OrderPaidHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new OrderPaidHandler(_mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSendConfirmStockReservationCommand_WithCorrectData()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var items = new List<OrderPaidItem>
        {
            new() { VariantId = Guid.NewGuid(), Quantity = 2 },
            new() { VariantId = Guid.NewGuid(), Quantity = 5 }
        };

        var @event = new OrderPaidEvent
        {
            OrderId = orderId,
            StoreId = storeId,
            PaidAt = DateTimeOffset.Now,
            Items = items
        };

        // Act
        await _handler.Handle(@event);

        // Assert
        // Xác nhận Mediator.Send được gọi đúng 1 lần với Command chứa dữ liệu khớp với Event
        _mediatorMock.Verify(x => x.Send(
            It.Is<ConfirmStockReservationCommand>(cmd =>
                cmd.OrderId == orderId &&
                cmd.StoreId == storeId &&
                cmd.Items == items),
            It.IsAny<CancellationToken>()),
        Times.Once);
    }
}