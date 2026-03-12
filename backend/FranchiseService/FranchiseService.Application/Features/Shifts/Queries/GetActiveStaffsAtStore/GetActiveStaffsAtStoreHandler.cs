using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;
using System.Runtime.InteropServices;

namespace FranchiseService.Application.Features.Shifts.Queries.GetActiveStaffsAtStore
{
    public class GetActiveStaffsAtStoreHandler : IRequestHandler<GetActiveStaffsAtStoreQuery, List<ActiveStaffDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly FranchiseService.Application.ClientInterfaces.IIdentityServiceClient _identityClient;

        public GetActiveStaffsAtStoreHandler(IShiftRepository shiftRepository, FranchiseService.Application.ClientInterfaces.IIdentityServiceClient identityClient)
        {
            _shiftRepository = shiftRepository;
            _identityClient = identityClient;
        }

        public async Task<List<ActiveStaffDto>> Handle(GetActiveStaffsAtStoreQuery request, CancellationToken cancellationToken)
        {
            // 1. Determine "Today" and "Now" in Vietnam timezone
            var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var todayVn = DateOnly.FromDateTime(nowVn);
            var currentTime = nowVn.TimeOfDay;

            // 2. Fetch all Published shifts for the store today
            var shifts = await _shiftRepository.GetShiftsWithAssignmentsAsync(
                request.StoreId,
                todayVn,
                todayVn,
                1, // 1 = Published
                cancellationToken);

            var activeStaffs = new List<ActiveStaffDto>();
            var seenUserIds = new HashSet<Guid>();

            // 3. Filter shifts covering current time and collect assignments
            foreach (var shift in shifts)
            {
                bool isCurrentlyInShift;
                if (shift.StartTime <= shift.EndTime)
                {
                    isCurrentlyInShift = (currentTime >= shift.StartTime.ToTimeSpan() && currentTime <= shift.EndTime.ToTimeSpan());
                }
                else
                {
                    // Over midnight shift
                    isCurrentlyInShift = (currentTime >= shift.StartTime.ToTimeSpan() || currentTime <= shift.EndTime.ToTimeSpan());
                }

                if (isCurrentlyInShift)
                {
                    foreach (var assignment in shift.ShiftAssignments)
                    {
                        if (!seenUserIds.Contains(assignment.UserId))
                        {
                            var userInfo = await _identityClient.GetUserByIdAsync(assignment.UserId);
                            if (userInfo != null)
                            {
                                activeStaffs.Add(new ActiveStaffDto
                                {
                                    UserId = assignment.UserId,
                                    FirstName = userInfo.FirstName ?? "",
                                    LastName = userInfo.LastName ?? "",
                                    Email = userInfo.Email ?? "",
                                    AvatarUrl = userInfo.AvatarUrl ?? ""
                                });
                                seenUserIds.Add(assignment.UserId);
                            }
                        }
                    }
                }
            }

            return activeStaffs;
        }
    }
}
