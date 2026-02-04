"use client"

import React, { useEffect, useState } from "react"
import Link from "next/link"
import { motion } from "framer-motion"
import { productApi } from "@/features/menu/services/productApi"
import { Product } from "@/features/menu/models/products/respone"
import { ProductCard, ProductVariantsModal, StoreSelectionModal } from "@/features/menu/components"
import { useDisclosure } from "@heroui/react"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { useCartStore } from "@/features/cart/store"

export default function FeaturedSection() {
    const [featuredItems, setFeaturedItems] = useState<Product[]>([])
    const [isLoading, setIsLoading] = useState(true)

    // Modal & Product Logic
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const setStoreId = useCartStore((s) => s.setStoreId)

    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const {
        isOpen: isStoreModalOpen,
        onOpen: onOpenStoreModal,
        onOpenChange: onStoreModalOpenChange
    } = useDisclosure({ defaultOpen: false })

    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()
    const isLogined = useTokenStore((s) => !!s.accessToken)

    useEffect(() => {
        if (selectedStore) {
            setStoreId(selectedStore.id)
        }
    }, [selectedStore, setStoreId])

    const handleOpenProduct = (product: Product) => {
        if (!isLogined) {
            onOpenLogin()
        } else if (!selectedStore) {
            // Need to check specific logic: typically we check if we have a storeId in cartStore? 
            // For now, follow MenuPage: force local selection if not set.
            // But we don't have initial 'selectedStore' from cart store here. 
            // Simplification: Ask user to select store if local state is null.
            onOpenStoreModal()
        } else {
            setSelectedProduct(product)
            onOpen()
        }
    }

    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const res = await productApi.getAll({ page: 1, pageSize: 4 })
                if (res?.products) {
                    setFeaturedItems(res.products.slice(0, 4))
                }
            } catch (error) {
                console.error("Failed to fetch featured products", error)
            } finally {
                setIsLoading(false)
            }
        }
        fetchProducts()
    }, [])

    if (isLoading) {
        return (
            <section className="py-20 bg-white">
                <div className="max-w-7xl mx-auto px-4 text-center">
                    <p>Đang tải sản phẩm nổi bật...</p>
                </div>
            </section>
        )
    }

    return (
        <section className="py-20 bg-white">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="text-center mb-16">
                    <motion.h2
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="text-4xl font-bold text-gray-900 mb-4"
                    >
                        Sản Phẩm Nổi Bật
                    </motion.h2>
                    <div className="w-24 h-1 bg-yellow-500 mx-auto rounded-full" />
                    <p className="mt-4 text-gray-600 text-lg">
                        Những thức uống được yêu thích nhất tại cửa hàng
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                    {featuredItems.map((item, index) => (
                        <motion.div
                            key={item.id}
                            initial={{ opacity: 0, y: 30 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: index * 0.1 }}
                        >
                            <div onClick={() => handleOpenProduct(item)} className="cursor-pointer">
                                <ProductCard getProductListItemDto={item} />
                            </div>
                        </motion.div>
                    ))}
                </div>

                <div className="text-center mt-12">
                    <Link href="/menu">
                        <button className="px-8 py-3 border-2 border-gray-900 text-gray-900 font-bold rounded-lg hover:bg-gray-900 hover:text-white transition-colors">
                            Xem tất cả menu
                        </button>
                    </Link>
                </div>

                {/* Modals */}
                <ProductVariantsModal
                    product={selectedProduct}
                    isOpen={isOpen}
                    onOpenChange={onOpenChange}
                    storeId={selectedStore?.id}
                />

                <StoreSelectionModal
                    isOpen={isStoreModalOpen}
                    onOpenChange={onStoreModalOpenChange}
                    onStoreSelect={(store) => {
                        setSelectedStore(store)
                    }}
                />
            </div>
        </section>
    )
}
