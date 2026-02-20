"use client"

import React, { useEffect, useState } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Chip,
    Spinner,
    addToast
} from "@heroui/react"
import { ButtonStyled, EnumPicker, ModalStyled } from "@/components"
import { OrderDetail } from "../OrderDetail"
import {
    ORDER_TEXT,
    OrderStatus,
    OrderStatusColors,
    OrderStatusLabels,
    PaymentMethod,
    PaymentMethodLabels
} from "@/features/orders/constants"
import { confirmDialog, useDay } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import { useGetOrderById, usePayment, useUpdateOrderStatus } from "@/features/orders/hooks"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { useRouter } from "next/navigation"
import { omitRecord } from "@/utils/helpers/uiHelper"

interface OrderDetailModalProps {
    isOpen: boolean
    onOpenChange: () => void
    onClose: () => void
    orderId: string
}

export const OrderDetailModal = ({
    isOpen,
    onOpenChange,
    onClose,
    orderId
}: OrderDetailModalProps) => {
    const router = useRouter()
    const { data: order, isLoading, error } = useGetOrderById({ id: orderId, enabled: isOpen })
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | undefined>(undefined)
    const { mutate: payment, isPending: isPaymentPending } = usePayment({
        id: orderId,
        onSuccess: (data: { paymentUrl: string }) => {
            if (data.paymentUrl) {
                router.push(data.paymentUrl)
            } else {
                addToast({
                    title: STRINGS.COMMON.SUCCESS,
                    description: "Thành công! Đang chờ nhà hàng xác nhận đơn...",
                    color: "success"
                })
            }
        }
    })
    const handlePayment = () => {
        if (paymentMethod === undefined) return

        payment({
            paymentMethod: paymentMethod
        })
    }

    const updateStatus = useUpdateOrderStatus({
        id: orderId,
        onSuccess: onClose
    })

    const handleCancel = async () => {
        if (!order) return

        const isConfirmed = await confirmDialog({
            title: "Xác nhận huỷ",
            message: (
                <div className="text-slate-600">
                    Bạn có chắc chắn muốn huỷ đơn hàng <br />
                    <span className="font-bold text-slate-900 text-lg">{order.code}</span> không?
                </div>
            ),
            confirmText: "Đồng ý huỷ",
            variant: "danger"
        })
        if (isConfirmed) updateStatus.mutate(OrderStatus.Cancel)
    }

    useEffect(() => {
        if (error) {
            const backendErr = error as BackendError
            addToast({
                title: STRINGS.COMMON.ERROR,
                description: backendErr.detail,
                color: "danger"
            })
        }
    }, [error])

    if (!isOpen) return null
    if (isLoading || !order) return <Spinner />

    return (
        <>
            <ModalStyled isOpen={isOpen} isDismissable onOpenChange={onOpenChange} size="2xl">
                <ModalContent>
                    {() => (
                        <>
                            <ModalHeader className="flex flex-col gap-1">
                                <div className="flex justify-between items-center mr-8">
                                    <span className="text-xl font-bold">
                                        {ORDER_TEXT.labels.ORDER_DETAIL_TITLE} #{order.code}
                                    </span>
                                    <Chip
                                        color={OrderStatusColors[order.status]}
                                        variant="bordered"
                                    >
                                        {OrderStatusLabels[order.status]}
                                    </Chip>
                                </div>
                                <p className="text-sm text-default-500 font-normal">
                                    {ORDER_TEXT.labels.ORDER_DATE}{" "}
                                    {formatDateTime({ date: order.createdAt })}
                                </p>
                            </ModalHeader>

                            <ModalBody>
                                <OrderDetail key={order.id} order={order} />
                            </ModalBody>

                            <ModalFooter>
                                {order.status === OrderStatus.PaymentPending && (
                                    <>
                                        {order.status === OrderStatus.PaymentPending && (
                                            <ButtonStyled
                                                variant="bordered"
                                                onPress={handleCancel}
                                                isLoading={updateStatus.isPending}
                                                isDisabled={
                                                    order.status !== OrderStatus.PaymentPending
                                                }
                                            >
                                                Huỷ
                                            </ButtonStyled>
                                        )}

                                        <EnumPicker
                                            placeholder="Chọn phương thức thanh toán"
                                            labels={omitRecord(PaymentMethodLabels, [
                                                PaymentMethod.Unknown
                                            ])}
                                            value={paymentMethod}
                                            onChange={(val) =>
                                                setPaymentMethod(val as PaymentMethod)
                                            }
                                            isClearable={false}
                                            isRequired
                                            className="w-70"
                                        />
                                        <ButtonStyled
                                            color="primary"
                                            onPress={handlePayment}
                                            isLoading={isPaymentPending}
                                            isDisabled={paymentMethod === undefined}
                                        >
                                            {ORDER_TEXT.labels.PAY}
                                        </ButtonStyled>
                                    </>
                                )}
                            </ModalFooter>
                        </>
                    )}
                </ModalContent>
            </ModalStyled>
        </>
    )
}
