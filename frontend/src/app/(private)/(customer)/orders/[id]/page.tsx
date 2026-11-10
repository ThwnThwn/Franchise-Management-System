import OrderDetailPageView from "@/features/orders/pages/OrderDetailPageView"
import React from "react"

export default async function OrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params

    return <OrderDetailPageView orderId={id} />
}
