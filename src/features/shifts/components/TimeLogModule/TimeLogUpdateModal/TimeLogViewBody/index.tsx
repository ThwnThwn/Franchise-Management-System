"use client"

import { Divider } from "@heroui/react"
import { TimeLogResponse } from "@/features/shifts/models/response"
import { TimeLogStatus } from "@/features/shifts/constants"
import { TimeLogStatusLabels } from "@/features/shifts/constants/labels"
import { TimeLogSharedInfo } from "../TimeLogSharedInfo"

interface TimeLogViewBodyProps {
    data: TimeLogResponse
}

export const TimeLogViewBody = ({ data }: TimeLogViewBodyProps) => {
    return (
        <div className="flex flex-col gap-6">
            <TimeLogSharedInfo data={data} />

            <Divider className="my-2" />

            <div className="bg-default-50 p-4 rounded-xl border border-dashed border-default-200">
                <p className="text-xs font-bold text-default-400 uppercase mb-3">
                    Kết quả phê duyệt
                </p>
                <div className="flex justify-start items-center gap-2 mb-4">
                    <span className="text-sm font-medium">Trạng thái hiện tại:</span>
                    <span
                        className={`text-sm font-black uppercase ${
                            data.status === TimeLogStatus.Valid
                                ? "text-success"
                                : data.status === TimeLogStatus.Invalid
                                  ? "text-danger"
                                  : "text-warning"
                        }`}
                    >
                        {TimeLogStatusLabels[data.status]}
                    </span>
                </div>
                {data.managerNote ? (
                    <div className="p-3 bg-white rounded-lg border shadow-sm text-sm italic text-default-600 relative">
                        <span className="absolute -top-2 left-2 bg-white px-1 text-[10px] font-bold text-primary uppercase">
                            Lời nhắn từ Quản lý
                        </span>
                        {data.managerNote}
                    </div>
                ) : (
                    <p className="text-xs italic text-default-400 text-center mt-2">
                        (Không có ghi chú nào từ quản lý)
                    </p>
                )}
            </div>
        </div>
    )
}
