// Enum trạng thái của Ca làm việc
export enum ShiftStatus {
    Draft = 0,
    Published = 1,
    Completed = 2,
    Cancelled = 3
}

// Enum trạng thái của Phân công (Assignment)
export enum ShiftAssignmentStatus {
    Assigned = 0, // Quản lý mới gán
    Confirmed = 1, // Nhân viên đã xác nhận
    Rejected = 2 // Nhân viên từ chối
}
