using EngagementService.Application.EventHandler;
using EngagementService.Application.Features.LoyaltyAccount.Commands.UpdatePoint;
using MediatR;
using Moq;
using SharedKernel.Events;

namespace EngagementService.Application.Tests.Features.EventHandlerTests;

public class UpdatePointIntegrationHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UpdatePointEventHandler _handler;

    public UpdatePointIntegrationHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new UpdatePointEventHandler(_mediatorMock.Object);
    }

    // Phủ xanh 100%: Nhận Event -> Map sang UpdatePointRequest -> Send Command
    [Fact]
    public async Task Handle_ShouldSendUpdatePointCommand_WhenEventReceived()
    {
        // Arrange
        var @event = new UpdatePointEvent
        {
            Id = Guid.NewGuid(),
            Point = 100,
            OrderId = Guid.NewGuid(),
            ActionType = UpdatePointType.Minus
        };

        // Act
        await _handler.Handle(@event);

        // Assert
        // Kiểm tra xem Mediator có nhận được Command với đúng các thông số từ Event không
        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdatePointCommand>(c =>
                c.Request.Id == @event.Id &&
                c.Request.Point == @event.Point &&
                c.Request.OrderId == @event.OrderId &&
                c.Request.ActionType == @event.ActionType),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}