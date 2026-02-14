import { ShiftStatus, TimeLogStatus } from "./enum"

export const ShiftStatusLabels: Record<ShiftStatus, string> = {
    [ShiftStatus.Draft]: "Nháp",
    [ShiftStatus.Published]: "Đã ban hành",
    [ShiftStatus.Completed]: "Hoàn thành",
    [ShiftStatus.Cancelled]: "Huỷ"
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
