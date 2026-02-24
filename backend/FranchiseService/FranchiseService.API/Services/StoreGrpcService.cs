using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability;
using FranchiseService.Application.Features.Stores.Queries.FindNearbyStores;
using FranchiseService.Application.Features.Stores.Queries.GetStoreDetail;
using FranchiseService.Application.Features.Stores.Queries.GetStores;
using Grpc.Core;
using MediatR;
using FranchiseService.Grpc;

namespace FranchiseService.API.Services
{
    public class StoreGrpcService : FranchiseService.Grpc.StoreGrpcService.StoreGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public StoreGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. CheckStoreAvailability
        public override async Task<CheckStoreAvailabilityResponse> CheckStoreAvailability(
            CheckStoreAvailabilityRequest request, ServerCallContext context)
        {
            var query = new CheckStoreAvailabilityQuery { StoreId = request.StoreId };

            var result = await _mediator.Send(query);

            return new FranchiseService.Grpc.CheckStoreAvailabilityResponse
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

            var response = new FranchiseService.Grpc.GetStoresResponse
            {
                TotalCount = stores.Count,
                TotalPages = 1
            };

            // Map thủ công DTO -> Proto
            response.Items.AddRange(stores.Select(s => MapToStoreDetailResponse(s)));

            return response;
        }

        // =================================================================
        // 3. GetStores (Apply Pagination)
        // =================================================================
        public override async Task<GetStoresResponse> GetStores(GetStoresRequest request, ServerCallContext context)
        {
            // Map Proto Request -> MediatR Query
            var query = new GetStoresQuery
            {
                // PaginationRequest sẽ xử lý logic < 1 hoặc > MaxSize
                Page = request.Page,
                PageSize = request.PageSize,
                Keyword = request.Keyword
            };

            // Gọi MediatR
            var pagedResult = await _mediator.Send(query);

            // Map MediatR Result -> Proto Response
            var response = new FranchiseService.Grpc.GetStoresResponse
            {
                TotalCount = (int)pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages
            };

            // Map List Items
            response.Items.AddRange(pagedResult.Items.Select(s => MapToStoreDetailResponse(s)));

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

            return MapToStoreDetailResponse(result);
        }

        private StoreDetailResponse MapToStoreDetailResponse(StoreDto dto)
        {
            var res = new StoreDetailResponse
            {
                Id = dto.Id.ToString(),
                Name = dto.Name,
                Address = dto.Address,
                IsOpenNow = dto.IsOpenNow
            };

            if (!string.IsNullOrEmpty(dto.Phone))
            {
                res.Phone = dto.Phone;
            }
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                res.ImageUrl = dto.ImageUrl;
            }
            if (dto.Longitude.HasValue)
            {
                res.Longitude = dto.Longitude.Value;
            }
            if (dto.Latitude.HasValue)
            {
                res.Latitude = dto.Latitude.Value;
            }

            return res;
        }
    }
}