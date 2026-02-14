"use client"

import React, { useMemo } from "react"
import { Card, CardBody, Avatar, AvatarGroup, Tooltip, Chip } from "@heroui/react"
import { MapPin, Clock, Moon, CalendarOff } from "lucide-react"
import dayjs from "dayjs"
import { StaffShiftAgenda } from "@/features/shifts/models/response"
import { useUserHelper } from "@/hooks"

interface AgendaListViewProps {
    shifts: StaffShiftAgenda[]
    selectedDate: dayjs.Dayjs // Thêm prop này để lọc theo ngày của Toolbar
}

export const AgendaListView = ({ shifts, selectedDate }: AgendaListViewProps) => {
    const { toFullName } = useUserHelper()

    // 1. Filter: Chỉ lấy những ca thuộc ngày đang chọn
    const dailyShifts = useMemo(() => {
        return shifts.filter((s) => dayjs(s.date).isSame(selectedDate, "day"))
    }, [shifts, selectedDate])

    // 2. Empty State: Nếu ngày đó không có ca làm
    if (dailyShifts.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 px-4 text-center">
                <div className="w-20 h-20 bg-default-100 rounded-full flex items-center justify-center mb-4 text-default-400">
                    <CalendarOff size={40} />
                </div>
                <h3 className="text-lg font-bold text-default-700">Không có ca làm!</h3>
                <p className="text-sm text-default-500 max-w-62.5 mt-1">
                    Không có ca làm việc nào được xếp cho ngày {selectedDate.format("DD/MM/YYYY")}.
                </p>
            </div>
        )
    }

    return (
        <div className="flex flex-col gap-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* Header ngày */}
            {/* <div className="flex items-center gap-3 mb-2">
                <div className="h-10 w-1.5 bg-primary rounded-full" />
                <div>
                    <h2 className="text-xl font-black capitalize">{selectedDate.format("dddd")}</h2>
                    <p className="text-xs font-bold text-default-400 uppercase tracking-widest">
                        Ngày {selectedDate.format("DD [Tháng] MM, YYYY")}
                    </p>
                </div>
            </div> */}

            {/* Danh sách ca làm trong ngày */}
            {dailyShifts.map((item) => (
                <Card key={item.id} className="border-none shadow-sm interactive-scale">
                    <CardBody className="p-5">
                        <div className="flex flex-col gap-4">
                            <div className="flex justify-between items-start">
                                <div className="flex items-center gap-3">
                                    <div className="p-3 bg-white rounded-2xl shadow-xs text-primary">
                                        {item.endTime < item.startTime ? (
                                            <Moon size={24} />
                                        ) : (
                                            <Clock size={24} />
                                        )}
                                    </div>
                                    <div>
                                        <h3 className="text-2xl font-black tabular-nums">
                                            {item.startTime.substring(0, 5)} -{" "}
                                            {item.endTime.substring(0, 5)}
                                        </h3>
                                        <div className="flex items-center gap-1 text-default-500 font-medium">
                                            <MapPin size={14} />
                                            <span className="text-sm">{item.store.name}</span>
                                        </div>
                                    </div>
                                </div>
                                {item.checkInAt && (
                                    <Chip
                                        size="sm"
                                        color="success"
                                        variant="flat"
                                        className="font-bold"
                                    >
                                        Đang làm
                                    </Chip>
                                )}
                            </div>

                            <div className="flex items-center justify-between pt-4 border-t border-divider">
                                <div className="flex flex-col gap-1">
                                    <span className="text-[10px] uppercase font-bold text-default-400 tracking-tighter">
                                        Làm cùng đồng nghiệp
                                    </span>
                                    <AvatarGroup
                                        isBordered
                                        size="sm"
                                        max={4}
                                        total={
                                            item.coWorkers.length > 4
                                                ? item.coWorkers.length - 4
                                                : 0
                                        }
                                    >
                                        {item.coWorkers.map((w) => (
                                            <Tooltip
                                                key={w.id}
                                                content={toFullName({
                                                    firstName: w.firstName,
                                                    lastName: w.lastName
                                                })}
                                            >
                                                <Avatar src={w.avatar} name={w.firstName} />
                                            </Tooltip>
                                        ))}
                                    </AvatarGroup>
                                </div>

                                <p className="text-[10px] text-default-400 italic max-w-30 text-right">
                                    {item.store.address}
                                </p>
                            </div>
                        </div>
                    </CardBody>
                </Card>
            ))}
        </div>
    )
}
