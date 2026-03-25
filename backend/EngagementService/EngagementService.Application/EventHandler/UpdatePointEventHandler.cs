using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler;

public class UpdatePointEventHandler(IMediator mediator
) : IIntegrationEventHandler<UpdatePointEvent>
{
    public async Task Handle(UpdatePointEvent @event)
    {
        var req = new Dtos.LoyaltyAccount.UpdatePointRequest
        {
            Id = @event.Id,
            Point = @event.Point,
            OrderId = @event.OrderId,
            ActionType = @event.ActionType
        };
        await mediator.Send(new Features.LoyaltyAccount.Commands.DetuctPoint.UpdatePointCommand(req));
    }
}