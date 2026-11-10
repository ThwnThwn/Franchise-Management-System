using EngagementService.Application.Features.Voucher.Commands.ReverseVoucher;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler
{
    public class ReverseVoucherEventHandler : IIntegrationEventHandler<ReverseVoucherEvent>
    {

        private readonly IMediator _mediator;

        public ReverseVoucherEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(ReverseVoucherEvent @event)
        {
            await _mediator.Send(new ReverseVoucherCommand(@event.OrderId));
        }
    }
}
