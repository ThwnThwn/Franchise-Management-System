// features/shifts/components/TimeLogSharedInfo.tsx
"use client"

import { Image } from "@heroui/react"
import { Clock, Info, Timer } from "lucide-react"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { StaffInfoCell } from "@/features/shifts/components"
import { useDay } from "@/hooks"

interface Props {
    data: TimeLogResponse
}

export const TimeLogSharedInfo = ({ data }: Props) => {
    const { formatDateTime, formatTime } = useDay()

    // Kiểm tra xem đã có dữ liệu tính toán phút chưa (Valid/Invalid)
    const hasCalculatedMinutes =
        data.regularMinutes !== undefined ||
        data.dailyOtMinutes !== undefined ||
        data.weeklyOtMinutes !== undefined

    // Tổng thời gian làm việc
    const totalMinutes =
        (data.regularMinutes || 0) + (data.dailyOtMinutes || 0) + (data.weeklyOtMinutes || 0)

    return (
        <div className="flex flex-col gap-6">
            {/* 1. Thông tin nhân viên */}
            <div className="flex justify-between items-start bg-default-50 p-4 rounded-2xl border border-divider">
                <StaffInfoCell staff={data.staff} />

                {/* HIỂN THỊ CA LÀM GỐC */}
                <div className="text-right flex flex-col items-end gap-1">
                    {/* <span className="text-[10px] font-bold text-default-400 uppercase tracking-widest block">
                                            Ca làm việc
                                        </span> */}
                    <div className="flex items-center gap-2 bg-primary-50 px-3 py-1 rounded-full border border-primary-100">
                        <Clock size={14} className="text-primary" />
                        <span className="text-sm font-black text-primary tabular-nums">
                            {formatTime(data.shift.startTime, false)} -{" "}
                            {formatTime(data.shift.endTime, false)}
                        </span>
                    </div>
                    <span className="px-3 text-[10px] font-medium text-default-500 italic max-w-50 truncate">
                        {data.shift.note || "Không có ghi chú ca"}
                    </span>
                </div>
            </div>

            {/* 2. Đối chiếu thời gian thực tế */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Check-in thực tế */}
                <div className="p-4 rounded-2xl bg-success-50 border border-success-100 relative overflow-hidden">
                    <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2 text-success-600 font-bold text-xs uppercase">
                            <Clock size={14} /> Check-in Thực Tế
                        </div>
                        <span className="text-[9px] font-bold bg-success-200 text-success-700 px-1.5 py-0.5 rounded uppercase">
                            Dự kiến: {formatTime(data.shift.startTime, false)}
                        </span>
                    </div>
                    <p className="text-2xl font-black tabular-nums text-success-700">
                        {formatDateTime({ date: data.checkInAt, format: "HH:mm" })}
                    </p>
                    <span className="text-tiny text-success-500 font-medium">
                        {formatDateTime({ date: data.checkInAt, format: "DD/MM/YYYY" })}
                    </span>
                </div>

                {/* Check-out thực tế */}
                <div className="p-4 rounded-2xl bg-warning-50 border border-warning-100 relative overflow-hidden">
                    <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2 text-warning-600 font-bold text-xs uppercase">
                            <Clock size={14} /> Check-out Thực Tế
                        </div>
                        <span className="text-[9px] font-bold bg-warning-200 text-warning-700 px-1.5 py-0.5 rounded uppercase">
                            Dự kiến: {formatTime(data.shift.endTime, false)}
                        </span>
                    </div>
                    <p className="text-2xl font-black tabular-nums text-warning-700">
                        {data.checkOutAt
                            ? formatDateTime({ date: data.checkOutAt, format: "HH:mm" })
                            : "--:--"}
                    </p>
                    <span className="text-tiny text-warning-500 font-medium">
                        {data.checkOutAt
                            ? formatDateTime({
                                  date: data.checkOutAt,
                                  format: "DD/MM/YYYY"
                              })
                            : "Nhân viên chưa bấm Tan ca"}
                    </span>
                </div>
            </div>

            {hasCalculatedMinutes && (
                <div className="p-4 rounded-2xl bg-default-50 border border-divider">
                    <div className="flex items-center gap-2 text-default-600 font-bold text-xs uppercase mb-4">
                        <Timer size={14} /> Thời gian làm việc
                    </div>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        {/* Cột Tổng */}
                        <div className="flex flex-col border-r border-divider">
                            <span className="text-[10px] text-default-500 uppercase font-semibold">
                                Tổng thời gian
                            </span>
                            <span className="text-xl font-black tabular-nums">{totalMinutes}p</span>
                        </div>
                        {/* Cột Tiêu chuẩn */}
                        <div className="flex flex-col md:border-r border-divider">
                            <span className="text-[10px] text-default-500 uppercase font-semibold">
                                Tiêu chuẩn
                            </span>
                            <span className="text-lg font-bold tabular-nums text-success-600">
                                {data.regularMinutes || 0}p
                            </span>
                        </div>
                        {/* Cột OT Ngày */}
                        <div className="flex flex-col border-r border-divider">
                            <span className="text-[10px] text-default-500 uppercase font-semibold">
                                OT Ngày
                            </span>
                            <span className="text-lg font-bold tabular-nums text-warning-600">
                                {data.dailyOtMinutes || 0}p
                            </span>
                        </div>
                        {/* Cột OT Tuần */}
                        <div className="flex flex-col">
                            <span className="text-[10px] text-default-500 uppercase font-semibold">
                                OT Tuần
                            </span>
                            <span className="text-lg font-bold tabular-nums text-danger-600">
                                {data.weeklyOtMinutes || 0}p
                            </span>
                        </div>
                    </div>
                </div>
            )}

            {/* 3. Bằng chứng hình ảnh (Nếu có) */}
            {(data.checkInImageUrl || data.checkOutImageUrl) && (
                <div className="space-y-3">
                    <span className="text-xs font-bold text-default-400 uppercase flex items-center gap-1">
                        <Info size={14} /> Bằng chứng hình ảnh
                    </span>
                    <div className="flex gap-4">
                        {data.checkInImageUrl && (
                            <Image
                                src={data.checkInImageUrl}
                                alt="Check-in"
                                className="w-32 h-32 object-cover rounded-xl border-2 border-white shadow-sm"
                            />
                        )}
                        {data.checkOutImageUrl && (
                            <Image
                                src={data.checkOutImageUrl}
                                alt="Check-out"
                                className="w-32 h-32 object-cover rounded-xl border-2 border-white shadow-sm"
                            />
                        )}
                    </div>
                </div>
            )}
        </div>
    )
}
