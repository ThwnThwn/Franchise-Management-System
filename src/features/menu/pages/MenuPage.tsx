"use client"
import React, { useState, useCallback } from "react"
import { motion, AnimatePresence } from "framer-motion"

import { ProductCard, ProductVariantsModal } from "../components"
import { MENU_ERRORS } from "../constants"
import { GetProductListItemDto } from "../models/schema/respone"
import { useGetAllProducts } from "../hooks"
import { SpinnerStyled } from "@/components"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { useDisclosure } from "@heroui/react"

export default function MenuPage() {
    const { data: products, isLoading } = useGetAllProducts({})
    const [selectedProduct, setSelectedProduct] = useState<GetProductListItemDto | null>(null)

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

    // const handleModalOpenChange = useCallback(() => {
    //     if () setSelectedProduct(null)
    // }, [])

    const isEmpty = !products || products.products == undefined || products.products.length === 0
    if (isLoading) return <SpinnerStyled />

    return (
        <div className="min-h-[80vh] max-w-7xl">
            {/* Cart Button - Fixed Position */}
            {/* {isLogined && (
                <div className="fixed bottom-6 right-6 z-30">
                    <CartButton onPress={() => setIsCartOpen(true)} />
                </div>
            )} */}

            <div className="mt-10 flex justify-center">
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
                            className="w-fit grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6"
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
                                    // onKeyDown={(e) => {
                                    //     if (e.key === "Enter" || e.key === " ") {
                                    //         e.preventDefault()
                                    //         handleOpenProduct(product)
                                    //     }
                                    // }}
                                    className="cursor-pointer"
                                >
                                    <ProductCard getProductListItemDto={product} />
                                </motion.div>
                            ))}
                        </motion.div>
                    )}
                </AnimatePresence>
            </div>

            <ProductVariantsModal
                product={selectedProduct}
                isOpen={isOpen}
                onOpenChange={onOpenChange}
            />

            {/* Cart Drawer */}
            {/* <CartDrawer isOpen={isCartOpen} onClose={() => setIsCartOpen(false)} /> */}
        </div>
    )
}
