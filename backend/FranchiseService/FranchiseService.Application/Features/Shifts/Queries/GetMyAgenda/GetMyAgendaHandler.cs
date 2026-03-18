using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Constants;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetMyAgenda;

/// <summary>
/// Handler xử lý logic lấy lịch cá nhân, bao gồm tính toán isOpenNow
/// và fetch thông tin đồng nghiệp qua gRPC.
/// </summary>
public class GetMyAgendaHandler : IRequestHandler<GetMyAgendaQuery, List<StaffShiftAgendaDto>>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IIdentityServiceClient _identityClient;

    public GetMyAgendaHandler(
        IShiftRepository shiftRepository,
        IIdentityServiceClient identityClient)
    {
        _shiftRepository = shiftRepository;
        _identityClient = identityClient;
    }

    public async Task<List<StaffShiftAgendaDto>> Handle(
        GetMyAgendaQuery request, CancellationToken cancellationToken)
    {
        // Bước 1: Lấy lịch phân công của nhân viên (đã include Shift, Store, StoreHours, TimeLogs)
        var myAssignments = await _shiftRepository.GetAgendaByUserAsync(
            request.UserId, request.StartDate, request.EndDate, cancellationToken);

        if (!myAssignments.Any())
            return new List<StaffShiftAgendaDto>();

        // Bước 2: Gom tất cả ShiftId để tìm đồng nghiệp cùng ca
        var shiftIds = myAssignments.Select(a => a.ShiftId).Distinct().ToList();

        var allCoworkerAssignments = await _shiftRepository.GetAssignmentsByShiftIdsAsync(
            shiftIds, cancellationToken);

        // Lọc bỏ bản thân user, gom theo ShiftId
        var coworkersByShift = allCoworkerAssignments
            .Where(a => a.UserId != request.UserId)
            .GroupBy(a => a.ShiftId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.UserId).ToList());

        // Bước 3: Gom UserId đồng nghiệp duy nhất, gọi IdentityService 1 lần
        var allCoworkerIds = coworkersByShift.Values
            .SelectMany(ids => ids)
            .Distinct()
            .ToList();

        var coworkerInfos = allCoworkerIds.Any()
            ? (await _identityClient.GetUsersByIdsAsync(allCoworkerIds))
                .ToDictionary(u => u.Id)
            : new Dictionary<Guid, UserInfo>();

        // Bước 4: Map sang DTO
        // Tính isOpenNow dùng logic chuẩn (StoreStatus + StoreHours + giờ hiện tại)
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById(DateConstants.GetVietnamTimeZoneId()));
        var currentDay = (int)vnTime.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(vnTime);

        var result = new List<StaffShiftAgendaDto>();

        foreach (var assignment in myAssignments)
        {
            // Lấy bản ghi chấm công gần nhất của assignment này
            var latestLog = assignment.TimeLogs?
                .OrderByDescending(tl => tl.CheckInAt)
                .FirstOrDefault();

            // Tính isOpenNow dựa trên StoreHours và StoreStatus
            var store = assignment.Shift.Store;
            var todayHours = store.StoreHours?.FirstOrDefault(h => h.DayOfWeek == currentDay);
            bool isOpenNow = store.Status == (int)StoreStatus.Open
                          && todayHours != null
                          && todayHours.IsClosed == false
                          && currentTime >= todayHours.OpenTime
                          && currentTime <= todayHours.CloseTime;

            // Tìm danh sách đồng nghiệp của ca này
            var coworkerDtos = new List<AgendaCoWorkerDto>();
            if (coworkersByShift.TryGetValue(assignment.ShiftId, out var cwIds))
            {
                foreach (var cwId in cwIds)
                {
                    coworkerInfos.TryGetValue(cwId, out var info);
                    coworkerDtos.Add(new AgendaCoWorkerDto
                    {
                        Id = cwId,
                        FirstName = info?.FirstName,
                        LastName = info?.LastName,
                        AvatarUrl = info?.AvatarUrl,
                        Phone = info?.Phone
                    });
                }
            }

            result.Add(new StaffShiftAgendaDto
            {
                Id = assignment.Id,
                Date = assignment.Shift.ShiftDate,
                StartTime = assignment.Shift.StartTime,
                EndTime = assignment.Shift.EndTime,
                Store = new AgendaStoreDto
                {
                    Id = store.Id,
                    Name = store.Name,
                    Address = store.Address,
                    Phone = store.Phone,
                    ImageUrl = store.ImageUrl,
                    Latitude = store.Latitude,
                    Longitude = store.Longitude,
                    IsOpenNow = isOpenNow
                },
                CoWorkers = coworkerDtos,
                CheckInAt = latestLog?.CheckInAt,
                CheckOutAt = latestLog?.CheckOutAt,

                ShiftId = assignment.ShiftId,
            });
        }

        return result;
    }
}