using EngagementService.Application.Features.PointLog.Queries;
using EngagementService.Grpc.Protos.PointLogGrpc;
using Grpc.Core;
using MediatR;

namespace EngagementService.API.Services.PointLog
{
    public class PointLogGrpcService : PointLogGrpc.PointLogGrpcBase
    {
        private readonly IMediator _mediator;

        public PointLogGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<GetMyPointLogRep> GetMyPointLog(GetMyPointLogRequest request, ServerCallContext context)
        {
            var res = await _mediator.Send(new ViewMyPointLogQuery()
            {
                UserId = Guid.Parse(request.Id),
                Page = request.Page,
                PageSize = request.PageSize,
                FromDate = request.FromDate?.ToDateTime(),
                ToDate = request.ToDate?.ToDateTime()
            });
            var response = new GetMyPointLogRep()
            {
                TotalPages = res.TotalPages,
                TotalCount = ((int)res.TotalCount)
            };

            var protoItems = res.Items.Select(item => new LogResponse
            {
                Id = item.Id.ToString(),
                UserId = item.UserId.ToString(),
                Amount = item.Amount.ToString(),
                BalanceAfter = item.BalanceAfter.ToString(),
                BalanceBefore = item.BalanceBefore.ToString(),
                ActionType = item.ActionType.ToString(),
                ReasonCode = item.ReasonCode.ToString(),
                Description = item.Description,
                ReferenceId = item.ReferenceId?.ToString() ?? string.Empty,
                ExpiredAt = item.ExpiredAt?.ToString("o") ?? string.Empty,
                CreatedAt = item.CreatedAt.ToString("o")

            });

            // 3. Add vào danh sách repeated của Proto
            response.LogRes.AddRange(protoItems);

            return response;
        }
    }
}
