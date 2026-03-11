using FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;
using MediatR;

namespace FranchiseService.Application.Features.ShiftSwaps.Queries.GetSwapRequestsByStore;

/// <summary>
/// Manager lấy danh sách các yêu cầu đổi ca trong cửa hàng của mình.
/// Có thể filter theo trạng thái (thường là PendingManager để duyệt đơn).
/// </summary>
public class GetSwapRequestsByStoreQuery : IRequest<List<SwapRequestDto>>
{
    public Guid StoreId { get; set; }
    
    // Bộ lọc trạng thái tùy chọn (Optional status filter)
    // Nullable để linh hoạt: null thì lấy ra tất cả, còn truyền vào thì lấy theo Status
    public int? Status { get; set; }
}
