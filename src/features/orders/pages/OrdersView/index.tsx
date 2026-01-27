"use client"

import React, { useEffect, useState } from "react"
import { addToast, useDisclosure } from "@heroui/react"
import { OrderCard, OrderDetailModal } from "@/features/orders/components/"
import { Order } from "@/features/orders/models/response"
import { ORDER_TEXT } from "@/features/orders/constants"
import { mockOrders } from "./mock-data"
import { useGetMyOrders } from "@/features/orders/hooks"
import { SpinnerStyled } from "@/components"
import { BackendError } from "@/models/common/respone"
import { STRINGS } from "@/constants/strings"

export default function OrdersView() {
    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const [selectedOrder, setSelectedOrder] = useState<Order | null>(null)

    const { data: ordes, isLoading, error } = useGetMyOrders()

    const handleOpenDetail = (order: Order) => {
        setSelectedOrder(order)
        onOpen()
    }

    useEffect(() => {
        if (error) {
            const backendErr = error as BackendError
            addToast({
                title: backendErr.title || STRINGS.COMMON.ERROR,
                description: backendErr.detail,
                color: "danger"
            })
        }
    })

    if (isLoading) return <SpinnerStyled />

    return (
        <div className="p-8 w-fit mx-auto">
            <h1 className="text-center text-2xl font-bold mb-6">
                {ORDER_TEXT.labels.ORDER_HISTORY}
            </h1>

            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
                {(ordes || mockOrders).map((order) => (
                    <OrderCard key={order.id} order={order} onClick={handleOpenDetail} />
                ))}
            </div>

            {selectedOrder && (
                <OrderDetailModal
                    isOpen={isOpen}
                    onOpenChange={onOpenChange}
                    order={selectedOrder}
                />
            )}
        </div>
    )
}
