import { ChipProps } from "@heroui/react"
import { ShiftApplicationStatus, ShiftStatus, ShiftSwapStatus, TimeLogStatus } from "./enum"

export const ShiftStatusStyles: Record<ShiftStatus, string> = {
    [ShiftStatus.Draft]: "bg-slate-100 border-slate-400 text-slate-500 border-dashed border-1",
    [ShiftStatus.Published]: "bg-blue-50 border-blue-400 text-blue-800", // Màu mặc định khi chưa full
    [ShiftStatus.Completed]: "bg-purple-50 border-purple-500 text-purple-800",
    [ShiftStatus.Cancelled]: "bg-red-50 border-red-400 text-red-700 opacity-80",
    [ShiftStatus.Open]: "bg-cyan-50 border-cyan-500 text-cyan-800"
}

export const TimeLogStatusStyles: Record<TimeLogStatus, string> = {
    // Đang chờ: Dùng màu Slate/Xám, nét đứt (vì chưa xong)
    [TimeLogStatus.Processing]:
        "bg-slate-50 border-slate-300 text-slate-500 border-dashed border-1",

    // Hợp lệ: Màu Emerald (Xanh lá sạch), nét liền
    [TimeLogStatus.Valid]: "bg-emerald-50 border-emerald-400 text-emerald-700 border-1",

    // Không hợp lệ: Màu Rose (Đỏ dịu), nét liền
    [TimeLogStatus.Invalid]: "bg-rose-50 border-rose-400 text-rose-700 border-1"
}

export const ShiftApplicationStatusColors: Record<ShiftApplicationStatus, ChipProps["color"]> = {
    [ShiftApplicationStatus.Pending]: "default",
    [ShiftApplicationStatus.Approved]: "success",
    [ShiftApplicationStatus.Rejected]: "danger"
}

export const ShiftApplicationStatusStyles: Record<ShiftApplicationStatus, string> = {
    [ShiftApplicationStatus.Pending]:
        "bg-default-100 border-default-400 text-default-700 border-dashed border-1",
    [ShiftApplicationStatus.Approved]:
        "bg-success-100 border-success-500 text-success-800 border-1",
    [ShiftApplicationStatus.Rejected]: "bg-danger-100 border-danger-500 text-danger-800 border-1"
}

export const ShiftSwapStatusColors: Record<ShiftSwapStatus, ChipProps["color"]> = {
    [ShiftSwapStatus.PendingTarget]: "secondary",
    [ShiftSwapStatus.PendingManager]: "warning",
    [ShiftSwapStatus.Approved]: "success",
    [ShiftSwapStatus.Rejected]: "danger",
    [ShiftSwapStatus.Expired]: "default",
    [ShiftSwapStatus.Cancelled]: "default"
}

export const ShiftSwapStatusStyles: Record<ShiftSwapStatus, string> = {
    [ShiftSwapStatus.PendingTarget]: "bg-indigo-50 border-indigo-400 text-indigo-700",
    [ShiftSwapStatus.PendingManager]: "bg-orange-50 border-orange-400 text-orange-800",
    [ShiftSwapStatus.Approved]: "bg-green-100 border-green-600 text-green-800",
    [ShiftSwapStatus.Rejected]: "bg-red-100 border-red-500 text-red-800",
    [ShiftSwapStatus.Expired]: "bg-slate-200 border-slate-400 text-slate-600",
    [ShiftSwapStatus.Cancelled]: "bg-gray-100 border-gray-400 text-gray-500"
}

// Style đặc biệt cho trường hợp đã Full (thường chỉ áp dụng cho Published)
export const ShiftFullStyle = "bg-green-100 border-green-500 text-green-800"

export const StaffAgendaStyles = {
    upcoming: "bg-blue-50 border-blue-400 text-blue-800", // Ca sắp tới (giống Published)
    ongoing: "bg-emerald-50 border-emerald-500 text-emerald-800", // Đang làm (Xanh lá nhạt)
    completed: "bg-slate-50 border-slate-300 text-slate-400", // Đã xong (Xám nhạt, ít nổi bật)
    overtime: "bg-orange-50 border-orange-400 text-orange-800" // Làm quá giờ (nếu cần)
}
