using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability
{
    public class CheckStoreAvailabilityHandler : IRequestHandler<CheckStoreAvailabilityQuery, AvailabilityResult>
    {
        private readonly IStoreRepository _storeRepo;

        public CheckStoreAvailabilityHandler(IStoreRepository storeRepo)
        {
            _storeRepo = storeRepo;
        }

        public async Task<AvailabilityResult> Handle(CheckStoreAvailabilityQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.StoreId, out var storeGuid))
            {
                throw new Exception("ID không hợp lệ");
            }

            // 1. Gọi Repo lấy dữ liệu
            var store = await _storeRepo.GetStoreWithHoursAsync(storeGuid, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy cửa hàng");

            if (store.Status != (int)StoreStatus.Open)
                return new AvailabilityResult { IsAvailable = false, Message = "Cửa hàng không mở" };

            // 3. Logic check giờ
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            var dayOfWeek = (int)vnTime.DayOfWeek;
            var currentTime = TimeOnly.FromDateTime(vnTime);

            var hours = store.StoreHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);

            if (hours == null || hours.IsClosed == true)
                return new AvailabilityResult { IsAvailable = false, Message = "Hôm nay cửa hàng nghỉ." };

            if (currentTime < hours.OpenTime || currentTime > hours.CloseTime)
            {
                return new AvailabilityResult { IsAvailable = false, Message = "Đã hết giờ phục vụ." };
            }

            return new AvailabilityResult { IsAvailable = true };
        }
    }
}