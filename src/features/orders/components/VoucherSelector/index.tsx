"use client"

import React, { useMemo, useState } from "react"
import { useGetAllVouchers } from "../../hooks/useVoucher"
import { Voucher, VoucherType, VoucherItemType } from "../../models/voucher"
import { Order } from "../../models/response"
import { Modal, ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Chip } from "@heroui/react"
import { Ticket, Truck, Gift, Tag, CheckCircle2 } from "lucide-react"

interface VoucherSelectorProps {
    order: Order | null
    isOpen: boolean
    onOpenChange: (isOpen: boolean) => void
    onApplyVouchers: (shippingVoucher: Voucher | null, discountVoucher: Voucher | null) => void
    appliedShippingVoucher: Voucher | null
    appliedDiscountVoucher: Voucher | null
}

export function VoucherSelector({
    order,
    isOpen,
    onOpenChange,
    onApplyVouchers,
    appliedShippingVoucher,
    appliedDiscountVoucher
}: VoucherSelectorProps) {
    const { data: vouchers, isLoading } = useGetAllVouchers()

    // Local state for selection before applying
    const [selectedShipping, setSelectedShipping] = useState<Voucher | null>(appliedShippingVoucher)
    const [selectedDiscount, setSelectedDiscount] = useState<Voucher | null>(appliedDiscountVoucher)

    // Reset local selection when modal opens
    React.useEffect(() => {
        if (isOpen) {
            setSelectedShipping(appliedShippingVoucher)
            setSelectedDiscount(appliedDiscountVoucher)
        }
    }, [isOpen, appliedShippingVoucher, appliedDiscountVoucher])

    // Phân loại voucher
    const shippingVouchers = useMemo(() => {
        if (!vouchers) return []
        return vouchers.filter(v =>
            v.type === VoucherType.SHIPPING_FIXED_AMOUNT ||
            v.type === VoucherType.SHIPPING_PERCENTAGE
        )
    }, [vouchers])

    const discountVouchers = useMemo(() => {
        if (!vouchers) return []
        return vouchers.filter(v =>
            v.type === VoucherType.DISCOUNT_FIXED_AMOUNT ||
            v.type === VoucherType.DISCOUNT_PERCENTAGE ||
            v.type === VoucherType.GIFT
        )
    }, [vouchers])

    // Kiểm tra tính khả dụng của voucher
    const checkVoucherEligibility = (voucher: Voucher): { isEligible: boolean, reason?: string } => {
        if (!order) return { isEligible: false, reason: "Không có thông tin đơn hàng" }

        const orderTotal = order.totalAmount || 0

        // Kiểm tra min_order_value
        if (voucher.min_order_value && orderTotal < Number(voucher.min_order_value)) {
            return { isEligible: false, reason: `Đơn tối thiểu ${Number(voucher.min_order_value).toLocaleString()}đ` }
        }

        // Nếu là GIFT: Kiểm tra xem có yêu cầu sản phẩm cụ thể (ITEM_REQUIRED) không
        if (voucher.type === VoucherType.GIFT && voucher.voucher_items && voucher.voucher_items.length > 0) {
            const requiredItem = voucher.voucher_items.find(i => i.type === VoucherItemType.ITEM_REQUIRED)
            if (requiredItem) {
                // Kiểm tra giỏ hàng có sản phẩm này và đủ số lượng không
                const orderItem = order.items.find(i => i.variantId === requiredItem.variant_id)
                if (!orderItem || orderItem.quantity < requiredItem.quantity) {
                    return { isEligible: false, reason: `Cần mua tối thiểu ${requiredItem.quantity} sản phẩm quy định` }
                }
            }
        }

        return { isEligible: true }
    }

    const renderVoucherList = (list: Voucher[], selected: Voucher | null, onSelect: (v: Voucher | null) => void, title: string, icon: React.ReactNode) => {
        if (!list || list.length === 0) return null

        return (
            <div className="mb-6">
                <div className="flex items-center gap-2 mb-3">
                    <div className="p-1.5 bg-primary/10 rounded-lg text-primary">
                        {icon}
                    </div>
                    <h3 className="font-bold text-lg">{title}</h3>
                </div>
                <div className="space-y-3">
                    {list.map(voucher => {
                        const { isEligible, reason } = checkVoucherEligibility(voucher)
                        const isSelected = selected?.id === voucher.id

                        return (
                            <div
                                key={voucher.id}
                                className={`flex border-2 rounded-xl overflow-hidden transition-all ${isSelected ? 'border-primary bg-primary/5' :
                                        isEligible ? 'border-default-200 hover:border-primary/50 cursor-pointer' :
                                            'border-default-100 opacity-50 cursor-not-allowed bg-default-50/50'
                                    }`}
                                onClick={() => {
                                    if (isEligible) {
                                        onSelect(isSelected ? null : voucher)
                                    }
                                }}
                            >
                                {/* Cột trái - Icon */}
                                <div className={`w-24 flex flex-col items-center justify-center border-r-2 border-dashed ${isSelected ? 'bg-primary/10 border-primary/20 text-primary' :
                                        isEligible ? 'bg-default-100 border-default-200 text-default-500' :
                                            'bg-default-50 border-default-100 text-default-300'
                                    }`}>
                                    {voucher.type === VoucherType.GIFT ? <Gift size={32} /> :
                                        voucher.type === VoucherType.SHIPPING_FIXED_AMOUNT || voucher.type === VoucherType.SHIPPING_PERCENTAGE ? <Truck size={32} /> :
                                            <Tag size={32} />}
                                </div>

                                {/* Cột phải - Nội dung */}
                                <div className="flex-1 p-3 flex flex-col justify-between relative">
                                    {isSelected && (
                                        <CheckCircle2 className="absolute top-2 right-2 text-primary" size={20} />
                                    )}
                                    <div className="pr-6">
                                        <h4 className="font-bold text-md text-foreground mb-1 leading-tight line-clamp-2">
                                            {voucher.code}
                                            {voucher.discount_value && (
                                                <span className="ml-2 text-primary">
                                                    (Giảm {voucher.type === VoucherType.SHIPPING_PERCENTAGE || voucher.type === VoucherType.DISCOUNT_PERCENTAGE ? `${voucher.discount_value}%` : `${Number(voucher.discount_value).toLocaleString()}đ`})
                                                </span>
                                            )}
                                        </h4>
                                        <p className="text-xs text-default-500 line-clamp-1">
                                            {voucher.min_order_value ? `Đơn tối thiểu ${Number(voucher.min_order_value).toLocaleString()}đ` : 'Mọi đơn hàng'}
                                            {voucher.max_discount_value && ` • Giảm tối đa ${Number(voucher.max_discount_value).toLocaleString()}đ`}
                                        </p>
                                    </div>

                                    {!isEligible && (
                                        <div className="mt-2 text-xs font-medium text-danger">
                                            {reason}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )
                    })}
                </div>
            </div>
        )
    }

    const handleApply = () => {
        onApplyVouchers(selectedShipping, selectedDiscount)
        onOpenChange(false)
    }

    return (
        <Modal
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="2xl"
            scrollBehavior="inside"
        >
            <ModalContent>
                {(onClose) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1 border-b">
                            <h2 className="text-xl font-black flex items-center gap-2">
                                <Ticket className="text-primary" /> Chọn Voucher
                            </h2>
                            <p className="text-sm font-normal text-default-500">Bạn có thể chọn 1 Voucher vận chuyển và 1 Voucher giảm giá/quà tặng</p>
                        </ModalHeader>
                        <ModalBody className="p-6">
                            {isLoading ? (
                                <div className="flex justify-center items-center py-20">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                                </div>
                            ) : (
                                <>
                                    {renderVoucherList(shippingVouchers, selectedShipping, setSelectedShipping, "Voucher Vận Chuyển", <Truck size={20} />)}
                                    {renderVoucherList(discountVouchers, selectedDiscount, setSelectedDiscount, "Voucher Giảm Giá & Quà Tặng", <Tag size={20} />)}

                                    {vouchers?.length === 0 && (
                                        <div className="text-center py-12 text-default-400">
                                            Không có voucher nào khả dụng
                                        </div>
                                    )}
                                </>
                            )}
                        </ModalBody>
                        <ModalFooter className="border-t">
                            <Button color="default" variant="light" onPress={onClose}>
                                Đóng
                            </Button>
                            <Button
                                color="primary"
                                className="font-bold px-8 shadow-md"
                                onPress={handleApply}
                            >
                                Áp dụng {(selectedShipping || selectedDiscount) ? 'Voucher' : ''}
                            </Button>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </Modal>
    )
}
