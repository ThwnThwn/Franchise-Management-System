using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler
{
    public class DetuctPointEventHandler : IIntegrationEventHandler<DetuctPointEvent>
    {
        private readonly IMediator _mediator;
        public DetuctPointEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(DetuctPointEvent @event)
        {
            var req = new Dtos.LoyaltyAccount.DetuctPointRequest
            {
                Id = @event.Id,
                PointToDetuct = @event.PointToDetuct
            };
            await _mediator.Send(new Features.LoyaltyAccount.Commands.DetuctPoint.DetuctPointCommand(req));
        }
    }
}