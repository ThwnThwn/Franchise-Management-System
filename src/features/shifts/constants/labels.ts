import { ShiftApplicationStatus, ShiftStatus, SwapRequestStatus, TimeLogStatus } from "./enum"

export const ShiftStatusLabels: Record<ShiftStatus, string> = {
    [ShiftStatus.Draft]: "Nháp",
    [ShiftStatus.Published]: "Đã ban hành",
    [ShiftStatus.Completed]: "Hoàn thành",
    [ShiftStatus.Cancelled]: "Huỷ",
    [ShiftStatus.Open]: "Chợ ca"
}

export const TimeLogStatusLabels: Record<TimeLogStatus, string> = {
    [TimeLogStatus.Processing]: "Đang chờ",
    [TimeLogStatus.Valid]: "Hợp lệ",
    [TimeLogStatus.Invalid]: "Không hợp lệ"
}

export const BulkShiftTypeLabels = {
    manual: "Tạo Thủ Công",
    template: "Dùng Mẫu Ca"
}

export const ShiftApplicationStatusLabels: Record<ShiftApplicationStatus, string> = {
    [ShiftApplicationStatus.Pending]: "Đang chờ duyệt",
    [ShiftApplicationStatus.Approved]: "Đã chấp thuận",
    [ShiftApplicationStatus.Rejected]: "Đã từ chối"
}

export const SwapRequestStatusLabels: Record<SwapRequestStatus, string> = {
    [SwapRequestStatus.PendingTarget]: "Chờ đồng nghiệp",
    [SwapRequestStatus.PendingManager]: "Chờ quản lý",
    [SwapRequestStatus.Approved]: "Thành công",
    [SwapRequestStatus.Rejected]: "Bị từ chối",
    [SwapRequestStatus.Expired]: "Quá hạn",
    [SwapRequestStatus.Cancelled]: "Đã hủy"
}
