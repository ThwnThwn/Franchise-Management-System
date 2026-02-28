using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.EventHandler
{
    public class ProcessOrderRewardHandler : IIntegrationEventHandler<ProcessOrderRewardEvent>
    {
        private readonly IMediator _mediator;
        public ProcessOrderRewardHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(ProcessOrderRewardEvent @event)
        {
            var id = @event.Id;
            var checkReq = new GetByIdQuery(Id: id);

            var account = await _mediator.Send(checkReq);
            var upsertRequest = new Application.Dtos.LoyaltyAccount.UpSertReqest
            {
                Id = id,
                OrderAmount = @event.OrderAmount
            };
            if (account == null)
            {
                // chưa có tài khoản thì tạo mới rồi mới tính điểm
                var createCommand = new Application.Features.LoyaltyAccount.Commands.Create.CreateCommand
                (upsertRequest);
                await _mediator.Send(createCommand);
            }
            var processCommand = new Application.Features.LoyaltyAccount.Commands.ProcessOrderReward.ProcessOrderRewardCommand
               (upsertRequest);
            await _mediator.Send(processCommand);
        }
    }
}
