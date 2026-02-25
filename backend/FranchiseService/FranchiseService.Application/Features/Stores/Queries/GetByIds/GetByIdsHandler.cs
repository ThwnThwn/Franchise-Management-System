using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetByIds
{
    public class GetByIdsHandler : IRequestHandler<GetByIdsQuery, IEnumerable<StoreDto>>
    {
        private readonly IStoreRepository _repo;

        public GetByIdsHandler(IStoreRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<StoreDto>> Handle(GetByIdsQuery request, CancellationToken cancellationToken)
        {
            // 1. Chuyển đổi list string IDs sang Guid (Bỏ qua những thằng invalid)
            var resultList = request.Ids?
                .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
                .Where(g => g.HasValue)
                .Select(g => g.Value)
                .ToList() ?? [];

            if (resultList.Count == 0) return [];

            // 2. Gọi Repo lấy danh sách Entity
            var stores = await _repo.GetByIdsAsync(resultList, cancellationToken);
            if (stores == null || !stores.Any()) return [];

            // 3. Chuẩn bị dữ liệu thời gian (Tính 1 lần duy nhất ở ngoài vòng lặp)
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var currentDay = (int)vnTime.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(vnTime);

            // 4. Map & Tính toán IsOpenNow cho từng Store trong danh sách
            return stores.Select(store =>
            {
                // Tìm giờ mở cửa của ngày hôm nay cho store này
                var todayHours = store.StoreHours?.FirstOrDefault(h => h.DayOfWeek == currentDay);

                bool isOpen = false;
                // Logic check: Trạng thái Open + Có cấu hình giờ + Không đánh dấu Đóng cửa + Nằm trong khung giờ
                if (store.Status == (int)StoreStatus.Open &&
                    todayHours != null &&
                    todayHours.IsClosed != true &&
                    currentTime >= todayHours.OpenTime &&
                    currentTime <= todayHours.CloseTime)
                {
                    isOpen = true;
                }

                return new StoreDto
                {
                    Id = store.Id,
                    Name = store.Name,
                    Address = store.Address,
                    Phone = store.Phone,
                    ImageUrl = store.ImageUrl,
                    Latitude = (double?)store.Latitude,
                    Longitude = (double?)store.Longitude,
                    IsOpenNow = isOpen
                };
            });
        }
    }
}