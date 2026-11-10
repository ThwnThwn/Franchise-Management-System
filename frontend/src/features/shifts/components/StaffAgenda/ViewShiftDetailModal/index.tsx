"use client"

import { ModalContent, ModalHeader, ModalBody, Chip, Tabs, Tab } from "@heroui/react"
import { ArrowRightLeft, Calendar, ClipboardCheck } from "lucide-react"
import dayjs from "dayjs"
import { StaffShiftAgendaWithLog } from "@/features/shifts/models/response"
import { ModalStyled } from "@/components"
import { ViewShiftDetailBody } from "./ViewShiftDetailBody"
import { SwapShiftTabBody, TimeLogViewBody } from "@/features/shifts/components"
import { ShiftStatus } from "@/features/shifts/constants"

interface ViewShiftDetailModalProps {
    isOpen: boolean
    onClose: () => void
    data: StaffShiftAgendaWithLog | null
}

export const ViewShiftDetailModal = ({ isOpen, onClose, data }: ViewShiftDetailModalProps) => {
    if (!data) return null

    return (
        <ModalStyled
            isOpen={isOpen}
            onClose={onClose}
            isDismissable
            isKeyboardDismissDisabled={false}
            size="2xl"
            scrollBehavior="inside"
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
                    <Tabs
                        fullWidth
                        aria-label="Shift Options"
                        color="primary"
                        variant="underlined"
                        classNames={{
                            tabList: "w-full relative rounded-none p-0 border-b border-divider",
                            cursor: "w-full bg-primary",
                            tab: "h-12", // Cao lên xíu cho dễ bấm
                            tabContent:
                                "group-data-[selected=true]:text-primary font-bold text-sm uppercase tracking-wide"
                        }}
                    >
                        <Tab
                            key="shift"
                            title={
                                <div className="flex items-center gap-2">
                                    <Calendar size={16} />
                                    <span>Ca làm việc</span>
                                </div>
                            }
                        >
                            <ModalBody className="py-4 px-0 overflow-y-auto overflow-x-hidden">
                                <ViewShiftDetailBody data={data} />
                            </ModalBody>
                        </Tab>

                        <Tab
                            key="timelog"
                            isDisabled={!data.timeLog} // Vô hiệu hóa nếu chưa đi làm / chưa có log
                            title={
                                <div className="flex items-center gap-2">
                                    <ClipboardCheck size={16} />
                                    <span>Chấm công</span>
                                </div>
                            }
                        >
                            <ModalBody className="py-4 px-0 overflow-y-auto overflow-x-hidden">
                                {data.timeLog ? (
                                    <TimeLogViewBody data={data.timeLog} />
                                ) : (
                                    <div className="py-10 text-center italic text-default-400">
                                        Chưa có dữ liệu chấm công.
                                    </div>
                                )}
                            </ModalBody>
                        </Tab>

                        <Tab
                            key="swap"
                            // Cho phép đổi/nhả ca nếu: Ca chưa qua (tương lai) VÀ trạng thái là Published hoặc Open
                            isDisabled={
                                !data.shiftInfo ||
                                ![ShiftStatus.Published, ShiftStatus.Open].includes(
                                    data.shiftInfo.status
                                ) ||
                                dayjs(data.date).isBefore(dayjs())
                            }
                            title={
                                <div className="flex items-center gap-2">
                                    <ArrowRightLeft size={16} />
                                    <span>Đổi ca / Nhả ca</span>
                                </div>
                            }
                        >
                            <ModalBody className="py-4 px-4 overflow-y-auto overflow-x-hidden min-h-75">
                                <SwapShiftTabBody shift={data.shiftInfo} assignmentId={data.id} />
                            </ModalBody>
                        </Tab>
                    </Tabs>
                </ModalBody>
            </ModalContent>
        </ModalStyled>
    )
}
