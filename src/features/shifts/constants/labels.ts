import { ShiftStatus } from "./enum"

export const ShiftStatusLabels: Record<ShiftStatus, string> = {
    [ShiftStatus.Draft]: "Nháp",
    [ShiftStatus.Published]: "Đã ban hành",
    [ShiftStatus.Completed]: "Hoàn thành",
    [ShiftStatus.Cancelled]: "Huỷ"
}
