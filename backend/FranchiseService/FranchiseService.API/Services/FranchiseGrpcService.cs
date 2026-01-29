using FranchiseService.Application.Features.Queries;
using FranchiseService.Application.Features.Queries.CheckStoreAvailability;
using FranchiseService.Application.Features.Queries.FindNearbyStores;
using FranchiseService.Application.Features.Queries.GetStoreDetail;
using FranchiseService.Application.Features.Queries.GetStores;
using FranchiseService.Application.Features.Queries.GetUserCurrentShift;
using Grpc.Core;
using MediatR;

namespace FranchiseService.API.Services
{
    public class FranchiseGrpcService : API.FranchiseGrpcService.FranchiseGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public FranchiseGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. CheckStoreAvailability
        public override async Task<CheckStoreAvailabilityResponse> CheckStoreAvailability(
            CheckStoreAvailabilityRequest request, ServerCallContext context)
        {
            var query = new CheckStoreAvailabilityQuery { StoreId = request.StoreId };

            var result = await _mediator.Send(query);

            return new CheckStoreAvailabilityResponse
            {
                IsAvailable = result.IsAvailable,
                Message = result.Message ?? ""
            };
        }

        // 2. FindNearbyStores
        public override async Task<GetStoresResponse> FindNearbyStores(
            FindNearbyStoresRequest request, ServerCallContext context)
        {
            var query = new FindNearbyStoresQuery
            {
                UserLat = request.UserLat,
                UserLong = request.UserLong,
                RadiusKm = request.RadiusKm,
                Limit = request.Limit
            };

            var stores = await _mediator.Send(query);

            var response = new GetStoresResponse
            {
                TotalCount = stores.Count,
                TotalPages = 1
            };

            // Map thủ công DTO -> Proto
            response.Stores.AddRange(stores.Select(s => new StoreDetailResponse
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone ?? "",
                ImageUrl = s.ImageUrl ?? "",
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                IsOpenNow = s.IsOpenNow
            }));

            return response;
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

        // =================================================================
        // 4. GetStores (Apply Pagination)
        // =================================================================
        public override async Task<GetStoresResponse> GetStores(GetStoresRequest request, ServerCallContext context)
        {
            // Map Proto Request -> MediatR Query
            var query = new GetStoresQuery
            {
                // PaginationRequest sẽ xử lý logic < 1 hoặc > MaxSize
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Keyword = request.Keyword
            };

            // Gọi MediatR
            var pagedResult = await _mediator.Send(query);

            // Map MediatR Result -> Proto Response
            var response = new GetStoresResponse
            {
                TotalCount = (int)pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages
            };

            // Map List Items
            response.Stores.AddRange(pagedResult.Items.Select(s => new StoreDetailResponse
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone ?? "",
                ImageUrl = s.ImageUrl ?? "",
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                IsOpenNow = s.IsOpenNow
            }));

            return response;
        }

        // =================================================================
        // 5. GetStoreDetail
        // =================================================================
        public override async Task<StoreDetailResponse> GetStoreDetail(GetStoreDetailRequest request, ServerCallContext context)
        {
            var query = new GetStoreDetailQuery { Id = request.Id };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy cửa hàng"));
            }

            return new StoreDetailResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                Address = result.Address,
                Phone = result.Phone ?? "",
                ImageUrl = result.ImageUrl ?? "",
                Latitude = result.Latitude,
                Longitude = result.Longitude,
                IsOpenNow = result.IsOpenNow
            };
        }
    }
}