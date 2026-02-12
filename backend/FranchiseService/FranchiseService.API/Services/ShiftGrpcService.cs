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

            // Khởi tạo response với các trường bắt buộc (Required/Non-optional)
            var response = new GetUserCurrentShiftResponse
            {
                HasShiftToday = result.HasShiftToday,
                StoreId = result.StoreId.ToString(),
                StoreName = result.StoreName,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                CanCheckIn = result.CanCheckIn,
                CanCheckOut = result.CanCheckOut,
                IsOvertime = result.IsOvertime,
                ServerTime = result.ServerTime
            };

            // --- MAPPING CÁC TRƯỜNG OPTIONAL (NULLABLE) ---

            // 1. Mapping IDs (Guid? -> string)
            if (result.AssignmentId.HasValue)
                response.AssignmentId = result.AssignmentId.Value.ToString();

            if (result.ShiftId.HasValue)
                response.ShiftId = result.ShiftId.Value.ToString();

            if (result.CurrentLogId.HasValue)
                response.CurrentLogId = result.CurrentLogId.Value.ToString();

            // 2. Mapping Tọa độ (double? -> double)
            if (result.StoreLat.HasValue)
                response.StoreLat = result.StoreLat.Value;

            if (result.StoreLong.HasValue)
                response.StoreLong = result.StoreLong.Value;

            // 3. Mapping Timestamps (ISO string? -> string)
            if (!string.IsNullOrEmpty(result.CheckInAt))
                response.CheckInAt = result.CheckInAt;

            if (!string.IsNullOrEmpty(result.CheckOutAt))
                response.CheckOutAt = result.CheckOutAt;

            return response;
        }
    }
}