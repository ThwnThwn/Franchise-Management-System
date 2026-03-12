using Ardalis.Result;
using MediatR;

namespace SalesService.Application.Features.Orders.Commands.AssignStaff;

public record AssignStaffToDeliveryCommand(Guid OrderId, Guid? StaffId) : IRequest<Result<AssignStaffToDeliveryResult>>;

public class AssignStaffToDeliveryResult
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid? DelivererId { get; set; }
    public string Message { get; set; } = string.Empty;
}
