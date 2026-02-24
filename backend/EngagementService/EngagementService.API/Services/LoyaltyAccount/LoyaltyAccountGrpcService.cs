using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using EngagementService.Application.Helper;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Infrastructure.Protos;
using Google.Protobuf.WellKnownTypes;
using MediatR;

namespace EngagementService.API.Services.LoyaltyAccount
{
    public class LoyaltyAccountGrpcService : LoyaltyAccountGrpc.LoyaltyAccountGrpcBase
    {
        private readonly IMediator _mediator;
        public LoyaltyAccountGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<Empty> ProcessOrderReward(ProcessOrderRewardRequest request, Grpc.Core.ServerCallContext context)
        {
            var id = Guid.Parse(request.Id);
            var checkReq = new GetByIdQuery(Id : id);

            var account = await _mediator.Send(checkReq);
            var upsertRequest = new Application.Dtos.LoyaltyAccount.UpSertReqest
            {
                Id = id,
                OrderAmount = int.Parse(request.OrderAmount)
            };
            if (account == null)
            {
                // chưa có tài khoản thì tạo mới rồi mới tính điểm
                var createCommand = new Application.Features.LoyaltyAccount.Commands.Create.CreateCommand
                (upsertRequest);
                await _mediator.Send(createCommand);
                return new Empty();
            }
            var processCommand = new Application.Features.LoyaltyAccount.Commands.ProcessOrderReward.ProcessOrderRewardCommand
               (upsertRequest);
            await _mediator.Send(processCommand);
            return new Empty();
        }
    }
}
