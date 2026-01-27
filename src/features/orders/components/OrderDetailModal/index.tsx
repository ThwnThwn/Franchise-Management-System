import React, { useEffect } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Chip,
    Spinner,
    addToast
} from "@heroui/react"
import { getOrderStatusColor, getOrderStatusLabel } from "@/features/orders/utils"
import { ModalStyled } from "@/components"
import { OrderDetail } from "./OrderDetail"
import { ORDER_TEXT } from "@/features/orders/constants"
import { useDay } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"
import { useGetOrderById } from "@/features/orders/hooks"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

interface OrderDetailModalProps {
    isOpen: boolean
    onOpenChange: () => void
    orderId: string
}

export const OrderDetailModal = ({ isOpen, onOpenChange, orderId }: OrderDetailModalProps) => {
    const { data: order, isLoading, error } = useGetOrderById({ id: orderId })
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    const handlePrintInvoice = () => {
        alert(ORDER_TEXT.labels.INVOICE_FEATURE_ALERT)
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

    if (isLoading || !order) return <Spinner />

    return (
        <ModalStyled
            isOpen={isOpen}
            isDismissable
            onOpenChange={onOpenChange}
            size="2xl"
            backdrop="blur"
        >
            <ModalContent>
                {(onOpenChange) => (
                    <>
                        <ModalHeader className="flex flex-col gap-1">
                            <div className="flex justify-between items-center mr-8">
                                <span className="text-xl font-bold">
                                    {ORDER_TEXT.labels.ORDER_DETAIL_TITLE} #{order.code}
                                </span>
                                <Chip color={getOrderStatusColor(order.status)} variant="solid">
                                    {getOrderStatusLabel(order.status)}
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
                            <Button color="danger" variant="light" onPress={onOpenChange}>
                                {ORDER_TEXT.labels.CLOSE}
                            </Button>

                            <Button color="primary" onPress={handlePrintInvoice}>
                                {ORDER_TEXT.labels.PRINT_INVOICE}
                            </Button>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </ModalStyled>
    )
}
