import React from "react"
import { ModalContent, ModalHeader, ModalBody, ModalFooter, Button, Chip } from "@heroui/react"
import { Order } from "@/features/orders/models/response"
import { getOrderStatusColor, getOrderStatusLabel } from "@/features/orders/utils"
import { ModalStyled } from "@/components"
import { OrderDetail } from "./OrderDetail"
import { ORDER_TEXT } from "@/features/orders/constants"
import { useDay } from "@/hooks"
import { DEFAULT_DATE_FORMAT } from "@/constants/constants"

interface OrderDetailModalProps {
    isOpen: boolean
    onOpenChange: () => void
    order: Order
}

export const OrderDetailModal = ({ isOpen, onOpenChange, order }: OrderDetailModalProps) => {
    const { formatDateTime } = useDay({ defaultFormat: DEFAULT_DATE_FORMAT })

    const handlePrintInvoice = () => {
        alert(ORDER_TEXT.labels.INVOICE_FEATURE_ALERT)
    }

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
