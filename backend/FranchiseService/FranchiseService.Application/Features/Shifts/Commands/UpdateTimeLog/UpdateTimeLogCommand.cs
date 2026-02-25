using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateTimeLog;

/// <summary>
/// Command cho Quản lý duyệt hoặc từ chối bản ghi chấm công.
/// </summary>
public class UpdateTimeLogCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public int Status { get; set; }       // Trạng thái (1: Valid, 2: Invalid)
    public string? ManagerNote { get; set; }
    //public Guid UserId { get; set; }      // Manager ID người duyệt
}