"use client"
import React, { useState, useCallback } from "react"
import { motion, AnimatePresence } from "framer-motion"

import { ProductCard, ProductVariantsModal, StoreSelectionModal } from "../components"
import { MENU_ERRORS } from "../constants"
import { GetProductListItemDto } from "../models/schema/respone"
import { useGetAllProducts } from "../hooks"
import { SpinnerStyled } from "@/components"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { useDisclosure, Input } from "@heroui/react"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { MapPin, Search } from "lucide-react"

export default function MenuPage() {
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const [searchQuery, setSearchQuery] = useState("")
    const [inputValue, setInputValue] = useState("")

    const { data: productsData, isLoading } = useGetAllProducts({
        query: {
            storeId: selectedStore?.id,
            page: 1,
            pageSize: 1000
        }
    })

    const products = React.useMemo(() => {
        if (!productsData) return { products: [], totalCount: 0 }
        if (!searchQuery) return productsData

        const normalize = (str: string) => str.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase()
        const lowerQuery = normalize(searchQuery)

        const filtered = productsData.products.filter(p =>
            normalize(p.name).includes(lowerQuery)
        )
        return {
            ...productsData,
            products: filtered
        }
    }, [productsData, searchQuery])
    const [selectedProduct, setSelectedProduct] = useState<GetProductListItemDto | null>(null)

    const handleSearch = () => {
        setSearchQuery(inputValue)
    }

    const { isOpen, onOpen, onOpenChange } = useDisclosure()
    const {
        isOpen: isStoreModalOpen,
        onOpen: onOpenStoreModal,
        onOpenChange: onStoreModalOpenChange
    } = useDisclosure({ defaultOpen: false })
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
            {/* Header with Store Info */}
            {/* Header with Store Info */}
            <div className="flex flex-col md:flex-row justify-between items-center gap-4 mt-6 px-4">
                <div className="flex items-center gap-4">
                    <h2 className="text-2xl font-bold">Menu</h2>
                    {selectedStore ? (
                        <div
                            className="flex items-center gap-2 cursor-pointer hover:text-primary transition-colors bg-default-100 px-3 py-1.5 rounded-full"
                            onClick={onOpenStoreModal}
                        >
                            <MapPin size={16} />
                            <div className="flex flex-col">
                                <span className="font-semibold text-sm leading-none">{selectedStore.name}</span>
                            </div>
                        </div>
                    ) : (
                        <button
                            onClick={onOpenStoreModal}
                            className="flex items-center gap-1 text-sm bg-primary text-white px-3 py-1.5 rounded-full hover:bg-primary/90 transition-colors"
                        >
                            <MapPin size={16} />
                            Chọn cửa hàng
                        </button>
                    )}
                </div>

                <div className="w-full md:w-1/3">
                    <Input
                        value={inputValue}
                        placeholder="Tìm kiếm món ăn..."
                        endContent={
                            <div
                                role="button"
                                tabIndex={0}
                                onClick={handleSearch}
                                className="text-primary hover:opacity-80 transition-opacity focus:outline-none cursor-pointer"
                            >
                                <Search size={20} />
                            </div>
                        }
                        onValueChange={setInputValue}
                        onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                        isClearable
                        onClear={() => {
                            setInputValue("")
                            setSearchQuery("")
                        }}
                    />
                </div>
            </div>
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
                            {!selectedStore ? "Vui lòng chọn cửa hàng để xem menu" : MENU_ERRORS.NOT_HAVE_PRODUCT}
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
                storeId={selectedStore?.id}
            />

            <StoreSelectionModal
                isOpen={isStoreModalOpen} // Only open when explicitly requested
                onOpenChange={onStoreModalOpenChange}
                onStoreSelect={(store) => {
                    setSelectedStore(store)
                    // Modal will be closed by the component or we can control it here if needed
                }}
            />

            {/* Cart Drawer */}
            {/* <CartDrawer isOpen={isCartOpen} onClose={() => setIsCartOpen(false)} /> */}
        </div>
    )
}
