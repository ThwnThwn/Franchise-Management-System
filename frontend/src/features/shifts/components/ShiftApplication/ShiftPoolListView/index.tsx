"use client"

import { useMemo } from "react"
import { Card, CardBody, Chip, Divider } from "@heroui/react"
import { Clock, Users } from "lucide-react"
import dayjs from "dayjs"
import { ShiftResponse } from "@/features/shifts/models/response"
import { formatCurrency } from "@/utils/dashboardMockData"
import { useGetMe } from "@/features/profile/hooks"
import { useDay } from "@/hooks"
import { AgendaViewMode } from "@/features/shifts/hooks"
import { DayOfWeekLabel } from "@/constants/labels"
import { DayOfWeek } from "@/constants/enums"

interface ShiftPoolListViewProps {
    shifts: ShiftResponse[]

    viewMode: AgendaViewMode
    selectedDate: dayjs.Dayjs
    dateRange: {
        startDate: string
        endDate: string
    }
    onCardClick: (shift: ShiftResponse) => void
}

export const ShiftPoolListView = ({
    shifts,
    viewMode,
    selectedDate,
    dateRange,
    onCardClick
}: ShiftPoolListViewProps) => {
    const { data: me } = useGetMe()
    const { getDiffMinutesCeil } = useDay()

    // Chỉ lấy ca của ngày hiện tại
    const dailyShifts = useMemo(() => {
        if (viewMode === "list") {
            return shifts.filter((s) => dayjs(s.shiftDate).isSame(selectedDate, "day"))
        } else {
            return shifts.filter(
                (s) =>
                    dayjs(s.shiftDate).isAfter(dateRange.startDate) &&
                    dayjs(s.shiftDate).isBefore(dateRange.endDate)
            )
        }
    }, [viewMode, shifts, selectedDate, dateRange.startDate, dateRange.endDate])

    if (dailyShifts.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 px-4 text-center">
                <div className="w-20 h-20 bg-default-100 rounded-full flex items-center justify-center mb-4 text-default-400">
                    <Clock size={40} />
                </div>
                <h3 className="text-lg font-bold text-default-700">Chưa có ca mở!</h3>
                <p className="text-sm text-default-500 max-w-62.5 mt-1">
                    Hiện tại không có ca làm việc nào đang cần người vào khoảng thời gian này.
                </p>
            </div>
        )
    }

    return (
        <div className="flex flex-col gap-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {dailyShifts.map((shift) => {
                const { shiftDate, startTime, endTime, breakDurationMinutes } = shift

                const dateObj = dayjs(shift.shiftDate)
                const dayLabel = DayOfWeekLabel[dateObj.day() as DayOfWeek] // Lấy "Thứ 2", "Chủ Nhật"...
                const fullDate = dateObj.format("DD/MM/YYYY")

                const totalShiftMinutes = getDiffMinutesCeil({
                    startTime,
                    endTime,
                    baseDate: shiftDate
                })

                const actualWorkedMinutes = totalShiftMinutes - (breakDurationMinutes || 0)

                // 3. Tính tiền: Lương giờ * (Số phút / 60)
                const estimatedWage =
                    me?.currentHourlyWage !== undefined && actualWorkedMinutes > 0
                        ? me.currentHourlyWage * (actualWorkedMinutes / 60)
                        : 0

                return (
                    <Card
                        key={shift.id}
                        isPressable
                        onPress={() => onCardClick(shift)} // Bấm vào Card để xem chi tiết
                        className="border-none shadow-sm hover:scale-[101%] transition-transform overflow-visible"
                    >
                        <CardBody className="p-5">
                            <div className="flex flex-col gap-3">
                                <div className="flex justify-between items-start">
                                    <div className="group flex gap-3 p-1">
                                        {/* Phần Ngày: Thứ + Ngày tháng */}
                                        <div className="flex items-center gap-2">
                                            <div className="flex flex-col items-center justify-center bg-primary-100 text-primary-700 rounded-lg w-12 h-12 shrink-0 border border-primary-200 shadow-sm">
                                                <span className="text-[10px] uppercase font-bold leading-none">
                                                    {dateObj.format("MMM")}
                                                </span>
                                                <span className="text-lg font-black leading-none">
                                                    {dateObj.format("DD")}
                                                </span>
                                            </div>

                                            <div className="flex flex-col">
                                                <h4 className="text-default-900 font-bold text-base leading-tight">
                                                    {dayLabel}
                                                </h4>
                                                <p className="text-default-400 text-xs font-medium">
                                                    {fullDate}
                                                </p>
                                            </div>
                                        </div>

                                        {/* Phần Giờ: Highlight cực mạnh */}
                                        <div className="flex items-center gap-3 bg-default-50 px-4 py-3 rounded-2xl border border-default-100 group-hover:border-primary-200 transition-colors">
                                            <Clock size={20} className="text-primary-500" />
                                            <h3 className="text-2xl font-black tabular-nums tracking-tight text-default-800 flex items-center gap-2">
                                                <span>{shift.startTime.substring(0, 5)}</span>
                                                <span className="text-default-300 font-extralight text-xl">
                                                    —
                                                </span>
                                                <span>{shift.endTime.substring(0, 5)}</span>
                                            </h3>
                                        </div>
                                    </div>
                                    {/* Mồi nhử - Tiền lương dự kiến */}
                                    {estimatedWage > 0 && (
                                        <Chip
                                            color="success"
                                            variant="flat"
                                            size="lg"
                                            className="font-black px-2"
                                        >
                                            + {formatCurrency(estimatedWage)}
                                        </Chip>
                                    )}
                                </div>

                                <Divider className="my-1 opacity-50" />

                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2 text-sm text-default-500">
                                        <Users size={16} />
                                        <span>
                                            Cần tuyển:{" "}
                                            <b className="text-primary">
                                                {shift.requiredStaffCount -
                                                    shift.assignedStaffCount}
                                            </b>{" "}
                                            người
                                        </span>
                                    </div>

                                    {/* NÚT APPLY NẰM TRONG CARD */}
                                    {/* <ButtonStyled
                                        color="primary"
                                        size="sm"
                                        className="font-bold shadow-md px-6"
                                        startContent={
                                            isApplyingId !== shift.id && <Send size={14} />
                                        }
                                        isLoading={isApplyingId === shift.id}
                                        onPress={(e) => {
                                            // Ngăn chặn sự kiện click bọt (tránh mở Modal chi tiết khi đang bấm Apply)
                                            if (e.continuePropagation) e.continuePropagation()
                                            onApplyClick(shift.id)
                                        }}
                                    >
                                        Nhận Ca
                                    </ButtonStyled> */}
                                </div>
                            </div>
                        </CardBody>
                    </Card>
                )
            })}
        </div>
    )
}
