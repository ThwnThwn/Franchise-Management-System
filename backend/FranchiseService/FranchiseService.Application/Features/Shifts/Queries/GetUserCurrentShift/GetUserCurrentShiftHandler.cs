using FranchiseService.Application.Features.Shifts.Dtos;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetUserCurrentShift
{
    public class GetUserCurrentShiftHandler : IRequestHandler<GetUserCurrentShiftQuery, UserShiftDto>
    {
        private readonly IScheduleRepository _scheduleRepo;

        public GetUserCurrentShiftHandler(IScheduleRepository scheduleRepo)
        {
            _scheduleRepo = scheduleRepo;
        }

        public async Task<UserShiftDto> Handle(GetUserCurrentShiftQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.UserId, out var userGuid))
                return new UserShiftDto { HasShiftToday = false };

            var today = DateTime.UtcNow.Date; // Convert sang giờ VN nếu cần

            // Gọi Repo
            var schedule = await _scheduleRepo.GetPublishedShiftAsync(userGuid, today, cancellationToken);

            if (schedule == null)
                return new UserShiftDto { HasShiftToday = false };

            return new UserShiftDto
            {
                HasShiftToday = true,
                StoreId = schedule.StoreId,
                StoreName = schedule.Store?.Name, // Null check vì Repo đã Include
                ShiftStart = schedule.StartTime.ToString("HH:mm"),
                ShiftEnd = schedule.EndTime.ToString("HH:mm")
            };
        }
    }
}