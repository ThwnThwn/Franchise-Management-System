using FranchiseService.Application.Features.Shifts.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Constants;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetUserCurrentShift
{
    public class GetUserCurrentShiftHandler : IRequestHandler<GetUserCurrentShiftQuery, UserShiftDto>
    {
        private readonly IShiftRepository _shiftRepo;

        public GetUserCurrentShiftHandler(IShiftRepository shiftRepo)
        {
            _shiftRepo = shiftRepo;
        }

        public async Task<UserShiftDto> Handle(GetUserCurrentShiftQuery request, CancellationToken cancellationToken)
        {
            // 1. Validate User
            if (!Guid.TryParse(request.UserId, out var userGuid))
                throw new UnauthorizedAccessException("Invalid UserId");

            var now = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(now, vnTimeZone);

            // 2. Gọi Repo bốc ca "khả dụng" dựa trên "Cửa sổ chấm công"
            var activeAssignment = await _shiftRepo.GetActiveShiftAssignmentAsync(
                userGuid,
                vnNow,
                AttendanceConstants.CheckInBufferMinutes,
                AttendanceConstants.CheckOutBufferHours
            );

            // Trường hợp không có ca nào thỏa mãn
            if (activeAssignment == null)
            {
                return new UserShiftDto { HasShiftToday = false };
            }

            var shift = activeAssignment.Shift;
            var lastLog = activeAssignment.TimeLogs.OrderByDescending(l => l.CreatedAt).FirstOrDefault();

            // --- Dựng lại ActualEnd ---
            // Nếu giờ bắt đầu < giờ kết thúc (Ca thường) -> Cùng 1 ngày
            // Nếu giờ bắt đầu > giờ kết thúc (Ca đêm) -> Kết thúc vào ngày hôm sau
            var endDt = shift.StartTime < shift.EndTime
                ? shift.ShiftDate.ToDateTime(shift.EndTime)
                : shift.ShiftDate.AddDays(1).ToDateTime(shift.EndTime);
            var actualEndUtc = DateTime.SpecifyKind(endDt, DateTimeKind.Utc);

            // 3. Mapping và trả về
            return new UserShiftDto
            {
                HasShiftToday = true,
                AssignmentId = activeAssignment.Id,
                ShiftId = shift.Id,
                StoreId = shift.StoreId,
                StoreName = shift.Store.Name,

                // Tọa độ để FE check GPS
                StoreLat = (double?)shift.Store?.Latitude,
                StoreLong = (double?)shift.Store?.Longitude,

                // Format giờ để FE hiển thị
                StartTime = shift.StartTime.ToString("HH:mm:ss"),
                EndTime = shift.EndTime.ToString("HH:mm:ss"),

                // Logic Flags cho UI
                CanCheckIn = lastLog == null,
                CanCheckOut = lastLog != null && lastLog.CheckOutAt == null,
                IsOvertime = now > actualEndUtc,

                // Thông tin Log chấm công (Quan trọng cho ca đang chạy)
                CurrentLogId = lastLog?.Id,              // Để khi Check-out FE gửi đúng ID này lên

                // Format chuẩn ISO 8601 có đuôi Z
                CheckInAt = lastLog?.CheckInAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CheckOutAt = lastLog?.CheckOutAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ServerTime = now.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
}