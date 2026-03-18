"use client"

import { useState } from "react"
import { Select, SelectItem, Button } from "@heroui/react"
import { OrderHeader } from "@/features/orders/models/response"
import { useTokenStore, useUserHelper } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { ActiveStaffDto, useGetActiveStaffs } from "@/features/stores"
import { useAssignStaffToDelivery, useSelfAssignStaffToDelivery } from "@/features/orders/hooks"
import { OrderStatus } from "@/features/orders/constants"

interface Props {
    order: OrderHeader
}

const VALID_DELIVERY_STATUSES = [
    OrderStatus.Preparing,
    OrderStatus.ReadyForPickup,
    OrderStatus.Delivering,
    OrderStatus.Completed
]

export const OrderDeliverySection = ({ order }: Props) => {
    const { toFullName } = useUserHelper()

    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isManager = roleSlug === RoleSlug.Manager || roleSlug === RoleSlug.Admin
    const isStaff = roleSlug === RoleSlug.Staff

    const deliveryMan = order.deliveryMan
    const hasDeliveryMan = !!deliveryMan?.id

    const { data: activeStaffs, isLoading: isLoadingStaffs } = useGetActiveStaffs({
        storeId: isManager ? order.store?.id : undefined,
        enabled: isManager && !hasDeliveryMan && VALID_DELIVERY_STATUSES.includes(order.status)
    })

    const { mutate: assignStaff, isPending: isAssigning } = useAssignStaffToDelivery()
    const { assignWithConfirm, isPending: isSelfAssigning } = useSelfAssignStaffToDelivery()

    const [selectedStaffId, setSelectedStaffId] = useState<string>("")

    const handleManagerAssign = () => {
        if (!selectedStaffId) return
        assignStaff({
            id: order.id,
            req: { staffId: selectedStaffId }
        })
    }

    const handleStaffSelfAssign = () => {
        assignWithConfirm(order.id)
    }

    if (!VALID_DELIVERY_STATUSES.includes(order.status)) {
        return null
    }

    // 1. NẾU ĐÃ CÓ NGƯỜI GIAO
    if (hasDeliveryMan) {
        const fullName = toFullName(deliveryMan)
        return (
            // Dùng pt-3 và border-t để tạo đường kẻ ngang phân cách
            <div className="pt-3 border-t border-default-100 flex gap-2 items-center">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    {order.status === OrderStatus.Completed
                        ? "Đã giao bởi"
                        : "Đơn hàng được giao bởi"}
                </p>
                <div className="flex items-center gap-2">
                    <span className="text-base font-bold text-default-900">{fullName}</span>
                    {deliveryMan.phone && (
                        <span className="text-sm font-medium text-default-500">
                            - SĐT: {deliveryMan.phone}
                        </span>
                    )}
                </div>
            </div>
        )
    }

    if (!isManager && !isStaff) {
        return null
    }

    // 2. RENDER CHO MANAGER ĐIỀU PHỐI
    if (isManager) {
        return (
            <div className="pt-3 border-t border-default-100 space-y-3">
                <p className="text-[10px] uppercase tracking-widest text-default-500 font-bold">
                    Điều phối giao hàng
                </p>
                <div className="flex items-end gap-3">
                    <Select
                        label="Chọn tài xế đang trong ca"
                        placeholder="-- Chọn tài xế --"
                        className="flex-1"
                        isLoading={isLoadingStaffs}
                        isDisabled={isLoadingStaffs || isAssigning}
                        selectedKeys={selectedStaffId ? [selectedStaffId] : []}
                        onChange={(e) => setSelectedStaffId(e.target.value)}
                    >
                        {(activeStaffs || []).map((staff: ActiveStaffDto) => (
                            <SelectItem key={staff.id} textValue={toFullName(staff)}>
                                <div className="flex flex-col">
                                    <span className="text-small">{toFullName(staff)}</span>
                                </div>
                            </SelectItem>
                        ))}
                    </Select>
                    <Button
                        color="primary"
                        className="h-14 px-6 font-medium"
                        isLoading={isAssigning}
                        isDisabled={!selectedStaffId}
                        onPress={handleManagerAssign}
                    >
                        Gán tài xế
                    </Button>
                </div>
            </div>
        )
    }

    // 3. RENDER CHO STAFF TỰ NHẬN ĐƠN
    if (isStaff) {
        return (
            <div className="pt-3 border-t border-default-100 flex flex-row items-center justify-between">
                <div>
                    <h3 className="text-sm font-bold text-default-900">Đơn hàng đang chờ giao</h3>
                    <p className="text-xs text-default-500 mt-1">Bạn có muốn nhận đơn này không?</p>
                </div>
                <Button
                    color="primary"
                    variant="solid"
                    className="text-white font-medium px-6"
                    isLoading={isSelfAssigning}
                    onPress={handleStaffSelfAssign}
                >
                    Nhận giao
                </Button>
            </div>
        )
    }

    return null
}
