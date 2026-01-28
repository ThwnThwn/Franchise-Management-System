"use client"
import React, { useState, useCallback } from "react"
import { motion, AnimatePresence } from "framer-motion"
import { useDisclosure } from "@heroui/react"
import {
    CardProductStyled,
    MENU_ERRORS,
    ProductVariantsModal,
    useGetAllProducts
} from "@/features/menu"
import { GetProductListItemDto } from "@/features/menu/models/schema/respone"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { SpinnerStyled } from "@/components"

// Import các components và hooks theo cấu trúc dự án của bạn

const CoffeeHome = () => {
    // 1. Lấy dữ liệu từ Hook (giống MenuPage)
    const { data, isLoading } = useGetAllProducts({})
    const [selectedProduct, setSelectedProduct] = useState<GetProductListItemDto | null>(null)

    // 2. Logic Modal & Auth
    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()
    const isLogined = useTokenStore((s) => !!s.accessToken)

    const handleOpenProduct = useCallback(
        (product: GetProductListItemDto) => {
            if (!isLogined) {
                onOpenLogin()
            } else {
                setSelectedProduct(product)
                onOpen()
            }
        },
        [isLogined, onOpen, onOpenLogin]
    )

    const products = data?.products || []
    const isEmpty = !isLoading && products.length === 0

    if (isLoading) return <SpinnerStyled />

    return (
        <div className="min-h-screen bg-[#FAF7F2] font-sans text-stone-800">
            {/* Hero Section - Giữ nguyên phong cách trang chủ */}
            <header className="relative h-[60vh] flex items-center justify-center overflow-hidden">
                <motion.img
                    initial={{ scale: 1.1 }}
                    animate={{ scale: 1 }}
                    transition={{ duration: 1.5 }}
                    src="https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&q=80&w=2070"
                    className="absolute w-full h-full object-cover z-0"
                    alt="Coffee Background"
                />
                <div className="absolute inset-0 bg-black/40 z-10"></div>

                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="relative z-20 text-center px-4 text-white"
                >
                    <h1 className="text-5xl md:text-7xl font-serif font-bold mb-4">Cà Hong Bạn</h1>
                    <p className="text-xl italic mb-8 opacity-90">
                        Hương vị nguyên bản trong từng giọt cà phê
                    </p>
                </motion.div>
            </header>

            {/* Main Content - Học tập Layout từ MenuPage */}
            <main className="max-w-7xl mx-auto py-20 px-6">
                <div className="flex justify-between items-end mb-12">
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                    >
                        <h2 className="text-3xl font-serif font-bold text-stone-900 uppercase tracking-tight">
                            Món nổi bật
                        </h2>
                        <div className="h-1.5 w-20 bg-amber-600 mt-2 rounded-full"></div>
                    </motion.div>
                </div>

                <AnimatePresence mode="wait">
                    {isEmpty ? (
                        <motion.div
                            key="empty"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            className="text-center py-20 text-gray-500"
                        >
                            {MENU_ERRORS.NOT_HAVE_PRODUCT}
                        </motion.div>
                    ) : (
                        <motion.div
                            key="grid"
                            initial="hidden"
                            whileInView="visible"
                            viewport={{ once: true }}
                            variants={{
                                hidden: { opacity: 0 },
                                visible: {
                                    opacity: 1,
                                    transition: { staggerChildren: 0.1 }
                                }
                            }}
                            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8"
                        >
                            {products.slice(0, 6).map(
                                (
                                    product // Chỉ lấy 6 món nổi bật cho trang Home
                                ) => (
                                    <motion.div
                                        key={product.id}
                                        variants={{
                                            hidden: { opacity: 0, y: 20 },
                                            visible: { opacity: 1, y: 0 }
                                        }}
                                        whileHover={{ y: -8, transition: { duration: 0.3 } }}
                                        onClick={() => handleOpenProduct(product)}
                                        className="cursor-pointer"
                                    >
                                        <CardProductStyled getProductListItemDto={product} />
                                    </motion.div>
                                )
                            )}
                        </motion.div>
                    )}
                </AnimatePresence>
            </main>

            {/* Modal hiển thị biến thể món ăn */}
            <ProductVariantsModal
                product={selectedProduct}
                isOpen={isOpen}
                onOpenChange={onOpenChange}
            />
        </div>
    )
}

export default CoffeeHome
