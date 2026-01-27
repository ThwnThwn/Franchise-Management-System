"use client"
import React, { useState, useCallback } from "react"
import { motion, AnimatePresence } from "framer-motion"

import { CardProductStyled, ProductVariantsModal } from "../components"
import { MENU_ERRORS } from "../constants"
import { GetProductListItemDto } from "../models/schema/respone"
import { useGetAllProducts } from "../hooks"
import { SpinnerStyled } from "@/components"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"

export default function MenuPage() {
    const { data: products, isLoading } = useGetAllProducts({})
    const [selectedProduct, setSelectedProduct] = useState<GetProductListItemDto | null>(null)
    // const [isCartOpen, setIsCartOpen] = useState(false)
    const isModalOpen = selectedProduct !== null

    const { onOpen: onOpenLogin } = useLoginDiscloresureSingleton()
    const isLogined = useTokenStore((s) => !!s.accessToken)

    const handleOpenProduct = useCallback(
        (product: GetProductListItemDto) => {
            if (!isLogined) {
                onOpenLogin()
            } else setSelectedProduct(product)
        },
        [isLogined, onOpenLogin]
    )

    const handleModalOpenChange = useCallback((open: boolean) => {
        if (!open) setSelectedProduct(null)
    }, [])

    const isEmpty = !products || products.products == undefined || products.products.length === 0
    if (isLoading) return <SpinnerStyled />

    return (
        <div className="min-h-[80vh] max-w-7xl mx-auto p-6 text-center relative">
            {/* Cart Button - Fixed Position */}
            {/* {isLogined && (
                <div className="fixed bottom-6 right-6 z-30">
                    <CartButton onPress={() => setIsCartOpen(true)} />
                </div>
            )} */}

            <div className="mt-10">
                <AnimatePresence mode="wait">
                    {isEmpty ? (
                        <motion.div
                            key="empty"
                            initial={{ opacity: 0, scale: 0.95 }}
                            animate={{ opacity: 1, scale: 1 }}
                            exit={{ opacity: 0 }}
                            transition={{ duration: 0.4 }}
                            className="text-gray-500 mt-10 text-base"
                        >
                            {MENU_ERRORS.NOT_HAVE_PRODUCT}
                        </motion.div>
                    ) : (
                        <motion.div
                            key="grid"
                            initial="hidden"
                            animate="visible"
                            variants={{
                                hidden: { opacity: 0, y: 20 },
                                visible: {
                                    opacity: 1,
                                    y: 0,
                                    transition: {
                                        staggerChildren: 0.05,
                                        duration: 0.5
                                    }
                                }
                            }}
                            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8"
                        >
                            {products.products.map((product) => (
                                <motion.div
                                    key={product.id}
                                    variants={{
                                        hidden: { opacity: 0, y: 20 },
                                        visible: { opacity: 1, y: 0 }
                                    }}
                                    whileHover={{
                                        scale: 1.03,
                                        transition: { duration: 0.2 }
                                    }}
                                    role="button"
                                    tabIndex={0}
                                    onClick={() => handleOpenProduct(product)}
                                    onKeyDown={(e) => {
                                        if (e.key === "Enter" || e.key === " ") {
                                            e.preventDefault()
                                            handleOpenProduct(product)
                                        }
                                    }}
                                    className="cursor-pointer"
                                >
                                    <CardProductStyled getProductListItemDto={product} />
                                </motion.div>
                            ))}
                        </motion.div>
                    )}
                </AnimatePresence>
            </div>

            <ProductVariantsModal
                product={selectedProduct}
                isOpen={isModalOpen}
                onOpenChange={handleModalOpenChange}
            />

            {/* Cart Drawer */}
            {/* <CartDrawer isOpen={isCartOpen} onClose={() => setIsCartOpen(false)} /> */}
        </div>
    )
}
