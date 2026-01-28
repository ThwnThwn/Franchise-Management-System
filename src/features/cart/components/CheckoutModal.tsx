"use client"

import React, { useState } from "react"
import {
    Modal,
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Divider
} from "@heroui/react"
import { useCartStore } from "@/features/cart/store"
import { PaymentMethod, useCreateOrder } from "@/features/orders"
import { CreateOrderRequest } from "@/features/orders/models/request"
import { EnumPicker } from "@/components/modules/EnumPicker"
import { PaymentMethodLabels } from "@/features/orders/constants/"
import { TextareaStyled } from "@/components"

interface CheckoutModalProps {
    isOpen: boolean
    onClose: () => void
    onSuccess?: (orderCode: string) => void
}

const TAX_RATE = 0.1 // 10% VAT

export const CheckoutModal: React.FC<CheckoutModalProps> = ({ isOpen, onClose, onSuccess }) => {
    const items = useCartStore((s) => s.items)
    const total = useCartStore((state) => state.total)
    const createOrderMutation = useCreateOrder()

    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash)
    const [notes, setNotes] = useState("")

    const tax = total * TAX_RATE
    const finalTotal = total + tax

    const handleCheckout = async () => {
        if (items.length === 0) return

        const payload: CreateOrderRequest = {
            items: items.map((item) => ({
                sku: item.sku,
                quantity: item.quantity
            })),
            tax,
            paymentMethod: paymentMethod,
            notes: notes.trim()
        }

        try {
            const result = await createOrderMutation.mutateAsync(payload)
            onSuccess?.(result.orderCode)
            onClose()
            setNotes("")
        } catch (error) {
            console.error("Failed to create order:", error)
        }
    }

    return (
        <Modal isOpen={isOpen} onClose={onClose} size="lg">
            <ModalContent>
                <ModalHeader>
                    <h3 className="text-xl font-bold">Xác nhận đơn hàng</h3>
                </ModalHeader>
                <ModalBody>
                    {/* Payment Method */}
                    <div>
                        <label className="text-sm font-medium mb-2 block">
                            Phương thức thanh toán
                        </label>
                        {/* <Select
                            selectedKeys={[paymentMethod.toString()]}
                            onChange={(e) => setPaymentMethod(Number(e.target.value))}
                            placeholder="Chọn phương thức thanh toán"
                        >
                            {paymentMethods.map((method) => (
                                <SelectItem key={method.value}>{method.label}</SelectItem>
                            ))}
                        </Select> */}
                        <EnumPicker
                            label={"Chọn phương thức thanh toán"}
                            labels={PaymentMethodLabels}
                            value={paymentMethod}
                            onChange={(val) => setPaymentMethod(val as PaymentMethod)}
                            isClearable={false}
                        />
                    </div>

                    {/* Notes */}
                    <div>
                        <label className="text-sm font-medium mb-2 block">Ghi chú</label>
                        <TextareaStyled
                            value={notes}
                            onChange={(e) => setNotes(e.target.value)}
                            placeholder="Thêm ghi chú cho đơn hàng (tùy chọn)"
                            minRows={3}
                        />
                    </div>

                    <Divider />

                    {/* Order Summary */}
                    <div className="space-y-2">
                        <div className="flex justify-between text-sm">
                            <span>Tạm tính:</span>
                            <span>{total.toLocaleString("vi-VN")} đ</span>
                        </div>
                        <div className="flex justify-between text-sm">
                            <span>Thuế VAT (10%):</span>
                            <span>{tax.toLocaleString("vi-VN")} đ</span>
                        </div>
                        <Divider />
                        <div className="flex justify-between text-lg font-bold">
                            <span>Tổng cộng:</span>
                            <span className="text-primary">
                                {finalTotal.toLocaleString("vi-VN")} đ
                            </span>
                        </div>
                    </div>
                </ModalBody>
                <ModalFooter>
                    <Button variant="light" onPress={onClose}>
                        Hủy
                    </Button>
                    <Button
                        color="primary"
                        onPress={handleCheckout}
                        isLoading={createOrderMutation.isPending}
                        isDisabled={items.length === 0}
                    >
                        Đặt hàng
                    </Button>
                </ModalFooter>
            </ModalContent>
        </Modal>
    )
}
