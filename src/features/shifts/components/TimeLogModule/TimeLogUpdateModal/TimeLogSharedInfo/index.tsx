// features/shifts/components/TimeLogSharedInfo.tsx
"use client"

import { Image } from "@heroui/react"
import { Clock, Info } from "lucide-react"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { StaffInfoCell } from "@/features/shifts/components"
import { useDay } from "@/hooks"

interface Props {
    data: TimeLogResponse
}

export const TimeLogSharedInfo = ({ data }: Props) => {
    const { formatDateTime, formatTime } = useDay()

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
                <div className="p-4 rounded-2xl bg-secondary-50 border border-secondary-100 relative overflow-hidden">
                    <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2 text-secondary-600 font-bold text-xs uppercase">
                            <Clock size={14} /> Check-out Thực Tế
                        </div>
                        <span className="text-[9px] font-bold bg-secondary-200 text-secondary-700 px-1.5 py-0.5 rounded uppercase">
                            Dự kiến: {formatTime(data.shift.endTime, false)}
                        </span>
                    </div>
                    <p className="text-2xl font-black tabular-nums text-secondary-700">
                        {data.checkOutAt
                            ? formatDateTime({ date: data.checkOutAt, format: "HH:mm" })
                            : "--:--"}
                    </p>
                    <span className="text-tiny text-secondary-500 font-medium">
                        {data.checkOutAt
                            ? formatDateTime({
                                  date: data.checkOutAt,
                                  format: "DD/MM/YYYY"
                              })
                            : "Nhân viên chưa bấm Tan ca"}
                    </span>
                </div>
            </div>

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
