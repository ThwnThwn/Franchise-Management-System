namespace FranchiseService.Domain.Enums;

public enum StoreType
{
    CompanyOwned = 0, // Của công ty mẹ
    Franchise = 1     // Nhượng quyền
}

public enum StoreStatus
{
    ComingSoon = 0,
    Open = 1,
    Closed = 2,
    Renovating = 3 // Đang sửa chữa
}

public enum ShiftStatus
{
    Draft = 0,      // Nháp
    Published = 1,  // Đã chốt
    Completed = 2,  // Hoàn thành
    Cancelled = 3,  // Đã hủy
    Open = 4        // Đang mở cho nhân viên đăng ký từ Chợ Ca
}

// Trạng thái phân công nhân viên vào ca
public enum ShiftAssignmentStatus
{
    Assigned = 0,   // Quản lý mới gán
    Confirmed = 1,  // Nhân viên đã xác nhận
    Rejected = 2    // Nhân viên từ chối
}

public enum TimeLogStatus
{
    Processing = 0, // Đang làm
    Valid = 1,      // Hợp lệ
    Invalid = 2,    // Lỗi/Quên checkout
    Approved = 3    // Đã duyệt
}

/// <summary>
/// Trạng thái đơn đăng ký nhận ca từ Chợ Ca (Shift Pool)
/// </summary>
public enum ShiftApplicationStatus
{
    Pending = 0,    // Đang chờ Manager duyệt
    Approved = 1,   // Đã duyệt, tự động tạo ShiftAssignment
    Rejected = 2    // Manager từ chối
}

/// <summary>
/// Trang thai yeu cau doi ca (State Machine)
/// </summary>
public enum SwapRequestStatus
{
    PendingTarget = 0,   // Cho B xac nhan
    PendingManager = 1,  // B dong y, cho Manager duyet
    Approved = 2,        // Manager da duyet, swap thanh cong
    Rejected = 3,        // B hoac Manager tu choi
    Expired = 4,         // Qua han 24h
    Cancelled = 5        // A tu huy
}

/// <summary>
/// Trạng thái hồ sơ đăng ký nhượng quyền.
/// </summary>
public enum FranchiseApplicationStatus
{
    Pending = 0,    // Chờ thẩm định
    Approved = 1,   // Duyệt hồ sơ → sinh Contract
    Rejected = 2    // Từ chối
}

/// <summary>
/// Trạng thái thanh toán tiền cọc / phí nhượng quyền đợt 1.
/// </summary>
public enum DepositStatus
{
    AwaitingPayment = 0, // Chưa thanh toán
    Paid = 1             // Đã thanh toán
}

/// <summary>
/// Tiến độ thi công / setup cửa hàng sau khi hợp đồng active.
/// </summary>
public enum ContractSetupStatus
{
    Design = 0,       // Đang thiết kế
    Construction = 1, // Đang thi công
    Inspection = 2,   // Nghiệm thu
    Ready = 3         // Sẵn sàng khai trương
}

/// <summary>
/// Trạng thái hợp đồng nhượng quyền — State Machine:
/// Draft → Approved → AwaitingSignature → AwaitingPayment → Active → (Suspended / Terminated / Expired)
/// </summary>
public enum ContractStatus
{
    Draft = 0,              // Vừa tạo, đang soạn thảo
    Approved = 1,           // Giám đốc / admin đã duyệt các điều khoản
    AwaitingSignature = 2,  // Đã gửi DocuSeal, chờ ký
    AwaitingPayment = 3,    // Ký xong, chờ chuyển khoản tiền cọc
    Active = 4,             // Đã thanh toán, hợp đồng có hiệu lực
    Suspended = 5,          // Tạm ngưng (vi phạm / đang giải quyết tranh chấp)
    Terminated = 6,         // Chấm dứt hợp đồng trước hạn
    Expired = 7             // Hết hạn tự nhiên
}