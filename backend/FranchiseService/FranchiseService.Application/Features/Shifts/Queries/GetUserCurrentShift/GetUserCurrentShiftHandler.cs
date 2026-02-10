using FranchiseService.Application.Features.Shifts.Dtos;
using FranchiseService.Application.Repositories;
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
            if (!Guid.TryParse(request.UserId, out var userGuid))
                return new UserShiftDto { HasShiftToday = false };

            var today = DateTime.UtcNow.Date; // Convert sang giờ VN nếu cần

            // Gọi Repo
            var shift = await _shiftRepo.GetPublishedShiftAsync(userGuid, today, cancellationToken);

            if (shift == null)
                return new UserShiftDto { HasShiftToday = false };

            return new UserShiftDto
            {
                HasShiftToday = true,
                StoreId = shift.StoreId,
                StoreName = shift.Store?.Name ?? "", // Null check vì Repo đã Include
                ShiftStart = shift.StartTime.ToString("HH:mm"),
                ShiftEnd = shift.EndTime.ToString("HH:mm")
            };
        }
    }
}