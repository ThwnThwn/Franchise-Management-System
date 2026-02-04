"use client"
import React, { useState, useCallback, useEffect } from "react"
import { motion, AnimatePresence } from "framer-motion"

import { ProductCard, ProductVariantsModal, StoreSelectionModal } from "../components"
import { CategoryFilter } from "../components/CategoryFilter"
import { MENU_ERRORS } from "../constants"
import { Product } from "../models/products/respone"
import { useGetAllProducts, useGetAllCategories } from "../hooks"
import { SpinnerStyled } from "@/components"
import { useLoginDiscloresureSingleton, useTokenStore } from "@/hooks"
import { useDisclosure, Input } from "@heroui/react"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { MapPin, Search } from "lucide-react"

import { useCartStore } from "@/features/cart/store"

export default function MenuPage() {
    const [selectedStore, setSelectedStore] = useState<StoreDetailDto | null>(null)
    const setStoreId = useCartStore((s) => s.setStoreId)

    // Sync store selection to cart store
    useEffect(() => {
        if (selectedStore) {
            setStoreId(selectedStore.id)
        }
    }, [selectedStore, setStoreId])

    const [searchQuery, setSearchQuery] = useState("")
    const [inputValue, setInputValue] = useState("")
    const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null)

    // Fetch Categories
    const { data: categoriesData } = useGetAllCategories()
    const categories = categoriesData || []

    const { data: productsData, isLoading } = useGetAllProducts({
        query: {
            storeId: selectedStore?.id,
            page: 1,
            pageSize: 1000,
            categoryId: selectedCategoryId || undefined // Use filter if selected
        }
    })

    const products = React.useMemo(() => {
        if (!productsData) return { products: [], totalCount: 0 }
        let filtered = productsData.products

        // Client-side search (existing logic)
        if (searchQuery) {
            const normalize = (str: string) =>
                str
                    .normalize("NFD")
                    .replace(/[\u0300-\u036f]/g, "")
                    .toLowerCase()
            const lowerQuery = normalize(searchQuery)
            filtered = filtered.filter((p) => normalize(p.name).includes(lowerQuery))
        }

        // Client-side category filter (if API doesn't filter, good to have double check or if we stick to client filtering for speed)
        // Since we pass categoryId to API, API should filter. But API might return all if categoryId is undefined.
        // If API usage in useGetAllProducts re-fetches on category change, we rely on API.
        // But useGetAllProducts query key includes 'query'. So it will refetch.

        return {
            ...productsData,
            products: filtered
        }
    }, [productsData, searchQuery])

    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)

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
        (product: Product) => {
            if (!isLogined) {
                onOpenLogin()
            } else if (!selectedStore) {
                onOpenStoreModal()
            } else {
                setSelectedProduct(product)
                onOpen()
            }
        },
        [isLogined, onOpen, onOpenLogin, onOpenStoreModal, selectedStore]
    )

    const isEmpty = !products || products.products == undefined || products.products.length === 0

    // We use placeholderData, so we don't need to block UI with spinner on refetch
    // Only show spinner on initial load if no data at all
    if (isLoading && !products.products) return <SpinnerStyled />

    return (
        <div className="min-h-[80vh] max-w-7xl mx-auto px-4 md:px-8">
            {/* Header Section */}
            <div className="flex flex-col xl:flex-row justify-between items-start xl:items-end gap-6 mt-8 mb-12">
                {/* Left: Title + Store Selection */}
                <div className="flex flex-col gap-4 max-w-2xl">
                    <h1 className="text-6xl md:text-7xl font-sans font-black uppercase text-slate-900 tracking-tighter leading-[0.9]">
                        CÀ MENU
                    </h1>

                    <div className="flex items-center gap-3">
                        {/* Store Selector */}
                        {selectedStore ? (
                            <div
                                className="flex items-center gap-2 cursor-pointer hover:bg-default-200 transition-colors bg-default-100 px-4 py-2 rounded-full"
                                onClick={onOpenStoreModal}
                            >
                                <MapPin size={18} className="text-primary" />
                                <div className="flex flex-col">
                                    <span className="font-bold text-sm text-slate-800">
                                        {selectedStore.name}
                                    </span>
                                </div>
                            </div>
                        ) : (
                            <button
                                onClick={onOpenStoreModal}
                                className="flex items-center gap-2 text-sm font-semibold bg-primary text-white px-5 py-2.5 rounded-full hover:bg-primary/90 transition-shadow shadow-md hover:shadow-lg"
                            >
                                <MapPin size={18} />
                                Chọn cửa hàng
                            </button>
                        )}

                        {/* Search Input */}
                        <div className="w-full md:w-80">
                            <Input
                                value={inputValue}
                                placeholder="Tìm kiếm món ăn..."
                                radius="full"
                                classNames={{
                                    inputWrapper: "bg-white shadow-sm border border-gray-200"
                                }}
                                endContent={
                                    <Search
                                        size={18}
                                        className="text-gray-400 cursor-pointer hover:text-primary transition-colors"
                                        onClick={handleSearch}
                                    />
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
                </div>

                {/* Right: Category Filter */}
                <div className="w-full xl:w-auto overflow-hidden">
                    <CategoryFilter
                        categories={categories.map(c => ({ id: c.id, name: c.name }))}
                        selectedId={selectedCategoryId}
                        onSelect={setSelectedCategoryId}
                    />
                </div>
            </div>

            {/* Product Grid */}
            <div className="flex justify-center pb-20">
                <AnimatePresence mode="popLayout">
                    {isEmpty && !isLoading ? (
                        <motion.div
                            key="empty"
                            initial={{ opacity: 0, scale: 0.95 }}
                            animate={{ opacity: 1, scale: 1 }}
                            exit={{ opacity: 0 }}
                            transition={{ duration: 0.4 }}
                            className="text-gray-500 mt-10 text-lg font-medium text-center w-full"
                        >
                            {!selectedStore
                                ? "Vui lòng chọn cửa hàng để xem menu"
                                : MENU_ERRORS.NOT_HAVE_PRODUCT}
                        </motion.div>
                    ) : (
                        <motion.div
                            key="grid"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            // exit={{ opacity: 0 }} // Removing exit animation for the gri container to prevent flashing
                            className="w-full grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-x-8 gap-y-12"
                        >
                            {products.products.map((product) => (
                                <motion.div
                                    key={product.id}
                                    layout // Add layout prop for smooth reordering
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    exit={{ opacity: 0, scale: 0.9 }}
                                    transition={{ duration: 0.3 }}
                                    role="button"
                                    tabIndex={0}
                                    onClick={() => handleOpenProduct(product)}
                                >
                                    <ProductCard getProductListItemDto={product} />
                                </motion.div>
                            ))}

                            {/* Loading overlay if fetching new data but keeping old data */}
                            {isLoading && (
                                <div className="fixed top-0 left-0 w-full h-1 bg-primary/30 z-50 animate-pulse" />
                            )}
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
                isOpen={isStoreModalOpen}
                onOpenChange={onStoreModalOpenChange}
                onStoreSelect={(store) => {
                    setSelectedStore(store)
                }}
            />
        </div>
    )
}
