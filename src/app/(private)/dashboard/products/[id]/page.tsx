import ProductFormPageView from "@/features/products/pages/ProductFormPageView"
import React from "react"

export default async function ProductDetailPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params

    return <ProductFormPageView productId={id} />
}
