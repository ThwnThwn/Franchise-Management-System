"use client"

import React from "react"
import { ModalContent, ModalHeader, ModalBody, Button, Avatar, Divider, Chip } from "@heroui/react"
import { MapPin, Phone, Users, Clock, Navigation } from "lucide-react"
import dayjs from "dayjs"
import { StaffShiftAgenda } from "@/features/shifts/models/response"
import { useUserHelper } from "@/hooks"
import { ModalStyled } from "@/components"

interface ViewShiftDetailModalProps {
    isOpen: boolean
    onClose: () => void
    data: StaffShiftAgenda | null
}

export const ViewShiftDetailModal = ({ isOpen, onClose, data }: ViewShiftDetailModalProps) => {
    const { toFullName } = useUserHelper()
    if (!data) return null

    // Hàm mở Google Maps nhanh
    const handleNavigation = () => {
        const url = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(data.store.address)}`
        window.open(url, "_blank")
    }

    return (
        <ModalStyled
            isOpen={isOpen}
            onClose={onClose}
            isDismissable
            isKeyboardDismissDisabled={false}
        >
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1">
                    <div className="flex items-center gap-2">
                        <span className="text-primary font-black uppercase text-sm">
                            Chi tiết ca làm
                        </span>
                        <Chip size="sm" variant="bordered" color="primary">
                            {dayjs(data.date).format("DD/MM/YYYY")}
                        </Chip>
                    </div>
                </ModalHeader>
                <ModalBody>
                    {/* Phần 1: Thông tin Store */}
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
                                <span className="font-bold">{data.store.phone}</span>
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
                            <p className="text-sm text-default-400 font-bold uppercase">
                                Thời gian ca làm
                            </p>
                            <p className="text-xl font-black tabular-nums">
                                {data.startTime.substring(0, 5)} - {data.endTime.substring(0, 5)}
                            </p>
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
                                        <Avatar
                                            src={worker.avatar}
                                            name={worker.firstName}
                                            size="sm"
                                        />
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
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
