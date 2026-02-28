
using EngagementService.Application.Features.LoyaltyAccount.Commands.Create;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.EventHandler
{
    public class CreateLoyaltyAccountHandler : IIntegrationEventHandler<CreateLoyaltyAccountEvent>
    {
        private readonly IMediator _mediator;
        public CreateLoyaltyAccountHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(CreateLoyaltyAccountEvent @event)
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
}
