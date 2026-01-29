using FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability;
using FranchiseService.Application.Features.Stores.Queries.FindNearbyStores;
using FranchiseService.Application.Features.Stores.Queries.GetStoreDetail;
using FranchiseService.Application.Features.Stores.Queries.GetStores;
using FranchiseService.Application.Features.Shifts.Queries.GetUserCurrentShift;
using Grpc.Core;
using MediatR;

namespace FranchiseService.API.Services
{
    public class ShiftGrpcService : API.ShiftGrpcService.ShiftGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public ShiftGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 3. GetUserCurrentShift
        public override async Task<GetUserCurrentShiftResponse> GetUserCurrentShift(
            GetUserCurrentShiftRequest request, ServerCallContext context)
        {
            var query = new GetUserCurrentShiftQuery { UserId = request.UserId };

            var result = await _mediator.Send(query);

            return new GetUserCurrentShiftResponse
            {
                HasShiftToday = result.HasShiftToday,
                StoreId = result.StoreId?.ToString() ?? "",
                StoreName = result.StoreName ?? "",
                ShiftStart = result.ShiftStart ?? "",
                ShiftEnd = result.ShiftEnd ?? "",
                StoreLat = result.StoreLat,
                StoreLong = result.StoreLong
            };
        }
    }
}