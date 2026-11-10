using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetShiftById
{
    public class GetShiftByIdHandler : IRequestHandler<GetShiftByIdQuery, ShiftResponse>
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IIdentityServiceClient _identityClient;

        public GetShiftByIdHandler(IShiftRepository shiftRepo, IIdentityServiceClient identityClient)
        {
            _shiftRepo = shiftRepo;
            _identityClient = identityClient;
        }

        public async Task<ShiftResponse> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepo.GetShiftByIdWithAssignmentsAsync(request.Id, cancellationToken);

            if (shift == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca làm việc {request.Id}");
            }

            return await MapToShiftResponseAsync(shift);
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