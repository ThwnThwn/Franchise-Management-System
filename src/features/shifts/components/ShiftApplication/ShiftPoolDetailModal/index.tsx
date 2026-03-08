"use client"

import React, { useMemo } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Tabs,
    Tab,
    Chip,
    User as HeroUser,
    Divider,
    Spinner
} from "@heroui/react"
import { Coffee, FileText, CheckCircle2, Clock } from "lucide-react"
import dayjs from "dayjs"

import { ButtonStyled, ModalStyled } from "@/components"
import { ShiftResponse } from "@/features/shifts/models/response"
import { useTokenStore, useUserHelper } from "@/hooks"
import { useGetApplicationsByShiftId, useApplyForShift } from "@/features/shifts/hooks" // Trỏ đường dẫn cho đúng nha ní
import { ShiftApplicationStatusLabels } from "@/features/shifts/constants/labels"
import { ShiftApplicationStatusColors } from "@/features/shifts/constants/styles"

interface ShiftPoolDetailModalProps {
    isOpen: boolean
    onClose: () => void
    shift: ShiftResponse | null
}

export const ShiftPoolDetailModal = ({ isOpen, onClose, shift }: ShiftPoolDetailModalProps) => {
    const userId = useTokenStore((s) => s.userId) // Giả định ní có lưu userId trong token store
    const { toFullName } = useUserHelper()

    // 1. Fetch danh sách đơn apply NGAY KHI MỞ MODAL
    const { data: applications, isLoading: isAppLoading } = useGetApplicationsByShiftId({
        shiftId: shift?.id || "",
        enabled: isOpen && !!shift?.id // Chỉ gọi API khi Modal đang mở và có ID
    })

    // Lấy hook apply có bọc sẵn Dialog Confirm
    const { applyWithConfirm, isPending: isApplying } = useApplyForShift({
        // Thành công thì đóng Modal luôn cho gọn
        onSuccess: onClose
    })

    // 2. Tìm xem User hiện tại đã nộp đơn cho ca này chưa
    const myApplication = useMemo(() => {
        if (!applications || !userId) return null
        return applications.find((app) => app.userId === userId)
    }, [applications, userId])

    // 3. Logic chặn nút Apply
    const isFull = shift ? shift.assignedStaffCount >= shift.requiredStaffCount : false
    const hasApplied = !!myApplication

    const handleApply = async () => {
        if (!shift) return
        try {
            await applyWithConfirm(shift.id)
        } catch {
            // User huỷ dialog, không làm gì thêm
        }
    }

    if (!shift) return null

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose}>
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1 pb-2">
                    <h2 className="text-xl font-bold">Chi tiết ca làm</h2>
                    <Chip size="sm" variant="bordered" color="primary">
                        {dayjs(shift.shiftDate).format("DD/MM/YYYY")}
                    </Chip>
                </ModalHeader>

                <ModalBody className="py-2">
                    <Tabs fullWidth size="md" color="primary" variant="underlined">
                        {/* TAB 1: THÔNG TIN CA & ĐỒNG NGHIỆP */}
                        <Tab key="info" title="Thông tin ca">
                            <div className="flex flex-col gap-4 py-3">
                                {/* Khối thông tin cơ bản */}
                                <div className="bg-default-50 p-4 rounded-xl border border-default-100 flex flex-col gap-3">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-default-100 rounded-2xl">
                                            <Clock className="text-default-600" size={20} />
                                        </div>
                                        <div>
                                            <p className="text-sm text-default-400 font-bold uppercase">
                                                Thời gian ca làm
                                            </p>
                                            <p className="text-xl font-black tabular-nums">
                                                {shift.startTime.substring(0, 5)} -{" "}
                                                {shift.endTime.substring(0, 5)}
                                            </p>

                                            {(shift.breakDurationMinutes ?? 0) > 0 && (
                                                <div className="flex items-center gap-1.5 mt-1 text-xs text-warning-600 font-medium bg-warning-50 w-fit px-2 py-0.5 rounded-full border border-warning-100">
                                                    <Coffee size={12} />
                                                    <span>
                                                        Nghỉ giữa ca: {shift.breakDurationMinutes}{" "}
                                                        phút
                                                    </span>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                    {shift.note && (
                                        <div className="flex items-start gap-3 text-default-700 mt-1">
                                            <FileText
                                                size={18}
                                                className="text-primary shrink-0 mt-0.5"
                                            />
                                            <span className="text-sm italic text-default-600 bg-white p-2 rounded-lg border flex-1">
                                                {shift.note}
                                            </span>
                                        </div>
                                    )}
                                </div>

                                <Divider />

                                {/* Khối Đồng nghiệp đã gán */}
                                <div>
                                    <h4 className="text-sm font-bold text-default-900 mb-3 flex items-center justify-between">
                                        <span>Đã phân công</span>
                                        <span className="text-primary">
                                            {shift.assignedStaffCount} / {shift.requiredStaffCount}{" "}
                                            người
                                        </span>
                                    </h4>

                                    <div className="flex flex-col gap-2 max-h-37.5 overflow-y-auto pr-2">
                                        {shift.assignments && shift.assignments.length > 0 ? (
                                            shift.assignments.map((assignment) => (
                                                <div
                                                    key={assignment.id}
                                                    className="flex items-center gap-3 p-2 bg-white border border-default-100 rounded-lg"
                                                >
                                                    <HeroUser
                                                        name={toFullName(assignment)}
                                                        avatarProps={{
                                                            src: assignment.avatarUrl,
                                                            size: "sm"
                                                        }}
                                                    />
                                                </div>
                                            ))
                                        ) : (
                                            <p className="text-sm text-default-400 italic text-center py-2">
                                                Chưa có ai nhận ca này. Hãy là người đầu tiên!
                                            </p>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </Tab>

                        {/* TAB 2: ĐƠN ỨNG TUYỂN CỦA TÔI */}
                        <Tab key="my-app" title="Đơn của bạn">
                            <div className="py-5 flex flex-col items-center justify-center min-h-62.5 text-center">
                                {isAppLoading ? (
                                    <Spinner label="Đang kiểm tra hồ sơ..." color="primary" />
                                ) : myApplication ? (
                                    <div className="flex flex-col items-center gap-4 w-full px-4">
                                        <div className="w-16 h-16 bg-success-50 text-success rounded-full flex items-center justify-center mb-2">
                                            <CheckCircle2 size={32} />
                                        </div>
                                        <h3 className="text-lg font-bold text-default-900">
                                            Bạn đã nộp đơn!
                                        </h3>
                                        <p className="text-sm text-default-500">
                                            Đơn ứng tuyển gửi lúc:{" "}
                                            {dayjs(myApplication.appliedAt).format(
                                                "HH:mm - DD/MM/YYYY"
                                            )}
                                        </p>

                                        <div className="w-full bg-default-50 p-4 rounded-xl border mt-2 text-left">
                                            <p className="text-xs font-bold text-default-400 uppercase mb-1">
                                                Trạng thái
                                            </p>
                                            <Chip
                                                color={
                                                    ShiftApplicationStatusColors[
                                                        myApplication.status
                                                    ]
                                                }
                                                variant="flat"
                                                className="font-bold"
                                            >
                                                {ShiftApplicationStatusLabels[myApplication.status]}
                                            </Chip>

                                            {myApplication.note && (
                                                <>
                                                    <p className="text-xs font-bold text-default-400 uppercase mt-3 mb-1">
                                                        Ghi chú
                                                    </p>
                                                    <p className="text-sm text-default-700 italic">
                                                        &quot;{myApplication.note}&quot;
                                                    </p>
                                                </>
                                            )}
                                        </div>
                                    </div>
                                ) : (
                                    <div className="flex flex-col items-center gap-2">
                                        <FileText size={40} className="text-default-300 mb-2" />
                                        <p className="text-default-600 font-medium">
                                            Bạn chưa ứng tuyển ca này.
                                        </p>
                                        <p className="text-sm text-default-400 px-8">
                                            Hãy kiểm tra lại lịch trình cá nhân và bấm Nhận Ca ở bên
                                            dưới nhé.
                                        </p>
                                    </div>
                                )}
                            </div>
                        </Tab>
                    </Tabs>
                </ModalBody>

                <ModalFooter className="border-t border-divider pt-4">
                    <ButtonStyled variant="light" onPress={onClose} isDisabled={isApplying}>
                        Đóng
                    </ButtonStyled>

                    {/* LOGIC NÚT APPLY THÔNG MINH */}
                    <ButtonStyled
                        color={hasApplied ? "default" : isFull ? "danger" : "primary"}
                        isDisabled={hasApplied || isFull || isAppLoading}
                        isLoading={isApplying}
                        onPress={handleApply}
                        className="font-bold shadow-md"
                    >
                        {isAppLoading
                            ? "Đang kiểm tra..."
                            : hasApplied
                              ? "Đã nộp đơn"
                              : isFull
                                ? "Đã đủ người"
                                : "Nhận Ca Ngay"}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
