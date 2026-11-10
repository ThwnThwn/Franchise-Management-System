using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler;

public class CreateLoyaltyAccountHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
    private readonly IMediator _mediator;

    public CreateLoyaltyAccountHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserRegisteredEvent @event)
    {
        var command = new CreateCommand(
        new Dtos.LoyaltyAccount.UpSertReqest()
        {
            Id = @event.CustomerId,
            OrderAmount = 0,
            OrderId = Guid.Empty
        });

        await _mediator.Send(command);
    }
}