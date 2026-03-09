"use client"

import React, { useEffect, useState } from "react"
import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Input, Select, SelectItem } from "@heroui/react"
import { Voucher, VoucherType, CreateVoucherRequest } from "@/features/orders/models/voucher"
import { useCreateVoucher, useUpdateVoucher } from "@/features/orders/hooks/useVoucher"

interface VoucherModalProps {
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    voucher: Voucher | null
    onSuccess: () => void
}

const voucherTypes = [
    { value: VoucherType.SHIPPING_FIXED_AMOUNT, label: "Freeship (Tiền cố định)" },
    { value: VoucherType.SHIPPING_PERCENTAGE, label: "Freeship (Phần trăm)" },
    { value: VoucherType.DISCOUNT_FIXED_AMOUNT, label: "Giảm Giá (Tiền cố định)" },
    { value: VoucherType.DISCOUNT_PERCENTAGE, label: "Giảm Giá (Phần trăm)" },
    { value: VoucherType.GIFT, label: "Quà tặng" }
]

export const VoucherModal = ({ isOpen, onOpenChange, voucher, onSuccess }: VoucherModalProps) => {
    const isEditing = !!voucher
    const { mutate: createVoucher, isPending: isCreating } = useCreateVoucher()
    const { mutate: updateVoucher, isPending: isUpdating } = useUpdateVoucher()

    const [formData, setFormData] = useState<Partial<CreateVoucherRequest>>({
        code: "",
        type: VoucherType.DISCOUNT_PERCENTAGE,
        discount_value: "",
        max_discount_value: "",
        min_order_value: "",
        init_quantity: 100,
        max_usage_per_user: 1,
    })

    useEffect(() => {
        if (isOpen) {
            if (voucher) {
                setFormData({
                    code: voucher.code,
                    type: voucher.type,
                    discount_value: voucher.discount_value,
                    max_discount_value: voucher.max_discount_value,
                    min_order_value: voucher.min_order_value,
                    init_quantity: voucher.init_quantity,
                    max_usage_per_user: voucher.max_usage_per_user,
                    end_date: voucher.end_date ? new Date(voucher.end_date).toISOString().split('T')[0] : ""
                })
            } else {
                setFormData({
                    code: "",
                    type: VoucherType.DISCOUNT_PERCENTAGE,
                    discount_value: "",
                    max_discount_value: "",
                    min_order_value: "",
                    init_quantity: 100,
                    max_usage_per_user: 1,
                })
            }
        }
    }, [isOpen, voucher])

    const handleSubmit = () => {
        const payload = {
            ...formData,
            init_quantity: Number(formData.init_quantity),
            max_usage_per_user: Number(formData.max_usage_per_user)
        }

        // Remove empty strings for optional fields
        if (!payload.max_discount_value) delete payload.max_discount_value
        if (!payload.min_order_value) delete payload.min_order_value
        if (!payload.discount_value && payload.type === VoucherType.GIFT) delete payload.discount_value

        if (isEditing && voucher) {
            // Remove code from update payload as it's typically immutable
            const { code, init_quantity, ...updatePayload } = payload as any
            updateVoucher({ id: voucher.id, req: updatePayload }, {
                onSuccess: () => {
                    onSuccess()
                    onOpenChange(false)
                }
            })
        } else {
            createVoucher(payload as CreateVoucherRequest, {
                onSuccess: () => {
                    onSuccess()
                    onOpenChange(false)
                }
            })
        }
    }

    const isGiftType = formData.type === VoucherType.GIFT

    return (
        <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="2xl">
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1">
                            {isEditing ? "Chỉnh sửa Voucher" : "Tạo Voucher mới"}
                        </ModalHeader>
                        <ModalBody>
                            <div className="grid grid-cols-2 gap-4">
                                <Input
                                    label="Mã Code"
                                    placeholder="Vd: SUMMER2026"
                                    value={formData.code}
                                    isDisabled={isEditing}
                                    isRequired
                                    onValueChange={(val) => setFormData(prev => ({ ...prev, code: val }))}
                                />

                                <Select
                                    label="Loại Khuyến mãi"
                                    selectedKeys={[String(formData.type)]}
                                    isRequired
                                    onChange={(e) => setFormData(prev => ({ ...prev, type: Number(e.target.value) }))}
                                >
                                    {voucherTypes.map((type) => (
                                        <SelectItem key={String(type.value)}>
                                            {type.label}
                                        </SelectItem>
                                    ))}
                                </Select>

                                {!isGiftType && (
                                    <Input
                                        label="Giá trị giảm"
                                        type="number"
                                        placeholder={formData.type === VoucherType.DISCOUNT_PERCENTAGE || formData.type === VoucherType.SHIPPING_PERCENTAGE ? "Vd: 10 (%)" : "Vd: 50000 (đ)"}
                                        value={formData.discount_value}
                                        isRequired
                                        onValueChange={(val) => setFormData(prev => ({ ...prev, discount_value: val }))}
                                    />
                                )}

                                {(formData.type === VoucherType.DISCOUNT_PERCENTAGE || formData.type === VoucherType.SHIPPING_PERCENTAGE) && (
                                    <Input
                                        label="Mức giảm tối đa (đ)"
                                        type="number"
                                        placeholder="Vd: 100000"
                                        value={formData.max_discount_value}
                                        onValueChange={(val) => setFormData(prev => ({ ...prev, max_discount_value: val }))}
                                    />
                                )}

                                <Input
                                    label="Giá trị đơn hàng tối thiểu (đ)"
                                    type="number"
                                    placeholder="Vd: 200000"
                                    value={formData.min_order_value}
                                    onValueChange={(val) => setFormData(prev => ({ ...prev, min_order_value: val }))}
                                />

                                {!isEditing && (
                                    <Input
                                        label="Số lượng Voucher"
                                        type="number"
                                        value={String(formData.init_quantity)}
                                        isRequired
                                        onValueChange={(val) => setFormData(prev => ({ ...prev, init_quantity: Number(val) }))}
                                    />
                                )}

                                <Input
                                    label="Số lần dùng tối đa/Người"
                                    type="number"
                                    value={String(formData.max_usage_per_user)}
                                    isRequired
                                    onValueChange={(val) => setFormData(prev => ({ ...prev, max_usage_per_user: Number(val) }))}
                                />
                            </div>
                        </ModalBody>
                        <ModalFooter>
                            <Button color="danger" variant="flat" onPress={onClose}>
                                Hủy
                            </Button>
                            <Button color="primary" onPress={handleSubmit} isLoading={isCreating || isUpdating}>
                                {isEditing ? "Cập nhật" : "Tạo mới"}
                            </Button>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </Modal>
    )
}
