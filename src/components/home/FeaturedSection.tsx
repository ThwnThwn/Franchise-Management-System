"use client"

import React, { useEffect, useState } from "react"
import Link from "next/link"
import { motion } from "framer-motion"
import { productApi } from "@/features/products/services/productApi"
import { Product } from "@/features/products/models/products/respone"
import { ProductCard, ProductVariantsModal } from "@/features/products/components"
import { cn, useDisclosure } from "@heroui/react"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { useCartStore } from "@/features/cart/store"
import { StoreSelectionModal } from "@/features/stores/components"

export default function FeaturedSection({ className }: { className?: string }) {
    const [featuredItems, setFeaturedItems] = useState<Product[]>([])
    const [isLoading, setIsLoading] = useState(true)

    // Modal & Product Logic
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const setStore = useCartStore((s) => s.setStore)

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
            setStore(selectedStore)
        }
    }, [selectedStore, setStore])

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
                const res = await productApi.getAll({
                    pagination: { page: 1, pageSize: 4 },
                    query: {}
                })
                if (res?.items) {
                    setFeaturedItems(res.items.slice(0, 4))
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
        <section className={cn("py-20 bg-white relative overflow-hidden", className)}>
            {/* Decorative Background Blob */}
            <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none opacity-30">
                <div className="absolute top-[-10%] left-[-5%] w-125 h-125 bg-amber-100 rounded-full blur-[80px]" />
                <div className="absolute bottom-[-10%] right-[-5%] w-100 h-100 bg-orange-50 rounded-full blur-[60px]" />
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                <div className="text-center mb-16">
                    <motion.div
                        initial={{ opacity: 0, y: 30 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6 }}
                    >
                        <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
                            Sản Phẩm <span className="text-amber-600">Nổi Bật</span>
                        </h2>
                        <div className="w-24 h-1.5 bg-linear-to-r from-amber-400 to-orange-500 mx-auto rounded-full mb-6" />
                        <p className="mt-4 text-gray-600 text-lg max-w-2xl mx-auto">
                            Những thức uống được yêu thích nhất, được pha chế tỉ mỉ từ những hạt cà
                            phê thượng hạng.
                        </p>
                    </motion.div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                    {featuredItems.map((item, index) => (
                        <motion.div
                            key={item.id}
                            initial={{ opacity: 0, scale: 0.9, y: 50 }}
                            whileInView={{ opacity: 1, scale: 1, y: 0 }}
                            viewport={{ once: true, margin: "-50px" }}
                            transition={{
                                duration: 0.5,
                                delay: index * 0.1,
                                type: "spring",
                                stiffness: 100
                            }}
                            whileHover={{ y: -10 }}
                        >
                            <div
                                onClick={() => handleOpenProduct(item)}
                                className="cursor-pointer h-full"
                            >
                                <ProductCard product={item} />
                            </div>
                        </motion.div>
                    ))}
                </div>

                <motion.div
                    className="text-center mt-12"
                    initial={{ opacity: 0 }}
                    whileInView={{ opacity: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.4 }}
                >
                    <Link href="/menu">
                        <button className="group relative px-8 py-3 bg-white border-2 border-[#3E2723] text-[#3E2723] font-bold rounded-full overflow-hidden transition-all hover:text-white hover:shadow-lg">
                            <div className="absolute inset-0 w-0 bg-[#3E2723] transition-all duration-[250ms] ease-out group-hover:w-full opacity-100" />
                            <span className="relative z-10 flex items-center gap-2">
                                Xem tất cả menu
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    className="h-5 w-5 transform group-hover:translate-x-1 transition-transform"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke="currentColor"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M17 8l4 4m0 0l-4 4m4-4H3"
                                    />
                                </svg>
                            </span>
                        </button>
                    </Link>
                </motion.div>

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
