"use client"

import React, { useMemo } from "react"
import { Listbox, ListboxItem, User, Tooltip, ScrollShadow, Spinner, cn } from "@heroui/react"
import {
    AlertTriangle,
    CheckCircle2,
    UserPlus,
    UserMinus,
    Info,
    Search,
    Sparkles
} from "lucide-react"
import { useShiftAssignment } from "@/features/shifts/hooks"
import { UnsavedChangesNotifier, InputStyled, ButtonStyled } from "@/components"
import { useUserHelper } from "@/hooks"
import { getValidationErrors } from "./helper"

type Props = {
    storeId: string
    shiftId: string
    requiredStaffCount: number
    isSubmitable: boolean
    onClose?: () => void
}

export function ShiftAssignmentTab({
    storeId,
    shiftId,
    requiredStaffCount,
    isSubmitable,
    onClose
}: Props) {
    const { toFullName } = useUserHelper()
    const {
        isLoading,
        isSyncing,
        assignedList,
        availableList,
        hasChanges,
        keyword,
        setKeyword,
        handleToggle,
        handleReset,
        handleQuickSuggest,
        syncAssignments
    } = useShiftAssignment({
        storeId,
        shiftId,
        requiredStaffCount: requiredStaffCount,
        onSuccess: onClose
    })

    // UI Logic: Màu sắc báo hiệu tình trạng nhân sự
    const capacityStatus = useMemo(() => {
        const count = assignedList.length
        if (count < requiredStaffCount) return { color: "text-warning", label: "Thiếu nhân sự" }
        if (count === requiredStaffCount) return { color: "text-success", label: "Đủ nhân sự" }
        return { color: "text-danger", label: "Thừa nhân sự" }
    }, [assignedList.length, requiredStaffCount])

    if (isLoading)
        return (
            <div className="flex items-center justify-center">
                <Spinner label="Đang quét danh sách ứng viên..." color="primary" />
            </div>
        )

    return (
        <div className="flex flex-col pb-10 relative">
            {/* Search Bar */}
            <div className="p-3 border-b border-divider bg-default-50/50">
                <InputStyled
                    placeholder="Tìm tên nhân viên..."
                    size="sm"
                    value={keyword}
                    onChange={(e) => setKeyword(e.target.value)}
                    startContent={<Search size={16} className="text-default-400" />}
                    isClearable
                    onClear={() => setKeyword("")}
                />
            </div>

            <ScrollShadow className="grow p-2">
                {/* 1. Nhóm Đã Gán */}
                <section className="mb-6">
                    <h4
                        className={cn(
                            "text-xs font-bold mb-3 flex items-center gap-2 px-2 uppercase tracking-wider",
                            capacityStatus.color
                        )}
                    >
                        <CheckCircle2 size={14} />
                        Đã gán ({assignedList.length}/{requiredStaffCount}) - {capacityStatus.label}
                    </h4>
                    {assignedList.length === 0 ? (
                        <p className="text-xs text-default-400 italic pl-8">
                            Chưa có ai trong ca này
                        </p>
                    ) : (
                        <Listbox items={assignedList} variant="flat" aria-label="Assigned list">
                            {(item) => (
                                <ListboxItem
                                    key={item.id}
                                    textValue={toFullName(item)}
                                    endContent={
                                        <UserMinus
                                            className="text-danger opacity-0 group-hover:opacity-100 transition-opacity"
                                            size={18}
                                        />
                                    }
                                    onPress={() => handleToggle(item.id)}
                                    isReadOnly={!isSubmitable}
                                    className="group"
                                >
                                    <User
                                        name={toFullName(item)}
                                        description={`${item.currentHourlyWage.toLocaleString()}đ/h`}
                                        avatarProps={{ src: item.avatarUrl, size: "sm" }}
                                    />
                                </ListboxItem>
                            )}
                        </Listbox>
                    )}
                </section>

                {/* 2. Nhóm Ứng Viên Khả Dụng */}
                <section>
                    <div className="flex justify-between">
                        <h4 className="text-xs font-bold mb-3 flex items-center gap-2 px-2 text-primary uppercase tracking-wider">
                            <UserPlus size={14} />
                            Ứng viên khả dụng ({availableList.length})
                        </h4>
                        {assignedList.length < requiredStaffCount && (
                            <ButtonStyled
                                size="sm"
                                variant="ghost"
                                color="primary"
                                startContent={<Sparkles size={14} />}
                                className="h-7 text-tiny font-bold"
                                onPress={handleQuickSuggest}
                                isDisabled={!isSubmitable}
                            >
                                Gợi ý nhanh
                            </ButtonStyled>
                        )}
                    </div>
                    <Listbox items={availableList} variant="flat" aria-label="Available list">
                        {(item) => {
                            // 1. Lấy danh sách lỗi từ Helper của bro
                            const errors = getValidationErrors(item)

                            // 2. Xác định các lỗi "Chặn cứng" (Không cho gán ca)
                            const isHardBlocked =
                                item.isConflict || item.validation?.isShuttleInvalid

                            // 3. Xác định các lỗi "Cảnh báo" (Vẫn cho gán nhưng hiện icon vàng)
                            const hasWarning = errors.length > 0

                            return (
                                <ListboxItem
                                    key={item.id}
                                    onPress={() => handleToggle(item.id)}
                                    // Chặn bấm nếu bị lỗi Hard-block
                                    className={cn(isHardBlocked && "opacity-40 cursor-not-allowed")}
                                    isReadOnly={isHardBlocked || !isSubmitable}
                                    endContent={
                                        hasWarning && (
                                            <Tooltip
                                                content={
                                                    <div className="p-1 max-w-50">
                                                        <div className="font-bold mb-1 text-tiny">
                                                            Lưu ý hệ thống:
                                                        </div>
                                                        {errors.map((err, i) => (
                                                            <div
                                                                key={i}
                                                                className="text-tiny text-white/90"
                                                            >
                                                                • {err}
                                                            </div>
                                                        ))}
                                                    </div>
                                                }
                                                color={isHardBlocked ? "danger" : "warning"}
                                                placement="right"
                                            >
                                                {/* Đỏ nếu bị chặn, Vàng nếu chỉ là cảnh báo */}
                                                <AlertTriangle
                                                    size={18}
                                                    className={
                                                        isHardBlocked
                                                            ? "text-danger"
                                                            : "text-warning"
                                                    }
                                                />
                                            </Tooltip>
                                        )
                                    }
                                >
                                    <div className="flex flex-col items-start">
                                        <User
                                            name={toFullName(item)}
                                            description={`${item.currentHourlyWage.toLocaleString()}đ/h`}
                                            avatarProps={{ src: item.avatarUrl, size: "sm" }}
                                        />
                                        {!item.isAvailable && item.note && (
                                            <span className="text-[10px] text-warning italic pl-10 flex items-center gap-1">
                                                <Info size={10} /> Bận: {item.note}
                                            </span>
                                        )}
                                    </div>
                                </ListboxItem>
                            )
                        }}
                    </Listbox>
                </section>
            </ScrollShadow>

            {/* Thanh thông báo thay đổi */}
            <UnsavedChangesNotifier
                hasChanges={hasChanges}
                isLoading={isSyncing}
                onSave={syncAssignments}
                onReset={handleReset}
            />
        </div>
    )
}
