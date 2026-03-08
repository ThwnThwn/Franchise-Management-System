"use client"

import { useMemo } from "react"
import { Card, CardBody, Chip, Divider } from "@heroui/react"
import { Clock, MapPin, Users } from "lucide-react"
import dayjs from "dayjs"
import { ShiftResponse } from "@/features/shifts/models/response"
import { formatCurrency } from "@/utils/dashboardMockData"
import { useGetMe } from "@/features/profile/hooks"
import { useDay } from "@/hooks"

interface ShiftPoolListViewProps {
    shifts: ShiftResponse[]
    selectedDate: dayjs.Dayjs
    onCardClick: (shift: ShiftResponse) => void
    // onApplyClick: (shiftId: string) => void
    // isApplyingId?: string | null
}

export const ShiftPoolListView = ({
    shifts,
    selectedDate,
    onCardClick
    // onApplyClick,
    // isApplyingId
}: ShiftPoolListViewProps) => {
    const { data: me } = useGetMe()
    const { getDiffMinutesCeil } = useDay()

    // Chỉ lấy ca của ngày hiện tại
    const dailyShifts = useMemo(() => {
        return shifts.filter((s) => dayjs(s.shiftDate).isSame(selectedDate, "day"))
    }, [shifts, selectedDate])

    if (dailyShifts.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 px-4 text-center">
                <div className="w-20 h-20 bg-default-100 rounded-full flex items-center justify-center mb-4 text-default-400">
                    <Clock size={40} />
                </div>
                <h3 className="text-lg font-bold text-default-700">Chưa có ca mở!</h3>
                <p className="text-sm text-default-500 max-w-62.5 mt-1">
                    Hiện tại không có ca làm việc nào đang cần người vào ngày{" "}
                    {selectedDate.format("DD/MM/YYYY")}.
                </p>
            </div>
        )
    }

    return (
        <div className="flex flex-col gap-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {dailyShifts.map((shift) => {
                const { shiftDate, startTime, endTime, breakDurationMinutes } = shift

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
                                    <div>
                                        <h3 className="text-2xl font-black tabular-nums text-default-900">
                                            {shift.startTime.substring(0, 5)} -{" "}
                                            {shift.endTime.substring(0, 5)}
                                        </h3>
                                        <div className="flex items-center gap-1.5 mt-1 text-default-500 font-medium">
                                            <MapPin size={14} />
                                            <span className="text-sm">Tại chi nhánh hiện tại</span>
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
