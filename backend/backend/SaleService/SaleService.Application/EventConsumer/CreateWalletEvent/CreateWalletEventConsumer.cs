using MediatR;
using SalesService.Application.Features.Wallets.Command.Create;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace SalesService.Application.EventHandler.CreateWalletEvent
{
    public class CreateWalletEventConsumer : IIntegrationEventHandler<UserRegisteredEvent>
    {
        private readonly IMediator _mediator;
        public CreateWalletEventConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(UserRegisteredEvent @event)
        {
            var command = new CreateWalletCommand(@event.CustomerId);
            await _mediator.Send(command);
        }
    }
}
