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

export enum TimeLogStatus {
    Processing = 0, // Vừa check-in (Đang trong ca làm việc)
    Valid = 1, // Đã check-out và khớp với giờ quy định (Đúng giờ)
    Invalid = 2 // Đã check-out nhưng sai giờ quy định (Đi muộn/Về sớm)
}
