using EngagementService.Application.EventHandler;
using EngagementService.Application.Features.Voucher.Commands.ReverseVoucher;
using MediatR;
using Moq;
using SharedKernel.Events;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.EventHandlerTests;

public class ReverseVoucherIntegrationHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ReverseVoucherEventHandler _handler;

    public ReverseVoucherIntegrationHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new ReverseVoucherEventHandler(_mediatorMock.Object);
    }

    // Phủ xanh 100%: Nhận Event -> Send ReverseVoucherCommand với đúng OrderId
    [Fact]
    public async Task Handle_ShouldSendReverseVoucherCommand_WhenEventReceived()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var @event = new ReverseVoucherEvent { OrderId = orderId };

        // Act
        await _handler.Handle(@event);

        // Assert
        // Kiểm tra xem Mediator có nhận được Command với đúng OrderId từ Event không
        _mediatorMock.Verify(x => x.Send(
            It.Is<ReverseVoucherCommand>(c => c.OrderId == orderId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}