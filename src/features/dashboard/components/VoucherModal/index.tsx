"use client"

import React, { useEffect, useState } from "react"
import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Input, Select, SelectItem, Divider, Card, CardBody, addToast } from "@heroui/react"
import { Voucher, VoucherType, CreateVoucherRequest, VoucherItemType, CreateVoucherItemReq } from "@/features/orders/models/voucher"
import { useCreateVoucher, useUpdateVoucher } from "@/features/orders/hooks/useVoucher"
import { Plus, Trash2, Package } from "lucide-react"
import { VoucherItemModal } from "../VoucherItemModal"

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

const voucherItemTypes = [
    { value: VoucherItemType.ITEM_GIFT, label: "Quà tặng (Item Gift)" },
    { value: VoucherItemType.ITEM_REQUIRED, label: "Yêu cầu (Item Required)" }
]

export const VoucherModal = ({ isOpen, onOpenChange, voucher, onSuccess }: VoucherModalProps) => {
    const isEditing = !!voucher
    const { mutate: createVoucher, isPending: isCreating } = useCreateVoucher()
    const { mutate: updateVoucher, isPending: isUpdating } = useUpdateVoucher()

    const [isItemModalOpen, setIsItemModalOpen] = useState(false)

    const [formData, setFormData] = useState<Partial<CreateVoucherRequest>>({
        code: "",
        type: VoucherType.DISCOUNT_PERCENTAGE,
        discount_value: "",
        max_discount_value: "",
        min_order_value: "",
        point_cost: "",
        init_quantity: 100,
        max_usage_per_user: 1,
        voucher_items: []
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
                    point_cost: voucher.point_cost,
                    init_quantity: voucher.init_quantity,
                    max_usage_per_user: voucher.max_usage_per_user,
                    end_date: voucher.end_date ? new Date(voucher.end_date).toISOString().split('T')[0] : "",
                    voucher_items: voucher.voucher_items?.map(item => ({
                        sku: item.sku,
                        quantity: item.quantity,
                        type: item.type
                    })) || []
                })
            } else {
                setFormData({
                    code: "",
                    type: VoucherType.DISCOUNT_PERCENTAGE,
                    discount_value: "",
                    max_discount_value: "",
                    min_order_value: "",
                    point_cost: "",
                    init_quantity: 100,
                    max_usage_per_user: 1,
                    voucher_items: []
                })
            }
        }
    }, [isOpen, voucher])

    const handleSubmit = () => {
        const payload = {
            ...formData,
            init_quantity: formData.init_quantity ? Number(formData.init_quantity) : undefined,
            max_usage_per_user: formData.max_usage_per_user ? Number(formData.max_usage_per_user) : undefined,
            voucher_items: formData.type === VoucherType.GIFT ? formData.voucher_items : undefined
        }

        // Remove empty strings for optional fields
        if (!payload.max_discount_value) delete payload.max_discount_value
        if (!payload.min_order_value) delete payload.min_order_value
        if (!payload.point_cost) delete payload.point_cost
        if (!payload.discount_value && payload.type === VoucherType.GIFT) delete payload.discount_value
        if (!payload.end_date) delete payload.end_date

        if (isEditing && voucher) {
            const { code, init_quantity, ...updatePayload } = payload as any
            updateVoucher({ id: voucher.id, req: updatePayload }, {
                onSuccess: () => {
                    onSuccess()
                    addToast({
                        title: "Thành công",
                        description: "Đã cập nhật voucher",
                        color: "success"
                    })
                    onOpenChange(false)
                }
            })
        } else {
            createVoucher(payload as CreateVoucherRequest, {
                onSuccess: () => {
                    onSuccess()
                    addToast({
                        title: "Thành công",
                        description: "Đã tạo voucher mới",
                        color: "success"
                    })
                    onOpenChange(false)
                }
            })
        }
    }

    const isGiftType = formData.type === VoucherType.GIFT

    return (
        <>
            <Modal isOpen={isOpen} onOpenChange={onOpenChange} size="3xl" scrollBehavior="inside">
                <ModalContent>
                    {(onClose) => (
                        <>
                            <ModalHeader className="flex flex-col gap-1 text-primary">
                                {isEditing ? "Chỉnh sửa Voucher" : "Tạo Voucher mới"}
                            </ModalHeader>
                            <ModalBody>
                                <div className="grid grid-cols-2 gap-6">
                                    <div className="space-y-4">
                                        <Divider />
                                        <h3 className="font-semibold text-sm">Thông tin cơ bản</h3>
                                        <Input
                                            label="Mã Code"
                                            placeholder="Vd: SUMMER2026"
                                            variant="bordered"
                                            value={formData.code}
                                            isDisabled={isEditing}
                                            isRequired
                                            onValueChange={(val) => setFormData(prev => ({ ...prev, code: val }))}
                                        />

                                        <Select
                                            label="Loại Khuyến mãi"
                                            variant="bordered"
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
                                                variant="bordered"
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
                                                variant="bordered"
                                                placeholder="Vd: 100000"
                                                value={formData.max_discount_value}
                                                onValueChange={(val) => setFormData(prev => ({ ...prev, max_discount_value: val }))}
                                            />
                                        )}
                                        <Input
                                            label="Ngày kết thúc"
                                            type="date"
                                            variant="bordered"
                                            value={formData.end_date}
                                            onValueChange={(val) => setFormData(prev => ({ ...prev, end_date: val }))}
                                        />
                                    </div>

                                    <div className="space-y-4">
                                        <Divider />
                                        <h3 className="font-semibold text-sm">Điều kiện & Giới hạn</h3>
                                        <Input
                                            label="Giá trị đơn hàng tối thiểu (đ)"
                                            type="number"
                                            variant="bordered"
                                            placeholder="Vd: 200000"
                                            value={formData.min_order_value}
                                            onValueChange={(val) => setFormData(prev => ({ ...prev, min_order_value: val }))}
                                        />

                                        <Input
                                            label="Điểm cần để đổi (Point Cost)"
                                            type="number"
                                            variant="bordered"
                                            placeholder="Vd: 1000 (Để trống nếu tặng trực tiếp)"
                                            value={formData.point_cost}
                                            onValueChange={(val) => setFormData(prev => ({ ...prev, point_cost: val }))}
                                        />

                                        {!isEditing && (
                                            <Input
                                                label="Tổng số lượng phát hành"
                                                type="number"
                                                variant="bordered"
                                                value={String(formData.init_quantity)}
                                                isRequired
                                                onValueChange={(val) => setFormData(prev => ({ ...prev, init_quantity: Number(val) }))}
                                            />
                                        )}

                                        <Input
                                            label="Giới hạn Đổi & Dùng / Người"
                                            type="number"
                                            variant="bordered"
                                            value={String(formData.max_usage_per_user)}
                                            isRequired
                                            description="Số lần một khách hàng có thể sở hữu/sử dụng voucher này."
                                            onValueChange={(val) => setFormData(prev => ({ ...prev, max_usage_per_user: Number(val) }))}
                                        />
                                    </div>
                                </div>

                                {isGiftType && (
                                    <div className="mt-6 space-y-4">
                                        <Divider />
                                        <div className="flex justify-between items-center bg-gray-50 p-4 rounded-xl border border-gray-100">
                                            <div>
                                                <h3 className="font-bold text-sm text-gray-700">Quản lý Quà tặng / Yêu cầu</h3>
                                                <p className="text-xs text-gray-500">
                                                    Đã chọn: <span className="font-bold text-primary">{formData.voucher_items?.length || 0}</span> items
                                                </p>
                                            </div>
                                            <Button
                                                size="md"
                                                startContent={<Plus size={18} />}
                                                color="primary"
                                                variant="shadow"
                                                onPress={() => setIsItemModalOpen(true)}
                                            >
                                                Mở Modal Quà tặng
                                            </Button>
                                        </div>
                                    </div>
                                )}
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

            <VoucherItemModal
                isOpen={isItemModalOpen}
                onOpenChange={setIsItemModalOpen}
                initialItems={formData.voucher_items || []}
                onSave={(items) => setFormData(prev => ({ ...prev, voucher_items: items }))}
            />
        </>
    )
}
