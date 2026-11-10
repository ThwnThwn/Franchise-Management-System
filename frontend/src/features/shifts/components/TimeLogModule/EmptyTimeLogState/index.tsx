import React from "react"
import { CalendarOff } from "lucide-react"

export const EmptyTimeLogState = ({
    message = "Không có nhật ký chấm công nào!"
}: {
    message?: string
}) => (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center bg-default-50 rounded-3xl border-2 border-dashed border-divider">
        <div className="w-16 h-16 bg-default-100 rounded-full flex items-center justify-center mb-4 text-default-400">
            <CalendarOff size={32} />
        </div>
        <h3 className="text-md font-bold text-default-700">{message}</h3>
        <p className="text-xs text-default-400 mt-1 max-w-60">
            Vui lòng điều chỉnh bộ lọc hoặc chọn khoảng ngày khác để xem kết quả.
        </p>
    </div>
)
