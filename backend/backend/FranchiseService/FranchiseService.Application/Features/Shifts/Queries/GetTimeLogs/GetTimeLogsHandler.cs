using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.Shifts.DTOs;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetTimeLogs;

/// <summary>
/// Handler lấy danh sách Time Logs và ánh xạ thông tin Staff thông qua IdentityClient.
/// </summary>
public class GetTimeLogsHandler : IRequestHandler<GetTimeLogsQuery, PagedResult<TimeLogDto>>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IIdentityServiceClient _identityClient;

    public GetTimeLogsHandler(
        IShiftRepository shiftRepository,
        IIdentityServiceClient identityClient)
    {
        _shiftRepository = shiftRepository;
        _identityClient = identityClient;
    }

    public async Task<PagedResult<TimeLogDto>> Handle(
        GetTimeLogsQuery request, CancellationToken cancellationToken)
    {
        // Xét theo mặc định lấy trong 30 ngày
        var fromDate = request.FromDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var toDate = request.ToDate ?? DateOnly.FromDateTime(DateTime.Today);

        // Gọi method mới trong IShiftRepository để filter và phân trang
        var (timeLogs, totalCount) = await _shiftRepository.GetTimeLogsAsync(
            storeId: request.StoreId,
            staffId: request.StaffId,
            status: request.Status,
            fromDate: fromDate,
            toDate: toDate,
            page: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        // Lấy danh sách ID người dùng duy nhất để tránh gọi gRPC trùng lặp
        var userIds = timeLogs.Select(tl => tl.ShiftAssignment.UserId).Distinct().ToList();

        // Gọi IdentityService DUY NHẤT 1 lần để lấy thông tin toàn bộ nhân viên tham gia
        var users = await _identityClient.GetUsersByIdsAsync(userIds);

        // Khởi tạo từ điển lưu trữ thông tin người dùng tra cứu nhanh 
        var userMap = users.ToDictionary(u => u.Id);

        var dtos = new List<TimeLogDto>();
        
        // Chuyển đổi dữ liệu và ghép thông tin nhân viên đã lưu trữ
        foreach (var tl in timeLogs)
        {
            userMap.TryGetValue(tl.ShiftAssignment.UserId, out var userInfo);

            dtos.Add(new TimeLogDto
            {
                Id = tl.Id,
                ShiftAssignmentId = tl.ShiftAssignmentId,
                CheckInAt = tl.CheckInAt,
                CheckInImageUrl = tl.CheckInImageUrl,
                CheckInLat = tl.CheckInLat,
                CheckInLong = tl.CheckInLong,
                CheckOutAt = tl.CheckOutAt,
                CheckOutImageUrl = tl.CheckOutImageUrl,
                CheckOutLat = tl.CheckOutLat,
                CheckOutLong = tl.CheckOutLong,
                RegularMinutes = tl.RegularMinutes,
                DailyOtMinutes = tl.DailyOtMinutes,
                WeeklyOtMinutes = tl.WeeklyOtMinutes,
                Status = tl.Status ?? 0,
                ManagerNote = tl.ManagerNote,
                CreatedAt = tl.CreatedAt,
                UpdatedAt = tl.UpdatedAt,
                Staff = new TimeLogStaffInfoDto
                {
                    Id = tl.ShiftAssignment.UserId,
                    FirstName = userInfo?.FirstName,
                    LastName = userInfo?.LastName,
                    AvatarUrl = userInfo?.AvatarUrl,
                    Email = userInfo?.Email
                },
                Shift = new TimeLogShiftInfoDto
                {
                    Id = tl.ShiftAssignment.Shift.Id,
                    ShiftDate = tl.ShiftAssignment.Shift.ShiftDate,
                    StartTime = tl.ShiftAssignment.Shift.StartTime,
                    EndTime = tl.ShiftAssignment.Shift.EndTime,
                    Note = tl.ShiftAssignment.Shift.Note
                }
            });
        }

        return new PagedResult<TimeLogDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
