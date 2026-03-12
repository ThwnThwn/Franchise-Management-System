using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Franchise;
using SalesService.Application.Persisitences.Orders;
using System.Runtime.InteropServices;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;

namespace SalesService.Application.Features.Orders.Commands.AssignStaff;

public sealed class AssignStaffToDeliveryCommandHandler
    : IRequestHandler<AssignStaffToDeliveryCommand, Result<AssignStaffToDeliveryResult>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIdentityServiceClient _identityServiceClient;
    private readonly IFranchiseServiceClient _franchiseServiceClient;
    private readonly ILogger<AssignStaffToDeliveryCommandHandler> _logger;

    // Role slugs that are allowed to be assigned as deliverer
    private static readonly HashSet<string> AllowedDelivererSlugs =
        new(StringComparer.OrdinalIgnoreCase) { "deliverer", "staff", "shipper" };

    public AssignStaffToDeliveryCommandHandler(
        IOrderRepository orderRepository,
        IIdentityServiceClient identityServiceClient,
        IFranchiseServiceClient franchiseServiceClient,
        ILogger<AssignStaffToDeliveryCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _identityServiceClient = identityServiceClient;
        _franchiseServiceClient = franchiseServiceClient;
        _logger = logger;
    }

    public async Task<Result<AssignStaffToDeliveryResult>> Handle(
        AssignStaffToDeliveryCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Load order
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order == null || order.IsDeleted)
        {
            return Result<AssignStaffToDeliveryResult>.NotFound(
                $"Order '{command.OrderId}' not found.");
        }

        // Ràng buộc: nếu đơn hàng đang ở trạng thái delivery thì sẽ không được gán nhân viên nữa
        if (order.Status == OrderStatus.Delivering)
        {
            return Result<AssignStaffToDeliveryResult>.Error(
                $"Đơn hàng '{order.Code}' đã ở trạng thái đang giao, không thể gán lại nhân viên.");
        }

        Guid assignedStaffId;

        if (command.StaffId.HasValue)
        {
            // 2a. Explicit staff assignment – validate the user exists and has the right role
            var user = await _identityServiceClient.GetUserByIdAsync(command.StaffId.Value);

            if (user == null)
            {
                return Result<AssignStaffToDeliveryResult>.Error(
                    $"User '{command.StaffId.Value}' not found.");
            }

            var roleSlug = user.Role?.Slug ?? string.Empty;

            if (!AllowedDelivererSlugs.Contains(roleSlug))
            {
                return Result<AssignStaffToDeliveryResult>.Error(
                    $"User '{command.StaffId.Value}' does not have a deliverer role (current role: '{roleSlug}').");
            }

            // 2b. Check if the staff is currently in an active shift
            var shiftInfo = await _franchiseServiceClient.GetUserCurrentShiftAsync(command.StaffId.Value);
            
            if (shiftInfo == null || !shiftInfo.HasShiftToday)
            {
                return Result<AssignStaffToDeliveryResult>.Error(
                    $"Nhân viên '{user.Username ?? command.StaffId.Value.ToString()}' không có ca làm việc trong ngày hôm nay.");
            }

            if (!string.IsNullOrEmpty(shiftInfo.CheckOutAt))
            {
                return Result<AssignStaffToDeliveryResult>.Error(
                    $"Nhân viên '{user.Username ?? command.StaffId.Value.ToString()}' đã check-out, không thể nhận giao đơn.");
            }

            // Evaluate if within shift timeframe
            var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var currentLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone).TimeOfDay;

            // Use ScheduledTime if available, otherwise use current LocalTime
            var targetLocalTime = order.ScheduledTime.HasValue ? order.ScheduledTime.Value.ToTimeSpan() : currentLocalTime;

            if (TimeSpan.TryParse(shiftInfo.StartTime, out var startTime) &&
                TimeSpan.TryParse(shiftInfo.EndTime, out var endTime))
            {
                if (startTime <= endTime)
                {
                    if (targetLocalTime < startTime || targetLocalTime > endTime)
                    {
                        var timeLabel = order.ScheduledTime.HasValue ? "Giờ hẹn giao hàng" : "Giờ hiện tại";
                        return Result<AssignStaffToDeliveryResult>.Error(
                            $"Nhân viên '{user.Username}' không có ca làm việc vào lúc này. Ca làm: {shiftInfo.StartTime}-{shiftInfo.EndTime}. {timeLabel}: {targetLocalTime:hh\\:mm}.");
                    }
                }
                else
                {
                    // Shift spans across midnight
                    if (targetLocalTime < startTime && targetLocalTime > endTime)
                    {
                        var timeLabel = order.ScheduledTime.HasValue ? "Giờ hẹn giao hàng" : "Giờ hiện tại";
                        return Result<AssignStaffToDeliveryResult>.Error(
                            $"Nhân viên '{user.Username}' không có ca làm làm việc qua đêm vào lúc này. Ca làm: {shiftInfo.StartTime}-{shiftInfo.EndTime}. {timeLabel}: {targetLocalTime:hh\\:mm}.");
                    }
                }
            }

            assignedStaffId = command.StaffId.Value;
        }
        else
        {
            // 2b. Auto-select logic
            if (!order.StoreId.HasValue)
            {
                return Result<AssignStaffToDeliveryResult>.Error("Order does not have a StoreId. Cannot auto-select staff.");
            }

            var activeStaffIds = await _franchiseServiceClient.GetActiveStaffsAtStoreAsync(order.StoreId.Value);

            if (activeStaffIds == null || !activeStaffIds.Any())
            {
                return Result<AssignStaffToDeliveryResult>.Error("No staff members are currently on shift at this store.");
            }

            Guid? selectedStaffId = null;

            foreach (var staffId in activeStaffIds)
            {
                var user = await _identityServiceClient.GetUserByIdAsync(staffId);
                if (user == null) continue;

                var roleSlug = user.Role?.Slug ?? string.Empty;
                if (AllowedDelivererSlugs.Contains(roleSlug))
                {
                    selectedStaffId = staffId;
                    break;
                }
            }

            if (!selectedStaffId.HasValue)
            {
                return Result<AssignStaffToDeliveryResult>.Error("No staff members on shift have a deliverer role at this store.");
            }

            assignedStaffId = selectedStaffId.Value;
        }

        // 3. Update Status to Delivering and Assign Deliverer
        order.DelivererId = assignedStaffId;
        order.Status = OrderStatus.Delivering; // 5 = Delivering
        order.UpdatedAt = DateTimeOffset.UtcNow;
        
        order.OrderTrackings.Add(new OrderTracking
        {
            OrderId = order.Id,
            Status = order.Status,
            Action = "Đã phân công nhân viên giao hàng",
            Note = $"Đơn hàng đã được chuyển trạng thái sang Đang giao và gán cho nhân viên: {assignedStaffId}.",
            CreatedBy = Guid.Empty, 
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation(
            "Assigned staff {StaffId} to order {OrderCode} and updated status to Delivering",
            assignedStaffId, order.Code);

        return Result<AssignStaffToDeliveryResult>.Success(new AssignStaffToDeliveryResult
        {
            OrderId = order.Id,
            OrderCode = order.Code,
            DelivererId = assignedStaffId,
            Message = $"Staff '{assignedStaffId}' successfully assigned to order '{order.Code}'."
        });
    }
}
