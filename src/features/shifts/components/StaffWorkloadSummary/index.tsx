"use client"

import React from "react"
import { Avatar, Progress, Accordion, AccordionItem, Chip } from "@heroui/react"
import { AlertTriangle, Users, ChevronDown } from "lucide-react"

import { useStaffWorkload } from "@/features/shifts/hooks"
import { ShiftResponse } from "@/features/shifts/models/response"

interface StaffWorkloadSummaryProps {
    shifts: ShiftResponse[]
    storeId: string
}
export const StaffWorkloadSummary = ({ shifts, storeId }: StaffWorkloadSummaryProps) => {
    // Gọi hook và lấy dữ liệu đã được "nấu" chín
    const { staffStats, overworkCount, maxHours } = useStaffWorkload(shifts, storeId)

    // Render Tiêu đề Accordion (Sử dụng overworkCount và maxHours từ hook)
    const AccordionTitle = (
        <div className="flex items-center justify-between w-full pr-4">
            <div className="flex items-center gap-3">
                <div className="p-2 bg-primary/10 rounded-lg text-primary">
                    <Users size={20} />
                </div>
                <div className="flex flex-col text-left">
                    <span className="text-sm font-bold text-gray-700">
                        Phân bổ nhân sự tuần này
                    </span>
                    <span className="text-tiny text-default-400">
                        {staffStats.length} nhân viên đang gán ca
                    </span>
                </div>
            </div>

            <div className="flex items-center gap-2">
                {overworkCount > 0 && (
                    <Chip
                        variant="flat"
                        color="danger"
                        size="sm"
                        startContent={<AlertTriangle size={12} />}
                        className="animate-pulse"
                    >
                        {overworkCount} người quá tải
                    </Chip>
                )}
                <Chip size="sm" variant="dot" color="primary">
                    Hạn mức {maxHours}h
                </Chip>
            </div>
        </div>
    )

    return (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <Accordion variant="light" className="px-2">
                <AccordionItem
                    key="workload"
                    title={AccordionTitle}
                    indicator={<ChevronDown size={18} />}
                    className="px-2"
                >
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-6 pb-4 px-2">
                        {staffStats.map((staff) => {
                            const percentage = Math.min((staff.totalHours / maxHours) * 100, 100)

                            return (
                                <div key={staff.id} className="group flex flex-col gap-2">
                                    <div className="flex justify-between items-end">
                                        <div className="flex items-center gap-2">
                                            <Avatar
                                                src={staff.avatar}
                                                name={staff.name}
                                                size="sm"
                                                className="w-7 h-7 text-tiny"
                                            />
                                            <span className="text-xs font-bold text-gray-600 truncate max-w-25">
                                                {staff.name}
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-1">
                                            <span
                                                className={`text-xs font-black ${staff.isOverworked ? "text-danger" : "text-primary"}`}
                                            >
                                                {staff.totalHours.toFixed(1)}h
                                            </span>
                                            {staff.isOverworked && (
                                                <AlertTriangle size={12} className="text-danger" />
                                            )}
                                        </div>
                                    </div>
                                    <Progress
                                        value={percentage}
                                        color={
                                            staff.isOverworked
                                                ? "danger"
                                                : percentage > 85
                                                  ? "warning"
                                                  : "primary"
                                        }
                                        size="sm"
                                        radius="full"
                                        className="shadow-inner"
                                    />
                                </div>
                            )
                        })}
                    </div>
                </AccordionItem>
            </Accordion>
        </div>
    )
}
