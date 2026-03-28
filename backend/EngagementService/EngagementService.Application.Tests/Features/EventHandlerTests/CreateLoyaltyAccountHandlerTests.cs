using EngagementService.Application.EventHandler;
using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using MediatR;
using Moq;
using SharedKernel.Events;

namespace EngagementService.Application.Tests.EventHandlerTests;

public class CreateLoyaltyAccountIntegrationHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateLoyaltyAccountHandler _handler;

    public CreateLoyaltyAccountIntegrationHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateLoyaltyAccountHandler(_mediatorMock.Object);
    }

    // Phủ xanh 100% logic: Nhận Event -> Map sang Command -> Send MediatR
    [Fact]
    public async Task Handle_ShouldSendCreateCommand_WhenUserRegisteredEventReceived()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var userRegisteredEvent = new UserRegisteredEvent { CustomerId = customerId };

        // Act
        await _handler.Handle(userRegisteredEvent);

        // Assert
        // Kiểm tra xem Mediator có nhận được CreateCommand với đúng Id không
        _mediatorMock.Verify(x => x.Send(
            It.Is<CreateCommand>(c =>
                c.Request.Id == customerId &&
                c.Request.OrderAmount == 0 &&
                c.Request.OrderId == Guid.Empty),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}