using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetStoreDetail
{
    public class GetStoreDetailHandler : IRequestHandler<GetStoreDetailQuery, StoreDto?>
    {
        private readonly IStoreRepository _repo;

        public GetStoreDetailHandler(IStoreRepository repo)
        {
            _repo = repo;
        }

        public async Task<StoreDto?> Handle(GetStoreDetailQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.Id, out var guid)) return null;

            // 1. Gọi Repo
            var store = await _repo.GetByIdAsync(guid, cancellationToken);
            if (store == null) return null;

            // 2. Logic tính toán IsOpenNow (Chính xác từng phút)
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(
                DateConstants.GetVietnamTimeZoneId()
            ));
            var currentDay = (int)vnTime.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(vnTime);

            // Tìm giờ mở cửa hôm nay
            var todayHours = store.StoreHours?.FirstOrDefault(h => h.DayOfWeek == currentDay);

            bool isOpen = false;
            if (store.Status == (int)StoreStatus.Open && todayHours != null && todayHours.IsClosed == false)
            {
                if (currentTime >= todayHours.OpenTime && currentTime <= todayHours.CloseTime)
                {
                    isOpen = true;
                }
            }

            // 3. Map Entity -> DTO
            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                Phone = store.Phone,
                ImageUrl = store.ImageUrl,
                Latitude = (double?)(store.Latitude),
                Longitude = (double?)(store.Longitude),
                IsOpenNow = isOpen
            };
        }
    }
}