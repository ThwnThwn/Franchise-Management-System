using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace EngagementService.Application.EventHandler
{
    public class UseVoucherEventHandler : IIntegrationEventHandler<UseVouchersEvent>
    {
        private readonly IMediator _mediator;
        public UseVoucherEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(UseVouchersEvent @event)
        {
            var command = new UseVoucherCommand(
            new UseVouchersRequest()
            {
                VoucherIds = @event.VoucherIds,
                OrderId = @event.OrderId,
                UserId = @event.UserId
            });

            await _mediator.Send(command);
        }
    }
}
