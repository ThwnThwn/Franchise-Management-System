import { Clock, AlertCircle, CheckCircle2 } from "lucide-react"
import { useDay } from "@/hooks"
import { Chip, Tooltip } from "@heroui/react"

interface Props {
    checkInAt?: string
    checkOutAt?: string
    regularMinutes?: number
    dailyOtMinutes?: number
    weeklyOtMinutes?: number
}

export const TimeRangeDisplay = ({
    checkInAt,
    checkOutAt,
    regularMinutes,
    dailyOtMinutes,
    weeklyOtMinutes
}: Props) => {
    const { formatDateTime } = useDay({ defaultFormat: "HH:mm" })

    // Tính tổng thời gian nếu có bất kỳ dữ liệu phút nào
    const hasAnyMinutes =
        regularMinutes !== undefined ||
        dailyOtMinutes !== undefined ||
        weeklyOtMinutes !== undefined

    const totalWorkMinutes = (regularMinutes || 0) + (dailyOtMinutes || 0) + (weeklyOtMinutes || 0)
    const hasOt = (dailyOtMinutes || 0) > 0 || (weeklyOtMinutes || 0) > 0

    return (
        <div className="flex flex-col gap-1.5">
            {/* Hiển thị giờ Check-in / Check-out */}
            <div className="flex items-center gap-1.5 font-bold tabular-nums">
                <span>{formatDateTime({ date: checkInAt }) || "--:--"}</span>
                <span className="text-default-400">-</span>
                <span>{formatDateTime({ date: checkOutAt }) || "--:--"}</span>
            </div>

            {/* Hiển thị chi tiết phút làm việc */}
            {hasAnyMinutes && (
                <div className="flex flex-wrap items-center gap-1.5 mt-0.5">
                    {/* Tổng thời gian */}
                    <div className="flex items-center gap-1 text-[11px] text-default-500 font-medium bg-default-100 px-2 py-0.5 rounded-full">
                        <Clock size={12} />
                        <span>Tổng: {totalWorkMinutes}p</span>
                    </div>

                    {/* Thời gian Tiêu chuẩn (Regular) */}
                    <div className="flex items-center gap-1 text-[11px] text-success-600 font-medium bg-success-50 px-2 py-0.5 rounded-full border border-success-100">
                        <CheckCircle2 size={12} />
                        <span>Chuẩn: {regularMinutes || 0}p</span>
                    </div>

                    {/* Hiển thị Badge OT nếu có phát sinh OT */}
                    {hasOt && (
                        <Tooltip
                            content={
                                <div className="px-1 py-1">
                                    <div className="text-tiny font-bold mb-1">Chi tiết Tăng ca</div>
                                    <div className="text-tiny">
                                        Tăng ca ngày: {dailyOtMinutes || 0}p
                                    </div>
                                    <div className="text-tiny">
                                        Tăng ca tuần: {weeklyOtMinutes || 0}p
                                    </div>
                                </div>
                            }
                        >
                            <Chip
                                size="sm"
                                color="warning"
                                variant="flat"
                                startContent={<AlertCircle size={12} className="ml-1" />}
                                className="h-5 text-[10px] cursor-help px-1"
                            >
                                OT: {(dailyOtMinutes || 0) + (weeklyOtMinutes || 0)}p
                            </Chip>
                        </Tooltip>
                    )}
                </div>
            )}
        </div>
    )
}
