"use client"

import React, { useMemo } from "react"
import { CardBody } from "@heroui/react"
import { Clock, DollarSign, Users, AlertTriangle } from "lucide-react"
import dayjs from "dayjs"
import { ShiftResponse } from "@/features/shifts/models/response"
import { ShiftStatus } from "@/features/shifts/constants"
import { CardStyled } from "@/components"
import { formatCurrency, formatCurrencyWithSymbol } from "@/utils/helpers/currency"

interface ScheduleStatsProps {
    shifts: ShiftResponse[]
}

export const ScheduleStats = ({ shifts }: ScheduleStatsProps) => {
    const stats = useMemo(() => {
        let totalMinutes = 0
        let totalBudget = 0
        let totalAssigned = 0
        let totalRequired = 0
        let emptyShifts = 0

        shifts.forEach((shift) => {
            if (shift.status === ShiftStatus.Cancelled) return

            // 1. Thời lượng ca (giờ)
            const durationInMinutes = dayjs(shift.endTime).diff(dayjs(shift.startTime), "minute")
            const durationInHours = durationInMinutes / 60
            totalMinutes += durationInMinutes

            // 2. Tính quỹ lương thực tế (Không dùng default)
            // Giả sử shift.assignedStaffs là mảng các object nhân viên có field hourlyWage
            if (shift.assignments && shift.assignments.length > 0) {
                const shiftBudget = shift.assignments.reduce((sum, staff) => {
                    return sum + durationInHours * (staff.hourlyWage || 0)
                }, 0)
                totalBudget += shiftBudget
            }

            // 3. Tỉ lệ lấp đầy
            totalAssigned += shift.assignedStaffCount
            totalRequired += shift.requiredStaffCount

            // 4. Cảnh báo trống (Chỉ tính ca đã công khai)
            if (
                shift.status === ShiftStatus.Published &&
                shift.assignedStaffCount < shift.requiredStaffCount
            ) {
                emptyShifts++
            }
        })

        const fillRate = totalRequired > 0 ? Math.round((totalAssigned / totalRequired) * 100) : 0

        return {
            totalHours: Math.round(totalMinutes / 60),
            totalBudget,
            fillRate,
            emptyShifts
        }
    }, [shifts])

    const items = [
        {
            label: "Tổng giờ công",
            value: `${formatCurrency(stats.totalHours)}h`,
            icon: <Clock className="text-primary" size={20} />,
            color: "bg-primary/10"
        },
        {
            label: "Quỹ lương dự kiến",
            value: `${formatCurrencyWithSymbol(stats.totalBudget)}`,
            icon: <DollarSign className="text-success" size={20} />,
            color: "bg-success/10"
        },
        {
            label: "Tỷ lệ lấp đầy",
            value: `${stats.fillRate}%`,
            icon: <Users className="text-secondary" size={20} />,
            color: "bg-secondary/10"
        },
        {
            label: "Ca chưa đủ người",
            value: stats.emptyShifts,
            icon: (
                <AlertTriangle
                    className={stats.emptyShifts > 0 ? "text-danger" : "text-default-400"}
                    size={20}
                />
            ),
            color: stats.emptyShifts > 0 ? "bg-danger/10" : "bg-default-100",
            textColor: stats.emptyShifts > 0 ? "text-danger" : ""
        }
    ]

    return (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {items.map((item, idx) => (
                <CardStyled
                    key={idx}
                    shadow="sm"
                    className="border-none bg-white/60 backdrop-blur-md"
                >
                    <CardBody className="flex flex-row items-center gap-4 p-4">
                        <div className={`p-2.5 rounded-xl ${item.color}`}>{item.icon}</div>
                        <div className="flex flex-col">
                            <span className="text-tiny text-default-500 font-medium uppercase tracking-wider">
                                {item.label}
                            </span>
                            <span
                                className={`text-lg font-bold ${item.textColor || "text-default-700"}`}
                            >
                                {item.value}
                            </span>
                        </div>
                    </CardBody>
                </CardStyled>
            ))}
        </div>
    )
}
