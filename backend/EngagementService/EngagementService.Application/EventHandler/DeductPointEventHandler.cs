using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler
{
    public class DeductPointEventHandler : IIntegrationEventHandler<DeductPointEvent>
    {
        private readonly IMediator _mediator;
        public DeductPointEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(DeductPointEvent @event)
        {
            var req = new Dtos.LoyaltyAccount.DeductPointRequest
            {
                Id = @event.Id,
                PointToDeduct = @event.PointToDeduct,
                OrderId = @event.OrderId
            };
            await _mediator.Send(new Features.LoyaltyAccount.Commands.DetuctPoint.DeductPointCommand(req));
        }
    }
}