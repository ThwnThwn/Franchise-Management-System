import { ShiftStatus } from "./enum"

export const ShiftStatusStyles: Record<ShiftStatus, string> = {
    [ShiftStatus.Draft]: "bg-slate-100 border-slate-400 text-slate-500 border-dashed border-1",
    [ShiftStatus.Published]: "bg-blue-50 border-blue-400 text-blue-800", // Màu mặc định khi chưa full
    [ShiftStatus.Completed]: "bg-purple-50 border-purple-500 text-purple-800",
    [ShiftStatus.Cancelled]: "bg-red-50 border-red-400 text-red-700 opacity-80"
}

// Style đặc biệt cho trường hợp đã Full (thường chỉ áp dụng cho Published)
export const ShiftFullStyle = "bg-green-100 border-green-500 text-green-800"
