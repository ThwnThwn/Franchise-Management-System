"use client"

import React, { useMemo, useState } from "react"
import { Spinner, User, Chip, Divider, ScrollShadow } from "@heroui/react"
import { Check, X, Clock, History, MessageSquareQuote, Star } from "lucide-react"
import dayjs from "dayjs"

import { useGetApplicationsByShiftId, useReviewShiftApplication } from "@/features/shifts/hooks"
import { ShiftApplicationStatus } from "@/features/shifts/constants"
import { ButtonStyled } from "@/components"
import { ShiftApplicationStatusColors } from "@/features/shifts/constants/styles"
import { ShiftApplicationStatusLabels } from "@/features/shifts/constants/labels"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

interface ShiftApplicationTabProps {
    shiftId: string
}

export const ShiftApplicationTab = ({ shiftId }: ShiftApplicationTabProps) => {
    // 1. Fetch dữ liệu đơn ứng tuyển của ca này
    const { data: applications, isLoading } = useGetApplicationsByShiftId({ shiftId })

    // 2. Hook xử lý duyệt/từ chối đã bọc sẵn Dialog
    const { reviewWithConfirm } = useReviewShiftApplication()

    // Lưu lại ID của đơn đang được bấm để hiện Loading Spinner đúng chỗ
    const [processingId, setProcessingId] = useState<string | null>(null)

    // 3. Phân loại đơn
    const { pendingApps, processedApps } = useMemo(() => {
        if (!applications) return { pendingApps: [], processedApps: [] }
        return {
            pendingApps: applications.filter(
                (app) => app.status === ShiftApplicationStatus.Pending
            ),
            processedApps: applications.filter(
                (app) => app.status !== ShiftApplicationStatus.Pending
            )
        }
    }, [applications])

    // 4. Hàm xử lý khi Manager bấm nút
    const handleAction = async (applicationId: string, isApproved: boolean) => {
        setProcessingId(applicationId)
        try {
            await reviewWithConfirm(applicationId, isApproved)
        } catch {
            // User bấm Hủy trong Dialog thì lơ đi
        } finally {
            setProcessingId(null)
        }
    }

    if (isLoading) {
        return (
            <div className="flex justify-center py-10">
                <Spinner label="Đang tải danh sách đơn..." color="primary" />
            </div>
        )
    }

    if (!applications || applications.length === 0) {
        return (
            <div className="text-center py-10 text-default-400 italic">
                Chưa có nhân viên nào ứng tuyển ca này.
            </div>
        )
    }

    return (
        <ScrollShadow className="flex flex-col gap-6 max-h-125 pr-2 overflow-y-auto pb-4">
            {/* KHỐI 1: ĐƠN ĐANG CHỜ DUYỆT */}
            <section>
                <div className="flex items-center gap-2 mb-3 px-1 text-warning-600">
                    <Clock size={18} />
                    <h4 className="font-bold uppercase tracking-wider text-sm">
                        Đang chờ duyệt ({pendingApps.length})
                    </h4>
                </div>

                {pendingApps.length === 0 ? (
                    <p className="text-sm text-default-400 italic px-7">
                        Không có đơn nào cần xử lý.
                    </p>
                ) : (
                    <div className="flex flex-col gap-3">
                        {pendingApps.map((app) => (
                            <div
                                key={app.id}
                                className="bg-white border border-warning-200 shadow-sm rounded-xl p-4 flex flex-col gap-3 relative overflow-hidden"
                            >
                                <div className="absolute left-0 top-0 bottom-0 w-1 bg-warning"></div>

                                <div className="flex justify-between items-start pl-2">
                                    <User
                                        name={
                                            <span className="font-bold text-default-900">
                                                {app.username}
                                            </span>
                                        }
                                        description={
                                            <div className="flex items-center gap-2 mt-1">
                                                <span className="text-success font-bold">
                                                    {formatCurrencyWithSymbol(app.hourlyWage)}/h
                                                </span>
                                                <span className="text-default-300">•</span>
                                                <span className="flex items-center text-amber-500 text-xs">
                                                    <Star
                                                        size={12}
                                                        className="fill-amber-500 mr-0.5"
                                                    />
                                                    {app.smartScore.toFixed(1)}
                                                </span>
                                            </div>
                                        }
                                        avatarProps={{ src: app.avatarUrl, size: "md" }}
                                    />
                                    <span className="text-[10px] text-default-400 font-medium bg-default-100 px-2 py-1 rounded-md">
                                        {dayjs(app.appliedAt).format("HH:mm DD/MM")}
                                    </span>
                                </div>

                                {app.note && (
                                    <div className="pl-12">
                                        <div className="bg-default-50 text-default-600 text-sm p-2 rounded-lg border border-default-100 italic flex items-start gap-2">
                                            <MessageSquareQuote
                                                size={14}
                                                className="text-primary shrink-0 mt-0.5"
                                            />
                                            <span>&quot;{app.note}&quot;</span>
                                        </div>
                                    </div>
                                )}

                                <div className="flex items-center justify-end gap-2 mt-1">
                                    <ButtonStyled
                                        size="sm"
                                        color="danger"
                                        variant="flat"
                                        startContent={processingId !== app.id && <X size={16} />}
                                        isLoading={processingId === app.id}
                                        isDisabled={
                                            processingId !== null && processingId !== app.id
                                        }
                                        onPress={() => handleAction(app.id, false)}
                                    >
                                        Từ chối
                                    </ButtonStyled>
                                    <ButtonStyled
                                        size="sm"
                                        color="success"
                                        className="text-white font-bold"
                                        startContent={
                                            processingId !== app.id && <Check size={16} />
                                        }
                                        isLoading={processingId === app.id}
                                        isDisabled={
                                            processingId !== null && processingId !== app.id
                                        }
                                        onPress={() => handleAction(app.id, true)}
                                    >
                                        Duyệt ngay
                                    </ButtonStyled>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>

            <Divider className="opacity-50" />

            {/* KHỐI 2: LỊCH SỬ XỬ LÝ (ĐÃ DUYỆT / TỪ CHỐI) */}
            <section>
                <div className="flex items-center gap-2 mb-3 px-1 text-default-500">
                    <History size={18} />
                    <h4 className="font-bold uppercase tracking-wider text-sm">
                        Lịch sử xử lý ({processedApps.length})
                    </h4>
                </div>

                {processedApps.length === 0 ? (
                    <p className="text-sm text-default-400 italic px-7">Chưa có lịch sử xử lý.</p>
                ) : (
                    <div className="flex flex-col gap-2">
                        {processedApps.map((app) => (
                            <div
                                key={app.id}
                                className="bg-default-50 border border-default-100 rounded-lg p-3 flex flex-col gap-2 opacity-80 hover:opacity-100 transition-opacity"
                            >
                                <div className="flex justify-between items-center">
                                    <User
                                        name={
                                            <span className="font-medium text-default-700">
                                                {app.username}
                                            </span>
                                        }
                                        description={
                                            <span className="text-xs">
                                                {formatCurrencyWithSymbol(app.hourlyWage)}/h
                                            </span>
                                        }
                                        avatarProps={{ src: app.avatarUrl, size: "sm" }}
                                    />
                                    <Chip
                                        size="sm"
                                        variant="flat"
                                        color={ShiftApplicationStatusColors[app.status]}
                                        className="font-bold"
                                    >
                                        {ShiftApplicationStatusLabels[app.status]}
                                    </Chip>
                                </div>

                                {/* Nếu Manager có để lại lời nhắn khi duyệt/từ chối (giả định field này tên là reviewNote, ní sửa lại theo API trả về nhé) */}
                                {/* {(app as any).reviewNote && (
                                    <div className="text-xs text-default-500 bg-white p-2 rounded border mt-1 ml-10">
                                        <span className="font-bold">Ghi chú duyệt:</span> {(app as any).reviewNote}
                                    </div>
                                )} */}
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </ScrollShadow>
    )
}
