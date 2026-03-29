using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.EventHandler;
using EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus;
using MediatR;
using Moq;
using SharedKernel.Events;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EngagementService.Application.Tests.EventHandlerTests;

public class UseVoucherIntegrationHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UseVoucherEventHandler _handler;

    public UseVoucherIntegrationHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new UseVoucherEventHandler(_mediatorMock.Object);
    }

    // Phủ xanh 100%: Nhận Event -> Map sang Request -> Send UseVoucherCommand
    [Fact]
    public async Task Handle_ShouldSendUseVoucherCommand_WhenEventReceived()
    {
        // Arrange
        var voucherIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var @event = new UseVouchersEvent
        {
            VoucherIds = voucherIds,
            OrderId = orderId,
            UserId = userId
        };

        // Act
        await _handler.Handle(@event);

        // Assert
        // Kiểm tra mapping dữ liệu từ Event sang Command
        _mediatorMock.Verify(x => x.Send(
            It.Is<UseVoucherCommand>(c =>
                c.Request.VoucherIds == voucherIds &&
                c.Request.OrderId == orderId &&
                c.Request.UserId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}