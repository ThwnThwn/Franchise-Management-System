using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetShifts
{
    public class GetShiftsHandler : IRequestHandler<GetShiftsQuery, IEnumerable<ShiftResponse>>
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IIdentityServiceClient _identityClient;

        public GetShiftsHandler(IShiftRepository shiftRepo, IIdentityServiceClient identityClient)
        {
            _shiftRepo = shiftRepo;
            _identityClient = identityClient;
        }

        public async Task<IEnumerable<ShiftResponse>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
        {
            var defaultFrom = DateOnly.FromDateTime(DateTime.Today);
            var defaultTo = defaultFrom.AddDays(7);

            var fromDate = request.FromDate ?? defaultFrom;
            var toDate = request.ToDate ?? defaultTo;

            // 1. Get List from Repo
            var shifts = await _shiftRepo.GetShiftsWithAssignmentsAsync(
                request.StoreId, fromDate, toDate, request.Status, cancellationToken);

            // 2. Pagination (InMemory for now, as Repo returns IEnumerable)
            //var totalCount = shifts.Count();
            var pagedShifts = shifts;
            // .Skip((request.PageNumber - 1) * request.PageSize)
            // .Take(request.PageSize)
            // .ToList();

            // 3. Map to DTO (Async Loop for external calls)
            var shiftResponses = new List<ShiftResponse>();
            foreach (var s in pagedShifts)
            {
                var dto = await MapToShiftResponseAsync(s);
                shiftResponses.Add(dto);
            }

            // Correction: PagedResult constructor order is (items, totalCount, pageIndex, pageSize)
            return shiftResponses;
        }

        private async Task<ShiftResponse> MapToShiftResponseAsync(Domain.Entities.Shift s)
        {
            var assignmentDtos = new List<ShiftAssignmentDto>();

            // Map Assignments & Fetch Staff Info via gRPC
            if (s.ShiftAssignments != null)
            {
                foreach (var a in s.ShiftAssignments)
                {
                    // Call Identity Service to get User Info
                    var userInfo = await _identityClient.GetUserByIdAsync(a.UserId);

                    assignmentDtos.Add(new ShiftAssignmentDto
                    {
                        Id = a.Id,
                        ShiftId = a.ShiftId,
                        UserId = a.UserId,
                        Status = a.Status,
                        HourlyWage = a.HourlyWage,

                        FirstName = userInfo?.FirstName ?? "",
                        LastName = userInfo?.LastName,
                        AvatarUrl = userInfo?.AvatarUrl,
                        Email = userInfo?.Email,

                        TimeLogs = a.TimeLogs.Select(tl => new TimeLogDto
                        {
                            Id = tl.Id,
                            CheckInAt = tl.CheckInAt,
                            CheckOutAt = tl.CheckOutAt,
                            Status = tl.Status ?? 0
                        }).ToList()
                    });
                }
            }

            return new ShiftResponse
            {
                Id = s.Id,
                StoreId = s.StoreId,
                ShiftDate = s.ShiftDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RequiredStaffCount = s.RequiredStaffCount,
                BreakDurationMinutes = s.BreakDurationMinutes,

                ShiftTemplateId = s.ShiftTemplateId,
                Status = s.Status,
                Note = s.Note,
                AssignedStaffCount = assignmentDtos.Count,
                Assignments = assignmentDtos
            };
        }
    }
}