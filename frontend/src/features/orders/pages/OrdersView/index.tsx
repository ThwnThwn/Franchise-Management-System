"use client"

import React, { useEffect } from "react"
import { addToast } from "@heroui/react"
import { OrderCard } from "@/features/orders/components/"
import { OrderSummary } from "@/features/orders/models/response"
import { ORDER_TEXT } from "@/features/orders/constants"
import { useGetMyOrders } from "@/features/orders/hooks"
import { SpinnerStyled } from "@/components"
import { BackendError } from "@/models/common/respone"
import { STRINGS } from "@/constants/strings"
import { useRouter } from "next/navigation"

export default function OrdersView() {
    const router = useRouter()
    // const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure()
    // const [selectedOrder, setSelectedOrder] = useState<OrderSummary | undefined>(undefined)

    const { data: ordes, isLoading, error } = useGetMyOrders()

    const handleOpenDetail = (order: OrderSummary) => {
        // setSelectedOrder(order)
        // onOpen()
        router.push(`/orders/${order.id}`)
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
        <div className="p-8 w-fit mx-auto min-h-[80vh]">
            <h1 className="text-center text-2xl font-bold mb-6">
                {ORDER_TEXT.labels.ORDER_HISTORY}
            </h1>

            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
                {(() => {
                    const orderList = ordes || []
                    // Find the latest order by createdAt date
                    const latestOrderId =
                        orderList.length > 0
                            ? [...orderList].sort(
                                  (a, b) =>
                                      new Date(b.createdAt).getTime() -
                                      new Date(a.createdAt).getTime()
                              )[0].id
                            : null

                    return orderList.map((order) => (
                        <OrderCard
                            key={order.id}
                            order={order}
                            onClick={handleOpenDetail}
                            isLatest={order.id === latestOrderId}
                        />
                    ))
                })()}
            </div>
        </div>
    )
}
