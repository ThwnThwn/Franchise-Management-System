using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using EngagementService.Application.Features.LoyaltyAccount.Commands.ProcessOrderReward;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using MediatR;
using Moq;
using SharedKernel.Events;

namespace EngagementService.Application.Tests.EventHandlerTests;

public class ProcessOrderRewardIntegrationHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Application.EventHandler.ProcessOrderRewardHandler _handler;

    public ProcessOrderRewardIntegrationHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new Application.EventHandler.ProcessOrderRewardHandler(_mediatorMock.Object);
    }

    // 1. Phủ nhánh: Tài khoản chưa tồn tại -> Phải Create rồi mới Process (Dòng 28-34)
    [Fact]
    public async Task Handle_ShouldSendCreateAndProcessCommand_WhenAccountDoesNotExist()
    {
        // Arrange
        var @event = new ProcessOrderRewardEvent
        {
            Id = Guid.NewGuid(),
            OrderAmount = 500,
            OrderId = Guid.NewGuid()
        };

        // Giả lập Mediator trả về null khi check tài khoản
        _mediatorMock.Setup(x => x.Send(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.LoyaltyAccount)null!);

        // Act
        await _handler.Handle(@event);

        // Assert
        // Kiểm tra gọi CreateCommand
        _mediatorMock.Verify(x => x.Send(
            It.Is<CreateCommand>(c => c.Request.Id == @event.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        // Kiểm tra gọi ProcessOrderRewardCommand
        _mediatorMock.Verify(x => x.Send(
            It.Is<ProcessOrderRewardCommand>(c => c.Request.Id == @event.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // 2. Phủ nhánh: Tài khoản đã tồn tại -> Chỉ gọi Process (Dòng 35-37)
    [Fact]
    public async Task Handle_ShouldOnlySendProcessCommand_WhenAccountExists()
    {
        // Arrange
        var @event = new ProcessOrderRewardEvent { Id = Guid.NewGuid(), OrderAmount = 200 };

        // Giả lập đã có tài khoản
        _mediatorMock.Setup(x => x.Send(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.LoyaltyAccount());

        // Act
        await _handler.Handle(@event);

        // Assert
        // KHÔNG được gọi CreateCommand
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCommand>(), It.IsAny<CancellationToken>()), Times.Never);

        // PHẢI gọi ProcessOrderRewardCommand
        _mediatorMock.Verify(x => x.Send(
            It.Is<ProcessOrderRewardCommand>(c => c.Request.Id == @event.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}