import ProductFormPageView from "@/features/products/pages/ProductFormPageView"

export default async function ProductDetailPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params

    return <ProductFormPageView productId={id} />
}
