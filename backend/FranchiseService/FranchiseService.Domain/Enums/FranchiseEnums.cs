using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FranchiseService.Domain.Enums
{
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
        Cancelled = 3   // Đã hủy
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
}