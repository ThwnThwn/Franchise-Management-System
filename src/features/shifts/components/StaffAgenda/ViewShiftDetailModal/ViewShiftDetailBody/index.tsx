// features/shifts/components/ShiftDetailBody.tsx
"use client"

import { Divider, Button, Avatar } from "@heroui/react"
import { MapPin, Navigation, Phone, Clock, Users, Coffee } from "lucide-react"
import { StaffShiftAgendaWithLog } from "@/features/shifts/models/response"
import { useUserHelper } from "@/hooks"
import { formatPhoneNumber } from "@/utils/helpers/stringHelper"

interface ShiftDetailBodyProps {
    data: StaffShiftAgendaWithLog
}

export const ViewShiftDetailBody = ({ data }: ShiftDetailBodyProps) => {
    const { toFullName } = useUserHelper()

    const breakDurationMinutes = data.shiftInfo?.breakDurationMinutes ?? 0

    const handleNavigation = () => {
        const url = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(data.store.address)}`
        window.open(url, "_blank")
    }

    return (
        <>
            <div className="space-y-4">
                <div className="flex items-start gap-4">
                    <div className="p-3 bg-default-100 rounded-2xl">
                        <MapPin className="text-primary" size={24} />
                    </div>
                    <div className="flex-1">
                        <h4 className="text-lg font-extrabold">{data.store.name}</h4>
                        <p className="text-sm text-default-500 leading-snug">
                            {data.store.address}
                        </p>
                        <Button
                            size="sm"
                            variant="light"
                            color="primary"
                            startContent={<Navigation size={14} />}
                            className="px-0 h-auto mt-1 font-bold"
                            onPress={handleNavigation}
                        >
                            Chỉ đường qua Maps
                        </Button>
                    </div>
                </div>

                {data.store.phone && (
                    <div className="flex items-center gap-4">
                        <div className="p-3 bg-default-100 rounded-2xl">
                            <Phone className="text-default-600" size={20} />
                        </div>
                        <span className="font-bold">{formatPhoneNumber(data.store.phone)}</span>
                    </div>
                )}
            </div>

            <Divider className="my-2" />

            {/* Phần 2: Thời gian */}
            <div className="flex items-center gap-4">
                <div className="p-3 bg-default-100 rounded-2xl">
                    <Clock className="text-default-600" size={20} />
                </div>
                <div>
                    <p className="text-sm text-default-400 font-bold uppercase">Thời gian ca làm</p>
                    <p className="text-xl font-black tabular-nums">
                        {data.startTime.substring(0, 5)} - {data.endTime.substring(0, 5)}
                    </p>

                    {breakDurationMinutes > 0 && (
                        <div className="flex items-center gap-1.5 mt-1 text-xs text-warning-600 font-medium bg-warning-50 w-fit px-2 py-0.5 rounded-full border border-warning-100">
                            <Coffee size={12} />
                            <span>Nghỉ giữa ca: {breakDurationMinutes} phút</span>
                        </div>
                    )}
                </div>
            </div>

            <Divider className="my-2" />

            {/* Phần 3: Đồng nghiệp */}
            <div className="space-y-3">
                <div className="flex items-center gap-2 text-default-400">
                    <Users size={18} />
                    <span className="text-xs font-black uppercase tracking-wider">
                        Làm cùng đồng nghiệp ({data.coWorkers.length})
                    </span>
                </div>

                <div className="grid grid-cols-1 gap-2">
                    {data.coWorkers.length > 0 ? (
                        data.coWorkers.map((worker) => (
                            <div
                                key={worker.id}
                                className="flex items-center gap-3 p-2 rounded-xl bg-default-50"
                            >
                                <Avatar src={worker.avatar} name={worker.firstName} size="sm" />
                                <div className="flex flex-col">
                                    <span className="text-sm font-bold">
                                        {toFullName({
                                            firstName: worker.firstName,
                                            lastName: worker.lastName
                                        })}
                                    </span>
                                    {worker.phone && (
                                        <span className="text-[10px] text-default-400">
                                            {worker.phone}
                                        </span>
                                    )}
                                </div>
                            </div>
                        ))
                    ) : (
                        <p className="text-sm italic text-default-400">
                            Không có đồng nghiệp nào trong ca này
                        </p>
                    )}
                </div>
            </div>
        </>
    )
}
